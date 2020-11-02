using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class InvoiceViewModel
    {
        #region Información Emisor de Factura
        [Display(Name = "Logo Empresa")]
        public string Logo { get; set; }

        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "RFC")]
        public string IssuingRFC { get; set; }

        [Display(Name = "Régimen Fiscal")]
        public string IssuingTaxRegime { get; set; }
        public string IssuingTaxRegimeId { get; set; }
        public List<SelectListItem> ListTaxRegime { get; set; }

        [Display(Name = "Núm. Cuenta Predial")]
        public string PropertyAccountNumber { get; set; }

        [Required]
        [Display(Name = "Tipo de Factura")]
        public string TypeInvoice { get; set; }
        public List<SelectListItem> ListTypeInvoices { get; set; }

        [Display(Name = "Tipo de Relación")]
        public string TypeRelationship { get; set; }
        public List<SelectListItem> ListTypeRelationship { get; set; }

        [Display(Name = "Sucursal")]
        public string BranchOffice { get; set; }
        public List<SelectListItem> ListBranchOffice { get; set; }

        [Display(Name = "E-mail")]
        public Int64 EmailIssuedId { get; set; }
        public List<SelectListItem> ListEmailIssued { get; set; }
        public string IssuingTaxEmail { get; set; }

        [Display(Name = "CFDI Relacionado")]
        public bool InvoiceComplementChk { get; set; }

        [Display(Name = "Factura relacionada")]
        public string InvoiceComplement { get; set; }

        //[Display(Name = "Factura relacionada")]
        public List<string> InvoicesCFDI { get; set; }
        #endregion

        #region Información Receptor de Facturación
        [Display(Name = "Cliente")]
        public string CustomerName { get; set; }
        public Int64 CustomerId { get; set; }
        public string TypeReceptor { get; set; }

        [Display(Name = "RFC Cliente")]
        public string RFC { get; set; }
        public string ReceiverType { get; set; }

        [Display(Name = "Ave./Calle")]
        public string Street { get; set; }

        [Display(Name = "No. Ext.")]
        public string OutdoorNumber { get; set; }

        [Display(Name = "No. Int.")]
        public string InteriorNumber { get; set; }

        [Display(Name = "Colonia")]
        public Int64? Colony { get; set; }
        public List<SelectListItem> ListColony { get; set; }

        //[Required]
        [Display(Name = "C.P.")]
        public string ZipCode { get; set; }

        [Display(Name = "Alc./Mpo")]
        public Int64? Municipality { get; set; }
        public List<SelectListItem> ListMunicipality { get; set; }

        [Display(Name = "Estado")]
        public Int64? State { get; set; }
        public List<SelectListItem> ListState { get; set; }

        [Display(Name = "País")]
        public Int64? Country { get; set; }
        public List<SelectListItem> ListCountry { get; set; }

        [Display(Name = "E-mail")]
        public string CustomerEmail { get; set; }
        public Int64 CustomerEmailId { get; set; }
        public List<SelectListItem> ListCustomerEmail { get; set; }
        #endregion

        #region Datos Fiscales para Facturar
        //[Required]
        //[Display(Name = "Tipo Comprobante")]
        //public string TypeVoucherId { get; set; }
        //public List<SelectListItem> ListTypeVoucher { get; set; }

        [Display(Name = "Serie y Folio")]
        public string SerieFolio { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }

        [Display(Name = "Uso de CFDI")]
        public string UseCFDI { get; set; }
        public List<SelectListItem> ListUseCFDI { get; set; }

        [Display(Name = "Forma de Pago")]
        public string PaymentForm { get; set; }
        public List<SelectListItem> ListPaymentForm { get; set; }

        [Display(Name = "Método de Pago")]
        public string PaymentMethod { get; set; }
        public List<SelectListItem> ListPaymentMethod { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }
        public List<SelectListItem> ListCurrency { get; set; }

        [Display(Name = "Tipo de Cambio")]
        public string ExchangeRate { get; set; }
        //public List<SelectListItem> ListExchangeRate { get; set; }

        [Display(Name = "Patente Aduanal")]
        public string CustomsPatent { get; set; }
        public List<SelectListItem> ListCustomsPatent { get; set; }

        [Display(Name = "Aduana")]
        public string Customs { get; set; }
        public List<SelectListItem> ListCustoms { get; set; }

        [Display(Name = "Núm. Pedimento")]
        public string MotionNumber { get; set; }
        public List<SelectListItem> ListMotionNumber { get; set; }
        #endregion

        #region Condiciones y Comentarios a Facturar:
        [Display(Name = "Condiciones de Pago")]
        public string PaymentConditions { get; set; }
        //public SelectList ListPaymentConditions { get; set; }

        [Display(Name = "% Descuento")]
        public decimal DiscountRate { get; set; }

        [Display(Name = "Comentarios")]
        public string Comments { get; set; }

        [Display(Name = "Impuestos")]
        public bool TaxesChk { get; set; }

        [Display(Name = "Comercio Internacional")]
        public bool InternationalChk { get; set; }
        #endregion

        #region Impuestos - depende del check de impuestos
        [Display(Name = "Retenciones")]
        public string Withholdings { get; set; }
        public List<SelectListItem> ListWithholdings { get; set; }

        [Display(Name = "Trasladados")]
        public string Transferred { get; set; }
        public List<SelectListItem> ListTransferred { get; set; }

        [Display(Name = "Tasa")]
        public string Valuation { get; set; }
        public List<SelectListItem> ListValuation { get; set; }
        #endregion

        #region Productos y/o Servicios a facturar        
        public List<ProductServiceDescriptionView> ProductServices { get; set; }

        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Descuento Total")]
        public decimal TotalDiscount { get; set; }

        [Display(Name = "Impuesto Trasladado (IVA/IEPS)")]
        public decimal TaxTransferred { get; set; }

        [Display(Name = "Impuesto Retenido (IVA)")]
        public decimal TaxWithheldIVA { get; set; }

        [Display(Name = "Impuesto Retenido (ISR)")]
        public decimal TaxWithheldISR { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }
        #endregion

        public InvoiceViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Seleccionar...", Value = "-1" });

            ListTypeInvoices = new List<SelectListItem>();
            ListTypeRelationship = new List<SelectListItem>();
            ListBranchOffice = new List<SelectListItem>();
            ListUseCFDI = new List<SelectListItem>();
            ListPaymentForm = new List<SelectListItem>();
            ListPaymentMethod = new List<SelectListItem>();
            ListCurrency = new List<SelectListItem>();
            ListWithholdings = new List<SelectListItem>();
            ListTransferred = new List<SelectListItem>();
            ListValuation = new List<SelectListItem>();
            ListCustomsPatent = new List<SelectListItem>();
            ListCustoms = new List<SelectListItem>();
            ListMotionNumber = new List<SelectListItem>();
            //ListExchangeRate = new List<SelectListItem>();

            ListCountry = new List<SelectListItem>();
            ListEmailIssued = new List<SelectListItem>();
            ListColony = new List<SelectListItem>();
            ListMunicipality = new List<SelectListItem>();
            ListState = new List<SelectListItem>();
            ListCustomerEmail = new List<SelectListItem>();
            ProductServices = new List<ProductServiceDescriptionView>();


        }
    }

    public class ProductServiceDescriptionView
    {
        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }

        [Display(Name = "Código SAT")]
        public string SATCode { get; set; }

        [Display(Name = "Descripción Producto o Servicio")]
        public string ProductServiceDescription { get; set; }

        [Display(Name = "Unidad SAT")]
        public string SATUnit { get; set; }

        [Display(Name = "Unidad")]
        public string Unit { get; set; }

        [Display(Name = "Precio Unitario")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "% Descuento")]
        public decimal DiscountRateProServ { get; set; }

        [Display(Name = "Impuesto (IEPS)")]
        public decimal TaxesIEPS { get; set; }

        [Display(Name = "Impuesto (IVA)")]
        public decimal TaxesIVA { get; set; }

        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }
    }

    public class InvoiceViewModelJson //Objeto json para guardar en formato 
    {
        #region Información Emisor de Factura        
        public string Logo { get; set; }        
        public string BusinessName { get; set; }      
        public string IssuingRFC { get; set; }
        public string IssuingTaxRegime { get; set; }
        public string IssuingTaxRegimeId { get; set; }
        public string PropertyAccountNumber { get; set; }
        public string TypeInvoice { get; set; }
        public string TypeRelationship { get; set; }
        public string BranchOffice { get; set; }
        public Int64 EmailIssuedId { get; set; }
        public string IssuingTaxEmail { get; set; }
        #endregion

        #region Información Receptor de Facturación
        public string CustomerName { get; set; }
        public Int64 CustomerId { get; set; }
        public string RFC { get; set; }
        public Int64 RFCId { get; set; }
        public string Street { get; set; }
        public string OutdoorNumber { get; set; }
        public string InteriorNumber { get; set; }
        public Int64 Colony { get; set; }        
        public string ZipCode { get; set; }
        public Int64 Municipality { get; set; }
        public Int64? State { get; set; }
        public Int64? Country { get; set; }
        public Int64 CustomerEmailId { get; set; }
        #endregion

        #region Datos Fiscales para Facturar
        public string TypeVoucherId { get; set; }
        public string SerieFolio { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string UseCFDI { get; set; }
        public string PaymentForm { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }
        public string CustomsPatent { get; set; }
        public string Customs { get; set; }
        public string MotionNumber { get; set; }
        #endregion

        #region Condiciones y Comentarios a Facturar:
        public string PaymentConditions { get; set; }
        public decimal DiscountRate { get; set; }    
        public string Comments { get; set; }
        public bool TaxesChk { get; set; }
        #endregion

        #region Impuestos - depende del check de impuestos
        public string Withholdings { get; set; }
        public string Transferred { get; set; }
        public string Rate { get; set; }
        #endregion

        #region Productos y/o Servicios a facturar        
        public List<ProductServiceDescriptionView> ProductServices { get; set; }
        
        public decimal Subtotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TaxTransferred { get; set; }
        public decimal TaxWithheldIVA { get; set; }
        public decimal TaxWithheldISR { get; set; }
        public decimal Total { get; set; }
        #endregion
    }

    public class InvoicesSavedViewModel
    {
        [Required]
        [Display(Name = "Tipo de Factura")]
        public string TypeInvoice { get; set; }
        public List<SelectListItem> ListTypeInvoices { get; set; }

        [Display(Name = "Folio")]
        public string Folio { get; set; }

        [Display(Name = "Serie")]
        public string Serie { get; set; }
    }

    public class InvoicesSavedList
    {
        public Int64 id { get; set; }
        public string folio { get; set; }
        public string serie { get; set; }
        public string paymentMethod { get; set; }
        public string paymentForm { get; set; }
        public string currency { get; set; }
        public string iva { get; set; }
        public string invoicedAt { get; set; }
        public string invoiceType { get; set; }
        public string total { get; set; }
        public string subtotal { get; set; }
        public string xml { get; set; }
    }
}