using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.CustomAttributes.Validations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class StringComparerAttribute : ValidationAttribute, IClientValidatable
    {
        public StringComparerAttribute(string propertyName, string restriccion = "=")
        {
            this.PropertyName = propertyName;
            this.Restriccion = restriccion;
        }
        public string Restriccion;
        public string PropertyName { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(PropertyName);
            string propertyValue = Convert.ToString(property.GetValue(validationContext.ObjectInstance, null));
            string valueAsString = Convert.ToString(value);
            if ("=".Equals(Restriccion))
            {
                if (propertyValue.Equals(valueAsString))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
            if ("!=".Equals(Restriccion))
            {
                if (!propertyValue.Equals(valueAsString))
                {
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                }
                return ValidationResult.Success;
            }
            throw new Exception("no se reconoce ninguno de los operadores de igualdad.");
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var modelClientValidationRule = new ModelClientValidationRule
            {
                ValidationType = "stringcomparer",
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName())
            };
            modelClientValidationRule.ValidationParameters.Add("restriction", this.Restriccion);
            modelClientValidationRule.ValidationParameters.Add("property", this.PropertyName);

            yield return modelClientValidationRule;
        }
    }
}