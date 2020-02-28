using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models
{
    public class RequestAcceptLetterNotifier
    {
        public string MessageID { get; set; }
        public string To { get; set; }
        public string LetterID { get; set; }
        public string AcceptID { get; set; }
        public string CorrespondenceDate { get; set; }
        public string Subject { get; set; }
    }
}