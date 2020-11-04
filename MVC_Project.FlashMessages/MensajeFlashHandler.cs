using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.FlashMessages
{
    public class MensajeFlashHandler
    {
        public static bool RecibioMensajeFlash()
        {
            var mensaje = System.Web.HttpContext.Current.Session[Configuracion.MENSAJE_FLASH_SESSION];
            if (mensaje != null &&
                mensaje is MensajeFlash)
            {
                return true;
            }
            return false;
        }
        public static void RegistrarMensaje(string mensaje, TiposMensaje? tipo = null, Posicion? posicion = Posicion.Arriba_centrado)
        {
            if (!tipo.HasValue)
            {
                tipo = TiposMensaje.Success;
            }
            System.Web.HttpContext.Current.Session.Add(Configuracion.MENSAJE_FLASH_SESSION, new MensajeFlash
            {
                Mensaje = mensaje,
                Tipo = tipo.Value.ObtenerCodigo(),
                Posicion = posicion.Value.ObtenerPosicion()
            });
        }

        public static void RegistrarMensaje(MensajeFlash mensajeFlash)
        {
            System.Web.HttpContext.Current.Session.Add(Configuracion.MENSAJE_FLASH_SESSION, mensajeFlash);
        }
        public static MensajeFlash ObtenerMensaje()
        {
            if (RecibioMensajeFlash())
            {
                MensajeFlash mensaje = (MensajeFlash)System.Web.HttpContext.Current.Session[Configuracion.MENSAJE_FLASH_SESSION];
                System.Web.HttpContext.Current.Session.Remove(Configuracion.MENSAJE_FLASH_SESSION);
                return mensaje;
            }
            return null;
        }
    }
}
