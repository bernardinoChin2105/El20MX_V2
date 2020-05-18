using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.Web.Models {

    public class RoleData
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Fecha de actualización")]
        public DateTime UpdatedAt { get; set; }
        public string Uuid { get; set; }
        [Display(Name = "Estatus")]
        public bool Status { get; set; }
    }
    public class RoleViewModel
    {
        public string Name { get; set; }
        public RoleData RoleData { get; set; }
    }

    public class RoleCreateViewModel
    {
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public IEnumerable<PermissionViewModel> Permissions { get; set; }
    }
    public class RoleEditViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public IEnumerable<PermissionViewModel> Permissions { get; set; }
    }

    public class PermissionViewModel {
        public int Id { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        public bool Assigned { get; set; }
    }
}