using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Utils
{
    public enum DocumentType
    {
        [Display(Name = "Curriculum Vitae")]
        CV,
        [Display(Name = "Contrato")]
        CONTRATO,
        [Display(Name = "Identificación")]
        IDENTIFICACION,
        [Display(Name = "Certificación")]
        CERTIFICACION,
        [Display(Name = "Otro")]
        OTRO
    }

    public enum RequestType
    {
        DOCUMENT
    }

    public enum DaysOfWeek
    {
        [Display(Name = "Domingo")]
        SUNDAY = 0,
        [Display(Name = "Lunes")]
        MONDAY = 1,
        [Display(Name = "Martes")]
        TUESDAY = 2,
        [Display(Name = "Miércoles")]
        WEDNESDAY = 3,
        [Display(Name = "Jueves")]
        THURSDAY = 4,
        [Display(Name = "Viernes")]
        FRIDAY = 5,
        [Display(Name = "Sábado")]
        SATURDAY = 6
    }

    public enum NotificationsInterval
    {
        WEEKLY,
        EVERYTWODAYS
    }

    public static class Enums
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
