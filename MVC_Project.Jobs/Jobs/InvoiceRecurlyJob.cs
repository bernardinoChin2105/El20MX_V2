using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.Recurly;
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
    public class InvoiceRecurlyJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IWebhookProcessService _webhookProcessService;
        static IAccountService _accountService;
        static NotificationService _notificationService;
        static CredentialService _credentialService;


        static RecurlySubscriptionService _recurlySubscriptionService;
        static RecurlyInvoiceService _recurlyInvoiceService;
        static RecurlyPaymentService _recurlyPaymentService;

        static readonly string JOB_CODE = "RecurlyJob_GenerateAccountStatement";

        public static void GenerateAccountStatement()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _webhookProcessService = new WebhookProcessService(new Repository<WebhookProcess>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));
            _notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));
            _recurlyInvoiceService = new RecurlyInvoiceService(new Repository<RecurlyInvoice>(_unitOfWork));
            _recurlySubscriptionService = new RecurlySubscriptionService(new Repository<RecurlySubscription>(_unitOfWork));
            _credentialService = new CredentialService(new Repository<Credential>(_unitOfWork));
            _recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));

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
                        System.Diagnostics.Trace.TraceInformation(string.Format("[BankJob_SyncAccounts] Executing at {0}", DateTime.Now));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                        //El proceso se ejecutara cada 4 del mes, a primera hora
                        //Validar a que hora se ejecutan los cobros con recurly.
                        //Todas las suscripciones deben cobrarse el mismo día 4
                        //Obtener los planes activo en recurly
                        //var plans = RecurlyService.GetPlans(siteId, provider);

                        //Obtener la lista de usuarios activo
                        var accountsRecurly = _accountService.GetAccountRecurly();
                        var now = DateUtil.GetDateTimeNow();
                        var pastMonth = now.AddMonths(-1);
                        var firstDayOfMonth = new DateTime(pastMonth.Year, pastMonth.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddMinutes(-1);

                        if (accountsRecurly != null)
                        {
                            foreach (var acc in accountsRecurly)
                            {

                                //Obtener el mes anterior para facturar rando del fecha ejemplo 1 al 30 de nov

                                //clasificar los usuarios nuevo o con plan antiguo de parte de el20
                                //Para los nuevos planes
                                /*Por cada usuario validar sus facturas totales y por el tipo de usuario rfc
                                Conteo total de las facturas recibidad y emitidas, no se cuentan las que tienen tipo de relación ni tipo de factura complemento de Pagos
                                - si el RFC es persona física solo el conteo de los totales. no se cobran las facturas emitidas de la plataforma   
                                - si el RFC es persona moral contar todas las facturas emitidas desde la plataforma y todas las facturas recibidas (en ambos casos se cuentan hasta las tipo de relación y complementos de pagos)                                 
                                */
                                
                                //Validar si existen suscripciones
                                //Crear el modelo de la suscripción en la cual la cuenta caera 
                                //Si la suscripción cambia realizar un cambio de suscripción
                                //Enviar los datos de la compra.
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