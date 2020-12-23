using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    public class Recurly
    {
        public static string CallServiceRecurly(string urlService, Object JsonString, string method) {
            string urlApi = ConfigurationManager.AppSettings["Recurly.Url"];
            string apiKey = ConfigurationManager.AppSettings["Recurly.ApyKey"];
            string version = ConfigurationManager.AppSettings["Recurly.Version"];

            try
            {
                Method met = (Method)Enum.Parse(typeof(Method), method, true);
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest(met);
                client.BaseUrl = new Uri(urlApi);
                // Resource should just be the path
                request.Resource = string.Format(urlService);
                // This looks correct assuming you are putting your actual x-api.key here
                request.AddHeader("Authorization", "Basic " + apiKey);
                request.AddHeader("Accept", "application/" + version);

                if (JsonString != null)
                {
                    var data = JsonConvert.SerializeObject(JsonString, Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                    if(met == Method.GET)
                    {
                        //var qs = new StringBuilder("?");

                        var objType = JsonString.GetType();

                        objType.GetProperties()
                               .Where(p => p.GetValue(JsonString, null) != null).ToList()
                               .ForEach(p =>
                               {
                                   request.AddQueryParameter(Uri.EscapeDataString(p.Name), Uri.EscapeDataString(p.GetValue(JsonString).ToString()));
                               });

                        //qs.ToString();
                        //request.REs(JsonString);
                    } else
                    {
                        request.AddParameter("application/json", data, ParameterType.RequestBody);
                    }
                    
                }

                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string jsonResponse = response.Content.ToString();
                    return jsonResponse;
                }

                throw new Exception(response.StatusDescription + ": "+response.Content.ToString());
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                throw new HttpRequestException("Request issue -> HTTP code:" + ex.Message.ToString());
            }         
        }
    }
}
