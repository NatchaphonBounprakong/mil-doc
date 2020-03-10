using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace WEB.API.DGA.MIL.DOC
{
    public static class Util
    {
        public static T GetValue<T>(object value)
        {
            if (value.GetType() == typeof(string))
            {
                if (value.ToString() != string.Empty)
                {

                }
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetBase64Value(string path)
        {
            try
            {
                Byte[] bytes = File.ReadAllBytes(path);
                String file = Convert.ToBase64String(bytes);
                return file;
            }
            catch(Exception ex)
            {
                return "";
            }
         
        }

        public static string GetStringValue(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                return value.Trim();
            }
        }

        public static string ConvertExtensionType(string ext)
        {
            string returnType = "";
            switch (ext)
            {
                case "application/msword":
                    returnType = ".doc";
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    returnType = ".docx";
                    break;
                case "application/pdf":
                    returnType = ".pdf";
                    break;
                case "application/rtf":
                    returnType = ".rtf";
                    break;
                case "application/vnd.ms-excel":
                    returnType = ".xls";
                    break;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    returnType = ".xlsx";
                    break;
                case "application/zip":
                    returnType = ".zip";
                    break;
                case "image/bmp":
                    returnType = ".bmp";
                    break;
                case "image/jpeg":
                    returnType = ".jpg";
                    break;
                case "image/png":
                    returnType = ".png";
                    break;
                case "image/tiff":
                    returnType = ".tiff";
                    break;
                case "text/plain":
                    returnType = ".txt";
                    break;
                case "application/vnd.ms-powerpoint":
                    returnType = ".ppt";
                    break;
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    returnType = ".pptx";
                    break;
            }

            return returnType;

        }

        public static string ConvertContentType(string content)
        {
            content = content.ToLower();
            string returnType = "";
            switch (content)
            {
                case ".doc":
                    returnType = "application/msword";
                    break;
                case ".docx":
                    returnType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".pdf":
                    returnType = "application/pdf";
                    break;
                case ".rtf":
                    returnType = "application/rtf";
                    break;
                case ".xls":
                    returnType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    returnType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".zip":
                    returnType = "application/zip";
                    break;
                case ".bmp":
                    returnType = "image/bmp";
                    break;
                case ".png":
                    returnType = "image/png";
                    break;
                case ".jpg":
                    returnType = "image/jpeg";
                    break;
                case ".tiff":
                    returnType = "image/tiff";
                    break;
                case ".txt":
                    returnType = "text/plain";
                    break;
                case ".ppt":
                    returnType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    returnType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
            }

            return returnType;

        }

        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static string ConvertXmlFormat(XElement xml)
        {

            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "no"), xml);

            string text = xml.ToString().Replace("\r\n", "");
            int space = 50;
            for (int i = 1; i <= space; i++)
            {
                string s = (" ").PadLeft(i, ' ');
                string tag = ">" + s + "<";
                text = text.Replace(tag, "><");
            }
            return text;
        }
    }


}