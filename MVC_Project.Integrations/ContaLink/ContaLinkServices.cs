using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MVC_Project.Integrations.ContaLink.ContaLinkModels;

namespace MVC_Project.Integrations.ContaLink
{
    public class ContaLinkServices
    {
        /*con este método se crean las transacciones bancarias*/
        public static ResponseBankTransaction CreateBankTransaction(dynamic model, string apiKey)
        {
            ResponseBankTransaction transactionModel = new ResponseBankTransaction();

            string url = "treasury/bank-transactions/";

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, model, "Post", apiKey);

            transactionModel = JsonConvert.DeserializeObject<ResponseBankTransaction>(response);

            return transactionModel;
        }

        /*con este método elimina un movimiento bancario*/
        public static ResponseBankTransaction DeleteBankTransaction(Int64 id, string apiKey)
        {
            ResponseBankTransaction transactionModel = new ResponseBankTransaction();            

            string url = "treasury/bank-transactions/"+id;

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, null, "Delete", apiKey);

            transactionModel = JsonConvert.DeserializeObject<ResponseBankTransaction>(response);

            return transactionModel;
        }

        /*con este método se obtiene un movimiento bancario*/
        public static ResponseBankTransaction GetBankTransaction(Int64 id, string apiKey)
        {
            ResponseBankTransaction transactionModel = new ResponseBankTransaction();

            string url = "treasury/bank-transactions/" + id;

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, null, "Get", apiKey);

            transactionModel = JsonConvert.DeserializeObject<ResponseBankTransaction>(response);

            return transactionModel;
        }

        public static InvoiceUploadResponse InvoiceUpload(dynamic model, string apiKey)
        {
            string url = "invoices/upload/";

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, model, "Post", apiKey);

            return JsonConvert.DeserializeObject<InvoiceUploadResponse>(response);
        }
    }
}
