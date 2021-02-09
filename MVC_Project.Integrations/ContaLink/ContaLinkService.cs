using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MVC_Project.Integrations.ContaLink.ContaLinkModels;

namespace MVC_Project.Integrations.ContaLink
{
    public class ContaLinkService
    {
        //Método Post para movimientos bancarios
        public static BankTransaction CreateBankTransaction(dynamic request, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.CreateBankTransaction(request);

                return contalinkModel; 
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }

        //Método Delete para movimientos bancarios
        public static BankTransaction DeleteBankTransaction(Int64 id, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.DeleteBankTransaction(id);

                return contalinkModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }

        //Método Delete para movimientos bancarios
        public static BankTransaction GetBankTransaction(Int64 id, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.GetBankTransaction(id);

                return contalinkModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }
    }
}
