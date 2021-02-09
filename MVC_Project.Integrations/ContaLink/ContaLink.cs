using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.ContaLink
{
    public class ContaLink
    {
        public static string CallServiceContaLink(string urlService, Object JsonString, string method)
        {
            string urlApi = ConfigurationManager.AppSettings["ContaLink.Url"];
            string apiKey = ConfigurationManager.AppSettings["ContaLink.ApiKey"];

            try
            {
                /*var client = new RestClient("{{baseUrl}}/treasury/bank-transactions/");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "<string>");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", "{\n    \"reference\": \"\",\n    \"date\": \"\",\n    \"bank\": \"\",\n    \"deposit\": 0,\n    \"withdrawal\": 0,\n    \"description\": \"\"\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);*/


                Method met = (Method)Enum.Parse(typeof(Method), method, true);
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest(met);
                client.BaseUrl = new Uri(urlApi);
                // Resource should just be the path
                request.Resource = string.Format(urlService);
                // This looks correct assuming you are putting your actual x-api.key here
                request.AddHeader("Authorization", apiKey);
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

                throw new Exception(response.StatusDescription + ": " + response.Content.ToString());
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                throw new HttpRequestException("Request issue -> HTTP code:" + ex.Message.ToString());
            }
        }
    }
}
