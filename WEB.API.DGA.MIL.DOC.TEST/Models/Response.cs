using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.TEST.Models
{
    public class Response
    {
        public bool Status { get; set; }

        public string Message { get; set; }

        public dynamic ResultData { get; set; }

        public string ApiName { get; set; }
    }
}