using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class CustomerViewModel
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
}