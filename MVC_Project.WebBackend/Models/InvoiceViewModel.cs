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
        [Required]
        [Display(Name = "Tipo de Factura")]
        public string TypeInvoice { get; set; }
        public SelectList ListTypeInvoices { get; set; }

        [Display(Name = "Tipo de Factura")]
        public string TypeRelationship { get; set; }
        public SelectList ListTypeRelationship { get; set; }

        [Display(Name = "Sucursal")]
        public string BranchOffice { get; set; }
        public SelectList ListBranchOffice { get; set; }

        [Display(Name = "E-mail")]
        public Int64 EmailIssuedId { get; set; }
        public SelectList ListEmailIssued { get; set; }
        #endregion

        #region Información Receptor de Facturación
        [Display(Name = "Customer")]
        public string CustomerName { get; set; }
        public Int64 CustomerId { get; set; }
        //public SelectList ListEmails { get; set; }

        [Display(Name = "RFC Cliente")]
        public string RFC { get; set; }
        public Int64 RFCId { get; set; }
        public SelectList ListRFC { get; set; }

        [Display(Name = "Ave./Calle")]
        public string Street { get; set; }
        public Int64 StreetId { get; set; }
        public SelectList ListStreet { get; set; }

        [Display(Name = "No. Ext.")]
        public string OutdoorNumber { get; set; }
        public Int64 OutdoorNumberId { get; set; }
        public SelectList ListOutdoorNumber { get; set; }

        [Display(Name = "No. Int.")]
        public string InteriorNumber { get; set; }
        public Int64 InteriorNumberId { get; set; }
        public SelectList ListInteriorNumber { get; set; }

        [Display(Name = "Colonia")]
        public Int64 Colony { get; set; }
        public SelectList ListColony { get; set; }

        [Required]
        [Display(Name = "C.P.")]
        public string ZipCode { get; set; }

        [Display(Name = "Alc./Mpo")]
        public Int64 Municipality { get; set; }
        public SelectList ListMunicipality { get; set; }

        [Display(Name = "Estado")]
        public Int64? State { get; set; }
        public SelectList ListState { get; set; }

        [Display(Name = "País")]
        public Int64? Country { get; set; }
        public SelectList ListCountry { get; set; }

        [Display(Name = "E-mail")]
        public Int64 CustomerEmailId { get; set; }
        public SelectList ListCustomerEmail { get; set; }
        #endregion

        #region Datos Fiscales para Facturar
        [Required]
        [Display(Name = "Tipo Comprobante")]
        public string TypeVoucherId { get; set; }
        public SelectList ListTypeVoucher { get; set; }

        [Display(Name = "Serie y Folio")]
        public string SerieFolio { get; set; }
        public SelectList ListSerieFolio { get; set; }

        [Display(Name = "Uso de CFDI")]
        public string UseCFDI { get; set; }
        public SelectList ListUseCFDI { get; set; }

        [Display(Name = "Forma de Pago")]
        public string PaymentForm { get; set; }
        public SelectList ListPaymentForm { get; set; }

        [Display(Name = "Método de Pago")]
        public string PaymentMethod { get; set; }
        public SelectList ListPaymentMethod { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }
        public SelectList ListCurrency { get; set; }

        [Display(Name = "Tipo de Cambio")]
        public string ExchangeRate { get; set; }
        public SelectList ListExchangeRate { get; set; }

        [Display(Name = "Patente Aduanal")]
        public string CustomsPatent { get; set; }
        public SelectList ListCustomsPatent { get; set; }

        [Display(Name = "Aduana")]
        public string Customs { get; set; }
        public SelectList ListCustoms { get; set; }

        [Display(Name = "Núm. Pedimento")]
        public string MotionNumber { get; set; }
        public SelectList ListMotionNumber { get; set; }
        #endregion

        #region Condiciones y Comentarios a Facturar:
        [Display(Name = "Núm. Pedimento")]
        public string PaymentConditions { get; set; }
        public SelectList ListPaymentConditions { get; set; }

        [Display(Name = "% Descuento")]
        public decimal DiscountRate { get; set; }

        [Display(Name = "Comentarios")]
        public string Comments { get; set; }
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
    }

    public class ProductServiceDescriptionView
    {
        [Display(Name = "#")]
        public int NumberLine { get; set; }

        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }

        [Display(Name = "Código SAT")]
        public string SATCode { get; set; }

        [Display(Name = "Descripción Producto o Servicio")]
        public string ProductServiceDescription { get; set; }

        [Display(Name = "Unidad SAT")]
        public string SATUnit { get; set; }

        [Display(Name = "Precio Unitario")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "% Descuento")]
        public decimal DiscountRate { get; set; }

        [Display(Name = "Impuesto (IVA/IEPS)")]
        public decimal Taxes { get; set; }

        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }   
    }
}