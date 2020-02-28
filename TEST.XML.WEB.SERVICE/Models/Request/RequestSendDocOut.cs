using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.API.DGA.MIL.DOC.Models
{
    public class RequestSendDocOut
    {
        public string MessageID { get; set; }
        public string To { get; set; }
        public string ID { get; set; }
        public string CorrespondenceDate { get; set; }
        public string Subject { get; set; }
        public string SecretCode { get; set; }
        public string SpeedCode { get; set; }

        public string SenderGivenName { get; set; }
        public string SenderFamilyName { get; set; }
        public string SenderJobTitle { get; set; }
        public string SenderDeptID { get; set; }
        public string SenderMinistryID { get; set; }

        public string ReceiverGivenName { get; set; }
        public string ReceiverFamilyName { get; set; }
        public string ReceiverJobTitle { get; set; }
        public string ReceiverMinistryID { get; set; }
        public string ReceiverDeptID { get; set; }
        //public string ReferenceID { get; set; }
        //public string ReferrenceCorrespondenceDate { get; set; }
        //public string ReferenceSubject { get; set; }
        public string Attachment { get; set; }
        public string SendDate { get; set; }
        public string Description { get; set; }
        public string MainLetterBinaryObjectMimeCode { get; set; }
        public string MainLetterBinaryObject { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<Reference> References { get; set; }
        public string GovernentSignature { get; set; }

    }
    public class Attachment
    {
        public string AttachmentBinaryObject { get; set; }
        public string AttachmentBinaryObjectMimeCode { get; set; }
    }

    public class Reference
    {
        public string ID { get; set; }
        public string CorrespondenceDate { get; set; }
        public string Subject { get; set; }
        
    }



}
