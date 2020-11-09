using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class SATViewModel
    {
        public Int64 id { get; set; }
        public string uuid {get; set;}
        [DisplayName("RFC")]
        public string rfc { get; set; }
        [DisplayName("Razón Social")]
        public string name { get; set; }

        [DisplayName(".cer")]
        public HttpPostedFileBase cer { get; set; }
        [DisplayName(".key")]
        public HttpPostedFileBase key { get; set; }

        public string cerUrl { get; set; }
        public string keyUrl { get; set; }

        [DisplayName("ciec")]
        public string ciec { get; set; }
        [DisplayName("efirma")]
        public string efirma { get; set; }

        [DisplayName("Conexión CIEC")]
        public string ciecStatus { get; set; }
        [DisplayName("Conexión FIEL")]
        public string efirmaStatus { get; set; }

        public string ciecUuid { get; set; }
        public string efirmaUuid { get; set; }

        [DisplayName("Avatar")]
        public string avatar { get; set; }
        public DateTime cerExpiryDate { get; set; }
    }
}