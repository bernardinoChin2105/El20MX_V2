using MVC_Project.API.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.API.Components
{
    public class LanguageInit : DelegatingHandler
    {
        //<summary>Recibe la peticion del servidor antes de que llegue al controlador para iniciar el lenguage de la aplicacion.</summary>
        /// <param name="cancellationToken"></param>
        /// <param name="request">Objeto que contiene informacion de las peticiones.</param>
        /// <returns>Respuesta del servidor</returns>
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            LanguageMngr.SetDefaultLanguage();
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
