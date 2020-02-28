using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models.Response
{
    public class ResponseInvalidLetterNotifier
    {
        public string ProcessID { get; set; }
        public string ProcessTime { get; set; }
        public InvalidLetterNotifier InvalidLetterNotifier { get; set; }

    }

    public class InvalidLetterNotifier
    {
        public string LetterID { get; set; }
        public string CorrespondenceData { get; set; }
        public string Subject { get; set; }
        public string AcceptDate { get; set; }

        public AcceptDepartment AcceptDepartment { get; set; }
    }
}