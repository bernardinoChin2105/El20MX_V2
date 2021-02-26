using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.ContaLink;
using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using static MVC_Project.Integrations.ContaLink.ContaLinkModels;

namespace MVC_Project.Jobs
{
    public class ContalinkInvoicingLoad
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IAccountService _accountService;
        static InvoiceIssuedService _invoicesIssuedService;
        static InvoiceReceivedService _invoicesReceivedService;

        static readonly string JOB_CODE = "ContalinkJob_InvoicingLoad";

        public static void InvoicingLoad()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));
            _invoicesIssuedService = new InvoiceIssuedService(new Repository<InvoiceIssued>(_unitOfWork));
            _invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));

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

                        var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                        var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                        var provider = ConfigurationManager.AppSettings["ContaLinkProvider"];

                        var accounts = _accountService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString()
                        && x.credentials.Any(y => y.provider == SystemProviders.CONTALINK.ToString() && y.status == SystemStatus.ACTIVE.ToString()));

                        foreach(var account in accounts)
                        {
                            var credential = account.credentials.FirstOrDefault(y => y.provider == SystemProviders.CONTALINK.ToString() && y.status == SystemStatus.ACTIVE.ToString());

                            var issueds = _invoicesIssuedService.FindBy(x => x.account.id == account.id && x.xml != null && x.xml.Length > 0
                            && (x.loadStatus == null || x.loadStatus != SystemStatus.LOADED.ToString()));

                            foreach (var issued in issueds)
                            {
                                try
                                {
                                    MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoicesIssued, account.rfc + "/" + issued.uuid + ".xml");
                                    stream.Position = 0;
                                    byte[] result = stream.ToArray();
                                    var body = new InvoiceUpload { name = issued.uuid.ToString(), xml = Convert.ToBase64String(result) };
                                    var response = ContaLinkService.InvoiceUpload(body, provider, credential?.idCredentialProvider);
                                    issued.loadStatus = response.status == 1 ? SystemStatus.LOADED.ToString() : SystemStatus.FAILED.ToString();
                                    issued.loadResponse = response.status == 1 ? response.result.url : response.message;
                                    issued.modifiedAt = DateUtil.GetDateTimeNow();
                                }
                                catch (Exception ex)
                                {
                                    issued.loadStatus = SystemStatus.FAILED.ToString();
                                    issued.loadResponse = ex.Message;
                                    issued.modifiedAt = DateUtil.GetDateTimeNow();
                                }
                                _invoicesIssuedService.Update(issueds);
                            }

                            var receiveds = _invoicesReceivedService.FindBy(x => x.account.id == account.id && x.xml != null && x.xml.Length > 0
                            && (x.loadStatus == null || x.loadStatus != SystemStatus.LOADED.ToString()));

                            foreach (var received in receiveds)
                            {
                                try
                                {
                                    MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoicesReceived, account.rfc + "/" + received.uuid + ".xml");
                                    stream.Position = 0;
                                    byte[] result = stream.ToArray();
                                    var body = new InvoiceUpload { name = received.uuid.ToString(), xml = Convert.ToBase64String(result) };
                                    var response = ContaLinkService.InvoiceUpload(body, provider, credential?.idCredentialProvider);
                                    received.loadStatus = response.status == 1 ? SystemStatus.LOADED.ToString() : SystemStatus.FAILED.ToString();
                                    received.loadResponse = response.status == 1 ? response.result.url : response.message;
                                    received.modifiedAt = DateUtil.GetDateTimeNow();
                                }
                                catch (Exception ex)
                                {
                                    received.loadStatus = SystemStatus.FAILED.ToString();
                                    received.loadResponse = ex.Message;
                                    received.modifiedAt = DateUtil.GetDateTimeNow();
                                }
                                _invoicesReceivedService.Update(received);
                            }
                            
                        }
                        
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