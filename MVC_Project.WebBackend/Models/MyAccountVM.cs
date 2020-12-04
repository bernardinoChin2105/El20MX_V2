using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class MyAccountVM
    {
        [Display(Name = "Estatus")]
        public string Status { get; set; }        
        public List<SelectListItem> ListStatusPayment { get; set; }
        
        [Display(Name = "Fecha")]
        public DateTime? RegisterAt { get; set; }
        public string FilterInitialDate { get; set; }
        public string FilterEndDate { get; set; }

        public MyAccountVM()
        {
            ListStatusPayment = new List<SelectListItem>();            
        }
    }
}