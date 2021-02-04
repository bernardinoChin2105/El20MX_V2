using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class AccountCredentialModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string rfc { get; set; }
        public string provider { get; set; }
        public string idCredentialProvider { get; set; }
        public string statusProvider { get; set; }
        public string hostedKey { get; set; }
        public string planSchema { get; set; }
        public string planFijo { get; set; }
        public DateTime? inicioFacturacion { get; set; }
    }

    public class AccountCredentialProspectModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string rfc { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string statusAccount { get; set; }
        public DateTime? inicioFacturacion { get; set; }
        public string planFijo { get; set; }
        public Int32 totalDays { get; set; }
        public Int64 credentialId { get; set; }
        public string provider { get; set; }
        public string idCredentialProvider { get; set; }
        public string statusProvider { get; set; }
        public DateTime createdAtCredential { get; set; }
        public string statusCredential { get; set; }
        public Int64 accountId { get; set; }
        public string credentialType { get; set; }     
    }

    public class AccountPaymentsModel
    {
        public Int64 paymentId { get; set; }
        public decimal subtotal { get; set; }
        public decimal total { get; set; }
        public string paymentGateway { get; set; }
        public string statusCode { get; set; }
        public Int64 accountId { get; set; }
        public string statusMessage { get; set; }
        public DateTime createdAt { get; set; }
        public Int64 subscriptionId { get; set; }
        public Int64 number { get; set; }
        public string name { get; set; }
        public string rfc { get; set; }
        public DateTime createdAtAccount { get; set; }
        public string status { get; set; }
        public string planSchema { get; set; }
        public DateTime? inicioFacturacion { get; set; }
        public string planFijo { get; set; }
        public Int64 credentialId { get; set; }
        public string provider { get; set; }
        public string idCredentialProvider { get; set; }
        public string statusCredential { get; set; }
    }
}
