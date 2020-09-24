using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.API.Models
{
    public class WebhookEventModel
    {
        public string id { get; set; }
        public string type { get; set; }
        
        public WebhookData data { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class WebhookData
    {
        public WebhookObject @object { get; set; }
    }

    public class WebhookObject
    {
        public string id { get; set; }
        public string status { get; set; }
        public WebhookTaxpayer taxpayer { get; set; }
        public WebhookOptions options { get; set; }
    }

    public class WebhookTaxpayer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string personType { get; set; }
        public string registrationDate { get; set; }
    }

    public class WebhookOptions
    {
        public WebhookPeriod period { get; set; }
    }

    public class WebhookPeriod
    {
        public string to { get; set; }
        public string from { get; set; }
    }
}