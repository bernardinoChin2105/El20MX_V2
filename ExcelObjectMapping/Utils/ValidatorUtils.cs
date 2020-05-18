using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    //<summary>Clase utilizada para validar segun las DataAnnotation de los modelos</summary>
    public class ValidatorUtil
    {
        //<summary>Lista de errores encontradas en la validación</summary>
        public static IList<ValidationResult> Errors { get; private set; }
        //<summary>Si hay errores en modelo segun los datos enviados.</summary>
        /// <<returns>boolean true si encontro errores en el modelo, false si no encontro errores.</returns>
        public bool HasError
        {
            get { return Errors.Count > 0; }
        }
        //<summary>Obtencion de los mensajes de error resultado de la validación.</summary>
        /// <returns>Lista de mensajes de errores.</returns>
        public IList<string> GetErrorsMessages()
        {
            IList<string> errorsMessages = new List<string>();
            foreach (ValidationResult validation in Errors)
            {
                errorsMessages.Add(validation.ErrorMessage);
            }
            return errorsMessages;
        }

        //<summary>Valida todos los atributos del modelo o entidad que se le indique.</summary>
        /// <param name="entity">Objeto a validar</param>
        /// <returns>boolean true si es valido el objeto, false si no es valido el objeto.</returns>
        public bool ValidateAllAttributes<T>(T entity)
        {
            return Validate(entity, true);
        }

        //<summary>Valida los atributos del modelo o entidad que se le indique.</summary>
        /// <param name="entity">Objeto a validar</param>
        /// <param name="validateAll">true si se requiere validar todos los atributos, false si no se requiere la validación.</param>
        /// <returns>boolean true si es valido el objeto, false si no es valido el objeto.</returns>
        public bool Validate<T>(T entity, bool validateAll)
        {
            Errors = new List<ValidationResult>();
            ValidationContext vc = new ValidationContext(entity, null, null);
            return Validator.TryValidateObject(entity, vc, Errors, validateAll);
        }
        public static bool ValidateValue(object value, string memberName, IEnumerable<ValidationAttribute> validationAttributes, out IList<ValidationResult> errors)
        {
            errors = null;
            string valueToEvaluate = Convert.ToString(value);
            IList<ValidationResult> validationResults = new List<ValidationResult>();
            ValidationContext vc = new ValidationContext(valueToEvaluate, null, null);
            vc.MemberName =  memberName;
            bool isValid = Validator.TryValidateValue(valueToEvaluate, vc, validationResults, validationAttributes);
            if (!isValid)
            {
                errors = validationResults;
            }
            return isValid;
        }
    }
}
