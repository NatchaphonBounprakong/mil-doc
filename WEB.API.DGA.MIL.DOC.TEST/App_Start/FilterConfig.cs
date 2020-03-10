using System.Web;
using System.Web.Mvc;

namespace WEB.API.DGA.MIL.DOC.TEST
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
