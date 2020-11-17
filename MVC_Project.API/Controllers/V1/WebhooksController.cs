//using LogHubSDK.Models;
using LogHubSDK.Models;
using Microsoft.Web.Http;
using MVC_Project.API.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.SAT;
using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MVC_Project.API.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/v{version:apiVersion}/Webhooks")]
    [EnableCors(origins:"*", headers:"*", methods:"*")]
    public class WebhooksController : ApiController
    {
        private IWebhookService _webhookService;
        private ICustomerService _customerService;
        private IProviderService _providerService;
        private IInvoiceIssuedService _invoicesIssuedService;
        private IInvoiceReceivedService _invoicesReceivedService;
        private IAccountService _accountService;
        private IDiagnosticService _diagnosticService;
        private IDiagnosticDetailService _diagnosticDetailService;
        private IDiagnosticTaxStatusService _diagnosticTaxStatusService;
        private ICredentialService _credentialService;

        public WebhooksController(IWebhookService webhookService, ICustomerService customerService, IProviderService providerService,
            IInvoiceIssuedService invoicesIssuedService, IInvoiceReceivedService invoicesReceivedService, IAccountService accountService,
            IDiagnosticService diagnosticService, IDiagnosticDetailService diagnosticDetailService, IDiagnosticTaxStatusService diagnosticTaxStatusService,
            ICredentialService credentialService)
        {
            _webhookService = webhookService;
            _customerService = customerService;
            _providerService = providerService;
            _invoicesIssuedService = invoicesIssuedService;
            _invoicesReceivedService = invoicesReceivedService;
            _accountService = accountService;
            _diagnosticService = diagnosticService;
            _diagnosticDetailService = diagnosticDetailService;
            _diagnosticTaxStatusService = diagnosticTaxStatusService;
            _credentialService = credentialService;
        }

        [HttpPost]
        [Route("WebhookPaybook")]
        public HttpResponseMessage WebhookPaybook(Object response)
        {
            DateTime today = DateUtil.GetDateTimeNow();
            Webhook model = new Webhook()
            {
                uuid = Guid.NewGuid(),
                createdAt = today,
                provider = SystemProviders.SYNCFY.GetDisplayName(),
                response = JsonConvert.SerializeObject(response.ToString()),
            };

            try
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                var eventWh = responseData.First(x => x.Key == "event").Value;
                model.eventWebhook = eventWh.ToString();

                var endpointsWh = responseData.First(x => x.Key == "endpoints").Value;
                var endpoints = JsonConvert.DeserializeObject<Dictionary<string, object>>(endpointsWh.ToString());

                if (eventWh.ToString() == "credential_create" || eventWh.ToString() == "credential_update")
                {
                    var credentials = endpoints.FirstOrDefault(x => x.Key == "credential");
                    if (credentials.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(credentials.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }
                }               
                else if (eventWh.ToString() == "refresh")
                {                    
                    var accounts = endpoints.FirstOrDefault(x => x.Key == "accounts");
                    if (accounts.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(accounts.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var attachments = endpoints.FirstOrDefault(x => x.Key == "attachments");
                    if (attachments.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(attachments.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var credentials = endpoints.FirstOrDefault(x => x.Key == "credential");
                    if (credentials.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(credentials.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var transactions = endpoints.FirstOrDefault(x => x.Key == "transactions");
                    if (transactions.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(transactions.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }
                }
                else
                {
                    //result = "Event unidentified";
                    _webhookService.Create(model);
                }
                               
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception Ex)
            {
                model.endpoint = Ex.Message.ToString();
                _webhookService.Create(model);
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SatwsExtractionHandler")]
        public HttpResponseMessage SatwsExtractionHandler(Object webhookEventModel)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<WebhookEventModel>(webhookEventModel.ToString());

                if (data != null && data.data != null && data.type == SatwsEvent.EXTRACTION_UPDATED.GetDisplayName() &&
                    (data.data.@object.status == SatwsStatusEvent.FINISHED.GetDisplayName() || data.data.@object.status == SatwsStatusEvent.FAILED.GetDisplayName()))
                {
                    var account = _accountService.FirstOrDefault(x => x.rfc == data.data.@object.taxpayer.id);

                    if (account == null)
                        throw new Exception("No existe rfc a procesar");

                    var diagnostic = _diagnosticService.FirstOrDefault(x => x.account.id == account.id && x.status == SystemStatus.PENDING.ToString());

                    if (diagnostic == null)
                        throw new Exception("No existe diagnostico");

                    if (data.data.@object.status == SatwsStatusEvent.FAILED.GetDisplayName())
                    {

                        diagnostic.businessName = data.data.@object.taxpayer.name;
                        diagnostic.status = SystemStatus.FAILED.ToString();
                        diagnostic.modifiedAt = DateTime.Now;
                        _diagnosticService.Update(diagnostic);
                        throw new Exception("El servicio de satws generó un fallo en la extracción");
                    }
                    try
                    {
                        account.name = data.data.@object.taxpayer.name;
                        _accountService.Update(account);

                        diagnostic.businessName = data.data.@object.taxpayer.name;
                        diagnostic.status = SystemStatus.PROCESSING.ToString();
                        diagnostic.modifiedAt = DateTime.Now;
                        _diagnosticService.Update(diagnostic);

                        var modelInvoices = SATService.GetInvoicesByExtractions(data.data.@object.taxpayer.id, data.data.@object.options.period.from, data.data.@object.options.period.to, "SATWS");

                        //Crear clientes

                        List<string> customerRfcs = modelInvoices.Customers.Select(c => c.rfc).Distinct().ToList();
                        var ExistC = _customerService.ValidateRFC(customerRfcs, account.id);
                        List<string> NoExistC = customerRfcs.Except(ExistC).ToList();

                        List<Customer> customers = modelInvoices.Customers
                            .Where(x => NoExistC.Contains(x.rfc)).GroupBy(x => new { x.rfc, x.businessName })
                            .Select(x => new Customer
                            {
                                uuid = Guid.NewGuid(),
                                account = account,
                                zipCode = x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault() != null ? x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault().zipCode : null,
                                businessName = x.Key.businessName,
                                rfc = x.Key.rfc,
                                createdAt = DateUtil.GetDateTimeNow(),
                                modifiedAt = DateUtil.GetDateTimeNow(),
                                status = SystemStatus.ACTIVE.ToString()
                            }).ToList();

                        _customerService.Create(customers);

                        //Crear proveedores
                        List<string> providersRfcs = modelInvoices.Providers.Select(c => c.rfc).Distinct().ToList();
                        var ExistP = _providerService.ValidateRFC(providersRfcs, account.id);
                        List<string> NoExistP = providersRfcs.Except(ExistP).ToList();

                        List<Provider> providers = modelInvoices.Providers
                            .Where(x => NoExistP.Contains(x.rfc)).GroupBy(x => new { x.rfc, x.businessName })
                            .Select(x => new Provider
                            {
                                uuid = Guid.NewGuid(),
                                account = account,
                                zipCode = x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault() != null ? x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault().zipCode : null,
                                businessName = x.Key.businessName,
                                rfc = x.Key.rfc,
                            //taxRegime = 
                            createdAt = DateUtil.GetDateTimeNow(),
                                modifiedAt = DateUtil.GetDateTimeNow(),
                                status = SystemStatus.ACTIVE.ToString()
                            }).Distinct().ToList();

                        _providerService.Create(providers);

                        var provider = ConfigurationManager.AppSettings["SATProvider"];

                        List<string> IdIssued = modelInvoices.Customers.Select(x => x.idInvoice).ToList();
                        var invoicesIssuedExist = _invoicesIssuedService.FindBy(x => x.account.id == account.id).Select(x => x.uuid.ToString()).ToList();

                        IdIssued = IdIssued.Except(invoicesIssuedExist).ToList();

                        /*Obtener los CFDI's*/
                        var customersCFDI = SATService.GetCFDIs(IdIssued, provider);
                        var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                        List<InvoiceIssued> invoiceIssued = new List<InvoiceIssued>();
                        foreach (var cfdi in customersCFDI)
                        {
                            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(cfdi.Xml);
                            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                            var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesIssued, account.rfc);

                            invoiceIssued.Add(new InvoiceIssued
                            {
                                uuid = Guid.Parse(cfdi.id),
                                folio = cfdi.Folio,
                                serie = cfdi.Serie,
                                paymentMethod = cfdi.MetodoPago,
                                paymentForm = cfdi.FormaPago,
                                currency = cfdi.Moneda,
                                iva = modelInvoices.Customers.FirstOrDefault(y => y.idInvoice == cfdi.id).tax,
                                invoicedAt = cfdi.Fecha,
                                xml = upload.Item1,
                                createdAt = DateTime.Now,
                                modifiedAt = DateTime.Now,
                                status = IssueStatus.STAMPED.ToString(),
                                account = account,
                                customer = _customerService.FirstOrDefault(y => y.rfc == cfdi.Receptor.Rfc),
                                invoiceType = cfdi.TipoDeComprobante,
                                subtotal = cfdi.SubTotal,
                                total = cfdi.Total,
                                homemade = false
                            });
                        }

                        _invoicesIssuedService.Create(invoiceIssued);

                        List<string> IdReceived = modelInvoices.Providers.Select(x => x.idInvoice).ToList();
                        var invoicesReceivedExist = _invoicesReceivedService.FindBy(x => x.account.id == account.id).Select(x => x.uuid.ToString()).ToList();

                        IdReceived = IdReceived.Except(invoicesReceivedExist).ToList();

                        /*Obtener los CFDI's*/
                        var providersCFDI = SATService.GetCFDIs(IdReceived, provider);
                        var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                        List<InvoiceReceived> invoiceReceiveds = new List<InvoiceReceived>();
                        foreach (var cfdi in providersCFDI)
                        {
                            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(cfdi.Xml);
                            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                            var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesReceived, account.rfc);

                            invoiceReceiveds.Add(new InvoiceReceived
                            {
                                uuid = Guid.Parse(cfdi.id),
                                folio = cfdi.Folio,
                                serie = cfdi.Serie,
                                paymentMethod = cfdi.MetodoPago,
                                paymentForm = cfdi.FormaPago,
                                currency = cfdi.Moneda,
                                iva = modelInvoices.Providers.FirstOrDefault(y => y.idInvoice == cfdi.id).tax,
                                invoicedAt = cfdi.Fecha,
                                xml = upload.Item1,
                                createdAt = DateTime.Now,
                                modifiedAt = DateTime.Now,
                                status = IssueStatus.STAMPED.ToString(),
                                account = account,
                                provider = _providerService.FirstOrDefault(y => y.rfc == cfdi.Emisor.Rfc),
                                invoiceType = cfdi.TipoDeComprobante,
                                subtotal = cfdi.SubTotal,
                                total = cfdi.Total,
                                homemade = false
                            });
                        }

                        _invoicesReceivedService.Create(invoiceReceiveds);

                        DateTime from = DateTime.Parse(data.data.@object.options.period.from);
                        DateTime to = DateTime.Parse(data.data.@object.options.period.to);

                        List<DiagnosticDetail> details = new List<DiagnosticDetail>();

                        IdIssued = modelInvoices.Customers.Select(x => x.idInvoice).ToList();
                        var invoicesIssued = _invoicesIssuedService.FindBy(x => x.account.id == account.id && x.invoicedAt >= from && x.invoicedAt <= to).ToList();

                        details.AddRange(invoicesIssued
                            .GroupBy(x => new
                        {
                            x.invoicedAt.Year,
                            x.invoicedAt.Month,
                        })
                        .Select(b => new DiagnosticDetail()
                        {
                            diagnostic = diagnostic,
                            year = b.Key.Year,
                            month = b.Key.Month,
                            typeTaxPayer = TypeIssuerReceiver.ISSUER.ToString(),
                            numberCFDI = b.Count(),
                            totalAmount = b.Sum(y => y.total),
                            createdAt = DateUtil.GetDateTimeNow()
                        }));

                        IdReceived = modelInvoices.Providers.Select(x => x.idInvoice).ToList();
                        var invoicesReceived = _invoicesReceivedService.FindBy(x => x.account.id == account.id && x.invoicedAt >= from && x.invoicedAt <= to).ToList();

                        details.AddRange(invoicesReceived
                            .Where(x=>x.invoiceType != TipoComprobante.N.ToString()) //Si es solo ingreso en las factura descomentar esta linea
                            .GroupBy(x => new
                        {
                            x.invoicedAt.Year,
                            x.invoicedAt.Month,
                        })
                        .Select(b => new DiagnosticDetail()
                        {
                            diagnostic = diagnostic,
                            year = b.Key.Year,
                            month = b.Key.Month,
                            typeTaxPayer = TypeIssuerReceiver.RECEIVER.ToString(),
                            numberCFDI = b.Count(),
                            totalAmount = b.Sum(y => y.total),
                            createdAt = DateUtil.GetDateTimeNow()
                        }));
                        _diagnosticDetailService.Create(details);

                        var taxStatusModel = SATService.GetTaxStatus(account.rfc, provider);
                        if (!taxStatusModel.Success)
                            throw new Exception(taxStatusModel.Message);

                        var taxStatus = taxStatusModel.TaxStatus
                            .Select(x => new DiagnosticTaxStatus
                            {
                                diagnostic = diagnostic,
                                createdAt = DateUtil.GetDateTimeNow(),
                                statusSAT = x.status,
                                businessName = x.person != null ? x.person.fullName : x.company.tradeName,
                                taxMailboxEmail = x.email,
                                taxRegime = x.taxRegimes.Count > 0 ? String.Join(",", x.taxRegimes.Select(y => y.name).ToArray()) : null,
                                economicActivities = x.economicActivities.Count > 0 ? String.Join(",", x.economicActivities.Select(y => y.name).ToArray()) : null
                            }).ToList();
                        _diagnosticTaxStatusService.Create(taxStatus);

                        diagnostic.taxStatus = taxStatus;
                        diagnostic.status = SystemStatus.ACTIVE.ToString();
                        diagnostic.modifiedAt = DateTime.Now;

                        _diagnosticService.Update(diagnostic);

                        LogUtil.AddEntry(descripcion: "Extraccón finalizada con exito " + account.rfc, eLogLevel: ENivelLog.Debug,
                        usuarioId: (Int64)1, usuario: "Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());
                    }
                    catch(Exception ex)
                    {
                        diagnostic.status = SystemStatus.FAILED.ToString();
                        diagnostic.modifiedAt = DateTime.Now;
                        _diagnosticService.Update(diagnostic);
                        LogUtil.AddEntry(descripcion: "Error al generar el diagnostico fiscal " + account.rfc + " Error: " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                        usuarioId: (Int64)1, usuario: "Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la extracción " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SatwsCredentialUpdateHandler")]
        public HttpResponseMessage SatwsCredentialUpdateHandler(Object webhookEventModel)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<WebhookEventModel>(webhookEventModel.ToString());

                if (data != null && data.data != null && data.data.@object != null && data.type == SatwsEvent.CREDENTIAL_UPDATE.GetDisplayName())
                {
                    var credential = _credentialService.FirstOrDefault(x => x.idCredentialProvider == data.data.@object.id);
                    if (credential == null)
                        throw new Exception("No existe una credencial para el rfc " + data.data.@object.rfc);

                    credential.statusProvider = data.data.@object.status;
                    
                    switch (data.data.@object.status)
                    {
                        case "pending":
                            break;
                        case "valid":
                            credential.status = SystemStatus.ACTIVE.ToString();
                            break;
                        case "invalid":
                            credential.status = SystemStatus.INACTIVE.ToString();
                            break;
                        case "deactivated":
                            credential.status = SystemStatus.INACTIVE.ToString();
                            break;
                        case "error":
                            credential.status = SystemStatus.INACTIVE.ToString();
                            break;
                        default:
                            credential.status = SystemStatus.INACTIVE.ToString();
                            break;
                    }

                    _credentialService.Update(credential);

                    LogUtil.AddEntry(descripcion: "Credencial actualizadá con exito", eLogLevel: ENivelLog.Debug,
                    usuarioId: (Int64)1, usuario: "Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: webhookEventModel.ToString());
                }
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la actualización de credenciales " + ex.Message, eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: webhookEventModel.ToString());

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
