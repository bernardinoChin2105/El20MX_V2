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
        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }
        [Display(Name = "RFC")]
        public string Rfc { get; set; }
        [Display(Name = "Estado")]
        public int Status { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
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
    }

    public class ChargeClientEditViewModel
    {
        public string Uuid { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "Inicio de Facturación")]
        public DateTime? BillingStart { get; set; }

        [Display(Name = "Estatus en el sistema")]
        public bool Status { get; set; }

        [Display(Name = "Plan fijo")]
        public string Plan { get; set; }

        public List<SelectListItem> PlanList { get; set; }
    }
}