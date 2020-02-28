using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

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
    }


}