using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MVC_Project.API.Models
{
    [DataContract]
    public class ApiResponse<T> where T : class
    {
        [DataMember(Name = "result")]
        public string Result { get; set; }
        [DataMember(Name = "code")]
        public int StatusCode { get; set; }
        [DataMember(Name = "data")]
        public T ResponseData { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}