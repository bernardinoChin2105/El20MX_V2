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
        public static BankTransaction CreateBankTransaction(dynamic model)
        {
            BankTransaction transactionModel = new BankTransaction();

            string url = "treasury/bank-transactions/";

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, model, "Post");

            transactionModel = JsonConvert.DeserializeObject<BankTransaction>(response);

            return transactionModel;
        }

        /*con este método elimina un movimiento bancario*/
        public static BankTransaction DeleteBankTransaction(Int64 id)
        {
            BankTransaction transactionModel = new BankTransaction();            

            string url = "treasury/bank-transactions/"+id;

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, null, "Delete");

            transactionModel = JsonConvert.DeserializeObject<BankTransaction>(response);

            return transactionModel;
        }

        /*con este método se obtiene un movimiento bancario*/
        public static BankTransaction GetBankTransaction(Int64 id)
        {
            BankTransaction transactionModel = new BankTransaction();

            string url = "treasury/bank-transactions/" + id;

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, null, "Get");

            transactionModel = JsonConvert.DeserializeObject<BankTransaction>(response);

            return transactionModel;
        }

        public static InvoiceUploadResponse InvoiceUpload(dynamic model)
        {
            string url = "invoices/upload/";

            //Llamar al servicio y obtiene respuesta
            var response = ContaLink.CallServiceContaLink(url, model, "Post");

            return JsonConvert.DeserializeObject<InvoiceUploadResponse>(response);
        }
    }
}
