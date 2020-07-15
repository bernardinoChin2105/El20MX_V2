using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATwsViewModel
    {
    }

    public class InvoicesInfo
    {
        public string id { get; set; }
        public string uuid { get; set; }
        public decimal version { get; set; }
        public string type { get; set; }
        public string usage { get; set; }
        public string paymentType { get; set; }
        public string paymentMethod { get; set; }
        public string placeOfIssue { get; set; }
        public IssuerReceiver issuer { get; set; }
        public IssuerReceiver receiver { get; set; }
        public string currency { get; set; }
        public decimal? discount { get; set; }
        public decimal? subtotal { get; set; }
        public decimal total { get; set; }
        public decimal amount { get; set; }
        public string exchangeRate { get; set; } //en satws viene null, no se que tipo sea
        public string status { get; set; }
        public string pac { get; set; }
        public DateTime issuedAt { get; set; }
        public DateTime certifiedAt { get; set; }
        public string cancellationStatus { get; set; }
        public string cancellationProcessStatus { get; set; } //en satws viene null, no se que tipo sea
        public string canceledAt { get; set; } //en satws viene null, no se que tipo sea
        public bool xml { get; set; }
        public bool pdf { get; set; }

        public List<ServicesDescriptionCFDI> items { get; set; }
    }

    public class ServicesDescriptionCFDI
    {
        public decimal taxRate { get; set; }
        public string taxType { get; set; }
        public int quantity { get; set; }
        public string unitCode { get; set; }
        public decimal taxAmount { get; set; }
        public decimal unitAmount { get; set; }
        public string description { get; set; }
        public decimal totalAmount { get; set; }
        public decimal discountAmount { get; set; }
        public string identificationNumber { get; set; }
        public string productIdentification { get; set; }
    }

    public class IssuerReceiver
    {
        public string rfc { get; set; }
        public string name { get; set; }
    }

    public class CustumersInfo {
        public string rfc { get; set; }
        public string name { get; set; }
        public string zipCode { get; set; }
        public string regime { get; set; }
    }    
}
