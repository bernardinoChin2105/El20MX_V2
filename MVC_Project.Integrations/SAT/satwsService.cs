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

                //Estamos buscando mis clientes
                customers = modelInvoices.Where(x => x.issuer.rfc == RFC)
                    .Select(x => new CustomersInfo
                    {
                        rfc = x.receiver.rfc,
                        businessName = x.receiver.name
                        //regime = x.
                    }).ToList();

                //Estamos buscando mis proveedores
                providers = modelInvoices.Where(x => x.receiver.rfc == RFC)
                    .Select(x => new ProvidersInfo
                    {
                        zipCode = x.placeOfIssue,
                        businessName = x.issuer.name,
                        rfc = x.issuer.rfc
                        //regime =             
                    }).ToList();                
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
