using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC
{
    public class Response
    {           
        public string Message { get; set; }            
        public dynamic ResultData { get; set; }
        public bool Status { get; set; }
        public string Exception { get; set; }

    }
}