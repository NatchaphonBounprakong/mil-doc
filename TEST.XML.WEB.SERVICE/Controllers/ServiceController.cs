using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WEB.API.DGA.MIL.DOC.DataAccesLayer;
using WEB.API.DGA.MIL.DOC.Services;
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
            doc.BookId =doc.No;
            doc.MimeCode = ConvertContentType(System.IO.Path.GetExtension(doc.MainAttachmentName));

            foreach (var item in doc.DocumentAttachment)
            {
                item.MimeCode = ConvertContentType(System.IO.Path.GetExtension(item.AttachmentName));
            }

            var resp = docService.AddDocument(doc);

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