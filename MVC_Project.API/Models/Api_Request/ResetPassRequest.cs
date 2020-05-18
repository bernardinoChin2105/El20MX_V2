using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MVC_Project.API.Models.Api_Request
{
    [DataContract]
    public class ResetPassRequest
    {
        [DataMember(Name = "token")]
        public string Token { get; set; }
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}