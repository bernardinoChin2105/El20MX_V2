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
        public int Id { get; set; }

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

        [Display(Name = "Calle y Cruzamientos")]
        public string Street { get; set; }

        [Display(Name = "Número Exterior")]
        public string OutdoorNumber { get; set; }

        [Display(Name = "Número Interior")]
        public string InteriorNumber { get; set; }

        [Display(Name = "Colonia")]
        public int? Colony { get; set; }
        public SelectList ListColony { get; set; }

        [Display(Name = "C.P.")]
        public string ZipCode { get; set; }

        [Display(Name = "Alcaldía/Municipio")]
        public int? Municipality { get; set; }
        public SelectList ListMunicipality { get; set; }

        [Display(Name = "Estado")]
        public int? State { get; set; }
        public SelectList ListState { get; set; }

        //[Display(Name = "Nombre/Razón Social")]
        public bool DeliveryAddress { get; set; }

        public List<CustomerContact> Emails { get; set; }
        public List<CustomerContact> Phones { get; set; }
    }

    public class CustomerContact
    {
        public Int64 Id { get; set; }
        public string TypeContact { get; set; }
        public string EmailOrPhone { get; set; }
    }
}