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
            if (provider == "SATWS")
            {
                var loginSat = new LogInSATModel { rfc = request.rfc, password = request.ciec, type = "ciec" };
                var satModel = SATwsService.CreateCredentialSat(loginSat);

                return new CredentialsResponse { id = satModel.id, status = satModel.status };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de facturación");
            }
        }
    }
}
