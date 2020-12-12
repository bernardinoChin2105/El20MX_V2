//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace MVC_Project.API.Models
//{
//    public class WebhookEventModel
//    {
//        public string id { get; set; }
//        public string type { get; set; }
        
//        public WebhookData data { get; set; }
//        public string createdAt { get; set; }
//        public string updatedAt { get; set; }
//    }

//    public class WebhookData
//    {
//        public WebhookObject @object { get; set; }
//    }

//    public class WebhookObject
//    {
//        public string id { get; set; }
//        public string status { get; set; }
//        public WebhookTaxpayer taxpayer { get; set; }
//        public WebhookOptions options { get; set; }
//        public string rfc { get; set; }
//        public string type { get; set; }
//    }

//    public class WebhookTaxpayer
//    {
//        public string id { get; set; }
//        public string name { get; set; }
//        public string personType { get; set; }
//        public string registrationDate { get; set; }
//    }

//    public class WebhookOptions
//    {
//        public WebhookPeriod period { get; set; }
//    }

//    public class WebhookPeriod
//    {
//        public string to { get; set; }
//        public string from { get; set; }
//    }

//    public class SyncfyWebhookModel
//    {
//        public SyncfyEndpointModel endpoints { get; set; }
//        public string @event{ get;set; }
//        public string id_credential { get; set; }
//        public string id_external { get; set; }
//        public string id_job { get; set; }
//        public string id_job_uuid { get; set; }
//        public string id_site { get; set; }
//        public string id_site_organization { get; set; }
//        public string id_site_organization_type { get; set; }
//        public string id_user { get; set; }
//    }
//    public class SyncfyEndpointModel
//    {
//        public List<string> accounts { get; set; }
//        public List<string> attachments { get; set; }
//        public List<string> credential { get; set; }
//        public List<string> transactions { get; set; }
//    }
//}