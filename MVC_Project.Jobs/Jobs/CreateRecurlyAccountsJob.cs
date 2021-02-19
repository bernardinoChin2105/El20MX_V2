using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Integrations.Recurly.Models;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace MVC_Project.Jobs
{
    public class CreateRecurlyAccountsJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IAccountService _accountService;
        static CredentialService _credentialService;

        static InvoiceIssuedService _invoicesIssuedService;
        static InvoiceReceivedService _invoicesReceivedService;

        static RecurlySubscriptionService _recurlySubscriptionService;
        static RecurlyInvoiceService _recurlyInvoiceService;
        static RecurlyPaymentService _recurlyPaymentService;

        static readonly string JOB_CODE = "RecurlyJob_CreateAccounts";

        public static void CreateAccounts()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _accountService = new AccountService(new Repository<Domain.Entities.Account>(_unitOfWork));
            _recurlyInvoiceService = new RecurlyInvoiceService(new Repository<RecurlyInvoice>(_unitOfWork));
            _recurlySubscriptionService = new RecurlySubscriptionService(new Repository<RecurlySubscription>(_unitOfWork));
            _recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));
            _invoicesIssuedService = new InvoiceIssuedService(new Repository<InvoiceIssued>(_unitOfWork));
            _invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));
            _credentialService = new CredentialService(new Repository<Credential>(_unitOfWork));

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
                        var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                        //Obtener la lista de usuarios activo

                        List<Integrations.Recurly.Models.Account> recurlyAccountsList = new List<Integrations.Recurly.Models.Account>();
                        var accountsResponse = RecurlyService.GetAccounts(siteId);
                        recurlyAccountsList.AddRange(accountsResponse.data);

                        while (accountsResponse.has_more)
                        {
                            accountsResponse = RecurlyService.GetNextAccountsPage(accountsResponse.next);
                            recurlyAccountsList.AddRange(accountsResponse.data);
                        }

                        var storedAccounts = _accountService.
                            FindBy(x => (x.status == SystemStatus.ACTIVE.ToString() || x.status == SystemStatus.CONFIRMED.ToString()) &&
                            !x.credentials.Any(y => y.provider == SystemProviders.RECURLY.ToString()));

                        foreach (var account in storedAccounts)
                        {
                            if (!recurlyAccountsList.Any(x => x.Code.ToLower() == account.uuid.ToString().ToLower()))
                            {
                                CreateAccountModel newAccount = new CreateAccountModel();
                                DateTime todayDate = DateUtil.GetDateTimeNow();

                                    newAccount.code = account.uuid.ToString();
                                    newAccount.username = account.rfc; //Se agrego el RFC para diferenciar si los nombres de usuario
                                    newAccount.preferred_locale = "es-MX";
                                    newAccount.company = account.name;

                                    var membership = account.memberships.FirstOrDefault(x => x.role.code == SystemRoles.ACCOUNT_OWNER.ToString());

                                    if (membership != null && membership.user != null)
                                    {
                                        newAccount.email = membership.user.name;
                                        newAccount.first_name = membership.user.profile?.firstName;
                                        newAccount.last_name = membership.user.profile?.lastName;
                                    newAccount.address = new AddressModel { phone = membership.user.profile?.phoneNumber, country = "MX" };
                                    }

                                    var accountRecurly = RecurlyService.CreateAccount(newAccount, siteId, provider);

                                    if (accountRecurly != null)
                                    {
                                        var credential = new Domain.Entities.Credential()
                                        {
                                            account = new Domain.Entities.Account { id = account.id },
                                            uuid = Guid.NewGuid(),
                                            provider = provider,
                                            idCredentialProvider = accountRecurly.id,
                                            statusProvider = accountRecurly.state,
                                            createdAt = todayDate,
                                            modifiedAt = todayDate,
                                            status = SystemStatus.ACTIVE.ToString(),
                                            credentialType = accountRecurly.hosted_login_token //Token para la pagina
                                        };

                                    _credentialService.Create(credential);
                                }
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