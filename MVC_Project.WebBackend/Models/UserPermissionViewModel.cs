using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class UserPermissionViewModel
    {
        public string Uuid { get; set; }
        public string Nombre { get; set; }
        public int? Status { get; set; }
        public SelectList ListStatus { get; set; }
    }
    public class UserPermissionEditViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public IEnumerable<PermissionViewModel> Permissions { get; set; }
    }
    public class UserPermissionData
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public string Uuid { get; set; }

        public string Email { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Fecha de actualización")]
        public DateTime UpdatedAt { get; set; }
        [Display(Name = "Estatus")]
        public bool Status { get; set; }
    }
}