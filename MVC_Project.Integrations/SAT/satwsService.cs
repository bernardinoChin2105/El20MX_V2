using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class satwsService
    {
        /*Método para obtener la información de los clientes y proveedores*/
        public List<IssuerReceiver> GetIssuersReceivers(string accountRFC, string typePerson, string DateOnStart, string DateOnEnd) {

            List<IssuerReceiver> list = new List<IssuerReceiver>();

            string url = "/taxpayers/" + accountRFC + "/invoices?issuedAt[after]=" + DateOnStart + "&issuedAt[before]=" + DateOnEnd;
            var responseCFDIS = SATws.CallServiceSATws(url, null, "Get");
            var modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(responseCFDIS);

            //separar recibidas, emitidas
            //foreach
            if (typePerson == "issuer")
            {
                list = modelInvoices.Where(x => x.issuer.rfc == accountRFC).Select(x => x.receiver).ToList();                                   
            }

            if (typePerson == "receiver")
            {
                list = modelInvoices.Where(x => x.receiver.rfc == accountRFC).Select(x => x.issuer).ToList();
            }
           
            return list;
        }
    }
}
