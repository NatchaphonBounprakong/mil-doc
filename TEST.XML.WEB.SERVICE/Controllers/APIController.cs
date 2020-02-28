using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using WEB.API.DGA.MIL.DOC.DataAccesLayer;
using WEB.API.DGA.MIL.DOC.Models;
using WEB.API.DGA.MIL.DOC.Services;
using WEB.API.DGA.MIL.DOC.Utility;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace WEB.API.DGA.MIL.DOC.Controllers
{

    public class APIController : Controller
    {
        public RequestErrorResponse errorResponse = null;
        DocumentServices docService = new DocumentServices();

        // GET: API
        public ActionResult SendDoc()
        {
            string scp1 = @"http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2";
            string scp2 = @"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2";

            var bookProcessID = "";
            var bookLetterID = "";
            var bookCorrespondenceID = "";

            //scp 1 ส่งหนังสือไปให้ scp 2
            RequestSendDocOut book = GetTestBook();

            var sendDocResponse = RequestSendDocument2(scp1, book);

            //scp 2 รับหนังสือจาก scp 1
            var receiveDocResponse = RequestReceiveLetter2(scp2, "scp 2 รับหนังสือจาก scp 1") as ContentResult;

            XmlDocument receiveDocument = new XmlDocument();

            receiveDocument.LoadXml(receiveDocResponse.Content.ToString());

            var error = receiveDocument.GetElementsByTagName("ErrorDetail");

            #region === Check Error ===
            if (error.Count > 0)
            {
                return Content(receiveDocResponse.Content, "application/xml");
            }
            else
            {
                var pID = receiveDocument.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = receiveDocument.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = receiveDocument.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }
            #endregion

            //scp 2 ลบหนังสือ

            RequestDeleteQueue deleteSource = new RequestDeleteQueue()
            {
                MessageID = "scp 2 ลบหนังสือ",
                ProcessID = bookProcessID,
                To = scp2,
            };

            var deleteResponse = RequestDeleteDocumentQueue2(scp2, deleteSource) as ContentResult;

            #region === Check Error ===
            XmlDocument deleteDocument = new XmlDocument();
            deleteDocument.LoadXml(deleteResponse.Content.ToString());
            error = deleteDocument.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                return Content(receiveDocResponse.Content, "application/xml");
            }
            #endregion

            //scp2 ส่งตอบรับกลับไป scp1 
            RequestReceiveLetterNotifier receiveSource = new RequestReceiveLetterNotifier()
            {
                To = scp1,
                MessageID = "scp2 ส่งตอบรับกลับไป scp1",
                CorrespondenceDate = bookCorrespondenceID,
                LetterID = bookLetterID,
                Subject = "scp2 ส่งตอบรับกลับไป scp1"
            };

            var documentNotifierResponse = RequestReceiveLetterNotifier2(scp2, receiveSource) as ContentResult;

            #region === Check Error ===
            XmlDocument notifierDocument = new XmlDocument();
            deleteDocument.LoadXml(documentNotifierResponse.Content.ToString());
            error = receiveDocument.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                return Content(documentNotifierResponse.Content, "application/xml");
            }
            #endregion

            //scp1 รับหนังสือตอบรับ
            var receiveDocResponse2 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือตอบรับจาก scp 2") as ContentResult;

            XmlDocument receiveDocument2 = new XmlDocument();

            receiveDocument2.LoadXml(receiveDocResponse2.Content.ToString());

            #region === Check Error ===
            error = receiveDocument2.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = receiveDocument2.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = receiveDocument2.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = receiveDocument2.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }
            #endregion

            //scp1 ลบหนังสือตอบรับ
            RequestDeleteQueue deleteSource2 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };

            var deleteResponse2 = RequestDeleteDocumentQueue2(scp1, deleteSource2) as ContentResult;

            #region === Check Error ===
            XmlDocument deleteDocument2 = new XmlDocument();
            deleteDocument2.LoadXml(deleteResponse2.Content.ToString());
            error = deleteDocument2.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                return Content(deleteResponse2.Content, "application/xml");
            }
            #endregion

            //scp2 ส่งแจ้งเลขไป scp1

            RequestAcceptLetterNotifier acceptSource = new RequestAcceptLetterNotifier()
            {
                To = scp1,
                Subject = "ทดสอบระบบ",
                AcceptID = "เลขแจ้งรับ",
                CorrespondenceDate = bookCorrespondenceID,
                LetterID = bookLetterID,
                MessageID = "ทดสอบระบบ"
            };

            var sendNumber = RequestAcceptLetterNotifier2(scp2, acceptSource) as ContentResult;

            #region === Check Error ===
            XmlDocument acceptDocument = new XmlDocument();
            acceptDocument.LoadXml(sendNumber.Content.ToString());
            error = deleteDocument2.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                return Content(deleteResponse2.Content, "application/xml");
            }
            #endregion


            //scp1 รับหนังสือแจ้งเลข
            var receiveContent3 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือแจ้งเลข scp 2") as ContentResult;
            XmlDocument doc3 = new XmlDocument();

            doc3.LoadXml(receiveContent3.Content.ToString());

            var error3 = doc3.GetElementsByTagName("ErrorDetail");

            if (error3.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc3.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc3.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc3.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp1 ลบหนังสือตอบรับ
            RequestDeleteQueue source4 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };
            var deleteContent3 = RequestDeleteDocumentQueue2(scp1, source4) as ContentResult;

            return Content(deleteContent3.Content, "Application/xml");
        }
        // GET: API
        public ActionResult SendRejectDoc()
        {
            string scp1 = @"http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2";
            string scp2 = @"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2";

            //scp 1 ส่งหนังสือไปให้ scp 2
            RequestSendDocOut book = GetTestBook();
            var senderSendContent = RequestSendDocument2(scp1, book);

            //scp 2 รับหนังสือจาก scp 1
            var receiverRecieveBook = RequestReceiveLetter2(scp2, "scp 2 รับหนังสือจาก scp 1") as ContentResult;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(receiverRecieveBook.Content.ToString());

            var error = doc.GetElementsByTagName("ErrorDetail");
            var bookProcessID = "";
            var bookLetterID = "";
            var bookCorrespondenceID = "";


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp 2 ลบหนังสือ
            RequestDeleteQueue source = new RequestDeleteQueue()
            {
                MessageID = "scp 2 ลบหนังสือ",
                ProcessID = bookProcessID,
                To = scp2,
            };
            var deleteContent = RequestDeleteDocumentQueue2(scp2, source);

            //scp2 ส่ง Reject กลับไป scp1 
            RequestRejectLetterNotifier source2 = new RequestRejectLetterNotifier()
            {
                CorrespondenceData = bookCorrespondenceID,
                LetterID = bookLetterID,
                MessageID = "ทดสอบ Reject",
                Subject = "ทดสอบ Reject",
                To = scp1
            };
            var documentReject = RequestRejectLetterNotifier2(scp2, source2);

            //scp1 รับหนังสือ reject
            var receiveContent2 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือ reject จาก scp 2") as ContentResult;
            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(receiveContent2.Content.ToString());

            var error2 = doc2.GetElementsByTagName("ErrorDetail");


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc2.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc2.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc2.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp1 ลบหนังสือ Reject
            RequestDeleteQueue source3 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };
            var deleteContent2 = RequestDeleteDocumentQueue2(scp1, source3);

            //scp1 ขอลบหนังสือเพื่อส่งใหม่
            RequestDeleteGovernmentDocument source4 = new RequestDeleteGovernmentDocument()
            {
                To = scp1,
                MessageID = "scp2 ขอลบหนังสือ",
                LetterID = bookLetterID,
                AcceptDepartment = book.ReceiverDeptID,
                CorrespondenceData = bookCorrespondenceID,
                SenderDepartment = book.SenderDeptID,
            };

            var deleteResend = RequestDeleteGovernmentDocument2(scp1, source4);

            return View();
        }
        // GET: API
        public ActionResult SendInvalidDoc()
        {
            string scp1 = @"http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2";
            string scp2 = @"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2";

            //scp 1 ส่งหนังสือไปให้ scp 2
            RequestSendDocOut book = GetTestBook();
            var senderSendContent = RequestSendDocument2(scp1, book);

            //scp 2 รับหนังสือจาก scp 1
            var receiverRecieveBook = RequestReceiveLetter2(scp2, "scp 2 รับหนังสือจาก scp 1") as ContentResult;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(receiverRecieveBook.Content.ToString());

            var error = doc.GetElementsByTagName("ErrorDetail");
            var bookProcessID = "";
            var bookLetterID = "";
            var bookCorrespondenceID = "";


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp 2 ลบหนังสือ
            RequestDeleteQueue source = new RequestDeleteQueue()
            {
                MessageID = "scp 2 ลบหนังสือ",
                ProcessID = bookProcessID,
                To = scp2,
            };
            var deleteContent = RequestDeleteDocumentQueue2(scp2, source);

            //scp2 ส่ง Invalid กลับไป scp1 
            RequestInvalidLetterNotifier source2 = new RequestInvalidLetterNotifier()
            {
                CorrespondenceData = bookCorrespondenceID,
                LetterID = bookLetterID,
                MessageID = "ทดสอบ Reject",
                Subject = "ทดสอบ Reject",
                To = scp1
            };
            var invalidDocumentReponse = RequestInvalidLetterNotifier2(scp2, source2);

            //scp1 รับหนังสือ Invalid
            var receiveContent2 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือ reject จาก scp 2") as ContentResult;
            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(receiveContent2.Content.ToString());

            var error2 = doc2.GetElementsByTagName("ErrorDetail");


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc2.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc2.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc2.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp1 ลบหนังสือ Invalid
            RequestDeleteQueue source3 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };
            var deleteContent2 = RequestDeleteDocumentQueue2(scp1, source3);

            //scp1 ขอลบหนังสือเพื่อส่งใหม่
            RequestDeleteGovernmentDocument source4 = new RequestDeleteGovernmentDocument()
            {
                To = scp1,
                MessageID = "scp2 ขอลบหนังสือ",
                LetterID = bookLetterID,
                AcceptDepartment = book.ReceiverDeptID,
                CorrespondenceData = bookCorrespondenceID,
                SenderDepartment = book.SenderDeptID,
            };

            var deleteResend = RequestDeleteGovernmentDocument2(scp1, source4);

            return View();
        }
        // GET: API
        public ActionResult SendInvalidAcceptDoc()
        {
            string scp1 = @"http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2";
            string scp2 = @"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2";

            //scp 1 ส่งหนังสือไปให้ scp 2
            RequestSendDocOut book = GetTestBook();
            var senderSendContent = RequestSendDocument2(scp1, book);

            //scp 2 รับหนังสือจาก scp 1
            var receiverRecieveBook = RequestReceiveLetter2(scp2, "scp 2 รับหนังสือจาก scp 1") as ContentResult;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(receiverRecieveBook.Content.ToString());

            var error = doc.GetElementsByTagName("ErrorDetail");
            var bookProcessID = "";
            var bookLetterID = "";
            var bookCorrespondenceID = "";


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp 2 ลบหนังสือ
            RequestDeleteQueue source = new RequestDeleteQueue()
            {
                MessageID = "scp 2 ลบหนังสือ",
                ProcessID = bookProcessID,
                To = scp2,
            };
            var deleteContent = RequestDeleteDocumentQueue2(scp2, source);

            //scp2 ส่งตอบรับกลับไป scp1 
            RequestReceiveLetterNotifier source2 = new RequestReceiveLetterNotifier()
            {
                To = scp1,
                MessageID = "scp2 ส่งตอบรับกลับไป scp1",
                CorrespondenceDate = bookCorrespondenceID,
                LetterID = bookLetterID,
                Subject = "scp2 ส่งตอบรับกลับไป scp1"
            };
            var documentNotifier = RequestReceiveLetterNotifier2(scp2, source2);

            //scp1 รับหนังสือตอบรับ
            var receiveContent2 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือตอบรับจาก scp 2") as ContentResult;
            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(receiveContent2.Content.ToString());

            var error2 = doc2.GetElementsByTagName("ErrorDetail");


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc2.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc2.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc2.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp1 ลบหนังสือตอบรับ
            RequestDeleteQueue source3 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };
            var deleteContent2 = RequestDeleteDocumentQueue2(scp1, source3);

            //scp2 ส่งแจ้งเลขไป scp1
            RequestAcceptLetterNotifier accept = new RequestAcceptLetterNotifier()
            {
                To = scp1,
                Subject = "ทดสอบระบบ",
                AcceptID = "เลขแจ้งรับ",
                CorrespondenceDate = bookCorrespondenceID,
                LetterID = bookLetterID,
                MessageID = "ทดสอบระบบ"
            };
            var sendNumber = RequestAcceptLetterNotifier2(scp2, accept) as ContentResult;

            //scp1 รับหนังสือแจ้งเลข
            var receiveContent3 = RequestReceiveLetter2(scp1, "scp 1 รับหนังสือแจ้งเลข scp 2") as ContentResult;
            XmlDocument doc3 = new XmlDocument();

            doc3.LoadXml(receiveContent3.Content.ToString());

            var error3 = doc3.GetElementsByTagName("ErrorDetail");

            if (error3.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc3.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc3.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc3.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp1 ลบหนังสือตอบรับ
            RequestDeleteQueue source4 = new RequestDeleteQueue()
            {
                MessageID = "scp 1 ลบหนังสือตอบรับ",
                ProcessID = bookProcessID,
                To = scp1,
            };
            var deleteContent3 = RequestDeleteDocumentQueue2(scp1, source4);


            //scp1 แจ้งเลขรับผิดไปที่ scp2
            RequestInvalidAcceptIDNotifier source5 = new RequestInvalidAcceptIDNotifier()
            {
                AcceptID = "เลขแจ้งรับ",
                CorrespondenceData = bookCorrespondenceID,
                LetterID = bookLetterID,
                MessageID = "scp1 แจ้งเลขรับผิดไปที่ scp2",
                Subject = "scp1 แจ้งเลขรับผิดไปที่ scp2",
                To = scp2
            };

            var sendInvalidAcceptIDResponse = RequestInvalidAcceptIDNotifier2(scp1, source5);

            var receiveDocumentResponse = RequestReceiveLetter2(scp2, "scp 2 รับหนังสือจาก scp 1") as ContentResult;

            XmlDocument doc4 = new XmlDocument();
            doc4.LoadXml(receiveDocumentResponse.Content.ToString());

            error = doc4.GetElementsByTagName("ErrorDetail");


            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc4.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    bookProcessID = pID[0].InnerXml.ToString();
                }

                var lID = doc4.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    bookLetterID = lID[0].InnerXml.ToString();
                }

                var cDate = doc4.GetElementsByTagName("ram:CorrespondenceDate");
                if (lID.Count > 0)
                {
                    bookCorrespondenceID = cDate[0].InnerXml.ToString();
                }
            }

            //scp 2 ลบหนังสือ
            RequestDeleteQueue source6 = new RequestDeleteQueue()
            {
                MessageID = "scp 2 ลบหนังสือ",
                ProcessID = bookProcessID,
                To = scp2,
            };

            var deleteDocumentResponse = RequestDeleteDocumentQueue2(scp2, source6);
            return View();
        }

        #region ================================= OLD CODE ======================================
        //ส่งหนังสือ
        //[EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        public ActionResult RequestSendDocument2(string from, [FromBody]RequestSendDocOut source)
        {
            try
            {
                var xml = XMLCreation.RequestSendDocument(source);
                var response = postXMLData(from, xml);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                var processID = string.Empty;
                return Content(response, "application/xml");

            }
            catch (Exception ex)
            {

                return Content("Error :" + ex.Message, "application/xml");
            }
        }

        //รับหนังสือ
        [HttpPost]
        public ActionResult RequestReceiveLetter2(string to, string messageID)
        {
            //รับหนังสือ
            RequestReceive source = new RequestReceive()
            {
                MessageID = messageID,
                To = to
            };
            var xml = XMLCreation.RequestReceiveDocumentLetter(source);
            var response = postXMLData(to, xml);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);

            var error = doc.GetElementsByTagName("ErrorDetail");
            var processID = "";
            var letterID = "";

            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    processID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    letterID = lID[0].InnerXml.ToString();
                }
            }

            return Content(response, "application/xml");
        }

        //ลบหนังสือออกจากคิว
        [HttpPost]
        public ActionResult RequestDeleteDocumentQueue2(string to, [FromBody]RequestDeleteQueue source)
        {
            //ลบหนังสือ 
            var xml = XMLCreation.RequestDeleteDocumentQueue(source);
            var response = postXMLData(to, xml);


            return Content(response, "application/xml");
        }

        //ส่งหนังสือตอบรับ
        [HttpPost]
        public ActionResult RequestReceiveLetterNotifier2(string from, [FromBody]RequestReceiveLetterNotifier source)
        {

            var xml = XMLCreation.RequestReceiveLetterNotifier(source);
            var response = postXMLData(from, xml);

            return Content(response, "application/xml");
        }
        //ส่งหนังสือแจ้งเลขรับ
        [HttpPost]
        public ActionResult RequestAcceptLetterNotifier2(string from, [FromBody]RequestAcceptLetterNotifier source)
        {


            var xml = XMLCreation.RequestAcceptLetterNotifier(source);
            var response = postXMLData(from, xml);
            return Content(response, "application/xml");
        }
        //ปฏิเสธหนังสือ
        [HttpPost]
        public ActionResult RequestRejectLetterNotifier2(string from, [FromBody]RequestRejectLetterNotifier source)
        {
            var xml = XMLCreation.RequestRejectLetterNotifier(source);
            var response = postXMLData(from, xml);
            return Content(response, "application/xml");
        }
        //หนังสือผิด
        [HttpPost]
        public ActionResult RequestInvalidLetterNotifier2(string from, [FromBody]RequestInvalidLetterNotifier source)
        {
            var xml = XMLCreation.RequestInvalidLetterNotifier(source);
            var response = postXMLData(from, xml);
            return Content(response, "application/xml");
        }
        //ลบหนังสือส่งใหม่
        [HttpPost]
        public ActionResult RequestDeleteGovernmentDocument2(string from, [FromBody]RequestDeleteGovernmentDocument source)
        {
            var xml = XMLCreation.RequestDeleteGovernmentDocument(source);
            var response = postXMLData(from, xml);
            return Content(response, "application/xml");
        }
        //แจ้งเลขผิด
        [HttpPost]
        public ActionResult RequestInvalidAcceptIDNotifier2(string from, [FromBody]RequestInvalidAcceptIDNotifier source)
        {
            var xml = XMLCreation.RequestInvalidAcceptIDNotifier(source);
            var response = postXMLData(from, xml);
            return Content(response, "application/xml");
        }

        public ActionResult RequestReceiveDocumentLetter2()
        {
            //รับหนังสือ
            RequestReceive source = new RequestReceive()
            {
                MessageID = "Test",
                To = "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2"
            };
            var xml = XMLCreation.RequestReceiveDocumentLetter(source);
            var response = postXMLData(@"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2", xml);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);

            var error = doc.GetElementsByTagName("ErrorDetail");
            var processID = "";
            var letterID = "";

            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    processID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    letterID = lID[0].InnerXml.ToString();
                }
            }
            //ส่งหนังสือตอบรับ
            //RequestReceiveLetterNotifier source4 = new RequestReceiveLetterNotifier()
            //{
            //    MessageID = "Test",
            //    To = "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2",
            //    CorrespondenceDate = "2020-02-11",
            //    LetterID = letterID.Trim(),
            //    Subject = "หนังสือตอบรับ"
            //};
            //var xml4 = XMLCreation.RequestReceiveLetterNotifier(source4);
            //var response4 = postXMLData(@"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2", xml4);


            //ลบหนังสือ 
            if (processID != string.Empty)
            {
                RequestDeleteQueue source3 = new RequestDeleteQueue()
                {
                    MessageID = "ทดสอบ",
                    ProcessID = processID.Trim(),
                    To = "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2",


                };
                var xml3 = XMLCreation.RequestDeleteDocumentQueue(source3);
                response = postXMLData(@"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2", xml3);
            }




            return Content(response, "application/xml");
        }

        public ActionResult RequestAcceptLetterNotifier2()
        {

            var b = "ทดสอบระบบ/38-13";
            //ส่งหนังสือตอบรับ
            RequestAcceptLetterNotifier source4 = new RequestAcceptLetterNotifier()
            {
                MessageID = "Test",
                To = "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2",
                CorrespondenceDate = "2020-02-11",
                LetterID = b,
                Subject = "หนังสือแจ้งเลขรับ",
                AcceptID = "Test"
            };
            var xml = XMLCreation.RequestAcceptLetterNotifier(source4);
            var response = postXMLData(@"http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2", xml);


            return Content(response, "application/xml");
        }


        public RequestSendDocOut GetTestBook()
        {
            byte[] pdfBytes = System.IO.File.ReadAllBytes(@"D:\\script.sql");
            string pdfBase64 = Convert.ToBase64String(pdfBytes);
            RequestSendDocOut obj = new RequestSendDocOut()
            {
                MessageID = "ทดสอบระบบ",
                To = "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2",
                ID = "ทดสอบระบบ/" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString(),
                CorrespondenceDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Subject = "ทดสอบระบบ",
                SecretCode = "001",
                SpeedCode = "001",
                Attachment = "Attachment",
                SendDate = "2019-01-31T05:00:00",
                Description = "Description",
                MainLetterBinaryObjectMimeCode = "application/pdf",
                MainLetterBinaryObject = pdfBase64,
                SenderFamilyName = "FamilyName",
                SenderGivenName = "GivenName",
                SenderJobTitle = "JobTitle",
                SenderDeptID = "0031500001",
                SenderMinistryID = "00",
                ReceiverFamilyName = "FamilyName",
                ReceiverGivenName = "GivenName",
                ReceiverJobTitle = "JobTitle",
                ReceiverDeptID = "0031500002",
                ReceiverMinistryID = "00",
                References = new List<Reference>(),
                Attachments = new List<Attachment>()

            };

            Reference reference = new Reference()
            {
                ID = "ทดสอบ",
                Subject = "ทดสอบ",
                CorrespondenceDate = "2019-01-31"
            };
            obj.References.Add(reference);
            obj.References.Add(reference);
            Attachment attach = new Attachment()
            {
                AttachmentBinaryObject = pdfBase64,
                AttachmentBinaryObjectMimeCode = "application/pdf",
            };
            obj.Attachments.Add(attach);
            obj.Attachments.Add(attach);

            return obj;
        }

        public bool IsResponseSuccess(XmlDocument doc)
        {
            var error = doc.GetElementsByTagName("ErrorDetail");
            if (error.Count > 0)
            {
                errorResponse = new RequestErrorResponse()
                {
                    ErrorDetail = new List<ErrorDetail>()
                };
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                    errorResponse.ErrorDetail.Add(errorDetail);
                }


                return false;
            }
            else
            {
                return true;
            }

        }
        #endregion
        public ActionResult RequestReceiveDocumentLetterDelete(string delfrom)
        {
            var from = delfrom;
            //รับหนังสือ
            RequestReceive source = new RequestReceive()
            {
                MessageID = "Test",
                To = from
            };
            var xml = XMLCreation.RequestReceiveDocumentLetter(source);
            var response = postXMLData(from, xml);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);

            var error = doc.GetElementsByTagName("ErrorDetail");
            var processID = "";
            var letterID = "";

            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }
            }
            else
            {
                var pID = doc.GetElementsByTagName("ProcessID");
                if (pID.Count > 0)
                {
                    processID = pID[0].InnerXml.ToString();
                }

                var lID = doc.GetElementsByTagName("ram:ID");
                if (lID.Count > 0)
                {
                    letterID = lID[0].InnerXml.ToString();
                }
            }

            //ลบหนังสือ 
            if (processID != string.Empty)
            {
                RequestDeleteQueue source3 = new RequestDeleteQueue()
                {
                    MessageID = "ทดสอบ",
                    ProcessID = processID.Trim(),
                    To = from,


                };
                var xml3 = XMLCreation.RequestDeleteDocumentQueue(source3);
                response = postXMLData(from, xml3);
            }




            return Content(response, "application/xml");
        }
        public string postXMLData(string destinationUrl, string requestXml)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                byte[] bytes;
                bytes = System.Text.Encoding.UTF8.GetBytes(requestXml);
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return responseStr;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //ส่งหนังสือ
        [HttpPost]
        public JsonResult RequestSendDocument(int id)
        {
            var postResponse = "";
            Response resp = new Response();
            try
            {

                var docResponse = docService.GetdocumentWithAtt(id);

                if (docResponse.Status)
                {
                    var processID = string.Empty;
                    var document = (Document)docResponse.ResponseObject;
                    byte[] pdfBytes = document.MainAttachmentBinary;
                    string pdfBase64 = Convert.ToBase64String(pdfBytes);
                    var receive = docService.GetOrganizationById(document.ReceiverOrganizationId).ResponseObject;

                    RequestSendDocOut source = new RequestSendDocOut()
                    {
                        MessageID = document.Id.ToString(),
                        To = receive.Url.ToString(),
                        ID = document.No,
                        CorrespondenceDate = document.Date,
                        Subject = document.Subject,
                        SecretCode = document.Secret,
                        SpeedCode = document.Speed,
                        SenderGivenName = document.SenderName,
                        SenderFamilyName = document.SenderSurname,
                        SenderDeptID = document.Organization.Code,
                        SenderMinistryID = document.Organization.Code.Substring(0, 2),
                        SenderJobTitle = document.SenderPosition,
                        ReceiverGivenName = document.ReceiverName,
                        ReceiverFamilyName = document.ReceiverSurname,
                        ReceiverDeptID = receive.Code.ToString(),
                        ReceiverJobTitle = document.ReceiverPosition,
                        ReceiverMinistryID = receive.Code.ToString().Substring(0, 2),
                        Attachment = "-",
                        SendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Description = document.Description,
                        MainLetterBinaryObjectMimeCode = ConvertContentType(Path.GetExtension(document.MainAttachmentName)),
                        MainLetterBinaryObject = pdfBase64,
                        References = new List<Reference>(),
                        Attachments = new List<Attachment>(),

                    };
                    if (document.DocumentReference.Count == 0)
                    {
                        Reference reference = new Reference();
                        source.References.Add(reference);
                    }
                    else
                    {
                        foreach (var refer in document.DocumentReference)
                        {
                            Reference reference = new Reference()
                            {
                                ID = refer.ReferenceBookNo,
                                CorrespondenceDate = refer.ReferenceBookDate,
                                Subject = refer.ReferenceBookSubject,
                            };

                            source.References.Add(reference);
                        }
                    }


                    if (document.DocumentAttachment.Count == 0)
                    {
                        Attachment attachment = new Attachment();
                        source.Attachments.Add(attachment);
                    }
                    else
                    {
                        foreach (var att in document.DocumentAttachment)
                        {
                            Attachment attachment = new Attachment()
                            {
                                AttachmentBinaryObject = Convert.ToBase64String(att.AttachmentBinary),
                                AttachmentBinaryObjectMimeCode = ConvertContentType(Path.GetExtension(att.AttachmentName)),

                            };
                            source.Attachments.Add(attachment);
                        }
                    }


                    var xml = XMLCreation.RequestSendDocument(source);
                    postResponse = postXMLData(document.Organization.Url, xml);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(postResponse);

                    var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {
                            resp.Status = false;

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };

                            resp.ResponseObject = errorDetail;
                        }
                    }
                    else
                    {
                        var pID = xmlDoc.GetElementsByTagName("ProcessID");
                        if (pID.Count > 0)
                        {
                            processID = pID[0].InnerXml.ToString();
                        }

                        resp = docService.UpdateDocumentStatus(id, processID, "ส่งหนังสือรอตอบรับ");
                        resp.ResponseObject = null;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }
            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //รับหนังสือส่ง
        [HttpPost]
        public ActionResult RequestReceiveDocument(string to, string messageID)
        {

            RequestReceive source = new RequestReceive()
            {
                MessageID = messageID,
                To = to
            };
            var xml = XMLCreation.RequestReceiveDocumentLetter(source);
            var xmlResponse = postXMLData(to, xml);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResponse);

            var error = xmlDoc.GetElementsByTagName("ErrorDetail");

            if (error.Count > 0)
            {
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };
                }

            }
            else
            {
                var invalid = xmlDoc.GetElementsByTagName("InvalidAcceptIDNotifier");
                var pID = xmlDoc.GetElementsByTagName("ProcessID");
                var lId = xmlDoc.GetElementsByTagName("LetterID");
                var cDate = xmlDoc.GetElementsByTagName("CorrespondenceDate");
                var sj = xmlDoc.GetElementsByTagName("Subject");
                var acceptId = xmlDoc.GetElementsByTagName("AcceptID");
                var governmentDoc = xmlDoc.GetElementsByTagName("rsm:GovernmentDocument");

                if (invalid.Count > 0)
                {
                    Document document = new Document()
                    {
                        ProcessId = pID[0].InnerXml.ToString(),
                        BookId = lId[0].InnerXml.ToString(),
                        No = lId[0].InnerXml.ToString(),
                        Date = cDate[0].InnerXml.ToString(),
                        Subject = sj[0].InnerXml.ToString()
                    };

                    var resp = docService.UpdateDocumentInvalidAcceptId(document);

                    if (resp.Status && resp.ResponseObject != 0)
                    {
                        RequestDeleteQueue delSource = new RequestDeleteQueue()
                        {
                            MessageID = resp.ResponseObject.ToString(),
                            ProcessID = document.ProcessId,
                            To = to,
                        };

                        var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                        var delResp = postXMLData(to, delXml);

                    }
                }
                else if (governmentDoc.Count > 0)
                {
                    DocumentIn docIn = new DocumentIn();

                    if (pID.Count > 0)
                    {
                        docIn.ProcessId = pID[0].InnerXml.ToString();
                    }

                    var lID = xmlDoc.GetElementsByTagName("ram:ID");
                    if (lID.Count > 0)
                    {
                        docIn.BookId = lID[0].InnerXml.ToString();
                        docIn.No = lID[0].InnerXml.Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">");
                    }

                    var correspondenceDate = xmlDoc.GetElementsByTagName("ram:CorrespondenceDate");
                    if (correspondenceDate.Count > 0) docIn.Date = correspondenceDate[0].InnerXml.ToString();

                    var subject = xmlDoc.GetElementsByTagName("ram:Subject");
                    if (subject.Count > 0) docIn.Subject = subject[0].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;

                    var secretCode = xmlDoc.GetElementsByTagName("ram:SecretCode");
                    if (secretCode.Count > 0) docIn.Secret = secretCode[0].InnerXml.ToString();

                    var speedCode = xmlDoc.GetElementsByTagName("ram:SpeedCode");
                    if (speedCode.Count > 0) docIn.Speed = speedCode[0].InnerXml.ToString();

                    var givenName = xmlDoc.GetElementsByTagName("ram:GivenName");
                    if (givenName.Count > 0)
                    {
                        docIn.SenderName = givenName[0].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                        docIn.ReceiverName = givenName[1].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                    }

                    var familyName = xmlDoc.GetElementsByTagName("ram:FamilyName");
                    if (familyName.Count > 0)
                    {
                        docIn.SenderSurname = familyName[0].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                        docIn.ReceiverSurname = familyName[1].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                    }

                    var jobTitle = xmlDoc.GetElementsByTagName("ram:JobTitle");
                    if (jobTitle.Count > 0)
                    {
                        docIn.SenderPosition = jobTitle[0].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                        docIn.ReceiverPosition = jobTitle[1].InnerXml.ToString().Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"); ;
                    }

                    var department = xmlDoc.GetElementsByTagName("ram:DepartmentOrganization");
                    if (department.Count > 0)
                    {
                        var obj = (WEB.API.DGA.MIL.DOC.DataAccesLayer.Organization)docService.GetOrganizationByCode(department[0].ChildNodes[0].InnerXml).ResponseObject;
                        docIn.SenderOrganizationId = obj.Id;

                        var obj2 = (WEB.API.DGA.MIL.DOC.DataAccesLayer.Organization)docService.GetOrganizationByCode(department[1].ChildNodes[0].InnerXml).ResponseObject;
                        docIn.ReceiverOrganizationId = obj2.Id;
                    }

                    var reference = xmlDoc.GetElementsByTagName("ram:ReferenceCorrespondence");
                    if (reference.Count > 0)
                    {
                        docIn.DocumentReference = new List<DocumentReference>();
                        for (int i = 0; i < reference.Count; i++)
                        {
                            DocumentReference refer = new DocumentReference()
                            {
                                ReferenceBookNo = reference[i].ChildNodes[0].InnerXml.Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"),
                                ReferenceBookDate = reference[i].ChildNodes[1].InnerXml,
                                ReferenceBookSubject = reference[i].ChildNodes[2].InnerXml.Replace("&lt;", "<")
                                                       .Replace("&amp;", "&")
                                                       .Replace("&gt;", ">"),
                            };

                            if (refer.ReferenceBookNo == "" && refer.ReferenceBookDate == "" && refer.ReferenceBookSubject == "")
                            {

                            }
                            else
                            {
                                docIn.DocumentReference.Add(refer);
                            }

                        }
                    }

                    var attachment = xmlDoc.GetElementsByTagName("ram:Attachment");
                    if (attachment.Count > 0)
                    {

                    }

                    var sendDate = xmlDoc.GetElementsByTagName("ram:SendDate");
                    if (attachment.Count > 0)
                    {

                    }

                    var mainAttachment = xmlDoc.GetElementsByTagName("ram:MainLetterBinaryObject");
                    if (mainAttachment.Count > 0)
                    {
                        docIn.MimeCode = mainAttachment[0].Attributes[0].InnerText;
                        byte[] bytes = System.Convert.FromBase64String(mainAttachment[0].InnerXml.Trim());
                        docIn.MainAttachmentBinary = bytes;
                        docIn.MainAttachmentName = "เอกสารหลัก" + ConvertExtensionType(mainAttachment[0].Attributes[0].InnerText);
                        docIn.FileSize = ConvertBytesToMegabytes(bytes.Length).ToString("N5") + " mb";
                    }

                    var attachmentBinaryObject = xmlDoc.GetElementsByTagName("ram:AttachmentBinaryObject");
                    if (attachmentBinaryObject.Count > 0)
                    {
                        docIn.DocumentAttachment = new List<DocumentAttachment>();
                        for (int i = 0; i < attachmentBinaryObject.Count; i++)
                        {
                            var MimeCode = attachmentBinaryObject[0].Attributes[0].InnerText;
                            DocumentAttachment att = new DocumentAttachment()
                            {
                                AttachmentBinary = System.Convert.FromBase64String(attachmentBinaryObject[i].InnerXml.Trim()),
                                State = "บันทึก",
                                Type = "2",
                                MimeCode = attachmentBinaryObject[i].Attributes[0].InnerText,
                                AttachmentName = "เอกสารแนบ" + (i + 1) + "" + ConvertExtensionType(attachmentBinaryObject[i].Attributes[0].InnerText),
                                FileSize = ConvertBytesToMegabytes(System.Convert.FromBase64String(attachmentBinaryObject[i].InnerXml.Trim()).Length).ToString("N5") + " mb"
                            };
                            if (att.AttachmentBinary.Length == 0)
                            {

                            }
                            else
                            {
                                docIn.DocumentAttachment.Add(att);
                            }

                        }

                    }
                    docIn.CreatedDate = DateTime.Now;
                    docIn.Status = "รอส่งหนังสือตอบรับ";
                    var resp = docService.AddDocumentIn(docIn);

                    if (resp.Status && resp.ResponseObject != null)
                    {
                        RequestDeleteQueue delSource = new RequestDeleteQueue()
                        {
                            MessageID = resp.ResponseObject.Id.ToString(),
                            ProcessID = resp.ResponseObject.ProcessId,
                            To = to,
                        };

                        var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                        var delResp = postXMLData(to, delXml);
                    }
                }

            }

            return Content("");
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        //ส่งหนังสือตอบรับ
        [HttpPost]
        public JsonResult RequestSendNotifierDocument(int id)
        {
            var resp = new Response();
            try
            {
                resp = docService.GetDocumentInWithAtt(Convert.ToInt32(id));
                if (resp.Status)
                {
                    DocumentIn document = resp.ResponseObject;
                    RequestReceiveLetterNotifier source = new RequestReceiveLetterNotifier()
                    {
                        CorrespondenceDate = document.Date,
                        LetterID = document.BookId,
                        MessageID = id.ToString(),
                        Subject = document.Subject,
                        To = document.Organization.Url,
                    };

                    var xml = XMLCreation.RequestReceiveLetterNotifier(source);
                    var response = postXMLData(document.Organization1.Url, xml);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    var error = doc.GetElementsByTagName("ErrorDetail");
                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };
                            resp.Status = false;
                        }
                    }
                    else
                    {
                        if (resp.Status)
                        {
                            var pID = doc.GetElementsByTagName("ProcessID");
                            if (pID.Count > 0)
                            {
                                var processId = pID[0].InnerXml.ToString();
                                resp = docService.UpdateDocumentInStatus(id, processId, "ส่งหนังสือตอบรับแล้ว");
                            }
                        }

                    }
                }
                resp.ResponseObject = null;
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ส่งหนังสือแจ้งเลขรับ
        [HttpPost]
        public JsonResult RequestSendNumberDocument(int id, string acceptId)
        {

            var resp = new Response();
            try
            {
                resp = docService.GetDocumentInWithAtt(id);
                if (resp.Status)
                {

                    DocumentIn document = resp.ResponseObject;
                    docService.UpdateDocumentInAcceptId(id, acceptId);
                    RequestAcceptLetterNotifier source = new RequestAcceptLetterNotifier()
                    {
                        CorrespondenceDate = document.Date,
                        LetterID = document.No,
                        MessageID = id.ToString(),
                        Subject = document.Subject,
                        To = document.Organization.Url,
                        AcceptID = acceptId
                    };

                    var xml = XMLCreation.RequestAcceptLetterNotifier(source);
                    var response = postXMLData(document.Organization1.Url, xml);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    var error = doc.GetElementsByTagName("ErrorDetail");
                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };
                        }
                        resp.Status = false;
                    }
                    else
                    {
                        if (resp.Status)
                        {
                            var pID = doc.GetElementsByTagName("ProcessID");
                            if (pID.Count > 0)
                            {
                                var processId = pID[0].InnerXml.ToString();
                                resp = docService.UpdateDocumentInStatus(id, processId, "ส่งหนังสือแจ้งเลขรับแล้ว");
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }
            resp.ResponseObject = null;
            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ส่งปฏิเสธหนังสือ
        [HttpPost]
        public JsonResult RequestRejectLetterNotifier(int id)
        {
            var resp = new Response();
            try
            {
                resp = docService.GetDocumentInWithAtt(id);
                if (resp.Status)
                {
                    DocumentIn document = resp.ResponseObject;
                    RequestRejectLetterNotifier source = new RequestRejectLetterNotifier()
                    {
                        CorrespondenceData = document.Date,
                        LetterID = document.No,
                        MessageID = document.Id.ToString(),
                        Subject = document.Subject,
                        To = document.Organization.Url,
                    };
                    var xml = XMLCreation.RequestRejectLetterNotifier(source);
                    var response = postXMLData(document.Organization1.Url, xml);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    var error = doc.GetElementsByTagName("ErrorDetail");
                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };
                        }
                        resp.Status = false;
                    }
                    else
                    {
                        var pID = doc.GetElementsByTagName("ProcessID");
                        if (pID.Count > 0)
                        {
                            var processId = pID[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentInStatus(id, processId, "ปฏิเสธการรับหนังสือ");
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }
            resp.ResponseObject = null;
            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ส่งแจ้งหนังสือผิด
        [HttpPost]
        public JsonResult RequestInvalidLetterNotifier(int id)
        {
            var resp = docService.GetDocumentInWithAtt(id);
            try
            {
                if (resp.Status)
                {
                    DocumentIn document = resp.ResponseObject;
                    RequestInvalidLetterNotifier source = new RequestInvalidLetterNotifier()
                    {
                        CorrespondenceData = document.Date,
                        LetterID = document.No,
                        MessageID = document.Id.ToString(),
                        Subject = document.Subject,
                        To = document.Organization.Url,

                    };
                    var xml = XMLCreation.RequestInvalidLetterNotifier(source);
                    var response = postXMLData(document.Organization1.Url, xml);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    var error = doc.GetElementsByTagName("ErrorDetail");
                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };
                        }
                        resp.Status = false;
                    }
                    else
                    {
                        var pID = doc.GetElementsByTagName("ProcessID");
                        if (pID.Count > 0)
                        {
                            var processId = pID[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentInStatus(id, processId, "แจ้งหนังสือผิด");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }
            resp.ResponseObject = null;
            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ส่งแจ้งเลขหนังสือผิด
        [HttpPost]
        public JsonResult RequestInvalidAcceptIDNotifier(int id)
        {

            var resp = new Response();
            try
            {
                resp = docService.GetdocumentWithAtt(id);
                if (resp.Status)
                {

                    Document document = resp.ResponseObject;
                    RequestInvalidAcceptIDNotifier source = new RequestInvalidAcceptIDNotifier()
                    {
                        CorrespondenceData = document.Date,
                        LetterID = document.No,
                        MessageID = document.Id.ToString(),
                        Subject = document.Subject,
                        To = document.Organization1.Url,
                        AcceptID = document.AcceptId,

                    };

                    var xml = XMLCreation.RequestInvalidAcceptIDNotifier(source);
                    var response = postXMLData(document.Organization.Url, xml);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    var error = doc.GetElementsByTagName("ErrorDetail");
                    if (error.Count > 0)
                    {
                        for (int i = 0; i < error.Count; i++)
                        {

                            ErrorDetail errorDetail = new ErrorDetail()
                            {
                                ErrorCode = error[i].ChildNodes[0].InnerXml,
                                ErrorDescription = error[i].ChildNodes[1].InnerXml,
                            };
                        }
                        resp.Status = false;
                    }
                    else
                    {
                        var pID = doc.GetElementsByTagName("ProcessID");
                        if (pID.Count > 0)
                        {
                            var processId = pID[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentStatus(id, processId, "ส่งหนังสือแจ้งเลขรับผิด");
                        }


                    }

                }
                resp.ResponseObject = null;
            }
            catch (Exception ex)
            {
                resp.Description = ex.Message;
                resp.Status = false;
            }

            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);



        }

        //รับหนังสือตอบรับ/ลบ/แจ้งเลข
        [HttpPost]
        public JsonResult RequestReceiveNotifierDocument(string to, string messageID)
        {
            Response resp = new Response();

            try
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = messageID,
                    To = to
                };
                var xml = XMLCreation.RequestReceiveDocumentLetter(source);
                var response = postXMLData(to, xml);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);

                var error = doc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };
                    }
                    resp.Status = false;
                    resp.Description = "ไม่สามารถดึงข้อมูลจากระบบ E-CMS ได้";

                }
                else
                {
                    var pID = doc.GetElementsByTagName("ProcessID");
                    var lId = doc.GetElementsByTagName("LetterID");
                    var cDate = doc.GetElementsByTagName("CorrespondenceDate");
                    var sj = doc.GetElementsByTagName("Subject");
                    var aId = doc.GetElementsByTagName("AcceptID");
                    var rj = doc.GetElementsByTagName("RejectLetterNotifier");
                    var iv = doc.GetElementsByTagName("InvalidLetterNotifier");
                    var iva = doc.GetElementsByTagName("InvalidAcceptIdNotifier");

                    if (iva.Count > 0)
                    {
                        resp.Description = "พบแจ้งหนังสือแจ้งเลขรับผิด";
                        if (pID.Count > 0)
                        {
                            Document document = new Document()
                            {
                                ProcessId = pID[0].InnerXml.ToString(),
                                BookId = lId[0].InnerXml.ToString(),
                                No = lId[0].InnerXml.ToString(),
                                Date = cDate[0].InnerXml.ToString(),
                                Subject = sj[0].InnerXml.ToString()
                            };

                            //var acceptId = aId[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentInvalidAcceptId(document);

                            if (resp.Status && resp.ResponseObject != 0)
                            {
                                RequestDeleteQueue delSource = new RequestDeleteQueue()
                                {
                                    MessageID = resp.ResponseObject.ToString(),
                                    ProcessID = document.ProcessId,
                                    To = to,
                                };

                                var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                                var delResp = postXMLData(to, delXml);
                                var docId = (int)resp.ResponseObject;
                                Document docSource = (Document)(docService.GetdocumentWithAtt(docId).ResponseObject);
                                RequestDeleteGovernmentDocument delDocSource = new RequestDeleteGovernmentDocument()
                                {
                                    AcceptDepartment = docSource.Organization1.Code,
                                    SenderDepartment = docSource.Organization.Code,
                                    CorrespondenceData = docSource.Date,
                                    LetterID = docSource.No,
                                    MessageID = docSource.Id.ToString(),
                                    To = docSource.Organization.Url
                                };

                                var delDocXml = XMLCreation.RequestDeleteGovernmentDocument(delDocSource);
                                var delDocresponse = postXMLData(docSource.Organization.Url, delDocXml);
                            }
                        }
                    }
                    else if (aId.Count > 0)
                    {
                        resp.Description = "พบหนังสือแจ้งเลขรับ";
                        if (pID.Count > 0)
                        {
                            Document document = new Document()
                            {
                                ProcessId = pID[0].InnerXml.ToString(),
                                BookId = lId[0].InnerXml.ToString(),
                                No = lId[0].InnerXml.ToString(),
                                Date = cDate[0].InnerXml.ToString(),
                                Subject = sj[0].InnerXml.ToString()
                            };

                            var acceptId = aId[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentAcceptId(document, acceptId);

                            if (resp.Status && resp.ResponseObject != 0)
                            {
                                RequestDeleteQueue delSource = new RequestDeleteQueue()
                                {
                                    MessageID = resp.RequestObject.ToString(),
                                    ProcessID = document.ProcessId,
                                    To = to,
                                };

                                var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                                var delResp = postXMLData(to, delXml);
                            }

                        }
                    }
                    else if (rj.Count > 0)
                    {
                        resp.Description = "พบหนังสือปฏิเสธ";
                        if (pID.Count > 0)
                        {
                            Document document = new Document()
                            {
                                ProcessId = pID[0].InnerXml.ToString(),
                                BookId = lId[0].InnerXml.ToString(),
                                No = lId[0].InnerXml.ToString(),
                                Date = cDate[0].InnerXml.ToString(),
                                Subject = sj[0].InnerXml.ToString()
                            };

                            //var acceptId = aId[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentReject(document);

                            if (resp.Status && resp.ResponseObject != 0)
                            {
                                RequestDeleteQueue delSource = new RequestDeleteQueue()
                                {
                                    MessageID = resp.ResponseObject.ToString(),
                                    ProcessID = document.ProcessId,
                                    To = to,
                                };

                                var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                                var delResp = postXMLData(to, delXml);
                                var docId = (int)resp.ResponseObject;
                                Document docSource = (Document)(docService.GetdocumentWithAtt(docId).ResponseObject);
                                RequestDeleteGovernmentDocument delDocSource = new RequestDeleteGovernmentDocument()
                                {
                                    AcceptDepartment = docSource.Organization1.Code,
                                    SenderDepartment = docSource.Organization.Code,
                                    CorrespondenceData = docSource.Date,
                                    LetterID = docSource.No,
                                    MessageID = docSource.Id.ToString(),
                                    To = docSource.Organization.Url
                                };

                                var delDocXml = XMLCreation.RequestDeleteGovernmentDocument(delDocSource);
                                var delDocresponse = postXMLData(docSource.Organization.Url, delDocXml);
                            }

                        }
                    }
                    else if (iv.Count > 0)
                    {
                        resp.Description = "พบแจ้งหนังสือผิด";
                        if (pID.Count > 0)
                        {
                            Document document = new Document()
                            {
                                ProcessId = pID[0].InnerXml.ToString(),
                                BookId = lId[0].InnerXml.ToString(),
                                No = lId[0].InnerXml.ToString(),
                                Date = cDate[0].InnerXml.ToString(),
                                Subject = sj[0].InnerXml.ToString()
                            };

                            //var acceptId = aId[0].InnerXml.ToString();
                            resp = docService.UpdateDocumentInvalid(document);

                            if (resp.Status && resp.ResponseObject != 0)
                            {
                                RequestDeleteQueue delSource = new RequestDeleteQueue()
                                {
                                    MessageID = resp.ResponseObject.ToString(),
                                    ProcessID = document.ProcessId,
                                    To = to,
                                };

                                var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                                var delResp = postXMLData(to, delXml);
                                var docId = (int)resp.ResponseObject;
                                Document docSource = (Document)(docService.GetdocumentWithAtt(docId).ResponseObject);
                                RequestDeleteGovernmentDocument delDocSource = new RequestDeleteGovernmentDocument()
                                {
                                    AcceptDepartment = docSource.Organization1.Code,
                                    SenderDepartment = docSource.Organization.Code,
                                    CorrespondenceData = docSource.Date,
                                    LetterID = docSource.No,
                                    MessageID = docSource.Id.ToString(),
                                    To = docSource.Organization.Url
                                };

                                var delDocXml = XMLCreation.RequestDeleteGovernmentDocument(delDocSource);
                                var delDocresponse = postXMLData(docSource.Organization.Url, delDocXml);
                            }

                        }
                    }
                    else
                    {
                        resp.Description = "พบหนังสือตอบกลับ";
                        if (pID.Count > 0)
                        {
                            Document document = new Document()
                            {
                                ProcessId = pID[0].InnerXml.ToString(),
                                BookId = lId[0].InnerXml.ToString(),
                                Date = cDate[0].InnerXml.ToString(),
                                Subject = sj[0].InnerXml.ToString(),
                                No = lId[0].InnerXml.ToString(),
                            };
                            resp = docService.UpdateDocumentReceiveNotifier(document);

                            if (resp.Status && resp.ResponseObject != 0)
                            {
                                RequestDeleteQueue delSource = new RequestDeleteQueue()
                                {
                                    MessageID = resp.RequestObject.ToString(),
                                    ProcessID = document.ProcessId,
                                    To = to,
                                };

                                var delXml = XMLCreation.RequestDeleteDocumentQueue(delSource);
                                var delResp = postXMLData(to, delXml);
                            }

                        }
                    }

                    resp.Status = true;
                    //resp.Description = "ไม่พบหนังสือ";

                }
                resp.ResponseObject = null;
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ลบหนังสือออกจากคิว
        [HttpPost]
        public JsonResult RequestDeleteDocumentQueue(int id)
        {
            var resp = new Response();
            try
            {
                resp = docService.GetDocumentInWithAtt(id);
                if (resp.Status)
                {
                    DocumentIn document = resp.ResponseObject;
                    RequestDeleteQueue source = new RequestDeleteQueue()
                    {
                        MessageID = document.Id.ToString(),
                        ProcessID = document.ProcessId,
                        To = document.Organization1.Url,
                    };

                    var xml = XMLCreation.RequestDeleteDocumentQueue(source);
                    var response = postXMLData(document.Organization1.Url, xml);

                }
                resp.ResponseObject = null;
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //ดาวโหลดไฟล์แนบ
        [HttpPost]
        public JsonResult DownloadOtherFile(int id)
        {

            var resp = docService.GetOtherAttachmentById(id);


            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);

        }

        //ดาวโหลดไฟล์หลัก
        [HttpPost]
        public JsonResult DownloadMainFile(int id)
        {

            var resp = docService.GetMainAttachmentById(id);
            return Json(resp, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);

        }

        public string ConvertExtensionType(string ext)
        {
            string returnType = "";
            switch (ext)
            {
                case "application/msword":
                    returnType = ".doc";
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    returnType = ".docx";
                    break;
                case "application/pdf":
                    returnType = ".pdf";
                    break;
                case "application/rtf":
                    returnType = ".rtf";
                    break;
                case "application/vnd.ms-excel":
                    returnType = ".xls";
                    break;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    returnType = ".xlsx";
                    break;
                case "application/zip":
                    returnType = ".zip";
                    break;
                case "image/bmp":
                    returnType = ".bmp";
                    break;
                case "image/jpeg":
                    returnType = ".jpg";
                    break;
                case "image/png":
                    returnType = ".png";
                    break;
                case "image/tiff":
                    returnType = ".tiff";
                    break;
                case "text/plain":
                    returnType = ".txt";
                    break;
                case "application/vnd.ms-powerpoint":
                    returnType = ".ppt";
                    break;
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    returnType = ".pptx";
                    break;
            }

            return returnType;

        }

        public string ConvertContentType(string ext)
        {
            ext = ext.ToLower();
            string returnType = "";
            switch (ext)
            {
                case ".doc":
                    returnType = "application/msword";
                    break;
                case ".docx":
                    returnType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".pdf":
                    returnType = "application/pdf";
                    break;
                case ".rtf":
                    returnType = "application/rtf";
                    break;
                case ".xls":
                    returnType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    returnType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".zip":
                    returnType = "application/zip";
                    break;
                case ".bmp":
                    returnType = "image/bmp";
                    break;
                case ".png":
                    returnType = "image/png";
                    break;
                case ".jpg":
                    returnType = "image/jpeg";
                    break;
                case ".tiff":
                    returnType = "image/tiff";
                    break;
                case ".txt":
                    returnType = "text/plain";
                    break;
                case ".ppt":
                    returnType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    returnType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
            }

            return returnType;

        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }




        #region ============== Sevice ==============
        public JsonResult RequestMinistryList(int organizationId)
        {
            var resp = docService.GetOrganizationById(organizationId);
            if (resp.Status)
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = "Test",
                    To = resp.ResponseObject.Url
                };
                var xml = XMLCreation.RequestGetMinistry(source);
                var response = postXMLData(source.To, xml);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    List<ErrorDetail> listError = new List<ErrorDetail>();
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };

                        listError.Add(errorDetail);

                    }
                    return Json(listError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var id = xmlDoc.GetElementsByTagName("MinistryID");
                    var thName = xmlDoc.GetElementsByTagName("Th-Name");

                    List<MinistryApi> list = new List<MinistryApi>();
                    if (id.Count > 0)
                    {

                        for (int i = 0; i < id.Count; i++)
                        {
                            MinistryApi organization = new MinistryApi()
                            {

                                ThName = thName[i].InnerXml,
                                Id = id[i].InnerXml,

                            };

                            list.Add(organization);
                        }

                    }
                    return Json(list, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(resp, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult RequestOrganizationList(int organizationId)
        {

            var resp = docService.GetOrganizationById(organizationId);
            if (resp.Status)
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = "Test",
                    To = resp.ResponseObject.Url
                };
                var xml = XMLCreation.RequestGetOrganizationList(source);
                var response = postXMLData(source.To, xml);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    List<ErrorDetail> listError = new List<ErrorDetail>();
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };

                        listError.Add(errorDetail);

                    }
                    return Json(listError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var enName = xmlDoc.GetElementsByTagName("En-Name");
                    var thName = xmlDoc.GetElementsByTagName("Th-Name");
                    var url = xmlDoc.GetElementsByTagName("ECMS-URL");
                    var id = xmlDoc.GetElementsByTagName("OrganizationID");
                    List<OrganizationApi> list = new List<OrganizationApi>();
                    if (enName.Count > 0)
                    {

                        for (int i = 0; i < enName.Count; i++)
                        {
                            OrganizationApi organization = new OrganizationApi()
                            {
                                EnName = enName[i].InnerXml,
                                ThName = thName[i].InnerXml,
                                Id = id[i].InnerXml,
                                Url = url[i].InnerXml
                            };

                            list.Add(organization);
                        }

                    }
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(resp, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult RequestFileList(int organizationId)
        {
            var resp = docService.GetOrganizationById(organizationId);
            if (resp.Status)
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = "Test",
                    To = "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2"
                };
                var xml = XMLCreation.RequestGetMimeCodes(source);
                var response = postXMLData(source.To, xml);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    List<ErrorDetail> listError = new List<ErrorDetail>();
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };

                        listError.Add(errorDetail);

                    }
                    return Json(listError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var ext = xmlDoc.GetElementsByTagName("File-Extension");
                    var content = xmlDoc.GetElementsByTagName("Content-type");

                    List<FileApi> list = new List<FileApi>();
                    if (ext.Count > 0)
                    {

                        for (int i = 0; i < ext.Count; i++)
                        {
                            FileApi organization = new FileApi()
                            {

                                FileExtension = ext[i].InnerXml,
                                ContentType = content[i].InnerXml
                            };

                            list.Add(organization);
                        }

                    }
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(resp, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult RequestSecretList(int organizationId)
        {
            var resp = docService.GetOrganizationById(organizationId);
            if (resp.Status)
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = "Test",
                    To = resp.ResponseObject.Url
                };
                var xml = XMLCreation.RequestGetSecretCodes(source);
                var response = postXMLData(source.To, xml);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    List<ErrorDetail> listError = new List<ErrorDetail>();
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };

                        listError.Add(errorDetail);

                    }
                    return Json(listError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var value = xmlDoc.GetElementsByTagName("Value");
                    var description = xmlDoc.GetElementsByTagName("Description");

                    List<SecretApi> list = new List<SecretApi>();
                    if (value.Count > 0)
                    {

                        for (int i = 0; i < value.Count; i++)
                        {
                            SecretApi organization = new SecretApi()
                            {

                                value = value[i].InnerXml,
                                description = description[i].InnerXml
                            };

                            list.Add(organization);
                        }

                    }
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
        
        }

        public JsonResult RequestSpeedList(int organizationId)
        {
            var resp = docService.GetOrganizationById(organizationId);
            if (resp.Status)
            {
                RequestReceive source = new RequestReceive()
                {
                    MessageID = "Test",
                    To = resp.ResponseObject.Url
                };
                var xml = XMLCreation.RequestGetSpeedCodes(source);
                var response = postXMLData(source.To, xml);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                var error = xmlDoc.GetElementsByTagName("ErrorDetail");

                if (error.Count > 0)
                {
                    List<ErrorDetail> listError = new List<ErrorDetail>();
                    for (int i = 0; i < error.Count; i++)
                    {

                        ErrorDetail errorDetail = new ErrorDetail()
                        {
                            ErrorCode = error[i].ChildNodes[0].InnerXml,
                            ErrorDescription = error[i].ChildNodes[1].InnerXml,
                        };

                        listError.Add(errorDetail);

                    }
                    return Json(listError, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var value = xmlDoc.GetElementsByTagName("Value");
                    var description = xmlDoc.GetElementsByTagName("Description");

                    List<SpeedApi> list = new List<SpeedApi>();
                    if (value.Count > 0)
                    {

                        for (int i = 0; i < value.Count; i++)
                        {
                            SpeedApi organization = new SpeedApi()
                            {

                                value = value[i].InnerXml,
                                description = description[i].InnerXml
                            };

                            list.Add(organization);
                        }

                    }
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
          
        }

        public ActionResult RequestTimeCheck(int organizationId)
        {
            RequestReceive source = new RequestReceive()
            {
                MessageID = "Test",
                To = "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2"
            };
            var xml = XMLCreation.RequestTimeCheck(source);
            var response = postXMLData(source.To, xml);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);
            var error = xmlDoc.GetElementsByTagName("ErrorDetail");

            if (error.Count > 0)
            {
                List<ErrorDetail> listError = new List<ErrorDetail>();
                for (int i = 0; i < error.Count; i++)
                {

                    ErrorDetail errorDetail = new ErrorDetail()
                    {
                        ErrorCode = error[i].ChildNodes[0].InnerXml,
                        ErrorDescription = error[i].ChildNodes[1].InnerXml,
                    };

                    listError.Add(errorDetail);

                }
                return Json(listError, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var value = xmlDoc.GetElementsByTagName("ECMSTime");
                if (value.Count > 0)
                {
                    return Content(value[0].InnerXml, "application/xml");
                }
            }
            return Content(response, "application/xml");
        }
        #endregion ============== Sevice ==============



    }

    public class OrganizationApi
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string EnName { get; set; }
        public string ThName { get; set; }

    }

    public class MinistryApi
    {
        public string Id { get; set; }
        public string ThName { get; set; }

    }

    public class SpeedApi
    {
        public string value { get; set; }
        public string description { get; set; }

    }

    public class SecretApi
    {
        public string value { get; set; }
        public string description { get; set; }
    }

    public class FileApi
    {
        public string FileExtension { get; set; }
        public string ContentType { get; set; }


    }
}