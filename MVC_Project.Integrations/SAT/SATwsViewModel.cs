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

    public class LogInSATModel
    {
        public string type { get; set; }
        public string rfc { get; set; }
        public string password { get; set; }

        public LogInSATModel()
        {
            type = "ciec";
        }
    }

    public class SatAuthResponseModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string rfc { get; set; }
        public string status { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class ExtractionsFilter
    {
        public string taxpayer { get; set; }
        public string extractor { get; set; }
        public string periodFrom { get; set; }
        public string periodTo { get; set; }
        public string status { get; set; }
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

    public class CustomersInfo
    {
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string zipCode { get; set; }
        public int regime { get; set; }
    }

    public class ProvidersInfo
    {
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string zipCode { get; set; }
        public Int32 regime { get; set; }
    }

    //Información fiscal del contribuyente
    public class TaxpayerInfo
    {
        public string id { get; set; }
        public string personType { get; set; }
        public string registrationDate { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }

    //Modelo de retorno para el DX0 y clientes
    public class InvoicesModel
    {

        public List<TaxStatus> TaxStatus { get; set; }
        public TaxpayerInfo Taxpayer { get; set; }
        public List<InvoicesInfo> Invoices { get; set; }
        public List<ProvidersInfo> Providers { get; set; }
        public List<CustomersInfo> Customers { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class TaxStatus
    {
        public string id { get; set; }
        public string rfc { get; set; }
        public Person person { get; set; }
        public Company company { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public Address address { get; set; }
        public List<EconomicActivities> economicActivities { get; set; }
        public List<TaxRegimes> taxRegimes { get; set; }
        public DateTime startedOperationsAt { get; set; }
        public string status { get; set; }
        public DateTime statusUpdatedAt { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Company
    {
        public string legalName { get; set; }
        public string tradeName { get; set; }
        public string entityType { get; set; }
    }

    public class Person
    {
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string curp { get; set; }
    }

    public class Address
    {
        public List<string> streetReferences { get; set; }
        public string streetNumber { get; set; }
        public string buildingNumber { get; set; }
        public string locality { get; set; }
        public string municipality { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
        public string streetName { get; set; }
        public string streetType { get; set; }
        public string neighborhood { get; set; }
    }

    public class EconomicActivities
    {
        public string name { get; set; }
        public string order { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public string percentage { get; set; }
    }

    public class TaxRegimes
    {
        public string name { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
    }
}
