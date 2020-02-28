using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models
{
    public class RequestNewDocument
    {
        public string MessageID { get; set; }
        public string To { get; set; }
        public string LetterID { get; set; }
        public string CorrespondenceData { get; set; }
        public string SenderDepartment { get; set; }
        public string AcceptDepartment { get; set; }
    }
}