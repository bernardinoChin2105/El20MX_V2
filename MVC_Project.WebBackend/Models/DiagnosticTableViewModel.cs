using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class DiagnosticTableViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime RegisterAt { get; set; }

        public string FilterInitialDate { get; set; }
        public string FilterEndDate { get; set; }

        [Display(Name = "Nombre/Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "RFC")]
        public string RFC { get; set; }

        [Display(Name = "Plan")]
        public string Plan { get; set; }

        [Display(Name = "Usuario(Email")]
        public string Email { get; set; }

        [Display(Name = "CAD Comercial")]
        public string commercialCAD { get; set; }
    }
}