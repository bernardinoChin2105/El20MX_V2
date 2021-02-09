using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Recurly;
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
        static IProviderService _providerService;
        static ICustomerService _customerService;
        static InvoiceReceivedService _invoicesReceivedService;
        static InvoiceIssuedService _invoiceIssuedService;
        static RecurlyPaymentService _recurlyPaymentService;
        static IAccountService _accountService;

        private static IDriveKeyService _driveKeyService;
        static IInvoiceEmissionParametersService _invoiceEmissionParametersService;
        private static HttpClient _client;

        static readonly string JOB_CODE = "RecurlyJob_IssueInvoices";

        public static void IssueInvoices()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));

            _invoiceEmissionParametersService = new InvoiceEmissionParametersService(new Repository<InvoiceEmissionParameters>(_unitOfWork));
            _driveKeyService = new DriveKeyService(new Repository<DriveKey>(_unitOfWork));

            _invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));
            _invoiceIssuedService = new InvoiceIssuedService(new Repository<InvoiceIssued>(_unitOfWork));
            
            _providerService = new ProviderService(new Repository<Provider>(_unitOfWork));
            _customerService = new CustomerService(new Repository<Customer>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));

            _recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));

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
                        var satProvider = ConfigurationManager.AppSettings["SATProvider"];
                        var recurlyProvider = ConfigurationManager.AppSettings["RecurlyProvider"];

                        var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                        var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];

                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];
                        var serverAcces = ConfigurationManager.AppSettings["_UrlServerAccess"];

                        _client.BaseAddress = new Uri(serverAcces);

                        var invoicingParameters = _invoiceEmissionParametersService.FirstOrDefault(x => x.status == SystemStatus.ACTIVE.ToString());
                        if (invoicingParameters == null)
                            throw new Exception("No se encontraron parámetros de facturación");
                        
                        var satUnit = _driveKeyService.GetDriveKey(invoicingParameters.claveUnidad).FirstOrDefault();

                        if (satUnit == null)
                            throw new Exception("No se encontró la 'Clave de Unidad' en la base de datos");

                        var successPaymentStatus = RecurlyPaymentStatus.SUCCESS.GetDisplayName();
                        var stampedStatus = IssueStatus.STAMPED.ToString();

                        var pendingInvoicePayments = _recurlyPaymentService.FindBy(x => x.statusCode == successPaymentStatus &&
                        x.account != null && x.invoiceNumber != null && x.invoiceNumber.Length > 0 &&
                        (x.stampStatus != stampedStatus || x.stampStatus == null) && x.stampAttempt < 3);

                        foreach (var payment in pendingInvoicePayments)
                        {
                            try
                            {
                                var recurlyInvoice = RecurlyService.GetInvoice(payment.invoiceNumber, siteId, recurlyProvider);

                                var issuer = new InvoiceIssuer
                                {
                                    Rfc = invoicingParameters.rfcEmisor,
                                    Nombre = invoicingParameters.nombreEmisor,
                                    RegimenFiscal = invoicingParameters.regimenEmisor,
                                };

                                var receiver = new InvoiceReceiver
                                {
                                    Rfc = payment.account.rfc,
                                    Nombre = payment.account.name,
                                    UsoCFDI = invoicingParameters.usoCFDIReceptor
                                };
                                var conceptos = new List<ConceptsData>();

                                var lineItemsApproved = recurlyInvoice.LineItems.Data.Where(x => x.UnitAmount.HasValue && x.UnitAmount.Value > 0);
                                foreach (var lineItem in lineItemsApproved)
                                {
                                    var conceptsData = new ConceptsData
                                    {
                                        ClaveProdServ = invoicingParameters.claveProdServ,
                                        ClaveUnidad = invoicingParameters.claveUnidad,
                                        Unidad = satUnit.name,
                                        Descripcion = lineItem.Description,
                                        Cantidad = lineItem.Quantity.HasValue ? lineItem.Quantity.Value : 1,
                                        ValorUnitario = lineItem.UnitAmount.Value,
                                        Importe = lineItem.Subtotal.Value,
                                        Descuento = lineItem.Discount.Value
                                    };
                                    if (lineItem.Taxable.Value && !lineItem.TaxExempt.Value)
                                    {
                                        var partialTotal = lineItem.Subtotal.Value - lineItem.Discount.Value;
                                        var taxRate = lineItem.TaxInfo != null ? lineItem.TaxInfo.Rate.Value : (decimal)0.00;
                                        var taxImport = partialTotal * taxRate;

                                        conceptsData.Traslados = new List<Traslados>();
                                        conceptsData.Traslados.Add(new Traslados
                                        {
                                            Base = partialTotal.ToString("0.000000"),
                                            Impuesto = "002",
                                            TipoFactor = "Tasa",
                                            TasaOCuota = taxRate.ToString("0.000000"),
                                            Importe = taxImport.ToString("0.000000")
                                        });
                                    }
                                    conceptos.Add(conceptsData);
                                }

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

                                var invoice = new InvoiceJson { data = invoiceData };

                                var serilaizeJson = JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                                dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                                InvoicesInfo stampResult = SATService.PostIssueIncomeInvoices(invoiceSend, satProvider);

                                payment.stampStatus = IssueStatus.STAMPED.ToString();
                                payment.stampStatusMessage = stampResult.status;
                                _recurlyPaymentService.Update(payment);

                                invoicingParameters.folio += 1;
                                _invoiceEmissionParametersService.Update(invoicingParameters);

                                try
                                {
                                    var provider = _providerService.FirstOrDefault(y => y.account.id == payment.account.id && y.rfc == issuer.Rfc);
                                    if (provider == null)
                                    {
                                        provider = new Provider
                                        {
                                            uuid = Guid.NewGuid(),
                                            account = payment.account,
                                            zipCode = invoicingParameters.lugarExpedicion,
                                            businessName = invoicingParameters.nombreEmisor,
                                            rfc = issuer.Rfc,
                                            taxRegime = issuer.Rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
                                            createdAt = DateUtil.GetDateTimeNow(),
                                            modifiedAt = DateUtil.GetDateTimeNow(),
                                            status = SystemStatus.ACTIVE.ToString()
                                        };
                                        _providerService.Create(provider);
                                    }

                                    var xml = SATService.GetXMLInvoice(stampResult.uuid, satProvider);

                                    byte[] byteArray = Encoding.UTF8.GetBytes(xml);
                                    System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                                    var upload = AzureBlobService.UploadPublicFile(stream, stampResult.uuid + ".xml", StorageInvoicesReceived, payment.account.rfc);

                                    var invoiceReceived = new InvoiceReceived
                                    {
                                        uuid = Guid.Parse(stampResult.uuid),
                                        folio = stampResult.internalIdentifier,
                                        serie = stampResult.reference,
                                        paymentMethod = stampResult.paymentType,
                                        paymentForm = stampResult.paymentMethod,
                                        currency = stampResult.currency,
                                        iva = stampResult.tax.GetValueOrDefault(),
                                        invoicedAt = stampResult.certifiedAt,
                                        xml = upload.Item1,
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = IssueStatus.STAMPED.ToString(),
                                        account = payment.account,
                                        provider = provider,
                                        invoiceType = stampResult.type,
                                        subtotal = stampResult.subtotal.Value,
                                        total = stampResult.total
                                    };

                                    _invoicesReceivedService.Create(invoiceReceived);
                                    payment.invoiceReceived = invoiceReceived;
                                    _recurlyPaymentService.Update(payment);

                                    var taskResponse = _client.GetAsync($"Invoicing/GetAsPDFContent?id={invoiceReceived.id}&type=RECEIVED&rfc={payment.account.rfc}");
                                    taskResponse.Wait();
                                    var result = taskResponse.Result;
                                    var resultTask = result.Content.ReadAsByteArrayAsync();
                                    resultTask.Wait();
                                    var contentBytes = resultTask.Result;

                                    System.IO.MemoryStream contentStream = new System.IO.MemoryStream(contentBytes);

                                    var uploadPDF = AzureBlobService.UploadPublicFile(contentStream, invoiceReceived.uuid + ".pdf", StorageInvoicesReceived, payment.account.rfc);

                                    if (!string.IsNullOrEmpty(payment.email))
                                        SendInvoice(payment.email, payment.account.rfc, payment.account.name, "", invoiceReceived.xml, uploadPDF.Item1);
                                    
                                    var el20mx = _accountService.FirstOrDefault(x => x.rfc == issuer.Rfc);
                                    if (el20mx != null)
                                    {
                                        var customer = _customerService.FirstOrDefault(y => y.id == el20mx.id && y.rfc == payment.account.rfc);
                                        if (customer == null)
                                        {
                                            customer = new Customer
                                            {
                                                uuid = Guid.NewGuid(),
                                                account = el20mx,
                                                businessName = payment.account.name,
                                                rfc = payment.account.rfc,
                                                taxRegime = payment.account.rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
                                                createdAt = DateUtil.GetDateTimeNow(),
                                                modifiedAt = DateUtil.GetDateTimeNow(),
                                                status = SystemStatus.ACTIVE.ToString()
                                            };
                                            _customerService.Create(customer);
                                        }

                                        byteArray = Encoding.UTF8.GetBytes(xml);
                                        stream = new System.IO.MemoryStream(byteArray);
                                        upload = AzureBlobService.UploadPublicFile(stream, stampResult.uuid + ".xml", StorageInvoicesIssued, el20mx.rfc);

                                        var invoiceIssued = new InvoiceIssued
                                        {
                                            uuid = Guid.Parse(stampResult.uuid),
                                            folio = stampResult.internalIdentifier,
                                            serie = stampResult.reference,
                                            paymentMethod = stampResult.paymentType,
                                            paymentForm = stampResult.paymentMethod,
                                            currency = stampResult.currency,
                                            iva = stampResult.tax.GetValueOrDefault(),
                                            invoicedAt = stampResult.certifiedAt,
                                            xml = upload.Item1,
                                            createdAt = DateUtil.GetDateTimeNow(),
                                            modifiedAt = DateUtil.GetDateTimeNow(),
                                            status = IssueStatus.STAMPED.ToString(),
                                            account = el20mx,
                                            customer = customer,
                                            invoiceType = stampResult.type,
                                            subtotal = stampResult.subtotal.Value,
                                            total = stampResult.total,
                                            homemade = true
                                        };

                                        _invoiceIssuedService.Create(invoiceIssued);
                                        payment.invoiceIssued = invoiceIssued;
                                        _recurlyPaymentService.Update(payment);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    payment.stampStatusMessage = "Se timbró la factura pero no se guardó en el sistema. " + ex.Message;
                                    _recurlyPaymentService.Update(payment);
                                }
                            }
                            catch (InvoiceException ex)
                            {
                                var messages = string.Join(" ", ex.invoiceResponse?.violations?.Select(x => x.message));
                                payment.stampAttempt += 1;
                                payment.stampStatus = SystemStatus.FAILED.ToString();
                                payment.stampStatusMessage = messages;
                                _recurlyPaymentService.Update(payment);
                            }
                            catch (Exception ex)
                            {
                                payment.stampAttempt += 1;
                                payment.stampStatus = SystemStatus.FAILED.ToString();
                                payment.stampStatusMessage = ex.Message;
                                _recurlyPaymentService.Update(payment);
                            }
                        }

                        var failedInvoices = _recurlyPaymentService.FindBy(x => x.statusCode == successPaymentStatus && x.stampStatus != stampedStatus && x.stampAttempt >= 3);

                        if (failedInvoices.Count() > 0)
                        {
                            var paymentsTable = BuildPaymentsTable(failedInvoices);
                            SendErrorsNotification(invoicingParameters.errosNotificationEmail, paymentsTable);
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