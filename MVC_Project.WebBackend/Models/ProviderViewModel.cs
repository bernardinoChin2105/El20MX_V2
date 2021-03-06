﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class ProviderFilterViewModel
    {
        public int Id { get; set; }

        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "Nombre/Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
     
        [Display(Name = "Teléfono")]
        public string Phone { get; set; }
    }

    public class ProviderViewModel
    {
        public Int64 Id { get; set; }

        //[Required]
        [Display(Name = "Nombre(s)")]
        public string FistName { get; set; }

        //[Required]
        [Display(Name = "Apellido(s)")]
        public string LastName { get; set; }

        //[Required]
        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "CURP")]
        public string CURP { get; set; }

        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Tipo Régimen Fiscal")]
        public string taxRegime { get; set; }
        public List<SelectListItem> ListRegimen { get; set; }

        [Display(Name = "Calle y Cruzamientos")]
        public string Street { get; set; }

        [Display(Name = "Número Exterior")]
        public string OutdoorNumber { get; set; }

        [Display(Name = "Número Interior")]
        public string InteriorNumber { get; set; }

        [Display(Name = "Colonia")]
        public Int64? Colony { get; set; }
        public List<SelectListItem> ListColony { get; set; }

        //[Required]
        [Display(Name = "C.P.")]
        public string ZipCode { get; set; }

        [Display(Name = "Alcaldía/Municipio")]
        public Int64? Municipality { get; set; }
        public List<SelectListItem> ListMunicipality { get; set; }

        [Display(Name = "Estado")]
        public Int64? State { get; set; }
        public List<SelectListItem> ListState { get; set; }

        [Display(Name = "País")]
        public Int64? Country { get; set; }
        public List<SelectListItem> ListCountry { get; set; }
        
        public bool DeliveryAddress { get; set; }

        public List<ProviderContactsViewModel> Emails { get; set; }
        public List<ProviderContactsViewModel> Phones { get; set; }
        public string dataContacts { get; set; }
        public string indexPhone { get; set; }
        public string indexEmail { get; set; }

        public ProviderViewModel()
        {
            ListColony = new List<SelectListItem>();
            ListState = new List<SelectListItem>();
            ListCountry = new List<SelectListItem>();
            ListMunicipality = new List<SelectListItem>();
            ListRegimen = new List<SelectListItem>();
            Emails = new List<ProviderContactsViewModel>();
            Phones = new List<ProviderContactsViewModel>();
        }
    }

    public class ProviderContactsViewModel
    {
        public Int64 Id { get; set; }
        public string TypeContact { get; set; }
        public string EmailOrPhone { get; set; }
    }

    public class InvoicesReceivedListVM
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
        public string type { get; set; }
        public bool hasXML { get; set; }
    }

    public class DataCFDI
    {

    }
}