using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    public class RecurlyService
    {
        //public static string GetSite(string siteId, string provider)
        //{
        //    if (provider == SystemProviders.RECURLY.ToString())
        //    {
        //        //llamadas al metodo, se descargo la librería, en dado caso que no funcione se tiene la clase donde esta la llamada a la api
        //        return "ejemplo";
        //    }
        //    else
        //    {
        //        throw new Exception( "Ejemplo");
        //    }
        //}

        public static AccountResponseModel CreateAccount(dynamic request, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {                
                var recurlyModel = RecurlyServices.CreateAccount(request, siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }
    }
}
