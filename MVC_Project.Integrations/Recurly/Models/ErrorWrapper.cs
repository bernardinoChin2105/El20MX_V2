using Newtonsoft.Json;

namespace MVC_Project.Integrations.Recurly.Models
{
    public class ErrorWrapper
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}
