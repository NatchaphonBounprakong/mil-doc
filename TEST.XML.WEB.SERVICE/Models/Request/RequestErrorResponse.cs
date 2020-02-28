using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.API.DGA.MIL.DOC.Models
{
    public class RequestErrorResponse
    {
        public List<ErrorDetail> ErrorDetail { get; set; }
    }

    public class ErrorDetail
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}