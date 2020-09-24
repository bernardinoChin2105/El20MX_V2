using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATService
    {
        public static CredentialsResponse CreateCredential(CredentialRequest request, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                var loginSat = new LogInSATModel { rfc = request.rfc, password = request.ciec, type = "ciec" };
                var satModel = SATwsService.CreateCredentialSat(loginSat);

                return new CredentialsResponse { id = satModel.id, status = satModel.status };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static void GenerateExtractions(string rfc, DateTime dateOnStart, DateTime dateOnEnd, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                SATwsService.GenerateExtractions(rfc, dateOnStart, dateOnEnd);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesModel GetInvoicesByExtractions(string rfc, string from, string to, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetInvoicesByExtractions(rfc, from, to);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static List<InvoicesCFDI> GetCFDIs(List<string> uuids, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetInvoicesCFDI(uuids);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }

        public static InvoicesModel GetTaxStatus(string rfc, string provider)
        {
            if (provider == SystemProviders.SATWS.ToString())
            {
                return SATwsService.GetTaxStatus(rfc);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso al información fiscal");
            }
        }
    }
}
