using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class AllianceFilterViewModel
    {
    //    [Display(Name = "Nombre")]
    //    public string Name { get; set; }
    }

    public class AllyFilterViewModel
    {        
        public Int64 Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }
}