using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Paybook
{
    public class Paybook
    {
        public static string CallServicePaybook(string urlService, Object JsonString, string method)
        {
            string urlapi = ConfigurationManager.AppSettings["Paybook.Url"];
            string apiKey = ConfigurationManager.AppSettings["Paybook.ApiKey"];

            try
            {
                Method met = (Method)Enum.Parse(typeof(Method), method, true); ;
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest(met);
                client.BaseUrl = new Uri(urlapi);
                // Resource should just be the path
                request.Resource = string.Format(urlService);
                // This looks correct assuming you are putting your actual x-api-key here
                request.AddHeader("Authorization", "api_key api_key="+apiKey);
                request.AddHeader("Content-Type", "application/json");

                if (JsonString != null)
                {
                    var data = JsonConvert.SerializeObject(JsonString);
                    request.AddParameter("application/json", data, ParameterType.RequestBody);
                }

                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string jsonResponse = response.Content.ToString();
                    return jsonResponse;
                }

                throw new Exception(response.StatusDescription);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                throw new HttpRequestException("Request issue -> HTTP code:" + ex.Message.ToString());
                //throw new HttpRequestException("Request issue -> HTTP code:" + response.StatusCode);
            }
            //return null;
        }
    }
}
