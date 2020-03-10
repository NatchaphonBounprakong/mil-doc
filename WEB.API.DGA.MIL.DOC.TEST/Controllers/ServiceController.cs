using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.API.DGA.MIL.DOC.TEST.Services;

namespace WEB.API.DGA.MIL.DOC.TEST.Controllers
{
    public class ServiceController : Controller
    {
        // GET: Service
        OrganizationServices orgService = null;
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetOrganizationById(int id)
        {
            orgService = new OrganizationServices();
            var response = orgService.GetOrganizationById(id);

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}