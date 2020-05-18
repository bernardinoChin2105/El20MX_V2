
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utils
{
    public class FormatUtils
    {
        public static string GetReportFileName(string name)
        {
            string date = DateUtil.GetDateTimeNow().ToString("dd/MM/yyyy");
            string fileName = name + date + ".csv";
            return fileName;
        }
        public static string GetCSVTableFormat(IList<string> headers, IList<IList<string>> rows)
        {
            StringBuilder content = new StringBuilder();
            content.Append(String.Join(",", headers));
            content.Append(Environment.NewLine);
            foreach (IList<string> row in rows)
            {
                content.Append(String.Join(",", row));
                content.Append(Environment.NewLine);
            }
            return content.ToString();
        }

        public static bool TryGetValue(object curremtObject, Type type, out object value)
        {
            value = null;
            Type gericType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                gericType = type.GenericTypeArguments[0];
                if (curremtObject == null)
                {
                    return true;
                }
            }
            if (curremtObject == null)
            {
                return false;
            }
            var methodInfo = (from m in gericType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                              where m.Name == "TryParse"
                              select m).FirstOrDefault();
            if (methodInfo == null)
                throw new ApplicationException("Unable to find TryParse method!");
            object result = methodInfo.Invoke(null, new object[] { curremtObject.ToString(), value });
            if ((result != null))
            {
                var methodParser = (from m in gericType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                    where m.Name == "Parse"
                                    select m).FirstOrDefault();
                if (methodInfo == null)
                    throw new ApplicationException("Unable to find Parse method!");
                bool isParser = ((bool)result);
                if (isParser)
                {
                    value = methodParser.Invoke(null, new object[] { curremtObject.ToString() });
                }
                return ((bool)result);
            }
            return false;
        }
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}