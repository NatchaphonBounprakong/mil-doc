using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WEB.API.DGA.MIL.DOC.Models;
using WEB.API.DGA.MIL.DOC.Services;

namespace WEB.API.DGA.MIL.DOC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            SetStaticData();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        public static void SetStaticData()
        {
            new DocumentServices().GetOrganizationList();
        }
    }
}
