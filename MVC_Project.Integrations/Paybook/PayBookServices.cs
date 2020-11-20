using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Paybook
{
    public class PayBookServices
    {
        public static List<CredentialsPaybook> GetCredentials(string idCredential, string token, string provider)
        {
            if (provider == SystemProviders.SYNCFY.ToString())
            {
                var newBanks = PaybookService.GetCredentials(idCredential, token);
                return newBanks;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso a conexión de bancos.");
            }            
        }
    }
}
