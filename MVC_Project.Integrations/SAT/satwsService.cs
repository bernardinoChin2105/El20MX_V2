using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATwsService
    {
        /*Método para obtener la información de los clientes y proveedores*/
        //public List<IssuerReceiver> GetIssuersReceivers(string accountRFC, string typePerson, string DateOnStart, string DateOnEnd)
        //{

        //    List<IssuerReceiver> list = new List<IssuerReceiver>();

        //    string url = "/taxpayers/" + accountRFC + "/invoices?issuedAt[after]=" + DateOnStart + "&issuedAt[before]=" + DateOnEnd;
        //    var responseCFDIS = SATws.CallServiceSATws(url, null, "Get");
        //    var modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(responseCFDIS);

        //    //separar recibidas, emitidas
        //    //foreach
        //    if (typePerson == "issuer")
        //    {
        //        list = modelInvoices.Where(x => x.issuer.rfc == accountRFC).Select(x => x.receiver).ToList();
        //    }

        //    if (typePerson == "receiver")
        //    {
        //        list = modelInvoices.Where(x => x.receiver.rfc == accountRFC).Select(x => x.issuer).ToList();
        //    }

        //    return list;
        //}

        /*
         * Realiza la extracción por el rfc y obtiene información de los CFDIs
         */
        public static InvoicesModel GetInformationByExtractions(string RFC, DateTime DateOnStart, DateTime DateOnEnd)
        {
            List<CustomersInfo> customers = new List<CustomersInfo>();
            List<ProvidersInfo> providers = new List<ProvidersInfo>();
            List<InvoicesInfo> modelInvoices = new List<InvoicesInfo>();
            TaxpayerInfo taxpayer = new TaxpayerInfo();
            try
            {
                //Realiza la solicitud de extracción
                ExtractionsFilter filter = new ExtractionsFilter()
                {
                    taxpayer = "/taxpayers/" + RFC,
                    extractor = "invoice",
                    periodFrom = DateOnStart.ToString("s") + "Z",
                    periodTo = DateOnEnd.ToString("s") + "Z"
                };

                var responseExtraction = SATws.CallServiceSATws("extractions", filter, "Post");
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseExtraction);
                var option = model.First(x => x.Key == "taxpayer").Value;
                JObject rItemValueJson = (JObject)option;
                taxpayer = JsonConvert.DeserializeObject<TaxpayerInfo>(rItemValueJson.ToString());

                //Se realiza la búsqueda de los cfdis
                string from = DateOnStart.ToString("yyyy-MM-dd HH:mm:ss");
                string to = DateOnEnd.ToString("yyyy-MM-dd HH:mm:ss");
                string url = "/taxpayers/" + RFC + "/invoices?issuedAt[after]=" + from + "&issuedAt[before]=" + to;
                var responseInvoices = SATws.CallServiceSATws(url, null, "Get");
                modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(responseInvoices);

                #region Obtener la información de los clientes y proveedores para guardarlos
                foreach (var item in modelInvoices)
                {

                    string cfdi = "/invoices/" + item.id + "/cfdi";
                    var responseCFDI = SATws.CallServiceSATws(cfdi, null, "Get");
                    var modelCFDI = JsonConvert.DeserializeObject<IDictionary<string, object>>(responseCFDI);

                    var zipCode = (String)modelCFDI["LugarExpedicion"];

                    //Estamos buscando mis clientes
                    if (item.issuer.rfc == RFC)
                    {
                        CustomersInfo customer = new CustomersInfo();
                        customer.zipCode = zipCode;

                        var myCustomer = JsonConvert.DeserializeObject<IDictionary<string, object>>(modelCFDI["Receptor"].ToString());
                        customer.businessName = (String)myCustomer["Nombre"];
                        customer.rfc = (String)myCustomer["Rfc"];
                        customers.Add(customer);
                    }

                    //Estamos buscando mis proveedores
                    if (item.receiver.rfc == RFC)
                    {
                        ProvidersInfo provider = new ProvidersInfo();
                        provider.zipCode = zipCode;

                        var myProvider = JsonConvert.DeserializeObject<IDictionary<string, object>>(modelCFDI["Emisor"].ToString());
                        provider.businessName = (String)myProvider["Nombre"];
                        provider.rfc = (String)myProvider["Rfc"];
                        provider.regime = Convert.ToInt32(myProvider["RegimenFiscal"]);
                        providers.Add(provider);
                    }
                }
                #endregion

                return new InvoicesModel
                {
                    Success = true,
                    Taxpayer = taxpayer,
                    Invoices = modelInvoices,
                    Providers = providers,
                    Customers = customers
                };
            }
            catch (Exception ex)
            {
                return new InvoicesModel
                {
                    Success = false,
                    Message = ex.Message.ToString(),
                    Taxpayer = taxpayer,
                    Invoices = modelInvoices,
                    Providers = providers,
                    Customers = customers
                };
            }                        
        }
    }
}
