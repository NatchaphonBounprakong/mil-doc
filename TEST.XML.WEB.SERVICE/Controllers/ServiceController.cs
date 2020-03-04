using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using WEB.API.DGA.MIL.DOC.DataAccesLayer;
using WEB.API.DGA.MIL.DOC.Models;
using WEB.API.DGA.MIL.DOC.Services;
using WEB.API.DGA.MIL.DOC.Utility;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace WEB.API.DGA.MIL.DOC.Controllers
{
    public class ServiceController : Controller
    {
        // GET: Service

        DocumentServices docService = new DocumentServices();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDocumentList()
        {
            var resp = docService.GetDocumentList();

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDocumentListByOrganizeId(int organizationId)
        {
            var resp = docService.GetDocumentListByOrganizeId(organizationId);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetOrganizationList()
        {
            var resp = docService.GetOrganizationList();

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDocumentInList()
        {
            var resp = docService.GetDocumentInList();

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDocumentInListByOrganizeId(int organizationId)
        {
            var resp = docService.GetDocumentInListByOrganizeId(organizationId);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Getdocument(int id)
        {
            var resp = docService.Getdocument(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetdocumentIn(int id)
        {
            var resp = docService.GetDocumentIn(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddDocument([FromBody]Document doc, string from, string to)
        {

            doc.BookId = doc.No;
            doc.MimeCode = ConvertContentType(System.IO.Path.GetExtension(doc.MainAttachmentName));

            foreach (var item in doc.DocumentAttachment)
            {
                item.MimeCode = ConvertContentType(System.IO.Path.GetExtension(item.AttachmentName));
            }

            var resp = docService.AddDocument(doc);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddCircleDocument([FromBody]Document doc, string from, string to, List<int> ids)
        {

            var resp = new Response();
            foreach (var id in ids)
            {
                doc.BookId = doc.No;
                doc.MimeCode = ConvertContentType(System.IO.Path.GetExtension(doc.MainAttachmentName));

                foreach (var item in doc.DocumentAttachment)
                {
                    item.MimeCode = ConvertContentType(System.IO.Path.GetExtension(item.AttachmentName));
                }
                doc.ReceiverOrganizationId = id;
                resp = docService.AddCircleDocument(doc);
            }

            return Json(resp, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public JsonResult AddCircleDocument2()
        {
            HttpFileCollectionBase files = Request.Files;
            var doc = Request.Form["doc"].ToString();
            var ids = Request.Form["Ids"].ToString();
            Document docObj = JsonConvert.DeserializeObject<Document>(doc);
            var organizeIds = JsonConvert.DeserializeObject<List<int>>(ids);

            var resp = new Response();

            List<byte[]> fileBytes = new List<byte[]>();
          
            int i = 0;
            foreach (string fileName in Request.Files)
            {
                if (fileName.Contains("otherFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    docObj.DocumentAttachment.ToList()[i].AttachmentBinary = ConverToBytes(file);
                    docObj.DocumentAttachment.ToList()[i].MimeCode = ConvertContentType(Path.GetExtension(docObj.DocumentAttachment.ToList()[i].AttachmentName));
                    i++;
                }

                if (fileName.Contains("mainFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    var mainFile = ConverToBytes(file);
                    docObj.MainAttachmentBinary = mainFile;
                    docObj.MimeCode = ConvertContentType(Path.GetExtension(docObj.MainAttachmentName));
                }
            }

            foreach (var item in organizeIds)
            {
                docObj.ReceiverOrganizationId = item;
                resp = docService.AddCircleDocument(docObj);
                if (resp.Status)
                {
                    var postResponse = "";
                    //resp = new Response();
                    try
                    {

                        var docResponse = docService.GetdocumentWithAtt(resp.ResponseObject.Id);

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
                                SendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", new CultureInfo("en-US")),
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
                                for (int j = 0; j < error.Count; j++)
                                {
                                    resp.Status = false;

                                    ErrorDetail errorDetail = new ErrorDetail()
                                    {
                                        ErrorCode = error[j].ChildNodes[0].InnerXml,
                                        ErrorDescription = error[j].ChildNodes[1].InnerXml,
                                    };

                                    resp.Description = error[j].ChildNodes[1].InnerXml + Environment.NewLine;
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

                                resp = docService.UpdateDocumentStatus(resp.ResponseObject.Id, processID, "ส่งหนังสือรอตอบรับ");
                                resp.ResponseObject = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        resp.Status = false;
                        resp.Description = ex.Message;
                    }
                }
                else
                {
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditDocument([FromBody]Document doc, string from, string to)
        {
            doc.MimeCode = ConvertContentType(System.IO.Path.GetExtension(doc.MainAttachmentName));


            foreach (var item in doc.DocumentAttachment)
            {
                item.MimeCode = ConvertContentType(System.IO.Path.GetExtension(item.AttachmentName));
            }

            var resp = docService.EditDocument(doc);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditDocumentAttachment()
        {
            HttpFileCollectionBase files = Request.Files;

            List<byte[]> fileBytes = new List<byte[]>();
            List<string> name = new List<string>();
            var id = int.Parse(Request.Form["ID"].ToString());
            foreach (string fileName in Request.Files)
            {



                if (fileName.Contains("mainFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    var mainFile = ConverToBytes(file);
                    docService.SetMainAttachment(id, mainFile);
                }
                else
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    name.Add(fileName);
                    fileBytes.Add(ConverToBytes(file));
                }

            }


            var resp = docService.EditDocumentAttachment(id, name, fileBytes);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddDocumentAttachment()
        {
            HttpFileCollectionBase files = Request.Files;
            var id = int.Parse(Request.Form["ID"].ToString());
            var form = Request.Form;
            List<byte[]> fileBytes = new List<byte[]>();
            foreach (string fileName in Request.Files)
            {
                if (fileName.Contains("otherFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    fileBytes.Add(ConverToBytes(file));
                }

                if (fileName.Contains("mainFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    var mainFile = ConverToBytes(file);
                    docService.SetMainAttachment(id, mainFile);
                }
            }


            var resp = docService.SetDocumentAttachment(id, fileBytes);
            resp.ResponseObject = id;
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddDocumentAttachmentThenSend()
        {
            HttpFileCollectionBase files = Request.Files;
            var id = int.Parse(Request.Form["ID"].ToString());
            var form = Request.Form;
            List<byte[]> fileBytes = new List<byte[]>();
            foreach (string fileName in Request.Files)
            {
                if (fileName.Contains("otherFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    fileBytes.Add(ConverToBytes(file));
                }

                if (fileName.Contains("mainFile"))
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    var mainFile = ConverToBytes(file);
                    docService.SetMainAttachment(id, mainFile);
                }
            }

            APIController api = new APIController();

            var resp = docService.SetDocumentAttachment(id, fileBytes);
            if (resp.Status)
            {
                api.RequestSendDocument(id);
            }
            resp.ResponseObject = id;
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDocumentAttachment(int id)
        {
            var resp = docService.DeleteDocumentAttachment(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteReferenceBook(int id)
        {
            var resp = docService.DeleteReferenceBook(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CancelSaveDocument(int id)
        {
            var resp = docService.CancelSaveDocument(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDocument(int id)
        {
            var resp = docService.DeleteDocument(id);

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public static byte[] ConverToBytes(HttpPostedFileBase file)
        {
            var length = file.InputStream.Length; //Length: 103050706
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }
            return fileData;
        }

        public string ConvertContentType(string ext)
        {
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

    }
}