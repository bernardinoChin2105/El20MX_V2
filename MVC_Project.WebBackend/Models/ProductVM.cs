using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class ProductVM
    {
        
    }

    public class PlansFilterViewModel
    {        
        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }
}