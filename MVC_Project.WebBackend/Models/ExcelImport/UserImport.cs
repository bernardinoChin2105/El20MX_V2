using ExcelEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.Web.Models.ExcelImport
{
    public class UserImport
    {
        [ExcelColumn(Letter = "A")]
        [Required]
        public int EmployeeNumber { get; set; }
        [ExcelColumn(Letter = "B")]
        [Required]
        public string Name { get; set; }
        [ExcelColumn(Letter = "C")]
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}