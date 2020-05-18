using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MVC_Project.API.Models.Api_Response
{
    [DataContract]
    public class MessageResponse
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    enum MessageType
    {
        info,
        error
    }
}