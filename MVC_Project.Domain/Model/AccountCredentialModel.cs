using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class AccountCredentialModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string rfc { get; set; }
        public string provider { get; set; }
        public string idCredentialProvider { get; set; }
        public string statusProvider { get; set; }
        public string hostedKey { get; set; }
        public string planSchema { get; set; }
    }
}
