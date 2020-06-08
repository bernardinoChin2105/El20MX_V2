using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class FeatureViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Módulo")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        public List<PermissionViewModel>Permissions { get; set; }
    }
}