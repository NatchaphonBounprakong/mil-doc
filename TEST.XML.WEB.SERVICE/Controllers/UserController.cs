using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.API.DGA.MIL.DOC.Services;

namespace WEB.API.DGA.MIL.DOC.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        UserServices userService = new UserServices();
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public JsonResult Login(string username,string password)
        {
            var resp = userService.Login(username, password);
            return Json(resp,JsonRequestBehavior.AllowGet);
            
        }

        
    }
}