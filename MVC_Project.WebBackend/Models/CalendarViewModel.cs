using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class EventData
    {
        public Int64 Id { get; set; }

        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Start { get; set; }

        public string End { get; set; }

        public bool IsFullDay { get; set; }

    }
}