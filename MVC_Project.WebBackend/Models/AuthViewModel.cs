﻿using MVC_Project.WebBackend.CustomAttributes.Validations;
using System.ComponentModel.DataAnnotations;
using MVC_Project.Resources;
using System;

namespace MVC_Project.BackendWeb.Models
{
    public class AuthViewModel
    {
        [Display(Name = "USERNAME", ResourceType = typeof(ViewLabels))]
        [Required(ErrorMessageResourceType = typeof(ViewLabels), ErrorMessageResourceName = "UsernameRequired"), EmailAddress]
        public string Email { get; set; }

        [Display(Name = "PASSWORD", ResourceType = typeof(ViewLabels))]
        [Required(ErrorMessageResourceType = typeof(ViewLabels), ErrorMessageResourceName = "PasswordRequired"), MinLength(8)]
        public string Password { get; set; }

        public bool RedSocial { get; set; }
        public string TypeRedSocial { get; set; }
        public string SocialId { get; set; }
    }
    public class RecoverPasswordViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico ")]
        [EmailAddress]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,6})$", ErrorMessage = "El Email no es válido")]
        public string Email { get; set; }
    }
    public class ResetPassword
    {
        public Guid Uuid { get; set; }
        [Display(Name = "Nueva contraseña")]
        [PasswordSecured(ErrorMessage = "La nueva contraseña debe contener al menos un número, mayúsculas y minúsculas")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Campo obligatorio"), MinLength(8, ErrorMessage = "{0} debe ser mínimo de {1} caracteres")]
        public string Password { get; set; }
        [Display(Name = "Confirmar nueva contraseña")]
        [DataType(DataType.Password)]
        [StringComparer("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Required(ErrorMessage = "Campo obligatorio"), MinLength(8, ErrorMessage = "{0} debe ser mínimo de {1} caracteres")]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Display(Name = "Nueva contraseña")]
        [PasswordSecured(ErrorMessage = "La nueva contraseña debe contener al menos un número, mayúsculas y minúsculas")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Campo obligatorio"), MinLength(8, ErrorMessage = "{0} debe ser mínimo de {1} caracteres")]
        public string Password { get; set; }

        [Display(Name = "Confirmar nueva contraseña")]
        [DataType(DataType.Password)]
        [StringComparer("Password",ErrorMessage = "Las contraseñas no coinciden")]
        [Required(ErrorMessage = "Campo obligatorio"), MinLength(8, ErrorMessage = "{0} debe ser mínimo de {1} caracteres")]
        public string ConfirmPassword { get; set; }

        public Guid Uuid { get; set; }

        [Display(Name="Nombre")]
        public string Name { get; set; }

        public Guid AcccountUuid { get; set; }

        public bool AgreeTerms { get; set; }
        public bool isBackOffice { get; set; }
    }

    //public class AuthLogin
    //{
    //    public string username { get; set; }
    //    public string password { get; set; }
    //    public string socialNetwork { get; set; }
    //    public string urlImage { get; set; }
    //}
}