using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class Invoice
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public bool issued { get; set; }
        public bool isHomeIssued { get; set; }
        public DateTime invoicedAt { get; set; }
        public string invoiceType { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
    }
}
