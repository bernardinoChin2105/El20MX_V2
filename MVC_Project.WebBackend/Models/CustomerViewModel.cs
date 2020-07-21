﻿using System;
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
            ListMunicipality = new SelectList(list);
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
}