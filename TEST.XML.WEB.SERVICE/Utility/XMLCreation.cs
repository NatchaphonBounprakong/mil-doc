using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using WEB.API.DGA.MIL.DOC.Models;

namespace WEB.API.DGA.MIL.DOC.Utility
{
    public static class XMLCreation
    {
        //ส่งหนังสือ
        public static string RequestSendDocument(RequestSendDocOut source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";
            XNamespace ram = "urn:th:gov:egif:data:standard:ReusableAggregateBusinessInformationEntity:1";
            XNamespace rsm = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";
            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement(rsm + "GovernmentDocument", new XAttribute(XNamespace.Xmlns + "ram", ram), new XAttribute(XNamespace.Xmlns + "rsm", rsm),
                            new XElement(ram + "CorrespondenceLetter", new XAttribute("ID", "Letter"))
                            ))));
            XElement[] array = {
                new XElement(ram + "ID", source.ID ?? ""),
                new XElement(ram + "CorrespondenceDate", source.CorrespondenceDate.ToString()?? ""),
                new XElement(ram + "Subject", source.Subject?? ""),
                new XElement(ram + "SecretCode", source.SecretCode?? ""),
                new XElement(ram + "SpeedCode", source.SpeedCode?? ""),
                new XElement(ram + "SenderParty",
                        new XElement(ram + "GivenName", source.SenderGivenName ?? ""),
                        new XElement(ram + "FamilyName", source.SenderFamilyName ?? ""),
                        new XElement(ram + "JobTitle", source.SenderJobTitle ?? ""),
                        new XElement(ram + "MinistryOrganization",
                            new XElement(ram + "ID", source.SenderMinistryID ?? "")),
                        new XElement(ram + "DepartmentOrganization",
                            new XElement(ram + "ID", source.SenderDeptID ?? ""))),
                new XElement(ram + "ReceiverParty",
                        new XElement(ram + "GivenName", source.ReceiverGivenName ?? ""),
                        new XElement(ram + "FamilyName", source.ReceiverFamilyName ?? ""),
                        new XElement(ram + "JobTitle", source.ReceiverJobTitle ?? ""),
                        new XElement(ram + "MinistryOrganization",
                            new XElement(ram + "ID", source.ReceiverMinistryID ?? "")),
                        new XElement(ram + "DepartmentOrganization",
                            new XElement(ram + "ID", source.ReceiverDeptID ?? ""))),             
                new XElement(ram + "Attachment", source.Attachment?? ""),
                new XElement(ram + "SendDate", source.SendDate?? ""),
                new XElement(ram + "Description", source.Description?? ""),
                    new XElement(ram + "MainLetterBinaryObject",
                            new XAttribute("mimeCode", source.MainLetterBinaryObjectMimeCode ?? ""),
                            source.MainLetterBinaryObject)
            };
            soapElement.Element(soap + "Body")
                        .Element("CorrespondenceLetterOutboundRequest")
                        .Element(rsm + "GovernmentDocument")
                        .Element(ram + "CorrespondenceLetter")
                        .Add(array);
            
            foreach (var attach in source.Attachments)
            {
                XElement attachment = new XElement(ram + "AttachmentBinaryObject",
                            new XAttribute("mimeCode", attach.AttachmentBinaryObjectMimeCode ?? ""),
                            attach.AttachmentBinaryObject);
                soapElement.Element(soap + "Body")
              .Element("CorrespondenceLetterOutboundRequest")
              .Element(rsm + "GovernmentDocument")
              .Element(ram + "CorrespondenceLetter")
              .Element(ram + "MainLetterBinaryObject")
              .AddAfterSelf(attachment);
            }

            foreach (var reference in source.References)
            {
                XElement attachment = new XElement(ram + "ReferenceCorrespondence",
                        new XElement(ram + "ID", reference.ID ?? ""),
                        new XElement(ram + "CorrespondenceDate", reference.CorrespondenceDate ?? ""),
                        new XElement(ram + "Subject", reference.Subject ?? ""));


                soapElement.Element(soap + "Body")
              .Element("CorrespondenceLetterOutboundRequest")
              .Element(rsm + "GovernmentDocument")
              .Element(ram + "CorrespondenceLetter")
              .Element(ram + "ReceiverParty")
              .AddAfterSelf(attachment);
            }

            return Util.ConvertXmlFormat(soapElement);
        }
        //ส่งหนังสือตอบรับ
        public static string RequestReceiveLetterNotifier(RequestReceiveLetterNotifier source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("ReceiveLetterNotifier",
                            new XElement("LetterID", source.LetterID ?? ""),
                            new XElement("CorrespondenceDate", source.CorrespondenceDate ?? ""),
                            new XElement("Subject", source.Subject ?? "")
                            ))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //ส่งหนังสือแจ้งเลขรับ
        public static string RequestAcceptLetterNotifier(RequestAcceptLetterNotifier source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("AcceptLetterNotifier",
                            new XElement("LetterID", source.LetterID ?? ""),
                            new XElement("AcceptID", source.AcceptID ?? ""),                         
                            new XElement("CorrespondenceDate", source.CorrespondenceDate ?? ""),
                            new XElement("Subject", source.Subject ?? "")
                            ))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //ส่งหนังสือปฏิเสธ
        public static string RequestRejectLetterNotifier(RequestRejectLetterNotifier source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("RejectLetterNotifier",
                            new XElement("LetterID", source.LetterID ?? ""),
                            new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                            new XElement("Subject", source.Subject ?? "")
                            ))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //ส่งหนังสือแจ้งหนังสือผิด
        public static string RequestInvalidLetterNotifier(RequestInvalidLetterNotifier source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("InvalidLetterNotifier",
                            new XElement("LetterID", source.LetterID ?? ""),
                            new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                            new XElement("Subject", source.Subject ?? "")
                            ))));
            return Util.ConvertXmlFormat(soapElement);
        }
        //ส่งหนังสือแจ้งเลขรับผิด
        public static string RequestInvalidAcceptIDNotifier(RequestInvalidAcceptIDNotifier source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("InvalidAcceptIDNotifier",
                            new XElement("LetterID", source.LetterID ?? ""),
                            new XElement("AcceptID", source.AcceptID ?? ""),
                            new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                            new XElement("Subject", source.Subject ?? "")
                            ))));
            return Util.ConvertXmlFormat(soapElement);
        }
        //การรับหนังสือภายนอก,การรับหนังสือตอบรับ,การรับหนังสือแจ้งเลขรับ,การรับหนังสือปฏิเสธ,การรับหนังสือแจ้งหนังสือผิด,การรับหนังสือแจ้งเลขรับผิด
        public static string RequestReceiveDocumentLetter(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp2.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterInboundRequest")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอลบหนังสือภายนอกเพื่อจะส่งหนังสือใหม่ (กรณีได้รับหนังสือปฏิเสธหรือแจ้งหนังสือผิด)
        public static string RequestDeleteGovernmentDocument(RequestDeleteGovernmentDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceDeleteGovernmentDocumentRequest",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? "")))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอลบหนังสือออกจากคิว 
        public static string RequestDeleteDocumentQueue(RequestDeleteQueue source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("CorrespondenceLetterDeleteRequest",
                        new XElement("ProcessID", source.ProcessID ?? ""))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอรหัสกระทรวง
        public static string RequestGetMinistry(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "Test", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("GetMinistryOrganizationList")));



            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอข้อมูลหน่วยงาน
        public static string RequestGetOrganizationList(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("GetOrganizationList")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอรหัสชั้นความลับ
        public static string RequestGetSecretCodes(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";


            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("GetSecretCodes")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอรหัสชั้นความเร็ว
        public static string RequestGetSpeedCodes(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";


            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("GetSpeedCodes")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอรหัสประเภทไฟล์
        public static string RequestGetMimeCodes(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";


            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("GetMimeCodes")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การสอบถามสถานะส่งหนังสือ
        public static string RequestGetStatus(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";


            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("OutboundStatusRequest")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การตรวจสอบเวลา
        public static string RequestTimeCheck(RequestReceive source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";


            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "http://dev.scp1.ecms.dga.or.th/ecms-ws01/service2", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body",
                    new XElement("TimeCheckRequest")));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือภายนอกใหม
        public static string RequestGetNewDocument(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("GovernmentDocument",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือตอบรับใหม่
        public static string RequestGetNewAcceptLetter(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("ReceiveLetterNotifier",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือแจ้งเลขรับใหม่
        public static string RequestGetNewAcceptLetterNotifier(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("AcceptLetterNotifier",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือปฏิเสธใหม่
        public static string RequestRejectLetterNotifier(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("RejectLetterNotifier",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือแจ้งหนังสือผิดใหม่
        public static string RequestInvalidLetterNotifier(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("InvalidLetterNotifier",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }
        //การขอหนังสือแจ้งเลขรับผิดใหม
        public static string RequestInvalidAcceptIDNotifier(RequestNewDocument source)
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", source.MessageID ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", source.To ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterInboundSpecialRequest",
                     new XElement("InvalidAcceptIDNotifier",
                        new XElement("LetterID", source.LetterID ?? ""),
                        new XElement("CorrespondenceDate", source.CorrespondenceData ?? ""),
                        new XElement("SenderDepartment",
                            new XElement("Code", source.SenderDepartment ?? "")),
                        new XElement("AcceptDepartment",
                            new XElement("Code", source.AcceptDepartment ?? ""))))));

            return Util.ConvertXmlFormat(soapElement);
        }

      
        public static string SendDocRejectWrongLetterNotifier()
        {
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsa = "http://www.w3.org/2005/08/addressing";

            XNamespace rsm = "urn:th:gov:egif:data:standard:ReuseableAggregateBusinessInformaionEntity:1";
            XNamespace ram = "urn:th:gov:egif:data:standard:GovernmentDocument:1.0";

            XElement soapElement = new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "SOAP-ENV", soap),
                new XElement(soap + "Header",
                    new XElement(wsa + "MessageID", null ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa)),
                    new XElement(wsa + "To", null ?? "", new XAttribute(XNamespace.Xmlns + "wsa", wsa))),
                new XElement(soap + "Body", new XAttribute("ID", "Body"),
                    new XElement("CorrespondenceLetterOutboundRequest",
                        new XElement("ProcessID", null ?? ""),
                        new XElement("ProcessTime", null ?? ""),
                        new XElement("InvalidAcceptIDNotifier",
                            new XElement("LetterID", null ?? ""),
                            new XElement("AcceptID", null ?? ""),
                            new XElement("CorrespondenceDate", null ?? ""),
                            new XElement("Subject", null ?? ""),
                            new XElement("AcceptDate", null ?? ""),
                            new XElement("AcceptDepartment",
                                new XElement("Code", null ?? ""))
                            ))));
            return Util.ConvertXmlFormat(soapElement);
        }

    }
}