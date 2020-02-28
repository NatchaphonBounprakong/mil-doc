using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models.Response
{
    public class ResponseAcceptLetterNotifier
    {
        public string ProcessID { get; set; }
        public string ProcessTime { get; set; }

        public AcceptLetterNotifier AcceptLetterNotifier { get; set; }
    }

    public class AcceptLetterNotifier
    {
        public string LetterID { get; set; }
        public string AcceptID { get; set; }
        public string CorrespondenceDate { get; set; }
        public string Subject { get; set; }
        public string AcceptDate { get; set; }

        public AcceptDepartment AcceptDepartment { get; set; }
    }
}