﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    #region Modelos de envio del post para la factura
    public class InvoiceJson
    {        
        public Email email { get; set; }
        public InvoiceData data { get; set; }        
    }

    public class InvoiceComplementJson
    {
        public Email email { get; set; }
        public InvoiceComplementData data { get; set; }
    }

    public class InvoiceRefundJson
    {
        public Email email { get; set; }
        public InvoiceRefundData data { get; set; }
    }

    public class Email
    {
        public List<string> to { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    }

    public class InvoiceData {
        public string Serie { get; set; }
        public Int32 Folio { get; set; }
        public string Fecha { get; set; }
        public string Moneda { get; set; }
        public decimal? TipoCambio { get; set; }
        public string TipoDeComprobante { get; set; }
        public string CondicionesDePago { get; set; }
        public string FormaPago { get; set; }
        public string MetodoPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string Confirmacion { get; set; }
        public string TipoRelacion { get; set; }        
        public InvoiceIssuer Emisor { get; set; }
        public InvoiceReceiver Receptor { get; set; }
        public List<ConceptsData> Conceptos { get; set; }
        public List<CfdiRelacionados> CfdiRelacionados { get; set; }
    }

    public class InvoiceComplementData
    {
        public string Serie { get; set; }
        public Int32 Folio { get; set; }
        public string Fecha { get; set; }
        public string Moneda { get; set; }
        public decimal? TipoCambio { get; set; }
        public string TipoDeComprobante { get; set; }
        public string LugarExpedicion { get; set; }
        public Complemento Complemento { get; set; }
        public InvoiceIssuer Emisor { get; set; }
        public InvoiceReceiver Receptor { get; set; }
        public List<ConceptsData> Conceptos { get; set; }
    }

    public class InvoiceRefundData
    {
        public string Serie { get; set; }
        public Int32 Folio { get; set; }
        public string Fecha { get; set; }
        public string Moneda { get; set; }
        public decimal? TipoCambio { get; set; }
        public string TipoDeComprobante { get; set; }
        public string CondicionesDePago { get; set; }
        public string FormaPago { get; set; }
        public string MetodoPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string Confirmacion { get; set; }
        public List<CfdiRelacionados> CfdiRelacionados { get; set; }
        public InvoiceIssuer Emisor { get; set; }
        public InvoiceReceiver Receptor { get; set; }
        public List<ConceptsData> Conceptos { get; set; }
    }

    public class InvoiceIssuer
    {
        public string Rfc { get; set; }
        public string Nombre { get; set; }
        public string RegimenFiscal { get; set; }
    }
    public class InvoiceReceiver {
        public string Rfc { get; set; }
        public string Nombre { get; set; }
        public string ResidenciaFiscal { get; set; }
        public string NumRegIdTrib { get; set; }
        public string UsoCFDI { get; set; }
    }
    public class ConceptsData
    {
        public string ClaveProdServ { get; set; }
        public string NoIdentificacion { get; set; }
        public Int32 Cantidad { get; set; }
        public string ClaveUnidad { get; set; }
        public string Unidad { get; set; }
        public string Descripcion { get; set; }
        public string ValorUnitario { get; set; }
        public string Descuento { get; set; }
        public string Importe { get; set; }

        public List<Traslados> Traslados { get; set; }
        public List<Retenciones> Retenciones { get; set; }
        public List<InformacionAduanera> InformacionAduanera { get; set; }
        public CuentaPredial CuentaPredial { get; set; }
        public List<Parte> Parte { get; set; }        
    }
    public class Traslados
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public string Importe { get; set; }
    }
    public class Retenciones
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public string Importe { get; set; }
    }
    public class InformacionAduanera
    {
        public string NumeroPedimento { get; set; }
    }
    public class CuentaPredial
    {
        public string Numero { get; set; }
    }
    public class Parte
    {
        public string ClaveProdServ { get; set; }
        public string NoIdentificacion { get; set; }
        public string Cantidad { get; set; }
        public string Unidad { get; set; }
        public string Descripcion { get; set; }
        public string ValorUnitario { get; set; }
        public string Importe { get; set; }
        public List<InformacionAduanera> InformacionAduanera { get; set; }        
    }
    public class Complemento
    {
        public List<Pagos> Pagos { get; set; }
    }

    public class Pagos
    {
        public string FechaPago { get; set; }
        public string FormaDePagoP { get; set; }
        public string MonedaP { get; set; }
        public string TipoCambioP { get; set; }
        public string Monto { get; set; }
        public List<DoctoRelacionado> DoctoRelacionado { get; set; }
        public string NumOperacion { get; set; }
        public string RfcEmisorCtaOrd { get; set; }
        public string NomBancoOrdExt { get; set; }
        public string RfcEmisorCtaBen { get; set; } 
        public string CtaBeneficiario { get; set; }
        public string CtaOrdenante { get; set; }
    }

    public class DoctoRelacionado
    {
        public string IdDocumento { get; set; }
        public string MonedaDR { get; set; }
        public string MetodoDePagoDR { get; set; }
        public string NumParcialidad { get; set; }
        public string ImpSaldoAnt { get; set; }
        public string ImpPagado { get; set; }
        public string ImpSaldoInsoluto { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
    }

    public class CfdiRelacionados
    {
        public string TipoRelacion { get; set; }
        public CfdiRelacionado CfdiRelacionado { get; set; }
    }

    public class CfdiRelacionado
    {
        public string UUID { get; set; }
    }

    #endregion

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

    public class EfirmaModel
    {
        public string type { get; set; }
        public string certificate { get; set; }
        public string privateKey { get; set; }
        public string password { get; set; }

        public EfirmaModel()
        {
            type = "efirma";
        }
    }

    public class CertificateModel
    {
        public string type { get; set; }
        public string certificate { get; set; }
        public string privateKey { get; set; }
        public string password { get; set; }

        public CertificateModel()
        {
            type = "csd";
        }
    }

    public class CertificateResponseModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string rfc { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
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
        public decimal? version { get; set; }
        public string type { get; set; }
        public string usage { get; set; }
        public string paymentType { get; set; }
        public string paymentMethod { get; set; }
        public string placeOfIssue { get; set; }
        public IssuerReceiver issuer { get; set; }
        public IssuerReceiver receiver { get; set; }
        public string currency { get; set; }
        public decimal? discount { get; set; }
        public decimal? tax { get; set; }
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
        public string internalIdentifier { get; set; }
        public string reference { get; set; }
    }

    public class ServicesDescriptionCFDI
    {
        public decimal taxRate { get; set; }
        public string taxType { get; set; }
        public decimal quantity { get; set; }
        public string unitCode { get; set; }
        public decimal taxAmount { get; set; }
        public decimal unitAmount { get; set; }
        public string description { get; set; }
        public decimal totalAmount { get; set; }
        public decimal discountAmount { get; set; }
        public string identificationNumber { get; set; }
        public string productIdentification { get; set; }
    }


    //public class InvoicesCFDI
    //{
    //    public string id { get; set; }
    //    public decimal version { get; set; }
    //    public string Folio { get; set; }
    //    public string Serie { get; set; }
    //    public string MetodoPago { get; set; }
    //    public string FormaPago { get; set; }
    //    public string Moneda { get; set; }
    //    public decimal SubTotal { get; set; }
    //    public decimal Total { get; set; }
    //    public string TipoDeComprobante { get; set; }
    //    public string LugarExpedicion { get; set; } //Código Postal
    //    public string xml { get; set; }


    //    ////IVA
    //    //public DateTime Fecha { get; set; }
    //    //public string Sello { get; set; }
    //    //public string NoCertificado { get; set; }
    //    //public string Certificado { get; set; }
    //    //public string CondicionesDePago { get; set; }
    //    //public decimal Descuento { get; set; }           
    //}

    public class IssuerReceiver
    {
        public string rfc { get; set; }
        public string name { get; set; }
    }

    public class CustomersInfo
    {
        public string idInvoice { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string zipCode { get; set; }
        public int regime { get; set; }
        public decimal tax { get; set; }
    }

    public class ProvidersInfo
    {
        public string idInvoice { get; set; }
        public string rfc { get; set; }
        public string businessName { get; set; }
        public string zipCode { get; set; }
        public Int32 regime { get; set; }
        public decimal tax { get; set; }
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
        public List<Obligation> obligations { get; set; }
    }

    public class Obligation
    {
        public string dueDate { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public string description { get; set; }
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

    public class InvoicesCFDI
    {        
        public DateTime Fecha { get; set; }
        public string Folio { get; set; }
        public string FormaPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string MetodoPago { get; set; }
        public string Moneda { get; set; }
        public string Serie { get; set; }
        public decimal SubTotal { get; set; }
        public string TipoDeComprobante { get; set; }
        public decimal Total { get; set; }
        public decimal Version { get; set; }
        public string NoCertificado { get; set; }
        //public string Sello { get; set; }
        //public string Certificado { get; set; }
        public Emisor Emisor { get; set; }
        public Receptor Receptor { get; set; }
        public string Xml { get; set; }
        public string id { get; set; }
        public string urlXml { get; set; }
    }

    public class Emisor
    {
        public string Nombre { get; set; }
        public Int32 RegimenFiscal { get; set; }
        public string Rfc { get; set; }
    }

    public class Receptor
    {
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public string UsoCFDI { get; set; }
    }
    /*"Conceptos": {
        "Concepto": {
            "Cantidad": 1,
            "ClaveProdServ": 86121501,
            "ClaveUnidad": "E48",
            "Descripcion": "COLEGIATURA MES DE MARZO 2020. NOMBRE: ROSENDO NICOLAS CHAN PECH. SECCIÓN: PREESCOLAR. CURP: CAPR141113HTNHCSA3. RVOE: 031PJN0292J.",
            "Importe": 1960,
            "Unidad": "Servicios",
            "ValorUnitario": 1960,
            "Impuestos": {
                "Traslados": {
                    "Traslado": {
                        "Base": 1960,
                        "Impuesto": "002",
                        "TipoFactor": "Exento"
                    }
                }
            },
            "ComplementoConcepto": {
                "InstEducativas": {
                    "CURP": "CAPR141113HTNHCSA3",
                    "AutRVOE": "031PJN0292J",
                    "NivelEducativo": "Preescolar",
                    "NombreAlumno": "ROSENDO NICOLAS CHAN PECH",
                    "RfcPago": "PEMY860416PR3",
                    "Version": 1
                }
            }
        }
    },
    "Complemento": {
        "TimbreFiscalDigital": {
            "Version": 1.1,
            "FechaTimbrado": "2020-03-31T12:13:50",
            "SelloCFD": "d0Qs2k/HQNk9Ak7wTJgedl7ksjDkF/JZ5er4b3hzpTVwol7NPKxF9TBemBINcIxWgYj7THr9h2A5ZfL+bgU/H3OMDbMyoQEHoxrYLqIf4apTjmkVasgYVO3soMyzOM+9kf6aKv8poxDhXGcwjOFfXr2vXl23B+Kej4DgepevSgmd9ZAkAiYM5UAoYhpWK/1kLzfp7r6IERDYlJ3kKZubpElsBbZg3KuT5ESVm7dlGHgkFFd1VAnbSuJMVTbA6HOPCMR06z9KhYsPJRXK+RYeKa3kdPr6NVh5cyKTtXmVyFYSVhGL4TbggvppWnS02ISvHoMZ4BvRP1f3kQ5R53mScg==",
            "UUID": "8CE08C2E-6113-4C4A-890E-6459DF90A337",
            "NoCertificadoSAT": "00001000000402636111",
            "RfcProvCertif": "SAD110722MQA",
            "SelloSAT": "kDsTsRRh6pR6ywGeUD4bp1VA5LxZ/qqUluaR5t07PzzZDNFnn+pb5dvs0QN7xy/pX0LIT1yhD2adOV6umTMzCxh1DhG2ofJZYsQx17kEtPoAGUC1nTrTjVQiwNIEohSQlQh7jBZ6vE9rdhsuqzS3eIBzxHyqy4sUK1RbabPNgsIKX2UuIs+9fu4AX/2kTQUZou++TyozfU8O6ekGHakvdYvqp1LvQOYe1xbYELgyppgOAaTeMRhVlfovCwdkTTJl5DwduFRxKylzY2Z3/xeJhp/fNQ2KkTMthrtaCczrrI/esqK7MXsx1koUTJMxwx/gUP/XWOxL2KmmgP3NEW2ryA=="
        }
    }*/

    public class WebhookEventModel
    {
        public string id { get; set; }
        public string type { get; set; }

        public WebhookData data { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class WebhookData
    {
        public WebhookObject @object { get; set; }
    }

    public class WebhookObject
    {
        public string id { get; set; }
        public string status { get; set; }
        public WebhookTaxpayer taxpayer { get; set; }
        public WebhookOptions options { get; set; }
        public string rfc { get; set; }
        public string type { get; set; }
        public string extractor { get; set; }
    }

    public class WebhookTaxpayer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string personType { get; set; }
        public string registrationDate { get; set; }
    }

    public class WebhookOptions
    {
        public WebhookPeriod period { get; set; }
    }

    public class WebhookPeriod
    {
        public string to { get; set; }
        public string from { get; set; }
    }

    public class InvoiceException : Exception
    {
        public InvoiceResponse invoiceResponse { get; set; }

        public InvoiceException(InvoiceResponse invoiceResponse)
        {
            this.invoiceResponse = invoiceResponse;
        }
    }

    public class InvoiceResponse
    {
        [JsonProperty("context")]
        public string context { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("violations")]
        public List<Violations> violations { get; set; }

    }

    public class Violations
    {
        [JsonProperty("propertyPath")]
        public string propertyPath { get; set; }
        [JsonProperty("message")]
        public string message { get; set; }
    }
}
