using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string FistName { get; set; }


        [Required]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }


        [Required]
        [Display(Name = "Correo electrónico ")]
        [EmailAddress]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,6})$", ErrorMessage = "El Email no es válido")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}