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
        public static string CallServicePaybook(string urlService, Object JsonString, string method, bool apiKeyBool = false, string token = null)
        {
            string urlapi = ConfigurationManager.AppSettings["Paybook.Url"];
            string apiKey = ConfigurationManager.AppSettings["Paybook.ApiKey"];

            try
            {
                //var client = new RestClient("https://https://api.syncfy.com/v1/credentials");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.GET);
                //IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);

                Method met = (Method)Enum.Parse(typeof(Method), method, true); ;
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest(met);
                client.BaseUrl = new Uri(urlapi);
                // Resource should just be the path
                request.Resource = string.Format(urlService);
                // This looks correct assuming you are putting your actual x-api-key here
                if (token != null)
                {
                    //client.AddHandler("Authorization", "Bearer " + token);
                    request.AddHeader("Authorization", "Bearer " + token);
                }
                if (apiKeyBool)
                {
                    request.AddHeader("Authorization", "api_key api_key=" + apiKey);
                }
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
