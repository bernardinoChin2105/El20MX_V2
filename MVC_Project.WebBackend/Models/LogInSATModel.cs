using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class LogInSATModel
    {
        public string type { get; set; }
        public string rfc { get; set; }
        public string password { get; set; }

        public LogInSATModel()
        {
            type = "ciec";
        }
    }

    public class SatAuthResponseModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string rfc { get; set; }
        public string status { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class LoginSATViewModel
    {
        public string RFC { get; set; }
        public string CIEC { get; set; }
    }
}