using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
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
using System.Net.Http;
using System.Text;
using System.Threading;

namespace MVC_Project.Jobs
{
    public class RecurlyInvoicingJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static NotificationService _notificationService;
        static IProviderService _providerService;
        static InvoiceReceivedService _invoicesReceivedService;
        static RecurlyPaymentService _recurlyPaymentService;

        private static IDriveKeyService _driveKeyService;
        static IInvoiceEmissionParametersService _invoiceEmissionParametersService;
        private static HttpClient _client;

        static readonly string JOB_CODE = "RecurlyJob_IssueInvoices";

        public static void IssueInvoices()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));
            _invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));
            _driveKeyService = new DriveKeyService(new Repository<DriveKey>(_unitOfWork));
            _invoiceEmissionParametersService = new InvoiceEmissionParametersService(new Repository<InvoiceEmissionParameters>(_unitOfWork));
            _notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));
            _providerService = new ProviderService(new Repository<Provider>(_unitOfWork));

            _client = new HttpClient();

            Process processJob = _processService.GetByCode(JOB_CODE);
            bool CAN_EXECUTE = processJob != null && processJob.Status && !processJob.Running; //Esta habilitado y no está corriendo (validacion por BD)

            ProcessExecution processExecution = new ProcessExecution
            {
                Process = processJob,
                StartAt = DateUtil.GetDateTimeNow(),
                Status = true
            };
            _processService.CreateExecution(processExecution);

            Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["Jobs.EnabledJobs"], out bool NotificationProcessEnabled);
            StringBuilder strResult = new StringBuilder();

            if (Monitor.TryEnter(thisLock))
            {
                try
                {
                    if (!executing && NotificationProcessEnabled && CAN_EXECUTE)
                    {
                        processJob.Running = true;
                        _processService.Update(processJob);
                        System.Diagnostics.Trace.TraceInformation(string.Format("[RecurlyJob_CreateAccounts] Executing at {0}", DateTime.Now));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var invoicingParameters = _invoiceEmissionParametersService.FirstOrDefault(x => x.status == SystemStatus.ACTIVE.ToString());
                        var provider = ConfigurationManager.AppSettings["SATProvider"];
                        var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                        var serverAcces = ConfigurationManager.AppSettings["_UrlServerAccess"];
                        _client.BaseAddress = new Uri(serverAcces);

                        if (invoicingParameters != null)
                        {
                            var successPaymentStatus = RecurlyPaymentStatus.SUCCESS.GetDisplayName();
                            var stampedStatus = IssueStatus.STAMPED.ToString();
                            var satUnit = _driveKeyService.GetDriveKey(invoicingParameters.claveUnidad).FirstOrDefault();
                            var pendingInvoicePayments = _recurlyPaymentService.FindBy(x => x.statusCode == successPaymentStatus && (x.stampStatus != stampedStatus || x.stampStatus == null) && x.stampAttempt < 3);

                            foreach (var payment in pendingInvoicePayments)
                            {
                                var subscription = payment.subscription;
                                var account = payment.subscription.account;

                                var issuer = new InvoiceIssuer
                                {
                                    Rfc = invoicingParameters.rfcEmisor,
                                    Nombre = invoicingParameters.nombreEmisor,
                                    RegimenFiscal = invoicingParameters.regimenEmisor,
                                };

                                var receiver = new InvoiceReceiver
                                {
                                    Rfc = account.rfc,
                                    Nombre = account.name,
                                    UsoCFDI = invoicingParameters.usoCFDIReceptor
                                };

                                var conceptsData = new ConceptsData
                                {
                                    ClaveProdServ = invoicingParameters.claveProdServ,
                                    Cantidad = 1,
                                    ClaveUnidad = invoicingParameters.claveUnidad,
                                    Descripcion = subscription.planName,
                                    ValorUnitario = payment.total,
                                    Importe = payment.total,
                                };

                                if (satUnit != null && satUnit.name.Count() <= 20)
                                {
                                    conceptsData.Unidad = satUnit.name;
                                }

                                var conceptos = new List<ConceptsData>();
                                conceptos.Add(conceptsData);

                                InvoiceData invoiceData = invoiceData = new InvoiceData
                                {
                                    Serie = invoicingParameters.serie,
                                    Folio = invoicingParameters.folio,
                                    Fecha = DateUtil.GetDateTimeNow().ToString("s"),
                                    Moneda = invoicingParameters.moneda,
                                    TipoDeComprobante = "I",
                                    FormaPago = invoicingParameters.formaPago,
                                    MetodoPago = invoicingParameters.metodoPago,
                                    LugarExpedicion = invoicingParameters.lugarExpedicion,
                                    Emisor = issuer,
                                    Receptor = receiver,
                                    Conceptos = conceptos
                                };

                                var invoice = new InvoiceJson
                                {
                                    data = invoiceData
                                };

                                var serilaizeJson = JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    });

                                dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                                InvoicesInfo stampResult = null;

                                try
                                {
                                    stampResult = SATService.PostIssueIncomeInvoices(invoiceSend, provider);
                                }
                                catch (Exception ex)
                                {

                                    payment.stampAttempt += 1;
                                    payment.stampStatus = SystemStatus.FAILED.ToString();
                                    payment.stampStatusMessage = ex.Message;
                                    _recurlyPaymentService.Update(payment);
                                }

                                if (stampResult != null)
                                {
                                    invoicingParameters.folio += 1;
                                    _invoiceEmissionParametersService.Update(invoicingParameters);

                                    payment.stampStatus = IssueStatus.STAMPED.ToString();
                                    payment.stampStatusMessage = stampResult.status;

                                    try
                                    {
                                        List<string> IdIssued = new List<string>();

                                        IdIssued.Add(stampResult.uuid.ToString());

                                        /*Obtener los CFDI's*/
                                        var providersCFDI = SATService.GetCFDIs(IdIssued, provider);
                                        var CFDIReceived = providersCFDI[0];

                                        if (CFDIReceived != null)
                                        {
                                            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(CFDIReceived.Xml);
                                            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                                            var upload = AzureBlobService.UploadPublicFile(stream, CFDIReceived.id + ".xml", StorageInvoicesReceived, account.rfc);

                                            var invoiceReceived = new InvoiceReceived
                                            {
                                                uuid = Guid.Parse(CFDIReceived.id),
                                                folio = CFDIReceived.Folio,
                                                serie = CFDIReceived.Serie,
                                                paymentMethod = CFDIReceived.MetodoPago,
                                                paymentForm = CFDIReceived.FormaPago,
                                                currency = CFDIReceived.Moneda,
                                                iva = stampResult.tax.GetValueOrDefault(),
                                                invoicedAt = CFDIReceived.Fecha,
                                                xml = upload.Item1,
                                                createdAt = DateUtil.GetDateTimeNow(),
                                                modifiedAt = DateUtil.GetDateTimeNow(),
                                                status = IssueStatus.STAMPED.ToString(),
                                                account = account,
                                                provider = _providerService.FirstOrDefault(y => y.rfc == CFDIReceived.Emisor.Rfc),
                                                invoiceType = CFDIReceived.TipoDeComprobante,
                                                subtotal = CFDIReceived.SubTotal,
                                                total = CFDIReceived.Total,
                                                homemade = true
                                            };

                                            _invoicesReceivedService.Create(invoiceReceived);
                                            payment.invoice = invoiceReceived;

                                            var taskResponse = _client.GetAsync($"Invoicing/GetAsPDFContent?id={invoiceReceived.id}&type=RECEIVED&rfc={account.rfc}");
                                            taskResponse.Wait();
                                            var result = taskResponse.Result;
                                            var resultTask = result.Content.ReadAsByteArrayAsync();
                                            resultTask.Wait();
                                            var contentBytes = resultTask.Result;

                                            System.IO.MemoryStream contentStream = new System.IO.MemoryStream(contentBytes);

                                            var uploadPDF = AzureBlobService.UploadPublicFile(contentStream, invoiceReceived.uuid + ".pdf", StorageInvoicesReceived, account.rfc);

                                            if (!string.IsNullOrEmpty(payment.email))
                                            {
                                                SendInvoice(payment.email, account.rfc, account.name, "", invoiceReceived.xml, uploadPDF.Item1);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        if (account != null)
                                        {
                                            var notification = new Notification
                                            {
                                                uuid = Guid.NewGuid(),
                                                account = account,
                                                createdAt = DateUtil.GetDateTimeNow(),
                                                status = NotificationStatus.ACTIVE.ToString(),
                                                message = "Factura timbrada con éxito, pero hubo un error: " + ex.Message.ToString()
                                            };
                                            _notificationService.Create(notification);
                                        }
                                    }

                                    _recurlyPaymentService.Update(payment);
                                }
                            }

                            var failedInvoices = _recurlyPaymentService.FindBy(x => x.statusCode == successPaymentStatus && x.stampStatus != stampedStatus && x.stampAttempt >= 3);

                            if (failedInvoices.Count() > 0)
                            {
                                var paymentsTable = BuildPaymentsTable(failedInvoices);
                                SendErrorsNotification(invoicingParameters.errosNotificationEmail, paymentsTable);
                            }
                        }

                        #endregion

                        //UPDATE JOB DATABASE
                        strResult.Append(string.Format("| End at {0}", DateUtil.GetDateTimeNow()));
                        processExecution.EndAt = DateUtil.GetDateTimeNow();
                        processExecution.Status = false;
                        processExecution.Success = true;
                        processExecution.Result = strResult.ToString();
                        _processService.UpdateExecution(processExecution);

                        processJob.Running = false;
                        processJob.Result = strResult.ToString();
                        processJob.LastExecutionAt = DateUtil.GetDateTimeNow();
                        _processService.Update(processJob);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Trace.TraceInformation(ex.Message);
                    if (processJob != null)
                    {
                        strResult.Append(string.Format("ERROR: {0}", ex.Message));

                        processExecution.EndAt = DateUtil.GetDateTimeNow();
                        processExecution.Status = false;
                        processExecution.Success = false;
                        processExecution.Result = strResult.ToString();
                        _processService.UpdateExecution(processExecution);

                        processJob.Running = false;
                        processJob.Result = strResult.ToString();
                        processJob.LastExecutionAt = DateUtil.GetDateTimeNow();
                        _processService.Update(processJob);
                    }
                }
                finally
                {
                    executing = false;
                    Monitor.Exit(thisLock);
                }
            }
        }

        private static string BuildPaymentsTable(IEnumerable<RecurlyPayment> failedInvoices)
        {
            var cellStyle = "border-top: 1px solid #dee2e6; padding: 5px;";
            var tableStyle = "width: 100%; text-align: center;";

            string table = $"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"{tableStyle}\">" +
                "<thead><tr>" +
                $"<th>RFC</th>" +
                $"<th>Plan</th>" +
                $"<th>Total</th>" +
                $"<th>Fecha de pago</th>" +
                "</tr></thead>";

            foreach (var payment in failedInvoices)
            {
                var paymentAccount = payment.subscription?.account;
                var paymentSubscription = payment.subscription;
                table += $"<tr><td style=\"{cellStyle}\">{paymentAccount?.rfc}</td>" +
                    $"<td style=\"{cellStyle}\">{paymentSubscription?.planName}</td>" +
                    $"<td style=\"{cellStyle}\">{payment.total.ToString("F02")}</td>" +
                    $"<td style=\"{cellStyle}\">{payment.createdAt.ToString("dd/MM/yyyy HH:mm")}</td></tr>";
            }

            table += "</table>";
            return table;
        }

        private static void SendInvoice(string email, string rfc, string businessName, string comments, string linkXml, string linkPdf)
        {
            Dictionary<string, string> customParams = new Dictionary<string, string>();
            customParams.Add("param_rfc", rfc);
            customParams.Add("param_razon_social", businessName);
            customParams.Add("param_comentarios", comments);

            customParams.Add("param_link_xml", linkXml);
            customParams.Add("param_link_pdf", linkPdf);
            NotificationUtil.SendNotification(email, customParams, Constants.NOT_TEMPLATE_RECURLY_INVOICING);
        }

        private static void SendErrorsNotification(string email, string paymentsListContent)
        {
            Dictionary<string, string> customParams = new Dictionary<string, string>();
            customParams.Add("param_payments_table", paymentsListContent);
            NotificationUtil.SendNotification(email, customParams, Constants.NOT_TEMPLATE_RECURLY_INVOICING_ERRORS);
        }
    }
}