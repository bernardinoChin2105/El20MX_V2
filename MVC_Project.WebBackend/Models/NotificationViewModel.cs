using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class NotificationViewModel
    {
        public Int64 Id { get; set; }
        public string Message { get; set; }
        public string Moment { get; set; }
    }
}