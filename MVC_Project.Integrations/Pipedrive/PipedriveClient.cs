using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVC_Project.Utils;
using Newtonsoft.Json;
using RestSharp;


namespace MVC_Project.Integrations.Pipedrive
{
    public class PipedriveClient
    {
        private RestClient client;
        private string api_token;
        public PipedriveClient()
        {
            client = new RestClient(System.Configuration.ConfigurationManager.AppSettings["Pipedrive.BaseURL"]);
            api_token = System.Configuration.ConfigurationManager.AppSettings["Pipedrive.ApiKey"];
        }

        public PipedriveResponse CreatePerson(PipedrivePerson person)
        {
            RestRequest request = new RestRequest("/persons", Method.POST);
            request.AddQueryParameter("api_token", api_token);
            request.AddJsonBody(JsonConvert.SerializeObject(person));
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<PipedriveResponse>(response.Content);
        }

    }
}
