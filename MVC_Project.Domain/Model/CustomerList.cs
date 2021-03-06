﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class CustomerList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public Int32 Total { get; set; }
    }

    public class CustomerFilter
    {
        public string uuid { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string email { get; set; }
    }

    public class ReceiverFilter
    {
        public string uuid { get; set; }
        //public string rfc { get; set; }
        public string businessName { get; set; }
        public string typeInvoice { get; set; }
    }

    public class ExportListCustomer
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string rfc { get; set; }
        public string curp { get; set; }
        public string businessName { get; set; }
        public string taxRegime { get; set; }
        public string street { get; set; }
        public string interiorNumber { get; set; }
        public string outdoorNumber { get; set; }
        public string zipCode { get; set; }
        public Int64 colony { get; set; }
        public Int64 municipality { get; set; }
        public Int64 state { get; set; }
        public Int64 country { get; set; }
        public bool deliveryAddress { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public Int64 accountId { get; set; }
        public Guid uuidAccount { get; set; }
        public string rfcAccount { get; set; }
        public string nameCountry { get; set; }
        public string nameState { get; set; }
        public string nameMunicipality { get; set; }
        public string nameSettlementType { get; set; }
        public string nameSettlement { get; set; }

        public string Observaciones { get; set; }
    }

    public class CustomerCFDIFilter
    {
        //public string uuid { get; set; }
        public Int64 accountId { get; set; }
        public string rfc { get; set; }
        public string folio { get; set; }
        public string paymentMethod { get; set; }
        public string paymentForm { get; set; }
        public string currency { get; set; }
        public string serie { get; set; }
        public string nombreRazonSocial { get; set; }
    }

    public class InvoicesIssuedList
    {
        public Int64 id { get; set; }
        //public Guid uuid { get; set; }
        public string folio { get; set; }
        public string serie { get; set; }
        public string paymentMethod { get; set; }
        public string paymentForm { get; set; }
        public string currency { get; set; }
        public decimal subtotal { get; set; }
        public decimal iva { get; set; }
        public decimal total { get; set; }
        public DateTime invoicedAt { get; set; }
        public Int64 customerId { get; set; }
        public Int64 accountId { get; set; }
        public string rfc { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string paymentFormDescription { get; set; }
        public string businessName { get; set; }
        public string taxRegime { get; set; }
        public int Total { get; set; }
        public string invoiceType { get; set; }
        public string xml { get; set; }
    }

    public class ListCustomersAC
    {
        public Int64 id { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }

    public class ListCustomersProvider
    {
        public Int64 id { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string taxRegime { get; set; }
    }

    
}
