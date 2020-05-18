using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.FlashMessages
{

    public enum TiposMensaje
    {
        Success,
        Warning,
        Info,
        Error


    }

    static class TiposMensajeExtension{
        public static string ObtenerCodigo(this TiposMensaje tipoMensaje)
        {
            string codigo = tipoMensaje.ToString();
            return codigo.ToLower();
        }
    }
    public class MensajeFlash
    {
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
    }
}
