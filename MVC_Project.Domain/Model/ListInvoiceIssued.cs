using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class ListInvoiceIssued
    {
        public Int64 id { get; set; }
        public string folio { get; set; }
        public string serie { get; set; }
        public string paymentMethod { get; set; }
        public string paymentForm { get; set; }
        public string currency { get; set; }
        public decimal iva { get; set; }
        public DateTime invoicedAt { get; set; }
        public string invoiceType { get; set; }
        public decimal total { get; set; }
        public decimal subtotal { get; set; }
        public string xml { get; set; }
    }
}
