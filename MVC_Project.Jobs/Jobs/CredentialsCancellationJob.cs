using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.SAT;
using MVC_Project.Integrations.Recurly;
//using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.Linq;
//using System.Net.Http;
using System.Text;
using System.Threading;
using LogHubSDK.Models;
using Newtonsoft.Json;

namespace MVC_Project.Jobs
{
    public class CredentialsCancellationJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IProviderService _providerService;
        static IAccountService _accountService;
        static ICredentialService _credentialService;
        //static NotificationService _notificationService;

        static readonly string JOB_CODE = "CancellationJob_CredentialsCancellation";

        /*
         * Proceso de cancelación de credenciales
         **/

        public static void CredentialsCancellation()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _providerService = new ProviderService(new Repository<Provider>(_unitOfWork));
            //_notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));
            //_recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));
            //_invoicesReceivedService = new InvoiceReceivedService(new Repository<InvoiceReceived>(_unitOfWork));
            //_driveKeyService = new DriveKeyService(new Repository<DriveKey>(_unitOfWork));
            //_invoiceEmissionParametersService = new InvoiceEmissionParametersService(new Repository<InvoiceEmissionParameters>(_unitOfWork));

            //_client = new HttpClient();

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
                        var SATProvider = ConfigurationManager.AppSettings["SATProvider"];
                        var BankProvider = ConfigurationManager.AppSettings["BankProvider"];
                        var RecurlyProvider = ConfigurationManager.AppSettings["RecurlyProvider"];
                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                        //Actividades
                        /*Proceso de cancelación de credenciales para los usuarios que son prospectos.*/
                        //Obtener las cuentas del todos los usuarios.                             
                        //Cada cuenta debe estar Activa, la fechaCreación > 16 días y que la FechaFacturación debe estar vacía
                        /** Parametros: FechaDelDía**/

                        var today = DateUtil.GetDateTimeNow();
                        var accountsProspect = _accountService.GetAccountCredentialProspect(today);
                        foreach (var prospect in accountsProspect)
                        {
                            //Se desactivaran las credenciales de Paybook y Satws para que deje de hacer la sincronización diaria.
                            //También considerar las de recurly

                            try
                            {
                                string provider = string.Empty;
                                bool delete = false;
                                if (prospect.provider == SystemProviders.SATWS.ToString())
                                {
                                    //Evento para desactivar la cuenta en satws
                                    //la opción que se tiene es delete credential
                                    provider = SystemProviders.SATWS.ToString();
                                    var response = SATService.DeleteCredential(prospect.idCredentialProvider, provider);
                                    delete = true;
                                }
                                else if (prospect.provider == SystemProviders.SYNCFY.ToString())
                                {
                                    //Evento para desactivar la cuenta en syncfy
                                    //La opcion que se tiene es delete credential       
                                    provider = SystemProviders.SYNCFY.ToString();
                                    var response = PaybookService.DeleteCredential(prospect.idCredentialProvider, "Delete", null);
                                    delete = true;
                                }
                                else if (prospect.provider == SystemProviders.RECURLY.ToString())
                                {
                                    //Evento para desactivar la cuenta en recurly
                                    //También sería el evento de delete account
                                    provider = SystemProviders.RECURLY.ToString();
                                    var response = RecurlyService.DeleteAccount(prospect.idCredentialProvider, siteId, provider);
                                    delete = true;
                                }

                                //Inactivar cuentas desde nuestras tablas 
                                if (delete)
                                {
                                    var credential = _credentialService.FirstOrDefault(x => x.idCredentialProvider == prospect.idCredentialProvider);
                                    if (credential != null)
                                    {
                                        credential.status = SystemStatus.INACTIVE.ToString();
                                        credential.modifiedAt = DateUtil.GetDateTimeNow();
                                        _credentialService.Update(credential);
                                    }
                                }
                                else
                                    throw new Exception("No se pudo realizar la desactivación de la credencial de " + provider + ", credentialId: " + prospect.credentialId + ", accountId: " + prospect.accountId);

                                //guardar logs
                                LogUtil.AddEntry(
                                    "Factura timbrada con éxito.",
                                    ENivelLog.Info,
                                    0,
                                    "Proccess",
                                    EOperacionLog.ACCESS,
                                     string.Format("Cuenta {0} | Fecha {1}", prospect.rfc, DateUtil.GetDateTimeNow()),
                                    "CredentialsCancellation",
                                    JsonConvert.SerializeObject(prospect)
                                );
                            }
                            catch (Exception Ex)
                            {
                                //Guardar en el log, el motivo de la excepción
                                LogUtil.AddEntry(
                                    "Detalle del error: " + Ex.Message.ToString(),
                                    ENivelLog.Error,
                                    0,//userId
                                    "Proccess",
                                    EOperacionLog.ACCESS,
                                    string.Format("Cuenta {0} | Fecha {1}", prospect.rfc, DateUtil.GetDateTimeNow()),
                                    "CredentialsCancellation",
                                    JsonConvert.SerializeObject(prospect)
                                );
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