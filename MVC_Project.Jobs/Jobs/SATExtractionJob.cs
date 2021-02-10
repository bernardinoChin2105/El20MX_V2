using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
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
using System.Text;
using System.Threading;
using System.Web;

namespace MVC_Project.Jobs
{
    public class SATExtractionJob
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
        static INotificationService _notificationService;
        static ICredentialService _credentialService;
        static ISATExtractionProcessService _satExtractionProcessService;

        static readonly string JOB_CODE = "SATExtractionJob_InvoiceExtractions";

        public static void InvoiceExtractions()
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
            _credentialService = new CredentialService(new Repository<Credential>(_unitOfWork));
            _satExtractionProcessService = new SATExtractionProcessService(new Repository<SATExtractionProcess>(_unitOfWork));

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
                        bool.TryParse(ConfigurationManager.AppSettings["SATws.IsHistoricalExtraction"], out bool isHistorical);
                        var provider = ConfigurationManager.AppSettings["SATProvider"];

                        List<Credential> credentials = _credentialService.
                            FindBy(x => x.credentialType == SATCredentialType.CIEC.ToString() &&
                            x.provider == SystemProviders.SATWS.ToString() &&
                            x.status == SystemStatus.ACTIVE.ToString() &&
                            (x.account.status == SystemStatus.ACTIVE.ToString() || x.account.status == SystemStatus.CONFIRMED.ToString())).
                            OrderBy(x => x.id).
                            ToList();

                        DateTime dateTo = DateUtil.GetDateTimeNow();
                        DateTime dateFrom = DateTime.UtcNow.AddMonths(-1);
                        dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);

                        if (isHistorical)
                        {
                            dateFrom = new DateTime(2014, 1, 1);

                            var accountsProcessed = _satExtractionProcessService.
                            FindBy(x => x.isHistorical).
                            Select(x => x.account.id);

                            credentials = credentials.
                                Where(x => !accountsProcessed.Contains(x.account.id)).
                                OrderBy(x => x.id).
                                Take(5).
                                ToList();
                        }

                        foreach (var credential in credentials)
                        {
                            SATExtractionProcess sATExtractionProcess = null;
                            try
                            {
                                string extractionId = SATService.GenerateExtractions(credential.account.rfc, dateFrom, dateTo, provider);
                                sATExtractionProcess = new SATExtractionProcess
                                {
                                    uuid = Guid.NewGuid(),
                                    processId = extractionId,
                                    provider = SystemProviders.SATWS.ToString(),
                                    createdAt = DateUtil.GetDateTimeNow(),
                                    @event = SatwsExtractionsTypes.INVOICE.ToString(),
                                    isHistorical = isHistorical,
                                    status = SystemStatus.ACTIVE.ToString(),
                                    account = credential.account
                                };
                            }
                            catch (Exception ex)
                            {
                                sATExtractionProcess = new SATExtractionProcess
                                {
                                    uuid = Guid.NewGuid(),
                                    provider = SystemProviders.SATWS.ToString(),
                                    createdAt = DateUtil.GetDateTimeNow(),
                                    @event = SatwsExtractionsTypes.INVOICE.ToString(),
                                    isHistorical = isHistorical,
                                    status = SystemStatus.FAILED.ToString(),
                                    account = credential.account,
                                    result = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty)
                                };

                            }
                            _satExtractionProcessService.Create(sATExtractionProcess);
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