using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATws
    {
        public static string CallServiceSATws(string urlService, Object JsonString, string token)
        {
            string urlapi = ConfigurationManager.AppSettings["SATws.Url"];
            string apiKey = ConfigurationManager.AppSettings["SATws.ApiKey"];

            try
            {
                var client = new RestClient(urlapi);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);


                //var client = new RestClient("http://myurl.com/api/");
                //var request = new RestRequest("getCatalog?token={token}", Method.GET);
                //request.AddParameter("token", "saga001", ParameterType.UrlSegment);
                //// request.AddUrlSegment("token", "saga001");             
                //request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                //var queryResult = client.Execute(request);            
                //Console.WriteLine(queryResult.Content);


                //var client = new RestClient("https://sync.paybook.com/v1/sessions");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("application/json", "{\n\t\"id_user\" : \"{{sync_id_user}}\"\n}", ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }
            return null;
        }


        
        //Console.WriteLine(response.Content);
    }
}
