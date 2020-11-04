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

    public enum Posicion
    {
        Arriba_derecha,
        Arriba_izquierda,
        Arriba_centrado,
        Abajo_derecha,
        Abajo_izquierda,
        Abajo_centrado,
        Arriba_completo,
        Abajo_completo
    }

    static class TiposMensajeExtension
    {
        public static string ObtenerCodigo(this TiposMensaje tipoMensaje)
        {
            string codigo = tipoMensaje.ToString();
            return codigo.ToLower();
        }

        public static string ObtenerPosicion(this Posicion posicion)
        {
            string mensajePosicion = string.Empty;
            switch (posicion)
            {
                case Posicion.Arriba_derecha:
                    mensajePosicion = "toast-top-right";
                    break;
                case Posicion.Arriba_izquierda:
                    mensajePosicion = "toast-top-left";
                    break;
                case Posicion.Arriba_centrado:
                    mensajePosicion = "toast-top-center";
                    break;
                case Posicion.Abajo_derecha:
                    mensajePosicion = "toast-bottom-right";
                    break;
                case Posicion.Abajo_izquierda:
                    mensajePosicion = "toast-bottom-left";
                    break;
                case Posicion.Abajo_centrado:
                    mensajePosicion = "toast-bottom-center";
                    break;
                case Posicion.Arriba_completo:
                    mensajePosicion = "toast-top-full-width";
                    break;
                case Posicion.Abajo_completo:
                    mensajePosicion = "toast-bottom-full-width";
                    break;
                default:
                    mensajePosicion = "";
                    break;
            }

            return mensajePosicion;
        }
    }
    public class MensajeFlash
    {
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
        public string Posicion { get; set; }
    }

   
}
