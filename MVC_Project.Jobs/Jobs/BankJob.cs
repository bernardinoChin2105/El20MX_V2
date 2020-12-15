using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace MVC_Project.Jobs
{
    public class BankJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IWebhookProcessService _webhookProcessService;
        static IAccountService _accountService;
        static NotificationService _notificationService;
        static BankService _bankService;
        static BankCredentialService _bankCredentialService;
        static CredentialService _credentialService;
        static BankAccountService _bankAccountService;
        static BankTransactionService _bankTransactionService;

        static readonly string JOB_CODE = "BankJob_SyncAccounts";

        public static void SyncAccounts()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _webhookProcessService = new WebhookProcessService(new Repository<WebhookProcess>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));
            _notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));
            _bankService = new BankService(new Repository<Bank>(_unitOfWork));
            _bankCredentialService = new BankCredentialService(new Repository<BankCredential>(_unitOfWork));
            _credentialService = new CredentialService(new Repository<Credential>(_unitOfWork));
            _bankAccountService = new BankAccountService(new Repository<BankAccount>(_unitOfWork));
            _bankTransactionService = new BankTransactionService(new Repository<BankTransaction>(_unitOfWork));
            
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
                        System.Diagnostics.Trace.TraceInformation(string.Format("[BankJob_SyncAccounts] Executing at {0}", DateTime.Now));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var webhookProcesses = _webhookProcessService.FindBy(x => x.provider == SystemProviders.SYNCFY.ToString() 
                        && x.@event == SyncfyEvent.REFRESH.ToString()
                        && x.status == SystemStatus.PENDING.ToString());

                        foreach (var webhookProcess in webhookProcesses)
                        {
                            Account account = null;
                            try
                            {
                                var response = JsonConvert.DeserializeObject<SyncfyWebhookModel>(webhookProcess.content.ToString());

                                var credential = _credentialService.FirstOrDefault(x => x.idCredentialProvider == response.id_user && x.provider == SystemProviders.SYNCFY.ToString());
                                if (credential == null)
                                    throw new Exception("Credencial de usuario no encontrada en el sistema, id_user: " + response.id_user);

                                account = credential.account;
                                var token = PaybookService.CreateToken(response.id_user);

                                var bankCredential = _bankCredentialService.FirstOrDefault(x => x.credentialProviderId == response.id_credential && x.account.id == credential.account.id && x.status == SystemStatus.ACTIVE.ToString());
                                if (bankCredential == null)
                                {
                                    var paybookCredentials = PaybookService.GetCredentials(response.id_credential, token);
                                    if (!paybookCredentials.Any())
                                        throw new Exception("Credencial bancaria no encontrada en Syncfy, id_credential: " + response.id_credential);

                                    var paybookCredential = paybookCredentials.FirstOrDefault();

                                    var bank = _bankService.FirstOrDefault(x => x.providerSiteId == response.id_site);

                                    if (bank == null)
                                    {
                                        var paybookBanks = PaybookService.GetBanksSites(response.id_site_organization, token);
                                        if (!paybookBanks.Any())
                                            throw new Exception("Organización bancaria no encontrada en Syncfy, id_site_organization: " + response.id_site_organization);

                                        var paybookBank = paybookBanks.FirstOrDefault();
                                        if (paybookBank.sites.Any())
                                            throw new Exception("La organización bancaria no cuenta con sitios en Syncfy, id_site_organization: " + response.id_site_organization);
                                        var site = paybookBank.sites.FirstOrDefault(x => x.id_site == response.id_site);
                                        if (site != null)
                                            throw new Exception("El sitio bancario no se encuentra en la organización en Syncfy, id_site: " + response.id_site);

                                        bank = new Bank
                                        {
                                            uuid = Guid.NewGuid(),
                                            name = paybookBank.name,
                                            providerId = paybookBank.id_site_organization,
                                            nameSite = site.name,
                                            providerSiteId = site.id_site,
                                            createdAt = DateUtil.GetDateTimeNow(),
                                            modifiedAt = DateUtil.GetDateTimeNow(),
                                            status = SystemStatus.ACTIVE.ToString(),
                                        };
                                        _bankService.Create(bank);
                                    }

                                    bankCredential = new BankCredential()
                                    {
                                        uuid = Guid.NewGuid(),
                                        account = account,
                                        credentialProviderId = paybookCredential.id_credential,
                                        createdAt = DateUtil.GetDateTimeNow(),
                                        modifiedAt = DateUtil.GetDateTimeNow(),
                                        status = paybookCredential.is_authorized != null ? (paybookCredential.is_authorized.Value.ToString() == "1" ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()) : SystemStatus.INACTIVE.ToString(),
                                        bank = bank,
                                        isTwofa = Convert.ToBoolean(paybookCredential.is_twofa),
                                        dateTimeAuthorized = DateUtil.UnixTimeToDateTime(paybookCredential.dt_authorized.Value),
                                        dateTimeRefresh = DateUtil.UnixTimeToDateTime(paybookCredential.dt_refresh.Value)
                                    };
                                    _bankCredentialService.Create(bankCredential);
                                }
                                else
                                {
                                    var paybookCredentials = PaybookService.GetCredentials(response.id_credential, token);
                                    if (!paybookCredentials.Any())
                                        throw new Exception("Credencial bancaria no encontrada en Syncfy" + response.id_credential);

                                    var paybookCredential = paybookCredentials.FirstOrDefault();
                                    if (paybookCredential.dt_authorized.HasValue)
                                        bankCredential.dateTimeAuthorized = DateUtil.UnixTimeToDateTime(paybookCredential.dt_authorized.Value);
                                    if (paybookCredential.dt_refresh.HasValue)
                                        bankCredential.dateTimeRefresh = DateUtil.UnixTimeToDateTime(paybookCredential.dt_refresh.Value);
                                    _bankCredentialService.Update(bankCredential);
                                }

                                var paybookAccounts = PaybookService.GetAccounts(response.id_credential, token);
                                foreach (var paybookAccount in paybookAccounts)
                                {
                                    var bankAccount = _bankAccountService.FirstOrDefault(x => x.accountProviderId == paybookAccount.id_account);
                                    if (bankAccount == null)
                                    {
                                        bankAccount = new BankAccount()
                                        {
                                            uuid = Guid.NewGuid(),
                                            bankCredential = bankCredential,
                                            accountProviderId = paybookAccount.id_account,
                                            accountProviderType = paybookAccount.account_type,
                                            name = paybookAccount.name,
                                            currency = paybookAccount.currency,
                                            balance = paybookAccount.balance,
                                            number = paybookAccount.number,
                                            isDisable = paybookAccount.is_disable,
                                            refreshAt = DateUtil.UnixTimeToDateTime(paybookAccount.dt_refresh),
                                            createdAt = DateUtil.GetDateTimeNow(),
                                            modifiedAt = DateUtil.GetDateTimeNow(),
                                            status = SystemStatus.ACTIVE.ToString()
                                        };
                                        _bankAccountService.Create(bankAccount);
                                    }
                                    else
                                    {
                                        bankAccount.balance = paybookAccount.balance;
                                        bankAccount.modifiedAt = DateUtil.GetDateTimeNow();
                                        _bankAccountService.Update(bankAccount);
                                    }
                                }
                                if (response.endpoints.transactions != null && response.endpoints.transactions.Any())
                                {
                                    foreach (string uri in response.endpoints.transactions)
                                    {
                                        var bankTransactions = new List<BankTransaction>();
                                        var paybookTransactions = PaybookService.GetTransactions(uri.Replace("/v1", ""), token);
                                        foreach (var paybookTransaction in paybookTransactions)
                                        {
                                            var bankAccount = _bankAccountService.FirstOrDefault(x => x.accountProviderId == paybookTransaction.id_account);
                                            BankTransaction bankTransaction = _bankTransactionService.FirstOrDefault(x => x.transactionId == paybookTransaction.id_transaction && x.bankAccount.id == bankAccount.id);
                                            if (bankTransaction == null)
                                            {
                                                //long d_rt = itemTransaction.dt_refresh;
                                                DateTime date_refresht = DateUtil.UnixTimeToDateTime(paybookTransaction.dt_refresh);
                                                DateTime date_transaction = DateUtil.UnixTimeToDateTime(paybookTransaction.dt_transaction);

                                                bankTransaction = new BankTransaction()
                                                {
                                                    uuid = Guid.NewGuid(),
                                                    bankAccount = bankAccount,
                                                    transactionId = paybookTransaction.id_transaction,
                                                    description = paybookTransaction.description,
                                                    amount = paybookTransaction.amount,
                                                    currency = paybookTransaction.currency,
                                                    reference = paybookTransaction.reference,
                                                    transactionAt = date_transaction,
                                                    createdAt = DateUtil.GetDateTimeNow(),
                                                    modifiedAt = DateUtil.GetDateTimeNow(),
                                                    status = SystemStatus.ACTIVE.ToString()
                                                };
                                                bankTransactions.Add(bankTransaction);
                                            }
                                        }
                                        if (bankTransactions.Any())
                                            _bankTransactionService.Create(bankTransactions);
                                    }
                                }

                                webhookProcess.status = SystemStatus.ACTIVE.ToString();
                                webhookProcess.modifiedAt = DateUtil.GetDateTimeNow();
                                _webhookProcessService.Update(webhookProcess);

                                var notification = new Notification
                                {
                                    uuid = Guid.NewGuid(),
                                    account = account,
                                    createdAt = DateUtil.GetDateTimeNow(),
                                    status = NotificationStatus.ACTIVE.ToString(),
                                    message = "Se han sincronizado sus cuentas bancarias " + DateUtil.GetDateTimeNow().ToShortDateString()
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
                                            message = "Ocurrio un problema con la sincronización de sus cuentas bancarias " + DateUtil.GetDateTimeNow().ToShortDateString()
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