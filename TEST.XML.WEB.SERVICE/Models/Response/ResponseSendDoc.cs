using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WEB.API.DGA.MIL.DOC.Models.Response
{
    public class ResponseSendDoc
    {
        public string ProcessID { get; set; }
        public string ProcessTime { get; set; }

        public RequestSendDocOut Document { get; set; }
    }
}