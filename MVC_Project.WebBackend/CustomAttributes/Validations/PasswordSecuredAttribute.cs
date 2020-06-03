using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.CustomAttributes.Validations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordSecuredAttribute : ValidationAttribute, IClientValidatable
    {
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var modelClientValidationRule = new ModelClientValidationRule
            {
                ValidationType = "passwordsecured",
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()) //Added
            };

            yield return modelClientValidationRule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Regex regex = new Regex(@"^(?=.*\d)(?=.*\W+)(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$");
            string valueString = Convert.ToString(value);
            Match match = regex.Match(valueString);
            if (match.Success)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            //return ValidationResult.Success;
        }
    }
}