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
        static InvoiceIssuedService _invoicesIssuedService;
        static InvoiceReceivedService _invoicesReceivedService;
        static NotificationService _notificationService;

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
                        System.Diagnostics.Trace.TraceInformation(string.Format("[SATJob_SyncBills] Executing at {0}", DateTime.Now));
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
                                webhookProcess.modifiedAt = DateTime.Now;
                                _webhookProcessService.Update(webhookProcess);

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
                                        taxRegime = x.Key.rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
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
                                        taxRegime = x.Key.rfc.Length == 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
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

                                webhookProcess.status = SystemStatus.ACTIVE.ToString();
                                webhookProcess.modifiedAt = DateTime.Now;
                                _webhookProcessService.Update(webhookProcess);

                                var notification = new Notification
                                {
                                    uuid = Guid.NewGuid(),
                                    account = account,
                                    createdAt = DateTime.Now,
                                    status = NotificationStatus.ACTIVE.ToString(),
                                    message = "Se han sincronizado sus facturas " + DateUtil.GetDateTimeNow().ToShortDateString()
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
                                            createdAt = DateTime.Now,
                                            status = NotificationStatus.ACTIVE.ToString(),
                                            message = "Ocurrio un problema con la sincronización de sus facturas del día: " + DateUtil.GetDateTimeNow().ToShortDateString()
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