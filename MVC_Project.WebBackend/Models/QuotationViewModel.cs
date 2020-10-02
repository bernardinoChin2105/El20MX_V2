using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class QuotationViewModel
    {
        public string account { get; set; }
    }

    public class QuotationData
    {
        public Int64 id { get; set; }
        public string account { get; set; }
        public decimal total { get; set; }
        public int partialitiesNumber { get; set; }
        public string status { get; set; }
        public string quoteLink { get; set; }
        public string quoteName { get; set; }
        public DateTime startedAt { get; set; }
    }
}