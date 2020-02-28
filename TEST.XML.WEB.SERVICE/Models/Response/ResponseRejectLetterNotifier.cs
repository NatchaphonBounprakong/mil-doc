using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models.Response
{
    public class ResponseRejectLetterNotifier
    {
        public string ProcessID { get; set; }
        public string ProcessTime { get; set; }

        public RejectLetterNotifier RejectLetterNotifier { get; set; }
    }

    public class RejectLetterNotifier
    {
        public string LetterID { get; set; }
        public string CorrespondenceData { get; set; }
        public string Subject { get; set; }
        public string AcceptDate { get; set; }

        public AcceptDepartment AcceptDepartment { get; set; }
    }
}