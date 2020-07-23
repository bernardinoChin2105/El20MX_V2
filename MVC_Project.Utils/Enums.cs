using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Utils
{
    public enum SystemStatus
    {
        [Display(Name = "ACTIVE")]
        ACTIVE,
        [Display(Name = "INACTIVE")]
        INACTIVE,
        [Display(Name = "UNCONFIRMED")]
        UNCONFIRMED
    }

    public enum SystemLevelPermission
    {
        [Display(Name = "No access")]
        NO_ACCESS,
        [Display(Name = "Full access")]
        FULL_ACCESS,
        [Display(Name = "Read only")]
        READ_ONLY
    }

    public enum SystemRoles
    {
        [Display(Name = "Owner")]
        ACCOUNT_OWNER,
        [Display(Name = "Lead")]
        LEAD
    }

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

    public enum SystemModules
    {
        [Display(Name = "Configuración")]
        CONFIGURATION,
        [Display(Name = "Prospectos")]
        PROSPECTS,
        [Display(Name = "Clientes")]
        CUSTOMERS,
        [Display(Name = "Proveedores")]
        PROVIDERS
    }

    public enum TermsAndConditions
    {
        [Display(Name = "Accept")]
        ACCEPT,
        [Display(Name = "Decline")]
        DECLINE
    }

    public enum SystemActions
    {
        [Display(Name = "Index")]
        INDEX,
        [Display(Name = "Create")]
        CREATE,
        [Display(Name = "Edit")]
        EDIT
    }

    public enum TypeIssuerReceiver
    {
        [Display(Name = "Issuer")]
        ISSUER,
        [Display(Name = "Receiver")]
        RECEIVER,
    }

    public enum TypeContact
    {
        [Display(Name = "Email")]
        EMAIL,
        [Display(Name = "Phone")]
        PHONE,
    }

    public enum TypeTaxRegimen
    {
        [Display(Name = "Persona Física")]
        [Description("Persona Física")]
        MORALPERSONSREGIME,
        [Display(Name = "Persona Moral")]
        [Description("Persona Moral")]
        NATURALPERSONSREGIME,
    }

    public enum SystemProviders
    {
        [Display(Name = "Syncfy")]
        SYNCFY,
        [Display(Name = "SAT.ws")]
        SATWS,
        [Display(Name = "Finerio")]
        FINERIO
    }

    //public enum MoralPersonsRegime
    //{
    //    Regime601 = 601,
    //    Regime603 = 603,
    //    Regime607 = 607,
    //    Regime609 = 609,
    //    Regime620 = 620,
    //    Regime622 = 622,
    //    Regime623 = 623,
    //    Regime624 = 624,
    //    Regime628 = 628,
    //}

    //public enum NaturalPersonsRegime
    //{
    //    Regime605 = 605,
    //    Regime606 = 606,
    //    Regime608 = 608,
    //    Regime611 = 611,
    //    Regime612 = 612,
    //    Regime614 = 614,
    //    Regime615 = 615,
    //    Regime616 = 616,
    //    Regime621 = 621,
    //    Regime622 = 622,
    //    Regime629 = 626,
    //    Regime630 = 630,
    //}
}
