using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class CredentialViewModel
    {

    }

    public class CredentialSATViewModel
    {
        public string uuid { get; set; }
        public string rfc { get; set; }
        public string password { get; set; }
        public string type { get; set; } //ciec, efirma
        
    }

    public class EfirmaViewModel
    {
        public HttpPostedFileBase cer { get; set; }
        public HttpPostedFileBase key { get; set; }
        
        public string efirmaUuid { get; set; }
        public string efirma { get; set; }
        public string rfc { get; set; }
    }
}