using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class AllianceFilterViewModel
    {
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Aliado")]
        public string AllyName { get; set; }
    }

    public class AlliancesListVM
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string allyName { get; set; }
        public decimal allyCommisionPercent { get; set; }
        public decimal customerDiscountPercent { get; set; }
        public string createdAt { get; set; }
        public string status { get; set; }
    }

    public class AllyFilterViewModel
    {        
        public Int64 Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }

    public class AlliesListVM
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string status { get; set; }
    }
}