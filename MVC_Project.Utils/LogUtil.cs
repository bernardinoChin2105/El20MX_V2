using LogHubSDK.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Utils
{
    public class LogUtil
    {
        public static LogHubSDK.LogHubClient logHubClient = new LogHubSDK.LogHubClient(ConfigurationManager.AppSettings["logHub.UrlAPI"], ConfigurationManager.AppSettings["logHub.ApiKey"]);

        public static void AddEntry(string descripcion, ENivelLog eLogLevel, int usuarioId, string usuario, EOperacionLog? eOperacionLog, string parametros, string modulo, string detalle)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["logHub.Enabled"]))
            {
                LogHubSDK.Models.LogMessage msg = new LogHubSDK.Models.LogMessage();
                msg.UserId = Convert.ToString(usuarioId);
                msg.Username = usuario;
                msg.Operation = eOperacionLog.Value.ToString();
                msg.Level = eLogLevel.ToString();
                msg.Parameters = parametros;
                msg.Description = descripcion;
                msg.Detail = detalle;
                msg.Module = modulo;

                try
                {
                    var respuesta = logHubClient.SendLog(msg);
                }
                catch (Exception er)
                {
                    string error = er.Message.ToString();
                }


                //Task.Run(() => logHubClient.SendLog(msg));
            }
        }
    }

}
