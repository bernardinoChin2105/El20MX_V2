using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.SAT;
using MVC_Project.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Configuration;
using MVC_Project.Integrations.Storage;

namespace MVC_Project.Jobs
{
    public class SATJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IWebhookProcessService _webhookProcessService;
        static IAccountService _accountService;
        static ICustomerService _customerService;
        static IProviderService _providerService;
        static IInvoiceIssuedService _invoicesIssuedService;
        static IInvoiceReceivedService _invoicesReceivedService;
        static INotificationService _notificationService;

        static readonly string JOB_CODE = "SATJob_SyncBills";

        public static void SyncBills()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _webhookProcessService = new WebhookProcessService(new Repository<WebhookProcess>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));
            _customerService = new CustomerService(new Repository<Customer>(_unitOfWork));
            _providerService = new ProviderService(new Repository<Provider>(_unitOfWork));
            _invoicesIssuedService = new InvoiceIssuedService(new Repository<InvoiceIssued>(_unitOfWork));
            _invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));
            _notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));

            Process processJob = _processService.GetByCode(JOB_CODE);
            bool CAN_EXECUTE = processJob!=null && processJob.Status && !processJob.Running; //Esta habilitado y no está corriendo (validacion por BD)

            ProcessExecution processExecution = new ProcessExecution
            {
                Process = processJob,
                StartAt = DateUtil.GetDateTimeNow(),
                Status = true
            };
            _processService.CreateExecution(processExecution);

            Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["Jobs.EnabledJobs"], out bool NotificationProcessEnabled);
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Jobs.Attempt"], out int attempt);
            StringBuilder strResult = new StringBuilder();

            if (Monitor.TryEnter(thisLock))
            {
                try
                {
                    if (!executing && NotificationProcessEnabled && CAN_EXECUTE)
                    {
                        processJob.Running = true;
                        _processService.Update(processJob);
                        System.Diagnostics.Trace.TraceInformation(string.Format("[SATJob_SyncBills] Executing at {0}", DateUtil.GetDateTimeNow()));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var webhookProcesses = _webhookProcessService.FindBy(x => x.provider == SystemProviders.SATWS.ToString() 
                        && x.@event == SatwsEvent.EXTRACTION_UPDATED.ToString()
                        && x.status == SystemStatus.PENDING.ToString());

                        foreach (var webhookProcess in webhookProcesses)
                        {
                            Account account = null;
                            try
                            {
                                var data = JsonConvert.DeserializeObject<WebhookEventModel>(webhookProcess.content.ToString());
                                account = _accountService.FirstOrDefault(x => x.rfc == data.data.@object.taxpayer.id);
                                if (data.data.@object.taxpayer != null && !string.IsNullOrEmpty(data.data.@object.taxpayer.name))
                                {
                                    account.name = data.data.@object.taxpayer.name;
                                    _accountService.Update(account);
                                }
                                webhookProcess.status = SystemStatus.PROCESSING.ToString();
                                webhookProcess.modifiedAt = DateUtil.GetDateTimeNow();
                                _webhookProcessService.Update(webhookProcess);

                                var modelInvoices = SATService.GetInvoicesByExtractions(data.data.@object.taxpayer.id, data.data.@object.options.period.from, data.data.@object.options.period.to, "SATWS");

                                //Crear clientes
                                List<string> customerRfcs = modelInvoices.Customers.Select(c => c.rfc).Distinct().ToList();

                                var ExistC = from customer in _customerService.FindBy(x => x.account.id == account.id)
                                             join rfc in customerRfcs
                                             on customer.rfc equals rfc
                                             select customer.rfc;

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
                                        taxRegime = x.Key.rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = SystemStatus.ACTIVE.ToString()
                                    }).ToList();

                                _customerService.Create(customers);

                                //Crear proveedores
                                List<string> providersRfcs = modelInvoices.Providers.Select(c => c.rfc).Distinct().ToList();

                                var ExistP = from prov in _providerService.FindBy(x => x.account.id == account.id)
                                             join rfc in providersRfcs
                                             on prov.rfc equals rfc
                                             select prov.rfc;

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
                                        taxRegime = x.Key.rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = SystemStatus.ACTIVE.ToString()
                                    }).Distinct().ToList();

                                _providerService.Create(providers);

                                var provider = ConfigurationManager.AppSettings["SATProvider"];

                                var invoiceIssuedExists = from invoice in modelInvoices.Invoices.Where(x => x.issuer.rfc == account.rfc)
                                                          join all in _invoicesIssuedService.FindBy(x => x.account.id == account.id)
                                                          on invoice.id equals all.uuid.ToString()
                                                          select invoice;

                                var customerInvoices = modelInvoices.Invoices.Where(x => x.issuer.rfc == account.rfc).Except(invoiceIssuedExists);
                                var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                                List<InvoiceIssued> invoiceIssued = new List<InvoiceIssued>();
                                foreach (var cfdi in customerInvoices)
                                {
                                    var xml = SATService.GetXMLInvoice(cfdi.id, provider);
                                    if (!string.IsNullOrEmpty(xml))
                                    {
                                        byte[] byteArray = Encoding.UTF8.GetBytes(SATService.GetXMLInvoice(cfdi.id, provider));
                                        System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                                        var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesIssued, account.rfc);
                                        xml = upload.Item1;
                                    }
                                    invoiceIssued.Add(new InvoiceIssued
                                    {
                                        uuid = Guid.Parse(cfdi.id),
                                        folio = cfdi.internalIdentifier,
                                        serie = cfdi.reference,
                                        paymentMethod = cfdi.paymentType,
                                        paymentForm = cfdi.paymentMethod,
                                        currency = cfdi.currency,
                                        iva = cfdi.tax.HasValue ? cfdi.tax.Value : 0,
                                        invoicedAt = cfdi.certifiedAt,
                                        xml = xml,
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = cfdi.status == "CANCELADO" ? IssueStatus.CANCELED.ToString() : IssueStatus.STAMPED.ToString(),
                                        account = account,
                                        customer = _customerService.FirstOrDefault(y => y.rfc == cfdi.receiver.rfc),
                                        invoiceType = cfdi.type,
                                        subtotal = cfdi.subtotal.HasValue ? cfdi.subtotal.Value : 0,
                                        total = cfdi.total,
                                        homemade = false
                                    });
                                }
                                _invoicesIssuedService.Create(invoiceIssued);

                                var invoiceReceivedExists = from invoice in modelInvoices.Invoices.Where(x => x.receiver.rfc == account.rfc)
                                                            join all in _invoicesReceivedService.FindBy(x => x.account.id == account.id)
                                                            on invoice.id equals all.uuid.ToString()
                                                            select invoice;

                                var providerInvoices = modelInvoices.Invoices.Where(x => x.receiver.rfc == account.rfc).Except(invoiceReceivedExists);
                                var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                                List<InvoiceReceived> invoiceReceiveds = new List<InvoiceReceived>();
                                foreach (var cfdi in providerInvoices)
                                {
                                    var xml = SATService.GetXMLInvoice(cfdi.id, provider);
                                    if (!string.IsNullOrEmpty(xml))
                                    {
                                        byte[] byteArray = Encoding.UTF8.GetBytes(SATService.GetXMLInvoice(cfdi.id, provider));
                                        System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                                        var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesReceived, account.rfc);
                                        xml = upload.Item1;
                                    }
                                    invoiceReceiveds.Add(new InvoiceReceived
                                    {
                                        uuid = Guid.Parse(cfdi.id),
                                        folio = cfdi.internalIdentifier,
                                        serie = cfdi.reference,
                                        paymentMethod = cfdi.paymentType,
                                        paymentForm = cfdi.paymentMethod,
                                        currency = cfdi.currency,
                                        iva = cfdi.tax.HasValue ? cfdi.tax.Value : 0,
                                        invoicedAt = cfdi.certifiedAt,
                                        xml = xml,
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = cfdi.status == "CANCELADO" ? IssueStatus.CANCELED.ToString() : IssueStatus.STAMPED.ToString(),
                                        account = account,
                                        provider = _providerService.FirstOrDefault(y => y.rfc == cfdi.issuer.rfc),
                                        invoiceType = cfdi.type,
                                        subtotal = cfdi.subtotal.HasValue ? cfdi.subtotal.Value : 0,
                                        total = cfdi.total,
                                        homemade = false
                                    });
                                }
                                _invoicesReceivedService.Create(invoiceReceiveds);
                                
                                webhookProcess.status = SystemStatus.ACTIVE.ToString();
                                webhookProcess.modifiedAt = DateUtil.GetDateTimeNow();
                                _webhookProcessService.Update(webhookProcess);

                                var notification = new Notification
                                {
                                    uuid = Guid.NewGuid(),
                                    account = account,
                                    createdAt = DateUtil.GetDateTimeNow(),
                                    status = NotificationStatus.ACTIVE.ToString(),
                                    message = "Sincronización de facturas completado"
                                };
                                _notificationService.Create(notification);
                            }
                            catch (Exception ex)
                            {
                                webhookProcess.attempt += 1;
                                if (webhookProcess.attempt == attempt)
                                {
                                    webhookProcess.status = SystemStatus.FAILED.ToString();
                                    if (account != null)
                                    {
                                        var notification = new Notification
                                        {
                                            uuid = Guid.NewGuid(),
                                            account = account,
                                            createdAt = DateUtil.GetDateTimeNow(),
                                            status = NotificationStatus.ACTIVE.ToString(),
                                            message = "Ocurrio un problema al sincronizar sus facturas"
                                        };
                                        _notificationService.Create(notification);
                                    }
                                }
                                else
                                {
                                    webhookProcess.status = SystemStatus.PENDING.ToString();
                                }
                                webhookProcess.result = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                                webhookProcess.modifiedAt = DateUtil.GetDateTimeNow();
                                _webhookProcessService.Update(webhookProcess);
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
    }
}