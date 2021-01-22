using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class ChargeClientFilterViewModel
    {
        [Display(Name = "Razón Social / Nombre")]
        public string BusinessName { get; set; }
        [Display(Name = "RFC")]
        public string Rfc { get; set; }
        [Display(Name = "Estatus")]
        public string Status { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        [Display(Name = "Contacto")]
        public string AccountOwner { get; set; }
    }

    public class ChargeListViewModel
    {
        public int id { get; set; }
        public string uuid { get; set; }
        public string businessName { get; set; }
        public string rfc { get; set; }
        public string billingStart { get; set; }
        public string plan { get; set; }
        public string status { get; set; }
        public string accountOwner { get; set; }
    }

    public class ChargeClientEditViewModel
    {
        public string Uuid { get; set; }

        [Display(Name = "Razón Social / Nombre")]
        public string Name { get; set; }

        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "Inicio de operaciones")]
        public DateTime? BillingStart { get; set; }

        [Display(Name = "Estatus")]
        [Required]
        public string Status { get; set; }

        [Display(Name = "Plan fijo")]
        public string Plan { get; set; }

        public List<SelectListItem> PlanList { get; set; }

        [Display(Name = "Contacto")]
        public string AccountOwner { get; set; }

        public List<SelectListItem> StatusList { get; set; }
    }
}