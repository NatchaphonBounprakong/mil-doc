using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC
{
    public class Response
    {           
        public string Description { get; set; }            
        public dynamic ResponseObject { get; set; }
        public bool Status { get; set; }
        public string Exception { get; set; }

    }
}