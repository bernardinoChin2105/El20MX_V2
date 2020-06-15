using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{

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
        [Display(Name = "Código")]
        public string Code { get; set; }
        public List<ModuleViewModel> Modules { get; set; }
        //public List<FeatureViewModel> Features { get; set; }
    }
    public class RoleEditViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        [Display(Name = "Código")]
        public string Code { get; set; }
        public List<ModuleViewModel> Modules { get; set; }
        //public IEnumerable<PermissionViewModel> Permissions { get; set; }
    }

    public class PermissionViewModel {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public int SystemAction { get; set; }

        public List<SelectListItem> SystemActions { get; set; }
    }

    public class ModuleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Módulo")]
        public string Name { get; set; }

        [Display(Name = "Permission")]
        public List<PermissionViewModel> Permissions { get; set; }
    }
}