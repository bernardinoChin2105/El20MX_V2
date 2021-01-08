//using LogHubSDK.Models;
using LogHubSDK.Models;
using Microsoft.Web.Http;
using MVC_Project.API.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
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
using System.Xml;

namespace MVC_Project.API.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/v{version:apiVersion}/Webhooks")]
    [EnableCors(origins:"*", headers:"*", methods:"*")]
    public class WebhooksController : ApiController
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IWebhookProcessService _webhookProcessService;

        private IRecurlySubscriptionService _recurlySubscriptionService;
        private IRecurlyPaymentService _recurlyPaymentService;
        private IRecurlyInvoiceService _recurlyInvoiceService;

        public WebhooksController(IAccountService accountService, ICredentialService credentialService, IWebhookProcessService webhookProcessService,
            IRecurlySubscriptionService recurlySubscriptionService, IRecurlyPaymentService recurlyPaymentService, IRecurlyInvoiceService recurlyInvoiceService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _webhookProcessService = webhookProcessService;
            _recurlySubscriptionService = recurlySubscriptionService;
            _recurlyPaymentService = recurlyPaymentService;
            _recurlyInvoiceService = recurlyInvoiceService;
        }

        [HttpPost]
        [Route("WebhookSyncfy")]
        public HttpResponseMessage WebhookSyncfy(Object syncfyWebhookModel)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<SyncfyWebhookModel>(syncfyWebhookModel.ToString());
                try
                {
                    var credential = _credentialService.FirstOrDefault(x => x.idCredentialProvider == response.id_user && x.provider == SystemProviders.SYNCFY.ToString());
                    if (credential == null)
                        throw new Exception("No existe la credencial a procesar, id_user: " + response.id_user);

                    var organizationSAT = ConfigurationManager.AppSettings["Paybook.OrganizationSite.SAT"];

                    if (response.@event == "refresh" && response.id_site_organization != organizationSAT)
                    {
                        var process = new WebhookProcess()
                        {
                            uuid = Guid.NewGuid(),
                            processId = response.id_job,
                            provider = SystemProviders.SYNCFY.ToString(),
                            @event = SyncfyEvent.REFRESH.ToString(),
                            reference = credential.account.uuid.ToString(),
                            createdAt = DateUtil.GetDateTimeNow(),
                            status = SystemStatus.PENDING.ToString(),
                            content = syncfyWebhookModel.ToString()
                        };

                        _webhookProcessService.Create(process);

                        DeleteSpecialCharacters(response);
                        LogUtil.AddEntry(descripcion: "Actualización realizada con exito SYNCFY " + credential.account.rfc, eLogLevel: ENivelLog.Debug,
                        usuarioId: (Int64)1, usuario: "Syncfy Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "WebhookSyncfy", detalle: JsonConvert.SerializeObject(response));
                    }
                }
                catch (Exception ex)
                {
                    DeleteSpecialCharacters(response);
                    LogUtil.AddEntry(descripcion: "Error en la actualizacion de SYNCFY " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                              usuarioId: (Int64)1, usuario: "Syncfy Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "WebhookSyncfy", detalle: JsonConvert.SerializeObject(response));
                }
            }
            catch(Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la conversion en la respuesta de SYNCFY " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                              usuarioId: (Int64)1, usuario: "Syncfy Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "WebhookSyncfy", detalle: "");
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void DeleteSpecialCharacters(SyncfyWebhookModel response)
        {
            if (response.endpoints.transactions != null && response.endpoints.transactions.Any())
                for (int i = 0; i < response.endpoints.transactions.Count; i++)
                    response.endpoints.transactions[i] = response.endpoints.transactions[i].Replace("?", "").Replace("&", "");

            if (response.endpoints.accounts != null && response.endpoints.accounts.Any())
                for (int i = 0; i < response.endpoints.accounts.Count; i++)
                    response.endpoints.accounts[i] = response.endpoints.accounts[i].Replace("?", "").Replace("&", "");

            if (response.endpoints.attachments != null && response.endpoints.attachments.Any())
                for (int i = 0; i < response.endpoints.attachments.Count; i++)
                    response.endpoints.attachments[i] = response.endpoints.attachments[i].Replace("?", "").Replace("&", "");

            if (response.endpoints.credential != null && response.endpoints.credential.Any())
                for (int i = 0; i < response.endpoints.credential.Count; i++)
                    response.endpoints.credential[i] = response.endpoints.credential[i].Replace("?", "").Replace("&", "");
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

                    if (data.data.@object.status == SatwsStatusEvent.FAILED.GetDisplayName())
                        throw new Exception("El servicio de satws generó un fallo en la extracción");

                    try
                    {
                        var process = _webhookProcessService.FirstOrDefault(x => x.processId == data.data.@object.id);
                        if (process == null)
                        {
                            process = new WebhookProcess()
                            {
                                uuid = Guid.NewGuid(),
                                processId = data.data.@object.id,
                                provider = SystemProviders.SATWS.ToString(),
                                @event = SatwsEvent.EXTRACTION_UPDATED.ToString(),
                                reference = account.uuid.ToString(),
                                createdAt = DateUtil.GetDateTimeNow(),
                                status = SystemStatus.PENDING.ToString(),
                                content = webhookEventModel.ToString()
                            };

                            _webhookProcessService.Create(process);

                            LogUtil.AddEntry(descripcion: "Extraccón finalizada con exito " + account.rfc, eLogLevel: ENivelLog.Debug,
                            usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.AddEntry(descripcion: "Error al generar el diagnostico fiscal " + account.rfc + " Error: " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                        usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la extracción " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsExtractionHandler", detalle: webhookEventModel.ToString());

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
                var provider = ConfigurationManager.AppSettings["SATProvider"];

                var data = JsonConvert.DeserializeObject<WebhookEventModel>(webhookEventModel.ToString());

                if (data != null && data.data != null && data.data.@object != null && data.type == SatwsEvent.CREDENTIAL_UPDATE.GetDisplayName())
                {
                    var credential = _credentialService.FirstOrDefault(x => x.idCredentialProvider == data.data.@object.id);
                    if (credential == null)
                        throw new Exception("No existe una credencial para el rfc " + data.data.@object.rfc);

                    credential.statusProvider = data.data.@object.status;
                    bool isUpdated = true;
                    bool isValid = false;
                    switch (data.data.@object.status)
                    {
                        case "pending":
                            isUpdated = false;
                            break;
                        case "valid":
                            credential.status = SystemStatus.ACTIVE.ToString();
                            isValid = true;
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

                    if (isUpdated)
                    {
                        _credentialService.Update(credential);

                        LogUtil.AddEntry(descripcion: "Credencial actualizadá con exito. New status:" + data.data.@object.status, eLogLevel: ENivelLog.Debug,
                        usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: webhookEventModel.ToString());
                    }
                    if (isValid)
                    {
                        SATService.GenerateTaxStatus(credential.account.rfc, provider);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la actualización de credenciales " + ex.Message, eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: webhookEventModel.ToString());

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SatwsTaxStatusHandler")]
        public HttpResponseMessage SatwsTaxStatusHandler(Object taxStatusEventModel)
        {
            try
            {
                LogUtil.AddEntry(descripcion: "TaxStatus", eLogLevel: ENivelLog.Debug,
                usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: taxStatusEventModel.ToString());
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en la actualización de credenciales " + ex.Message, eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Sat.ws Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "SatwsCredentialUpdateHandler", detalle: taxStatusEventModel.ToString());

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        
        [HttpPost]
        [AllowAnonymous]
        [Route("RecurlyUpdatesWebhook")]
        public HttpResponseMessage RecurlyUpdatesWebhook([FromBody]string webhookEventModel)
        {
            try
            {
                var task = Request.Content.ReadAsStreamAsync();
                task.Wait();
                var contentStream = task.Result;
                contentStream.Position = 0;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(contentStream);
                contentStream.Dispose();

                var contentNode = xmlDoc.ChildNodes[1];

                var accountCode = contentNode.SelectSingleNode("account").SelectSingleNode("account_code").InnerText;
                RecurlySubscription recurlySubscription = null;

                switch (contentNode.Name)
                {
                    case "successful_payment_notification":
                        recurlySubscription = _recurlySubscriptionService.FirstOrDefault(x => x.account.uuid.ToString().ToLower() == accountCode.ToLower() && x.status == "ACTIVE");
                        var transactionId = contentNode.SelectSingleNode("transaction").SelectSingleNode("id").InnerText;
                        var storedPaymentTransaction = _recurlyPaymentService.FirstOrDefault(x => x.transactionId == transactionId);
                        if(storedPaymentTransaction == null)
                        {
                            var amountInCents = contentNode.SelectSingleNode("transaction").SelectSingleNode("amount_in_cents").InnerText;
                            var total = int.Parse(amountInCents) / 100M;
                            var recurlyPayment = new RecurlyPayment
                            {
                                createdAt = DateTime.Parse(contentNode.SelectSingleNode("transaction").SelectSingleNode("date").InnerText),
                                subscription = recurlySubscription,
                                subtotal = total,
                                total = total,
                                paymentGateway = contentNode.SelectSingleNode("transaction").SelectSingleNode("gateway").InnerText,
                                customerMessage = "",
                                statusCode = contentNode.SelectSingleNode("transaction").SelectSingleNode("status").InnerText,
                                statusMessage = contentNode.SelectSingleNode("transaction").SelectSingleNode("message").InnerText,
                                email = contentNode.SelectSingleNode("account").SelectSingleNode("email").InnerText,
                                transactionId = transactionId
                            };
                            _recurlyPaymentService.Create(recurlyPayment);
                        }
                        break;
                    case "paid_charge_invoice_notification":
                        recurlySubscription = _recurlySubscriptionService.FirstOrDefault(x => x.account.uuid.ToString().ToLower() == accountCode.ToLower() && x.status == "ACTIVE");
                        var invoiceNumber = contentNode.SelectSingleNode("invoice").SelectSingleNode("invoice_number").InnerText;
                        var storedPaymentInvoice = _recurlyInvoiceService.FirstOrDefault(x => x.invoiceId == invoiceNumber);
                        if(storedPaymentInvoice == null)
                        {
                            var createdAt = DateTime.Parse(contentNode.SelectSingleNode("invoice").SelectSingleNode("created_at").InnerText);
                            var recurlyInvoice = new RecurlyInvoice
                            {
                                createdAt = createdAt,
                                mounth = createdAt.Month.ToString(),
                                year = createdAt.Year.ToString(),
                                uuid = Guid.NewGuid(),
                                subscription = recurlySubscription,
                                totalInvoice = 0,
                                totalInvoiceIssued = 0,
                                totalInvoiceReceived = 0,
                                extraBills = 0,
                                invoiceId = invoiceNumber
                            };
                            _recurlyInvoiceService.Create(recurlyInvoice);

                            if(recurlySubscription != null)
                            {
                                recurlySubscription.state = contentNode.SelectSingleNode("invoice").SelectSingleNode("state").InnerText;
                                recurlySubscription.modifiedAt = DateUtil.GetDateTimeNow();
                                _recurlySubscriptionService.Update(recurlySubscription);
                            }
                        }
                        break;
                    case "failed_payment_notification":
                        recurlySubscription = _recurlySubscriptionService.FirstOrDefault(x => x.account.uuid.ToString().ToLower() == accountCode.ToLower() && x.status == "ACTIVE");
                        var failedTransactionId = contentNode.SelectSingleNode("transaction").SelectSingleNode("id").InnerText;
                        var amountTransactionInCents = contentNode.SelectSingleNode("transaction").SelectSingleNode("amount_in_cents").InnerText;
                        var totalAmount = int.Parse(amountTransactionInCents) / 100M;
                        var recurlyFailedPayment = new RecurlyPayment
                        {
                            createdAt = DateTime.Parse(contentNode.SelectSingleNode("transaction").SelectSingleNode("date").InnerText),
                            subscription = recurlySubscription,
                            subtotal = totalAmount,
                            total = totalAmount,
                            paymentGateway = contentNode.SelectSingleNode("transaction").SelectSingleNode("gateway").InnerText,
                            customerMessage = "",
                            statusCode = contentNode.SelectSingleNode("transaction").SelectSingleNode("status").InnerText,
                            statusMessage = contentNode.SelectSingleNode("transaction").SelectSingleNode("message").InnerText,
                            transactionId = failedTransactionId
                        };
                        _recurlyPaymentService.Create(recurlyFailedPayment);

                        if (recurlySubscription != null)
                        {
                            recurlySubscription.state = contentNode.SelectSingleNode("transaction").SelectSingleNode("status").InnerText;
                            recurlySubscription.modifiedAt = DateUtil.GetDateTimeNow();
                            _recurlySubscriptionService.Update(recurlySubscription);
                        }
                        break;
                    case "expired_subscription_notification":
                        var planCode = contentNode.SelectSingleNode("subscription").SelectSingleNode("plan").SelectSingleNode("plan_code").InnerText;
                        recurlySubscription = _recurlySubscriptionService.FirstOrDefault(x => x.account.uuid.ToString().ToLower() == accountCode.ToLower() && x.planCode == planCode && x.status == "ACTIVE");
                        if(recurlySubscription != null)
                        {
                            var subscriptionState = contentNode.SelectSingleNode("subscription").SelectSingleNode("state").InnerText;
                            recurlySubscription.state = subscriptionState;
                            recurlySubscription.status = SystemStatus.CANCELLED.ToString();
                            recurlySubscription.modifiedAt = DateUtil.GetDateTimeNow();

                            _recurlySubscriptionService.Update(recurlySubscription);
                        }
                        break;
                }

                LogUtil.AddEntry(descripcion: "Recurly webhook ejecutado con exito", eLogLevel: ENivelLog.Debug,
                    usuarioId: (Int64)1, usuario: "Recurly.Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: contentNode.Name, detalle: contentNode.OuterXml);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(descripcion: "Error en webhook recurly: " + ex.Message, eLogLevel: ENivelLog.Debug,
                       usuarioId: (Int64)1, usuario: "Recurly.Webhook", eOperacionLog: EOperacionLog.AUTHORIZATION, parametros: "", modulo: "Recurly.Webhook", detalle: "");

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
