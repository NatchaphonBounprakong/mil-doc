using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WEB.API.DGA.MIL.DOC.DataAccesLayer;
using WEB.API.DGA.MIL.DOC.Models;

namespace WEB.API.DGA.MIL.DOC.Services
{
    public class DocumentServices
    {
        public Response resp = new Response();
        public Response GetSpeedIDByCode(string code)
        {
            var speedID = 0;
            try
            {

            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;

        }

        public Response GetSecretIDbyCode(string code)
        {
            var secretID = 0;
            try
            {

            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;

        }

        public Response Getdocument(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document
                        .Include("DocumentAttachment")
                        .Include("DocumentReference")
                        .Include("Organization")
                        .Include("Organization1").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        doc.MainAttachmentBinary = null;
                        foreach (var attachment in doc.DocumentAttachment)
                        {
                            attachment.Document = null;
                            attachment.AttachmentBinary = null;
                        }
                        foreach (var reference in doc.DocumentReference)
                        {
                            reference.Document = null;
                        }
                        //doc.Organization1 = ctx.Organization.Where(o => o.Id == doc.ReceiverOrganizationId).FirstOrDefault();
                        doc.Organization.DocumentIn = null;
                        doc.Organization.Document1 = null;
                        doc.Organization.DocumentIn1 = null;
                        doc.Organization.Document = null;
                        doc.Organization1.DocumentIn = null;
                        doc.Organization1.Document1 = null;
                        doc.Organization1.DocumentIn1 = null;
                        doc.Organization1.Document = null;
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetDocumentIn(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn
                        .Include("DocumentAttachment")
                        .Include("DocumentReference")
                        .Include("Organization")
                        .Include("Organization1").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        doc.MainAttachmentBinary = null;
                        foreach (var attachment in doc.DocumentAttachment)
                        {
                            attachment.Document = null;
                            attachment.DocumentIn = null;
                            attachment.AttachmentBinary = null;
                        }
                        foreach (var reference in doc.DocumentReference)
                        {
                            reference.Document = null;
                            reference.DocumentIn = null;
                        }
                        //doc.Organization = ctx.Organization.Where(o => o.Id == doc.SenderOrganizationId).FirstOrDefault();
                        //doc.Organization1 = ctx.Organization.Where(o => o.Id == doc.ReceiverOrganizationId).FirstOrDefault();

                        doc.Organization.DocumentIn = null;
                        doc.Organization.Document1 = null;
                        doc.Organization.DocumentIn1 = null;
                        doc.Organization.Document = null;
                        doc.Organization1.DocumentIn = null;
                        doc.Organization1.Document1 = null;
                        doc.Organization1.DocumentIn1 = null;
                        doc.Organization1.Document = null;

                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetdocumentWithAtt(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document
                        .Include("DocumentAttachment")
                        .Include("DocumentReference")
                        .Include("Organization")
                        .Include("Organization1").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {

                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentStatus(int id, string processId, string status)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        doc.ProcessId = processId;
                        doc.Status = status;

                        DocumentProcess docProcess = new DocumentProcess()
                        {
                            CreatedDate = DateTime.Now,
                            DocumentId = doc.Id,
                            ProcessId = processId,
                            Status = status
                        };

                        ctx.DocumentProcess.Add(docProcess);

                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentReceiveNotifier(Document doc,string acceptDept)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.Document.Where(o => o.No == doc.No
                    && o.Date == doc.Date
                    && o.Subject == doc.Subject
                    && o.Organization1.Code == acceptDept).FirstOrDefault();

                    if (obj != null)
                    {
                        if (obj.Status == "ปลายทางได้รับหนังสือ")
                        {
                            resp.Status = true;
                            resp.RequestObject = 0;
                        }
                        else
                        {
                            DocumentProcess docProcess = new DocumentProcess()
                            {
                                CreatedDate = DateTime.Now,
                                DocumentId = obj.Id,
                                ProcessId = doc.ProcessId,
                                Status = "ปลายทางได้รับหนังสือ"
                            };

                            ctx.DocumentProcess.Add(docProcess);
                          
                            obj.Status = "ปลายทางได้รับหนังสือ";
                            obj.ProcessId = doc.ProcessId;

                            ctx.SaveChanges();

                            resp.RequestObject = obj.Id;
                            resp.Status = true;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentAcceptId(Document doc, string acceptId,string acceptDept)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.Document.Where(o => o.No == doc.No
                    && o.Date == doc.Date
                    && o.Subject == doc.Subject
                    && o.Organization1.Code == acceptDept).FirstOrDefault();

                    if (obj != null)
                    {
                        if (obj.Status == "ปลายทางส่งเลขรับเพื่ออ้างอิง")
                        {
                            resp.Status = true;
                            resp.RequestObject = 0;
                        }
                        else
                        {
                            DocumentProcess docProcess = new DocumentProcess()
                            {
                                CreatedDate = DateTime.Now,
                                DocumentId = obj.Id,
                                ProcessId = doc.ProcessId,
                                Status = "ปลายทางส่งเลขรับเพื่ออ้างอิง"
                            };

                            ctx.DocumentProcess.Add(docProcess);

                            obj.AcceptId = acceptId;
                            obj.Status = "ปลายทางส่งเลขรับเพื่ออ้างอิง";
                            obj.ProcessId = doc.ProcessId;

                            ctx.SaveChanges();

                            resp.RequestObject = obj.Id;
                            resp.Status = true;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentReject(Document doc,string acceptDept)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.Document.Where(o => o.No == doc.No
                    && o.Date == doc.Date
                    && o.Subject == doc.Subject
                    && o.Organization1.Code == acceptDept).FirstOrDefault();

                    if (obj != null)
                    {
                        if (obj.Status == "ปฏิเสธการรับหนังสือ")
                        {
                            resp.Status = true;
                            resp.ResponseObject = 0;
                           
                           
                        }
                        else
                        {

                            DocumentProcess docProcess = new DocumentProcess()
                            {
                                CreatedDate = DateTime.Now,
                                DocumentId = obj.Id,
                                ProcessId = doc.ProcessId,
                                Status = "ปฏิเสธการรับหนังสือ"
                            };

                            ctx.DocumentProcess.Add(docProcess);
                            
                            obj.Status = "ปฏิเสธการรับหนังสือ";
                            obj.ProcessId = doc.ProcessId;
                            ctx.SaveChanges();
                            resp.ResponseObject = obj.Id;
                            resp.Status = true;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentInvalid(Document doc,string acceptDept)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.Document.Where(o => o.No == doc.No
                    && o.Date == doc.Date
                    && o.Subject == doc.Subject
                    && o.Organization1.Code == acceptDept).FirstOrDefault();

                    if (obj != null)
                    {
                        if (obj.Status == "แจ้งหนังสือผิด")
                        {
                            resp.Status = true;
                            resp.ResponseObject = 0;
                        }
                        else
                        {

                            DocumentProcess docProcess = new DocumentProcess()
                            {
                                CreatedDate = DateTime.Now,
                                DocumentId = obj.Id,
                                ProcessId = doc.ProcessId,
                                Status = "แจ้งหนังสือผิด"
                            };

                            ctx.DocumentProcess.Add(docProcess);

                            obj.Status = "แจ้งหนังสือผิด";
                            obj.ProcessId = doc.ProcessId;
                            ctx.SaveChanges();
                            resp.ResponseObject = obj.Id;
                            resp.Status = true;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentInvalidAcceptId(Document doc,string acceptId)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.DocumentIn.Where(o => o.No == doc.No
                    && o.Date == doc.Date
                    && o.Subject == doc.Subject
                    && o.Organization1.Code == acceptId).OrderByDescending(o => o.Id).FirstOrDefault();

                    if (obj != null)
                    {
                        if (obj.Status == "แจ้งเลขรับผิด")
                        {
                            resp.Status = true;
                            resp.ResponseObject = 0;
                        }
                        else
                        {
                            DocumentProcess docProcess = new DocumentProcess()
                            {
                                CreatedDate = DateTime.Now,
                                DocumentInId = obj.Id,
                                ProcessId = doc.ProcessId,
                                Status = "รับหนังสือแจ้งเลขรับผิด"
                            };

                            ctx.DocumentProcess.Add(docProcess);

                            obj.Status = "แจ้งเลขรับผิด";
                            obj.ProcessId = doc.ProcessId;
                            ctx.SaveChanges();
                            resp.ResponseObject = obj.Id;
                            resp.Status = true;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentInAcceptId(int id, string acceptId)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var obj = ctx.DocumentIn.Where(o => o.Id == id).FirstOrDefault();

                    if (obj != null)
                    {


                        obj.AcceptId = acceptId;

                        ctx.SaveChanges();

                        resp.RequestObject = obj.Id;
                        resp.Status = true;



                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response UpdateDocumentInStatus(int id, string processId, string status)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn.Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        doc.ProcessId = processId;
                        doc.Status = status;

                        DocumentProcess docProcess = new DocumentProcess()
                        {
                            CreatedDate = DateTime.Now,
                            DocumentInId = id,
                            ProcessId = processId,
                            Status = status
                        };

                        ctx.DocumentProcess.Add(docProcess);

                        ctx.SaveChanges();

                        resp.Status = true;
                    }


                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }
              
        public Response GetDocumentList()
        {
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Select(x => new
                    {
                        Id = x.Id,
                        No = x.No,
                        Date = x.Date,
                        Subject = x.Subject,
                        Receive = x.Organization1.Name,
                        Status = x.Status,
                        x.AcceptId,

                    }).OrderByDescending(o => o.Id).ToList();
                    if (doc != null)
                    {
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetDocumentListByOrganizeId(int organizationId)
        {
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Where(o => o.SenderOrganizationId == organizationId).Select(x => new
                    {
                        Id = x.Id,
                        No = x.No,
                        Date = x.Date,
                        Subject = x.Subject,
                        Receive = x.Organization1.Name,
                        Status = x.Status,
                        x.AcceptId,

                    }).OrderByDescending(o => o.Id).ToList();
                    if (doc != null)
                    {
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetDocumentInList()
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn.Select(x => new
                    {
                        Id = x.Id,
                        x.BookId,
                        Date = x.Date,
                        Subject = x.Subject,
                        Status = x.Status,
                        Receive = x.Organization.Name,//StaticOrganization.organizations[x.ReceiverOrganizationId].Name,
                        x.AcceptId
                    }).OrderByDescending(o => o.Id).ToList();
                    if (doc != null)
                    {
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetDocumentInListByOrganizeId(int organizationId)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn.Where(o => o.ReceiverOrganizationId == organizationId).Select(x => new
                    {
                        Id = x.Id,
                        x.BookId,
                        Date = x.Date,
                        Subject = x.Subject,
                        Status = x.Status,
                        Receive = x.Organization1.Name,
                        x.AcceptId
                    }).OrderByDescending(o => o.Id).ToList();
                    
                    if (doc != null)
                    {
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetOrganizationList()
        {
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var organizations = ctx.Organization.OrderByDescending(o => o.Name).ToList();
                    if (organizations != null)
                    {
                        resp.ResponseObject = organizations;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response EditDocument(Document doc)
        {

            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var document = ctx.Document.Include("DocumentReference").Where(o => o.Id == doc.Id).FirstOrDefault();
                    if (document != null)
                    {
                        document.No = doc.No;
                        document.From = doc.From;
                        document.Type = doc.Type;
                        document.Date = doc.Date;
                        document.Subject = doc.Subject;
                        document.Speed = doc.Speed;
                        document.Secret = doc.Secret;
                        document.Description = doc.Description;
                        document.MainAttachmentName = doc.MainAttachmentName;
                        document.MimeCode = doc.MimeCode;

                        document.SenderPosition = doc.SenderPosition;
                        document.SenderName = doc.SenderName;
                        document.SenderSurname = doc.SenderSurname;

                        document.ReceiverPosition = doc.ReceiverPosition;
                        document.ReceiverName = doc.ReceiverName;
                        document.ReceiverSurname = doc.ReceiverSurname;

                        document.ReceiverOrganizationId = doc.ReceiverOrganizationId;

                    }

                    if (doc.DocumentReference != null)
                    {
                        var newRef = doc.DocumentReference.Where(o => o.State == "เพิ่ม").ToList();
                        foreach (var refer in newRef)
                        {
                            refer.State = "บันทึก";
                            document.DocumentReference.Add(refer);
                        }

                        var deleteRef = doc.DocumentReference.Where(o => o.State == "รอลบ").ToList();
                        foreach (var refer in deleteRef)
                        {

                            var obj = ctx.DocumentReference.Where(o => o.Id == refer.Id).FirstOrDefault();
                            if (obj != null)
                            {
                                ctx.DocumentReference.Remove(obj);
                            }

                        }

                    }


                    var attList = ctx.DocumentAttachment.Where(o => o.DocumentId == document.Id).ToList();
                    foreach (var att in attList)
                    {
                        if (att.State == "รอลบ")
                        {
                            ctx.DocumentAttachment.Remove(att);
                        }
                    }




                    ctx.SaveChanges();
                    resp.Status = true;
                    document.DocumentReference = null;
                    document.DocumentAttachment = null;
                    resp.ResponseObject = document;
                    document.MainAttachmentBinary = null;



                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response EditDocumentAttachment(int id, List<string> fileName, List<byte[]> files)
        {

            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {


                    foreach (var file in fileName.Where(o => !o.Contains("mainFile")))
                    {
                        var i = 0;
                        DocumentAttachment att = new DocumentAttachment()
                        {
                            DocumentId = id,
                            AttachmentName = file,
                            AttachmentBinary = files[i],
                            MimeCode = ConvertContentType(System.IO.Path.GetExtension(file)),
                            State = "บันทึก",
                            Type = "1",
                            FileSize = ConvertBytesToMegabytes(files[i].Length).ToString("N5") + " mb",
                    };
                        ctx.DocumentAttachment.Add(att);
                        i++;
                    }


                    ctx.SaveChanges();
                    resp.Status = true;

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response AddDocument(Document doc)
        {

            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    if (doc.DocumentReference != null)
                    {
                        foreach (var item in doc.DocumentReference)
                        {
                            item.State = "บันทึก";
                        }
                    };
                    if (ctx.Document.Any(o => o.No == doc.No && o.SenderOrganizationId == doc.SenderOrganizationId && o.ReceiverOrganizationId == doc.ReceiverOrganizationId))
                    {
                        resp.Status = false;
                        resp.Description = "เลขที่หนังสือซ้ำ";
                    }
                    else
                    {
                        doc.CreatedDate = DateTime.Now;
                        ctx.Document.Add(doc);
                        ctx.SaveChanges();
                        resp.Status = true;
                        doc.DocumentAttachment = null;
                        doc.DocumentReference = null;
                        resp.ResponseObject = doc;
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response AddDocumentIn(DocumentIn docIn)
        {

            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn.Where(o => o.ProcessId == docIn.ProcessId).FirstOrDefault();

                    if (doc!=null)
                    {
                        resp.ResponseObject = doc;
                    }
                    else
                    {
                        if (docIn.DocumentReference != null)
                        {
                            foreach (var item in docIn.DocumentReference)
                            {
                                item.State = "บันทึก";
                                item.Type = 2;
                            }
                        };
                        ctx.DocumentIn.Add(docIn);
                        
                        DocumentProcess docProcess = new DocumentProcess()
                        {
                            CreatedDate = DateTime.Now,
                            DocumentInId = docIn.Id,
                            ProcessId = docIn.ProcessId,
                            Status = "รับหนังสือรอส่งหนังสือตอบรับ"
                        };

                        ctx.DocumentProcess.Add(docProcess);
                        
                        ctx.SaveChanges();
                        resp.ResponseObject = docIn;
                    }

                    resp.Status = true;
                    //resp.Description = "1";

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response DeleteDocument(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        ctx.Document.Remove(doc);
                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response CancelSaveDocument(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        foreach (var attachment in doc.DocumentAttachment)
                        {
                            attachment.State = "บันทึก";

                        }

                        foreach (var reference in doc.DocumentReference)
                        {
                            reference.State = "บันทึก";

                        }

                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response SetDocumentAttachment(int id, List<byte[]> files)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Include("DocumentAttachment").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {

                        int i = 0;
                        foreach (var attach in doc.DocumentAttachment)
                        {
                            attach.AttachmentBinary = files[i];
                            attach.FileSize = ConvertBytesToMegabytes(files[i].Length).ToString("N5") + " mb";
                            attach.State = "บันทึก";
                            attach.Type = "1";
                            i++;
                        }

                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public Response DeleteDocumentAttachment(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var attachment = ctx.DocumentAttachment.Where(o => o.Id == id).FirstOrDefault();
                    if (attachment != null)
                    {
                        attachment.State = "รอลบ";
                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response DeleteReferenceBook(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var reference = ctx.DocumentReference.Where(o => o.Id == id).FirstOrDefault();
                    if (reference != null)
                    {
                        reference.State = "รอลบ";
                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response SetMainAttachment(int id, byte[] file)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.Document.Include("DocumentAttachment").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {

                        doc.MainAttachmentBinary = file;
                        doc.FileSize = ConvertBytesToMegabytes(file.Length).ToString("N5") + " mb";

                        ctx.SaveChanges();
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
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

        public Response AddListOrganization(List<Organization> organizaions)
        {
            Response resp = new Response();
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {

                    ctx.Organization.AddRange(organizaions);
                    ctx.SaveChanges();
                    resp.Status = true;

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetOrganizationByCode(string Code)
        {
            Response resp = new Response();
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {

                    var obj = ctx.Organization.Where(o => o.Code == Code).FirstOrDefault();
                    if (obj != null)
                    {
                        resp.ResponseObject = obj;
                    }

                    resp.Status = true;

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetOrganizationById(int id)
        {
            Response resp = new Response();
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {

                    var obj = ctx.Organization.Where(o => o.Id == id).FirstOrDefault();
                    if (obj != null)
                    {
                        resp.ResponseObject = obj;
                    }

                    resp.Status = true;

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetDocumentInWithAtt(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var doc = ctx.DocumentIn
                        .Include("DocumentAttachment")
                        .Include("DocumentReference")
                        .Include("Organization")
                        .Include("Organization1").Where(o => o.Id == id).FirstOrDefault();
                    if (doc != null)
                    {
                        //doc.Organization = ctx.Organization.Where(o => o.Id == doc.SenderOrganizationId).FirstOrDefault();
                        //doc.Organization1 = ctx.Organization.Where(o => o.Id == doc.ReceiverOrganizationId).FirstOrDefault();
                        resp.ResponseObject = doc;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetOtherAttachmentById(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var att = ctx.DocumentAttachment.Where(o => o.Id == id).FirstOrDefault();

                    if (att != null)
                    {
                        resp.ResponseObject = att;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }
        public Response GetMainAttachmentById(int id)
        {
            try
            {
                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {
                    var docIn = ctx.DocumentIn.Where(o => o.Id == id).FirstOrDefault();

                    if (docIn != null)
                    {

                        resp.ResponseObject = docIn;
                        resp.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public Response GetOrganization()
        {
            Response resp = new Response();
            try
            {

                using (DGAMilDocEntities ctx = new DGAMilDocEntities())
                {

                    var obj = ctx.Organization.ToList();
                    if (obj != null)
                    {
                        StaticOrganization.organizations = obj;
                    }

                    resp.Status = true;

                }
            }
            catch (Exception ex)
            {
                resp.Status = false;
                resp.Description = ex.Message;
            }

            return resp;


        }


    }
}