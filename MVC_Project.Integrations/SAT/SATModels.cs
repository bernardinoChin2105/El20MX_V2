using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class CredentialsResponse
    {
        public string id { get; set; }
        public string status { get; set; }
    }

    public class ExtractionResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public string extractor { get; set; }
    }

    public class CredentialRequest
    {
        public string rfc { get; set; }
        public string ciec { get; set; }
    }

    public class CertificateResponse
    {
        public string id { get; set; }
    }
}
