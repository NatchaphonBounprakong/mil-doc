using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models
{
    public class RequestResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
        public string ErrorMessage { get; set; }
        
    }
}