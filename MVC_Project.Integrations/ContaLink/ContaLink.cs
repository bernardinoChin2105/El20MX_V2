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
        public static string CallServiceContaLink(string urlService, Object JsonString, string method, string apiKey)
        {
            string urlApi = ConfigurationManager.AppSettings["ContaLink.Url"];

            try
            {
                //var client = new RestClient("https://794lol2h95.execute-api.us-east-1.amazonaws.com/prod/treasury/bank-transactions/");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("Authorization", "24560659-645e-4240-868c-88d3adbccaf6 93681ccf-a50e-4227-93ee-0cb1c032ea96");
                //request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("application/json", "{\r\n  \"bank\": \"Banamex-Cta. Maestra - MXN-077\",\r\n  \"date\": \"2020-11-10\",\r\n  \"deposit\": 0,\r\n  \"description\": \"Prueba-0000000000150265 COMPRA INVERSION INTEGRAL AUT. 150265\",\r\n  \"reference\": \"\",\r\n  \"withdrawal\": 200.80\r\n}", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);


                Method met = (Method)Enum.Parse(typeof(Method), method, true);
                var client = new RestClient();
                client.Timeout = -1;
                var request = new RestRequest(met);
                client.BaseUrl = new Uri(urlApi);
                // Resource should just be the path
                request.Resource = string.Format(urlService);
                // This looks correct assuming you are putting your actual x-api.key here
                //request.AddHeader("Authorization", "24560659-645e-4240-868c-88d3adbccaf6 93681ccf-a50e-4227-93ee-0cb1c032ea96");
                //request.AddHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddHeader("Authorization", apiKey);
                //request.AddHeader("API_KEY", "[THIS IS THE API KEY]");
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
