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
        public static ResponseBankTransaction CreateBankTransaction(dynamic request, string apiKey, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.CreateBankTransaction(request, apiKey);

                return contalinkModel; 
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }

        //Método Delete para movimientos bancarios
        public static ResponseBankTransaction DeleteBankTransaction(Int64 id, string apiKey, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.DeleteBankTransaction(id, apiKey);

                return contalinkModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }

        //Método Delete para movimientos bancarios
        public static ResponseBankTransaction GetBankTransaction(Int64 id, string apiKey, string provider)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                var contalinkModel = ContaLinkServices.GetBankTransaction(id, apiKey);

                return contalinkModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }

        public static InvoiceUploadResponse InvoiceUpload(dynamic request, string provider, string apiKey)
        {
            if (provider == SystemProviders.CONTALINK.ToString())
            {
                return ContaLinkServices.InvoiceUpload(request, apiKey);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a la información.");
            }
        }
    }
}
