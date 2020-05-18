using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Web.Script.Serialization;
using System.Net.Http.Formatting;
using System.Globalization;

namespace MVC_Project.Utils
{
    public class JsonUtil
    {
        public static string mediatype
        {
            get { return "application/json"; }
        }
        /// <summary>Configura el formato para serializacion y envio por JSON.</summary>
        /// <returns>objeto JsonMediaTypeFormatter utilizado para el envio de datos via JSON de la API</returns>

        public static JsonMediaTypeFormatter Formatter()
        {
            var formatter = new JsonMediaTypeFormatter();
            var json = formatter.SerializerSettings;
            json.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            json.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.Formatting = Newtonsoft.Json.Formatting.Indented;
            json.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.Culture = new CultureInfo("it-IT");
            return formatter;

        }
        public static T ConvertToObject<T>(String json)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            return jsonSerializer.Deserialize<T>(json);
        }

        public static string ConvertToString<T>(T obj)
        {
            try { 
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            return jsonSerializer.Serialize(obj);
                }
            catch
            {
                return null;
            }
        }

        public static bool TryConvertToObject<T>(string json, out T data)
        {
            if (!String.IsNullOrWhiteSpace(json))
            {
                data = ConvertToObject<T>(json);
                return true;
            }
            data = ConvertToObject<T>(string.Empty);
            return false;
        }
    }
}
