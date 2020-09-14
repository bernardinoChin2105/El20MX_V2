using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class CustomerFilterViewModel
    {
        public int Id { get; set; }

        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "Nombre/Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
     
        [Display(Name = "Celular")]
        public string CellPhone { get; set; }
    }

    public class CustomerViewModel
    {
        public Int64 Id { get; set; }

        [Required]
        [Display(Name = "Nombre(s)")]
        public string FistName { get; set; }

        [Required]
        [Display(Name = "Apellido(s)")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "CURP")]
        public string CURP { get; set; }

        [Display(Name = "Nombre/Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Tipo Régimen Fiscal")]
        public string taxRegime { get; set; }
        public SelectList ListRegimen { get; set; }

        [Display(Name = "Calle y Cruzamientos")]
        public string Street { get; set; }

        [Display(Name = "Número Exterior")]
        public string OutdoorNumber { get; set; }

        [Display(Name = "Número Interior")]
        public string InteriorNumber { get; set; }

        [Display(Name = "Colonia")]
        public Int64? Colony { get; set; }
        public SelectList ListColony { get; set; }

        [Required]
        [Display(Name = "C.P.")]
        public string ZipCode { get; set; }

        [Display(Name = "Alcaldía/Municipio")]
        public Int64? Municipality { get; set; }
        public SelectList ListMunicipality { get; set; }

        [Display(Name = "Estado")]
        public Int64? State { get; set; }
        public SelectList ListState { get; set; }

        [Display(Name = "País")]
        public Int64? Country { get; set; }
        public SelectList ListCountry { get; set; }

        //[Display(Name = "Nombre/Razón Social")]
        public bool DeliveryAddress { get; set; }

        public List<CustomerContactsViewModel> Emails { get; set; }
        public List<CustomerContactsViewModel> Phones { get; set; }
        public string dataContacts { get; set; }
        public string indexPhone { get; set; }
        public string indexEmail { get; set; }

        public CustomerViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Seleccione...", Value = "-1" });

            ListColony = new SelectList(list);
            ListState = new SelectList(list);
            ListCountry = new SelectList(list);
            ListMunicipality = new SelectList(list);
            Emails = new List<CustomerContactsViewModel>();
            Phones = new List<CustomerContactsViewModel>();
        }
    }

    public class CustomerContactsViewModel
    {
        public Int64 Id { get; set; }
        public string TypeContact { get; set; }
        public string EmailOrPhone { get; set; }
    }

    public class Duplicates
    {
        public string RFCS { get; set; }
        public int Repetitions { get; set; }
    }

    public class InvoicesFilter
    {
        [Display(Name = "Serie")]
        public string Serie { get; set; }

        [Display(Name = "Folio")]
        public string Folio { get; set; }

        [Display(Name = @"Nombre\Razón Social")]
        public string NombreRazonSocial { get; set; }

        [Display(Name = "RFC Cliente")]
        public string RFC { get; set; }

        [Display(Name = "RFC Proveedor")]
        public string RFCP { get; set; }

        [Display(Name = "Método Pago")]
        public string PaymentMethod { get; set; }
        public SelectList ListPaymentMethod { get; set; }

        [Display(Name = "Forma Pago")]
        public string PaymentForm  { get; set; }
        public SelectList ListPaymentForm { get; set; }

        [Display(Name = "Divisa")]
        public string Currency { get; set; }
        public SelectList ListCurrency { get; set; }

        [Display(Name = "Fecha")]
        public string RegisterAt { get; set; }

        public string FilterInitialDate { get; set; }
        public string FilterEndDate { get; set; }

        public InvoicesFilter()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos...", Value = "-1" });

            ListPaymentMethod = new SelectList(list);
            ListPaymentForm = new SelectList(list);
            ListCurrency = new SelectList(list);
        }
    }

    public class InvoicesIssuedListVM
    {
        public Int64 id { get; set; }
        //public Guid uuid { get; set; }
        public string folio { get; set; }
        public string serie { get; set; }
        public string paymentMethod { get; set; }
        public string paymentForm { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string iva { get; set; }
        public string totalAmount { get; set; }
        public string invoicedAt { get; set; }
        public string rfc { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string paymentFormDescription { get; set; }
        public string businessName { get; set; }
        public string xml { get; set; }
    }
}