using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MVC_Project.Utils
{
    public class Constants
    {
        public static readonly string AUTHENTICATED_USER_PROFILE = "ddc30589-80ba-48e6-88ec-6454350f2cd7_USER_PROFILE";

        public static readonly int SEARCH_ALL = -1;

        //ROLE CODES
        public static readonly string ROLE_IT_SUPPORT = "IT_SUPPORT";
        public static readonly string ROLE_ADMIN = "ADMIN";
        public static readonly string ROLE_EMPLOYEE = "EMPLOYEE";
        public static readonly string ROLE_APP_USER = "APP_USER";
        
        //public static string STORAGE_MAIN_CONTAINER = ConfigurationManager.AppSettings["StorageMainContainer"];

        public static string DATE_FORMAT_DAY_MONTH = "dd/MM";
        public static string DATE_FORMAT = "dd/MM/yyyy";
        //public static string DATE_FORMAT_CALENDAR = "yyyy-MM-dd HH:mm";
        public static string DATE_FORMAT_CALENDAR = "dd/MM/yyyy HH:mm";
        public static string TIMEZONE_UTC = "UTC";
        public static int HOURS_EXPIRATION_KEY = 6;

        // Uuids de templates sendgrid
        public static string NOT_TEMPLATE_WELCOME = "51d34567-1960-4501-8e89-e349c18275e6";
        public static string NOT_TEMPLATE_ACTIVATION = "534f930b-d06f-4aad-a30a-ff42346e7c56";
        public static string NOT_TEMPLATE_PASSWORDRESET = "d12f6fbe-cab5-485f-95ec-6875d196d49c";
        public static string NOT_TEMPLATE_PASSWORDRECOVER = "aa61890e-5e39-43c4-92ff-fae95e03a711";
        public static string NOT_TEMPLATE_CHARGESUCCESS = "3e58558e-0a7c-4f2b-b003-ac27b5677d91";

        // Encabezados de archivo Excel
        public static string HEADER_USERIDENTIFIER = "Identificador";
        public static string HEADER_FIRSTNAME = "Nombre";
        public static string HEADER_LASTNAME = "Apellido Paterno";
        public static string HEADER_EMAIL = "Correo Electrónico";
        public static string HEADER_POSITION = "Puesto";

        //Mensajes
        public static string View_Message = "View.Message";
        public static string View_Error = "View.Error";
        
        // Status de notificaciones
        public static int PENDING = 0;
        public static int SENT = 1;
        public static int ERROR = 2;

        // URLs varias
        public static string DEFAULT_AVATAR = "https://bootdey.com/img/Content/avatar/avatar1.png";

        public enum SocialNetwork
        {
            Email = 0,
            Facebook = 1,
            Google = 2
        }
    }
}
