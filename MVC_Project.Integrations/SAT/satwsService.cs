using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public class SATwsService
    {
        public static ExtractionResponse GenerateExtractions(string RFC, DateTime DateOnStart, DateTime DateOnEnd)
        {
            TaxpayerInfo taxpayer = new TaxpayerInfo();

            //Realiza la solicitud de extracción
            ExtractionsFilter filter = new ExtractionsFilter()
            {
                taxpayer = "/taxpayers/" + RFC,
                extractor = "invoice",
                periodFrom = DateOnStart.ToString("s") + "Z",
                periodTo = DateOnEnd.ToString("s") + "Z"
            };

            var response = SATws.CallServiceSATws("extractions", filter, "Post");
            return JsonConvert.DeserializeObject<ExtractionResponse>(response);
        }

        public static InvoicesModel GetInvoicesByExtractions(string rfc, string from, string to)
        {
            List<CustomersInfo> customers = new List<CustomersInfo>();
            List<ProvidersInfo> providers = new List<ProvidersInfo>();
            List<InvoicesInfo> modelInvoices = new List<InvoicesInfo>();
            TaxpayerInfo taxpayer = new TaxpayerInfo();

            int page = 1;
            int itemsPerPage = int.Parse(ConfigurationManager.AppSettings["Invoices.ItemsPerPage"]);
            //Se realiza la búsqueda de los cfdis
            string url = "/taxpayers/" + rfc + "/invoices?issuedAt[after]=" + from + "&issuedAt[before]=" + to + "&page=" + page + "&itemsPerPage=" + itemsPerPage;
            var responseInvoices = SATws.CallServiceSATws(url, null, "Get", accept: "*");
            JObject obj = JsonConvert.DeserializeObject<JObject>(responseInvoices);

            modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(obj["hydra:member"].ToString()); 
            int totalItems = 0;
            int.TryParse(obj["hydra:totalItems"].ToString(), out totalItems);

            if(totalItems > itemsPerPage)
            {
                var numberPages = (int)Math.Ceiling((double)totalItems / (double)itemsPerPage);
                for (int i = page + 1; i <= numberPages; i++)
                {
                    url = "/taxpayers/" + rfc + "/invoices?issuedAt[after]=" + from + "&issuedAt[before]=" + to + "&page=" + i + "&itemsPerPage=" + itemsPerPage;
                    responseInvoices = SATws.CallServiceSATws(url, null, "Get", accept: "*");
                    obj = JsonConvert.DeserializeObject<JObject>(responseInvoices);
                    modelInvoices.AddRange(JsonConvert.DeserializeObject<List<InvoicesInfo>>(obj["hydra:member"].ToString()));
                }
            }

            //Estamos buscando mis clientes
            customers = modelInvoices.Where(x => x.issuer.rfc == rfc)
                .Select(x => new CustomersInfo
                {
                    idInvoice = x.id,
                    rfc = x.receiver.rfc,
                    businessName = x.receiver.name,
                    tax = (x.tax != null ? x.tax.Value : 0)
                        //regime = x.
                    }).ToList();

            //Estamos buscando mis proveedores
            providers = modelInvoices.Where(x => x.receiver.rfc == rfc)
                .Select(x => new ProvidersInfo
                {
                    idInvoice = x.id,
                    zipCode = x.placeOfIssue,
                    businessName = x.issuer.name,
                    rfc = x.issuer.rfc,
                    tax = (x.tax != null ? x.tax.Value : 0)
                }).ToList();

            return new InvoicesModel
            {
                Success = true,
                Invoices = modelInvoices,
                Providers = providers,
                Customers = customers
            };

        }

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
                        idInvoice = x.id,
                        rfc = x.receiver.rfc,
                        businessName = x.receiver.name,
                        tax = (x.tax != null? x.tax.Value : 0)
                        //regime = x.
                    }).ToList();

                //Estamos buscando mis proveedores
                providers = modelInvoices.Where(x => x.receiver.rfc == RFC)
                    .Select(x => new ProvidersInfo
                    {
                        idInvoice = x.id,
                        zipCode = x.placeOfIssue,
                        businessName = x.issuer.name,
                        rfc = x.issuer.rfc,
                        tax = (x.tax != null ? x.tax.Value : 0)
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

        /*
         * Obtener la Constancia de Situación Fisca
         * */
        public static InvoicesModel GetTaxStatus(string RFC)
        {
            List<TaxStatus> taxStatus = new List<TaxStatus>();

            string url = "/taxpayers/" + RFC + "/tax-status";
            var responseInvoices = SATws.CallServiceSATws(url, null, "Get");
            taxStatus = JsonConvert.DeserializeObject<List<TaxStatus>>(responseInvoices);

            return new InvoicesModel
            {
                Success = true,
                TaxStatus = taxStatus
            };
        }

        //Crear la cuenta del registro de RFC en SATws
        public static SatAuthResponseModel CreateCredentialSat(LogInSATModel model)
        {
            SatAuthResponseModel satModel = new SatAuthResponseModel();

            string url = "/credentials";

            //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
            var responseSat = SATws.CallServiceSATws(url, model, "Post");

            satModel = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);
            
            return satModel;
        }

        public static SatAuthResponseModel CreateCredentialEfirma(EfirmaModel model)
        {
            SatAuthResponseModel satModel = new SatAuthResponseModel();

            string url = "/credentials";

            //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
            var responseSat = SATws.CallServiceSATws(url, model, "Post");

            satModel = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);

            return satModel;
        }

        ///*Para crear el certificado*/
        public static CertificateResponseModel CreateCertificates(CertificateModel model)
        {
            CertificateResponseModel satModel = new CertificateResponseModel();

            string url = "/certificates";

            //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
            var responseSat = SATws.CallServiceSATws(url, model, "Post");

            satModel = JsonConvert.DeserializeObject<CertificateResponseModel>(responseSat);

            return satModel;
        }

        public static void DeleteCertificates(string id)
        {
            string url = "/certificates/" + id;

            //Llamar al servicio para eliminar la credencial en el sat.ws y obtener respuesta                  
            SATws.CallServiceSATws(url, null, "Delete");
            
        }

        //Obtener idCredencial del RFC
        public static SatAuthResponseModel GetCredentialSat(string idCredential)
        {
            SatAuthResponseModel satModel = new SatAuthResponseModel();

            string url = "/credentials/" + idCredential;

            //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
            var responseSat = SATws.CallServiceSATws(url, null, "Get");

            satModel = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);
            return satModel;
        }

        /*Obtener los CFDI's*/
        public static List<InvoicesCFDI> GetInvoicesCFDI(List<string> CFDIIds)
        {
            List<InvoicesCFDI> CFDI = new List<InvoicesCFDI>();

            foreach (var id in CFDIIds)
            {
                try
                {
                    string url = "invoices/" + id + "/cfdi";
                    var responsecfdi = SATws.CallServiceSATws(url, null, "get");
                    try
                    {
                        var model = JsonConvert.DeserializeObject<InvoicesCFDI>(responsecfdi);

                        var responseXML = SATws.CallServiceSATws(url, null, "get", SATwsEnumsAccept.textxml.GetDescriptionSAT());
                        var xml = responseXML;
                        model.Xml = xml;
                        model.id = id;

                        CFDI.Add(model);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                    }

                }
                catch (Exception ex)
                {
                    string error = ex.Message.ToString();
                }
            }

            return CFDI;
        }

        /*Crear timbrado de factura*/
        public static InvoicesInfo PostIssueIncomeInvoices(dynamic invoiceJson)
        {            
            InvoicesInfo invoice = new InvoicesInfo();            

            var responseInvoices =  SATws.CallServiceSATws("invoices", invoiceJson, "Post");
            invoice = JsonConvert.DeserializeObject<InvoicesInfo>(responseInvoices);
            return invoice;
        }

        /*Crear timbrado de factura complemento*/
        public static InvoicesInfo PostIssuePaymentInvoices(dynamic invoiceComplementJson)
        {
            InvoicesInfo invoice = new InvoicesInfo();
            
            var responseInvoices = SATws.CallServiceSATws("invoices/payment", invoiceComplementJson, "Post");
            invoice = JsonConvert.DeserializeObject<InvoicesInfo>(responseInvoices);
            return invoice;
        }

        /*Crear timbrado de factura relacionada*/
        public static InvoicesInfo PostRefundInvoices(dynamic invoiceRefundJson)
        {
            InvoicesInfo invoice = new InvoicesInfo();

            var responseInvoices = SATws.CallServiceSATws("invoices/refund", invoiceRefundJson, "Post");
            invoice = JsonConvert.DeserializeObject<InvoicesInfo>(responseInvoices);
            return invoice;
        }        
    }
}
