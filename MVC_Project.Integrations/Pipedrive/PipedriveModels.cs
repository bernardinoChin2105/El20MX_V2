using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Pipedrive
{

    public class PipedrivePerson
    {
        [JsonProperty("name")]
        public string Name { set; get; }
        [JsonProperty("first_name")]
        public string FirstName { set; get; }
        [JsonProperty("last_name")]
        public string LastName { set; get; }
        [JsonProperty("email")]
        public string Email { set; get; }
        [JsonProperty("phone")]
        public string Phone { set; get; }
        [JsonProperty("b1f08ca6ecde569f07208dc28489b28c7248eaa4")]
        public string RFC { set; get; }
        [JsonProperty("358097456870be3412a055133f29d4083503fe6d")]
        public string CIEC { set; get; }
        //[JsonProperty("visible_to")]
        //public string VisibleTo { get; set; }
    }

    public class PipedriveResponse
    {
        [JsonProperty("success")]
        public bool Success { set; get; }
        [JsonProperty("error")]
        public string Error { set; get; }
        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; }
        [JsonProperty("data")]
        public PipedriveData Data { set; get; }
    }

    public class PipedriveData
    {
        [JsonProperty("id")]
        public int Id { set; get; }
        [JsonProperty("company_id")]
        public int CompanyId { set; get; }
        
    }
}
