using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class EventsResponse
    {
        public string context { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        //public TaxPayerViewModel taxpayer { get; set; }
        public string source { get; set; }
        public string resource { get; set; }
        
    }
}