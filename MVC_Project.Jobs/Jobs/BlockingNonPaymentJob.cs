using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.SAT;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using LogHubSDK.Models;
using Newtonsoft.Json;
//using MVC_Project.Integrations.Recurly.Models;

namespace MVC_Project.Jobs
{
    public class BlockingNonPaymentJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static ICredentialService _credentialService;
        static IAccountService _accountService;

        static readonly string JOB_CODE = "CancellationJob_BlockingNonPayment";

        /*
         * Proceso de bloqueo de cuenta por falta de pagos.
         **/

        public static void BlockingNonPayment()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _accountService = new AccountService(new Repository<Account>(_unitOfWork));
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
                        var RecurlyProvider = ConfigurationManager.AppSettings["RecurlyProvider"];
                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];
                        var SATProvider = ConfigurationManager.AppSettings["SATProvider"];
                        var BankProvider = ConfigurationManager.AppSettings["BankProvider"];

                        //Actividades
                        /*Proceso de bloqueo de cuenta por falta de pagos.*/
                        /*
                         * - Obtener cuentas activas con facturaciones vigentes del mes en curso                         
                         * 
                         * Reglas:
                         * - El proceso se ejecuta de manera diaria
                         * - Validar su estatus de pago apartir del día 5 de cada mes                                                                        
                         * **/

                        /** Parametros: FechaDelDía**/
                        DateTime today = Convert.ToDateTime("2021-02-08"); //DateUtil.GetDateTimeNow();

                        //días posibles de suspenciones y cancelaciones
                        if (today.Day >= 8 && today.Day <= 24)
                        {
                            var startDate = new DateTime(today.Year, today.Month, 1);
                            var endDate = startDate.AddMonths(1).AddDays(-1);

                            /*Validar los invoice de las cuentas que generaron en recurly*/
                            List<Integrations.Recurly.Models.Invoice> invoices = new List<Integrations.Recurly.Models.Invoice>();
                            var paymentsAccount = _accountService.GetLastPaymentsAccount(null, null);
                            var invoiceListResponse = RecurlyService.GetInvoiceAll(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), siteId, RecurlyProvider);
                            invoices.AddRange(invoiceListResponse.data);

                            while (invoiceListResponse.has_more)
                            {
                                invoiceListResponse = RecurlyService.GetNextInvoiceAll(invoiceListResponse.next, RecurlyProvider);
                                invoices.AddRange(invoiceListResponse.data);
                            }

                            var leftOuterJoin = (from payment in paymentsAccount
                                                 join invoice in invoices.Where(x => x.State != "Closed" || x.State != "Voided") on payment.idCredentialProvider equals invoice.Account.Id into temp
                                                 from invoice in temp.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     paymentId = payment.paymentId,
                                                     subtotal = payment.subtotal,
                                                     total = payment.total,
                                                     paymentGateway = payment.paymentGateway,
                                                     statusCode = payment.statusCode,
                                                     accountId = payment.accountId,
                                                     statusMessage = payment.statusMessage,
                                                     createdAt = payment.createdAt,
                                                     subscriptionId = payment.subscriptionId,
                                                     number = payment.number,
                                                     name = payment.name,
                                                     rfc = payment.rfc,
                                                     createdAtAccount = payment.createdAtAccount,
                                                     status = payment.status,
                                                     planSchema = payment.planSchema,
                                                     inicioFacturacion = payment.inicioFacturacion,
                                                     planFijo = payment.planFijo,
                                                     credentialId = payment.credentialId,
                                                     provider = payment.provider,
                                                     idCredentialProvider = payment.idCredentialProvider,
                                                     statusCredential = payment.statusCredential,
                                                     invoiceId = invoice != null ? invoice.Id : string.Empty,
                                                     stateInvoice = invoice != null ? invoice.State : string.Empty,
                                                     originInvoice = invoice != null ? invoice.Origin : string.Empty,
                                                     accountIdInvoice = invoice != null ? invoice.Account.Id : string.Empty,
                                                     numberInvoice = invoice != null ? invoice.Number : string.Empty,
                                                     collectionMethodInvoice = invoice != null ? invoice.CollectionMethod : string.Empty,
                                                     createdAtInvoice = invoice != null ? invoice.CreatedAt : null,
                                                     updatedAtInvoice = invoice != null ? invoice.UpdatedAt : null
                                                 }).ToList();

                            //Buscar cuentas con el último pago fallido durante el mes
                            /* Validar que la cuenta realmente este con el pago fallido.
                             * Si resulta exitosa ajusta las tablas correspondientes para ponerlo exitoso
                            */

                            if (today.Day == 8)
                            {
                                #region El día 8 del mes en curso, se validará que la cuenta tenga un último cobro, si este es fallido la cuenta cambia a estatus "Suspendido".

                                var list = leftOuterJoin.Where(x => (x.statusCode != RecurlyPaymentStatus.SUCCESS.GetDisplayName() && x.createdAt >= startDate && x.createdAt <= endDate)
                                || x.stateInvoice != "Paid");

                                var listPaid = leftOuterJoin.Where(x => x.stateInvoice == "Paid");

                                foreach (var payment in list)
                                {
                                    //prospect.rfc == "HAE951128471" &&  && && CARJ850321758
                                    if (payment.rfc == "AUR040802HA5")
                                    {
                                        try
                                        {

                                            //Validar que este correctamente fallido el pago
                                            //var validateAccount = RecurlyService.GetAccount(payment.idCredentialProvider, siteId, SystemProviders.RECURLY.ToString());

                                            //Cambiar el status de la cuenta
                                            var account = _accountService.FirstOrDefault(x => x.id == payment.accountId);
                                            if (account != null)
                                            {
                                                account.modifiedAt = DateUtil.GetDateTimeNow();
                                                account.status = SystemStatus.SUSPENDED.ToString();
                                                _accountService.Update(account);

                                                LogUtil.AddEntry(
                                                    "Se cambio el status de la cuenta a suspendido",
                                                    ENivelLog.Info,
                                                    0,
                                                    "Proccess_" + JOB_CODE,
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Fecha {2}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                        }
                                        catch (Exception Ex)
                                        {
                                            //Guardar en el log, el motivo de la excepción
                                            LogUtil.AddEntry(
                                                "Detalle del error al obtener el último pago: " + Ex.Message.ToString(),
                                                ENivelLog.Error,
                                                0,//userId
                                                 "Proccess_" + JOB_CODE,
                                                EOperacionLog.ACCESS,
                                                string.Format("|Cuenta {0} | RFC {1} | Fecha {3}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                                "BlockingNonPayment",
                                                JsonConvert.SerializeObject(payment)
                                            );
                                        }
                                    }
                                }
                                #endregion
                            }
                            else if (today.Day == 24)
                            {
                                #region El día 24 si la cuenta continuá con pago fallido, se cambia el estatus de la cuenta a Cancelada y se realiza la cancelación de las credenciales de SAT.ws, Paybook y Recurly.                                        
                                //Buscar las credenciales de Satws, Recurly y Paybook de la cuenta que se va a cancelar

                                var list = leftOuterJoin.Where(x => (x.statusCode != RecurlyPaymentStatus.SUCCESS.GetDisplayName() && x.createdAt >= startDate && x.createdAt <= endDate)
                               || x.stateInvoice != "Paid");

                                var listPaid = leftOuterJoin.Where(x => x.stateInvoice == "Paid");

                                //var paymentsAccount = _accountService.GetLastPaymentsAccount(today, RecurlyPaymentStatus.OTHER.GetDisplayName());

                                foreach (var payment in list)
                                {
                                    try
                                    {
                                        //Validar que este correctamente fallido el pago
                                        //var validateAccount = RecurlyService.GetAccount(payment.idCredentialProvider, siteId, SystemProviders.RECURLY.ToString());

                                        var accountCredentials = _credentialService.FindBy(x => x.account.id == payment.accountId && x.status == SystemStatus.ACTIVE.ToString());

                                        foreach (var credential in accountCredentials)
                                        {
                                            try
                                            {
                                                string provider = string.Empty;
                                                bool delete = false;
                                                string statusProvider = "";

                                                if (credential.provider == SystemProviders.SATWS.ToString())
                                                {
                                                    //Evento para desactivar la cuenta en satws
                                                    //la opción que se tiene es delete credential
                                                    provider = SystemProviders.SATWS.ToString();
                                                    try
                                                    {
                                                        //Si es eliminada, no regresa nada
                                                        SATService.DeleteCredential(credential.idCredentialProvider, provider);
                                                        LogUtil.AddEntry(
                                                            "Credencial eliminada de SATws",
                                                            ENivelLog.Info,
                                                            0,
                                                            "Proccess_" + JOB_CODE,
                                                            EOperacionLog.ACCESS,
                                                            string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, payment.credentialId, DateUtil.GetDateTimeNow()),
                                                            "BlockingNonPayment",
                                                            "Se elimino correctamente la credencial."//JsonConvert.SerializeObject()
                                                        );
                                                        statusProvider = SystemStatus.INACTIVE.ToString();
                                                        delete = true;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //Si marca error entonces no se elimino la cuenta en satws
                                                        LogUtil.AddEntry(
                                                            "Error al eliminar la cuenta de SATws: " + ex.Message.ToString(),
                                                            ENivelLog.Error,
                                                            0,
                                                            "Proccess_" + JOB_CODE,
                                                            EOperacionLog.ACCESS,
                                                            string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, payment.credentialId, DateUtil.GetDateTimeNow()),
                                                            "BlockingNonPayment",
                                                            JsonConvert.SerializeObject(ex.Message.ToString())
                                                        );
                                                    }
                                                }
                                                else if (credential.provider == SystemProviders.SYNCFY.GetDisplayName())
                                                {
                                                    //Evento para desactivar la cuenta en syncfy
                                                    //La opcion que se tiene es delete credential       
                                                    provider = SystemProviders.SYNCFY.GetDisplayName();
                                                    var response = PaybookService.DeleteUser(credential.idCredentialProvider, "Delete", true);

                                                    LogUtil.AddEntry(
                                                        "Eliminación de la cuenta de Paybook.",
                                                        ENivelLog.Info,
                                                        0,
                                                        "Proccess_" + JOB_CODE,
                                                        EOperacionLog.ACCESS,
                                                        string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, payment.credentialId, DateUtil.GetDateTimeNow()),
                                                        "BlockingNonPayment",
                                                        JsonConvert.SerializeObject(response)
                                                    );

                                                    if (response)
                                                    {
                                                        delete = true;
                                                        statusProvider = SystemStatus.INACTIVE.ToString();
                                                    }
                                                }
                                                else if (credential.provider == SystemProviders.RECURLY.ToString())
                                                {
                                                    //Evento para desactivar la cuenta en recurly
                                                    //También sería el evento de delete account
                                                    provider = SystemProviders.RECURLY.ToString();
                                                    var response = RecurlyService.DeleteAccount(credential.idCredentialProvider, siteId, provider);
                                                    LogUtil.AddEntry(
                                                        "Respuesta de Recurly al eliminar la cuenta.",
                                                        ENivelLog.Info,
                                                        0,
                                                        "Proccess_" + JOB_CODE,
                                                        EOperacionLog.ACCESS,
                                                        string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, payment.credentialId, DateUtil.GetDateTimeNow()),
                                                        "BlockingNonPayment",
                                                        JsonConvert.SerializeObject(response)
                                                    );

                                                    if (response != null)
                                                    {
                                                        statusProvider = response.State;
                                                        delete = true;
                                                    }
                                                }

                                                //Inactivar cuentas desde nuestras tablas 
                                                if (delete)
                                                {
                                                    //var credential = _credentialService.FirstOrDefault(x => x.id == payment.credentialId);
                                                    //if (credential != null)
                                                    //{
                                                    if (!string.IsNullOrEmpty(statusProvider))
                                                        credential.statusProvider = statusProvider;

                                                    credential.status = SystemStatus.INACTIVE.ToString();
                                                    credential.modifiedAt = DateUtil.GetDateTimeNow();
                                                    _credentialService.Update(credential);
                                                    //}
                                                }
                                                else
                                                    throw new Exception("No se pudo realizar la desactivación de la credencial de " + provider + ", credentialId: " + credential.id + ", accountId: " + credential.account.id);

                                                //guardar logs
                                                LogUtil.AddEntry(
                                                    "Proceso de Cancelación de exitoso.",
                                                    ENivelLog.Info,
                                                    0,
                                                    "Process_" + JOB_CODE,
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, payment.credentialId, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                //Guardar en el log, el motivo de la excepción
                                                LogUtil.AddEntry(
                                                    "Detalle del error al cancelar la credencial de la cuenta: " + ex.Message.ToString(),
                                                    ENivelLog.Error,
                                                    0,//userId
                                                    "ProccessBlockingNonPayment",
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Credencial {2} | Fecha {3}", payment.accountId, payment.rfc, credential.id, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                        }

                                        var credentialConfirm = _credentialService.FindBy(x => x.account.id == payment.accountId && x.status == "ACTIVE");

                                        if (credentialConfirm.Count() == 0)
                                        {
                                            //Cambiar el status de la cuenta
                                            var account = _accountService.FirstOrDefault(x => x.id == payment.accountId);
                                            if (account != null)
                                            {
                                                account.modifiedAt = DateUtil.GetDateTimeNow();
                                                account.status = SystemStatus.CANCELLED.ToString();
                                                _accountService.Update(account);

                                                LogUtil.AddEntry(
                                                    "Se cambio el status de la cuenta de Suspendido a Cancelado.",
                                                    ENivelLog.Info,
                                                    0,
                                                    "Proccess_" + JOB_CODE,
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Fecha {2}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                        }

                                    }
                                    catch (Exception Ex)
                                    {
                                        //Guardar en el log, el motivo de la excepción
                                        LogUtil.AddEntry(
                                            "Detalle del error al obtener el último pago: " + Ex.Message.ToString(),
                                            ENivelLog.Error,
                                            0,//userId
                                            "ProccessBlockingNonPayment",
                                            EOperacionLog.ACCESS,
                                            string.Format("|Cuenta {0} | RFC {1} | Fecha {3}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                            "BlockingNonPayment",
                                            JsonConvert.SerializeObject(payment)
                                        );
                                    }
                                }
                                #endregion
                            }
                            else if (today.Day >= 9 && today.Day <= 23)
                            {
                                #region Si se detecta un pago exitoso entre los días 9 y 23 se cambia el status de la cuenta a Activo

                                /*Se cambio para validar los pagos fallidos con cuentas aun activas*/
                                //var paymentsAccount = _accountService.GetLastPaymentsAccount(today, null);

                                // var list = leftOuterJoin.Where(x => (x.statusCode == RecurlyPaymentStatus.SUCCESS.GetDisplayName() && x.createdAt >= startDate && x.createdAt <= endDate)
                                //|| x.stateInvoice == "Paid");

                                var listPaid = leftOuterJoin.Where(x => x.stateInvoice == "Paid");

                                foreach (var payment in leftOuterJoin)
                                {
                                    try
                                    {
                                        //Cambiar el status de la cuenta si este ha sido suspendido
                                        var account = _accountService.FirstOrDefault(x => x.id == payment.accountId);
                                        if (account != null)
                                        {
                                            if (((payment.statusCode == RecurlyPaymentStatus.SUCCESS.GetDisplayName() && payment.createdAt >= startDate && payment.createdAt <= endDate)
                                                || payment.stateInvoice == "Paid") && account.status == SystemStatus.SUSPENDED.ToString())
                                            {
                                                account.modifiedAt = DateUtil.GetDateTimeNow();
                                                account.status = SystemStatus.ACTIVE.ToString();
                                                _accountService.Update(account);

                                                LogUtil.AddEntry(
                                                    "Se cambio el status de la cuenta de Suspendido a Activo",
                                                    ENivelLog.Info,
                                                    0,
                                                    "Proccess_" + JOB_CODE,
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Fecha {2}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                            else if (payment.statusCode != RecurlyPaymentStatus.SUCCESS.GetDisplayName() && account.status == SystemStatus.ACTIVE.ToString())
                                            {
                                                account.modifiedAt = DateUtil.GetDateTimeNow();
                                                account.status = SystemStatus.SUSPENDED.ToString();
                                                _accountService.Update(account);

                                                LogUtil.AddEntry(
                                                    "Se cambio el status de la cuenta de Activo a Suspendido.",
                                                    ENivelLog.Info,
                                                    0,
                                                    "Proccess_" + JOB_CODE,
                                                    EOperacionLog.ACCESS,
                                                    string.Format("|Cuenta {0} | RFC {1} | Fecha {2}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                                    "BlockingNonPayment",
                                                    JsonConvert.SerializeObject(payment)
                                                );
                                            }
                                        }
                                    }
                                    catch (Exception Ex)
                                    {
                                        //Guardar en el log, el motivo de la excepción
                                        LogUtil.AddEntry(
                                            "Detalle del error al obtener el último pago: " + Ex.Message.ToString(),
                                            ENivelLog.Error,
                                            0,//userId
                                            "Proccess_" + JOB_CODE,
                                            EOperacionLog.ACCESS,
                                            string.Format("|Cuenta {0} | RFC {1} | Fecha {2}", payment.accountId, payment.rfc, DateUtil.GetDateTimeNow()),
                                            "BlockingNonPayment",
                                            JsonConvert.SerializeObject(payment)
                                        );
                                    }
                                }

                                #endregion
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

        //private static string BuildPaymentsTable(IEnumerable<RecurlyPayment> failedInvoices)
        //{
        //    var cellStyle = "border-top: 1px solid #dee2e6; padding: 5px;";
        //    var tableStyle = "width: 100%; text-align: center;";

        //    string table = $"<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"{tableStyle}\">" +
        //        "<thead><tr>" +
        //        $"<th>RFC</th>" +
        //        $"<th>Plan</th>" +
        //        $"<th>Total</th>" +
        //        $"<th>Fecha de pago</th>" +
        //        "</tr></thead>";

        //    foreach (var payment in failedInvoices)
        //    {
        //        var paymentAccount = payment.subscription?.account;
        //        var paymentSubscription = payment.subscription;
        //        table += $"<tr><td style=\"{cellStyle}\">{paymentAccount?.rfc}</td>" +
        //            $"<td style=\"{cellStyle}\">{paymentSubscription?.planName}</td>" +
        //            $"<td style=\"{cellStyle}\">{payment.total.ToString("F02")}</td>" +
        //            $"<td style=\"{cellStyle}\">{payment.createdAt.ToString("dd/MM/yyyy HH:mm")}</td></tr>";
        //    }

        //    table += "</table>";
        //    return table;
        //}

        //private static void SendInvoice(string email, string rfc, string businessName, string comments, string linkXml, string linkPdf)
        //{
        //    Dictionary<string, string> customParams = new Dictionary<string, string>();
        //    customParams.Add("param_rfc", rfc);
        //    customParams.Add("param_razon_social", businessName);
        //    customParams.Add("param_comentarios", comments);

        //    customParams.Add("param_link_xml", linkXml);
        //    customParams.Add("param_link_pdf", linkPdf);
        //    NotificationUtil.SendNotification(email, customParams, Constants.NOT_TEMPLATE_RECURLY_INVOICING);
        //}

        //private static void SendErrorsNotification(string email, string paymentsListContent)
        //{
        //    Dictionary<string, string> customParams = new Dictionary<string, string>();
        //    customParams.Add("param_payments_table", paymentsListContent);
        //    NotificationUtil.SendNotification(email, customParams, Constants.NOT_TEMPLATE_RECURLY_INVOICING_ERRORS);
        //}
    }
}