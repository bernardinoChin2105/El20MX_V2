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
        [Display(Name = "Activo")]
        ACTIVE,
        [Display(Name = "Inactivo")]
        INACTIVE,
        [Display(Name = "No confirmado")]
        UNCONFIRMED,
        [Display(Name = "Pendiente")]
        PENDING,
        [Display(Name = "En Proceso")]
        PROCESSING,
        [Display(Name = "Aprobado")]
        APPROVED,
        [Display(Name = "Cancelado")]
        CANCELLED,
        [Display(Name = "No válido")]
        INVALID,
        [Display(Name = "Fallido")]
        FAILED
    }

    public enum NotificationStatus
    {
        [Display(Name = "Activo")]
        ACTIVE,
        [Display(Name = "Confirmado")]
        CONFIRMED,
    }

    public enum IssueStatus
    {
        [Display(Name = "Guardado")]
        SAVED,
        [Display(Name = "Timbrado")]
        STAMPED,
        [Display(Name = "Cancelado")]
        CANCELED
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
        LEAD,
        [Display(Name = "System Administrator")]
        SYSTEM_ADMINISTRATOR,
        [Display(Name = "Dirección")]
        DIRECCION,
        [Display(Name = "Gerente")]
        GERENTE,
        [Display(Name = "Ejecutivo")]
        EJECUTIVO,
        [Display(Name = "Supervisor")]
        SUPERVISOR,
        [Display(Name = "CAD")]
        CAD
    }

    public enum SystemPermissionApply
    {
        [Display(Name = "Only Account")]
        ONLY_ACCOUNT,
        [Display(Name = "Only Back Office")]
        ONLY_BACK_OFFICE,
        [Display(Name = "Both")]
        BOTH
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
        [Display(Name = "Reportes")]
        REPORTS,
        [Display(Name = "Clientes")]
        CUSTOMERS,
        [Display(Name = "Proveedores")]
        PROVIDERS,
        [Display(Name = "Bancos")]
        BANKS,
        [Display(Name = "Facturación")]
        INVOICING,
        [Display(Name = "Mi Cuenta con El20.mx")]
        MY_ACCOUNT,
        [Display(Name = "Datos de la cuenta")]
        RECURLY_ACCOUNT,
        #region Modulos del BackOffice
        [Display(Name = "Planes")]
        PLANS,
        [Display(Name = "Alianzas y Descuentos")]
        ALLIANCES_DISCOUNTS,
        [Display(Name = "Regularizaciones")]
        QUOTATION
        #endregion
    }

    public enum StatusPayment
    {
        [Display(Name = "Vigente")]
        VALID,
        [Display(Name = "Pagada")]
        PAID,
        [Display(Name = "Vencida")]
        EXPIRED
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
        PERSONA_FISICA,
        [Display(Name = "Persona Moral")]
        [Description("Persona Moral")]
        PERSONA_MORAL,
    }

    public enum TypeMovements
    {
        [Display(Name = "Retiros")]
        [Description("Retiros")]
        RETIREMENT = 0,
        [Display(Name = "Depósitos")]
        [Description("Depósitos")]
        DEPOSITS = 1,
    }

    public enum BooleanText
    {
        [Description("SI")]
        YES,
        [Description("NO")]
        NO,
    }

    public enum SystemProviders
    {
        [Display(Name = "Syncfy")]
        SYNCFY,
        [Display(Name = "SAT.ws")]
        SATWS,
        [Display(Name = "Finerio")]
        FINERIO,
        [Display(Name = "Recurly")]
        RECURLY
    }

    public enum SATCredentialType
    {
        [Display(Name = "ciec")]
        CIEC,
        [Display(Name = "efirma")]
        EFIRMA
    }

    public enum TypeRetention
    {
        [Description("ISR")]
        ISR,
        [Description("IVA")]
        IVA
    }

    public enum TypeTransferred
    {
        [Description("IVA")]
        IVA,
        [Description("IEPS")]
        IEPS
    }

    public enum TypeValuation
    {
        [Description("Sin Tasa")]
        ST0 = 0,
        [Description("8%")]
        T8 = 8,
        [Description("16%")]
        T16 = 16
    }

    public enum MetodoPago
    {
        [Description("Pago en una sola exhibición")]
        PUE,
        [Description("Pago en parcialidades o diferido")]
        PPD
    }
    public enum RegimenFiscal
    {
        [Description("General de Ley Personas Morales")]
        RegimenFiscal601
    }

    public enum TipoComprobante
    {
        [Description("Ingreso")]
        I,
        [Description("Egreso")]
        E,
        [Description("Pago")]
        P,
        [Description("Nómina")]
        N,
        [Description("Traslado")]
        T
    }

    public enum TypeCurrency
    {
        [Description("Peso Mexicano")]
        MXN,
        [Description("Dolar Americano")]
        USD,
        [Description("Euro")]
        EUR
    }

    public enum UsoCFDI
    {

        [Description("Adquisición de Mercancías")]
        G01,
        [Description("Devoluciones, Descuentos o Bonificaciones")]
        G02,
        [Description("Gastos en General")]
        G03,
        [Description("Construcciones")]
        I01,
        [Description("Mobiliario y Equipo de Oficina por Inversiones")]
        I02,
        [Description("Equipo de Transporte")]
        I03,
        [Description("Equipo de Cómputo y Accesorios")]
        I04,
        [Description("Dados, Troqueles, Moldes, Matrices y Herramental")]
        I05,
        [Description("Comunicaciones Telefónicas")]
        I06,
        [Description("Comunicaciones Satelitales")]
        I07,
        [Description("Otra Maquinaria y Equipo")]
        I08,
        [Description("Honorarios Médicos, Dentales y Gastos Hospitalarios")]
        D01,
        [Description("Gastos Médicos por Incapacidad o Discapacidad")]
        D02,
        [Description("Gastos Funerales")]
        D03,
        [Description("Donativos")]
        D04,
        [Description("Intereses Reales Efectivamente Pagados por Créditos Hipotecarios (Casa Habitación)")]
        D05,
        [Description("Aportaciones Voluntarias al SAR")]
        D06,
        [Description("Primas por Seguros de Gastos Médicos")]
        D07,
        [Description("Gastos de Transportación Escolar Obligatoria")]
        D08,
        [Description("Depósitos en Cuentas para el Ahorro, Primas que tengan como Base Planes de Pensiones")]
        D09,
        [Description("Pagos por Servicios Educativos (Colegiaturas)")]
        D10,
        [Description("Por definir")]
        P01,
    }
    public enum ClaveUnidad
    {
        [Description("Carga masiva")]
        Unidad48
    }

    public enum ChangeType
    {
        [Display(Name = "Fijo")]
        FIXED,
        [Display(Name = "Variable")]
        VARIABLE
    }

    public enum Operation
    {
        [Display(Name = "Adición")]
        ADDITION,
        [Display(Name = "Subtracción")]
        SUBTRACTION,
        [Display(Name = "División")]
        DIVIDE,
        [Display(Name = "Multiplicación")]
        MULTIPLICITY,
        [Display(Name = "Rango")]
        RANGE,
        [Display(Name = "Igual")]
        EQUAL
    }

    public enum TypePromotions
    {
        [Display(Name = "Referido")]
        [Description("Referido")]
        REFERRED,
        [Display(Name = "Descuento Inicial")]
        [Description("Descuento Inicial")]
        INITIAL_DISCOUNT,
        [Display(Name = "Clientes")]
        [Description("Clientes")]
        CUSTOMERS,
    }

    public enum SystemControllers
    {
        [Display(Name = "Account")]
        ACCOUNT,
    }

    public enum SatwsEvent
    {
        [Display(Name = "extraction.updated")]
        EXTRACTION_UPDATED,
        [Display(Name = "credential.updated")]
        CREDENTIAL_UPDATE
    }

    public enum SyncfyEvent
    {
        [Display(Name = "refresh")]
        REFRESH
    }

    public enum SatwsStatusEvent
    {
        [Display(Name = "finished")]
        FINISHED,
        [Display(Name = "failed")]
        FAILED
    }

    public enum TypeInvoicing
    {
        [Display(Name = "issued")]
        ISSUED,
        [Display(Name = "received")]
        RECEIVED
    }

    public enum EmailProvider
    {
        [Display(Name = "SENDGRID")]
        SENDGRID,
        [Display(Name = "SMTP")]
        SMTP
    }
}
