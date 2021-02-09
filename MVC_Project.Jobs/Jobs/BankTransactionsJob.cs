﻿using MVC_Project.Data.Helpers;
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

namespace MVC_Project.Jobs
{
    public class BankTransactionsJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static ICredentialService _credentialService;
        static IAccountService _accountService;

        static readonly string JOB_CODE = "ContaLinkJob_BankTransaction";

        /*
         * Proceso de bloqueo de cuenta por falta de pagos.
         **/

        public static void BankTransaction()
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
                        System.Diagnostics.Trace.TraceInformation(string.Format("[ContaLinkJob_BankTransaction] Executing at {0}", DateTime.Now));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var ContaLinkProvider = ConfigurationManager.AppSettings["ContaLinkProvider"];


                        //Actividades
                        /*Proceso para gestionar el alta de los movimientos bancarios a Contalink..*/
                        /*  
                         * Reglas:
                         * - El proceso se ejecuta ¿?
                         * - Se mandaran todos los movimientos bancarios registrados en la base de datos que aun no esten registrados
                         * (agregar parametro identificador en la tabla de [bankTransactions])
                         * -  Los movimientos bancarios enviados a Contalink se marcaran en la tabla bankTransactions, 
                         * al recibir un error se registrará de igual manera el mensaje devuelto por el servicio web de Contalink.
                         * **/

                        /** Parametros: FechaDelDía**/
                        DateTime today = Convert.ToDateTime("2021-02-24"); //DateUtil.GetDateTimeNow();
                       
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