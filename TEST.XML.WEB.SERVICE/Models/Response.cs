using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC
{
    public class Response
    {
        public string Name { get; set; }
        public string API { get; set; }
        public string Type { get; set; }
        public string Page { get; set; }
        public dynamic Description { get; set; }
        public dynamic RequestFormats { get; set; }
        public dynamic RequestObject { get; set; }
        public dynamic ResponseFormats { get; set; }
        public dynamic ResponseObject { get; set; }
        public bool Status { get; set; }

    }
}