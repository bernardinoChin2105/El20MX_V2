using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Integrations.Recurly.Models;
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

        static InvoiceIssuedService _invoicesIssuedService;
        static InvoiceReceivedService _invoicesReceivedService;

        static RecurlySubscriptionService _recurlySubscriptionService;
        static RecurlyInvoiceService _recurlyInvoiceService;
        static RecurlyPaymentService _recurlyPaymentService;

        static readonly string JOB_CODE = "RecurlyJob_GenerateAccountStatement";

        public static void GenerateAccountStatement()
        {
            var _unitOfWork = new UnitOfWork();
            _processService = new ProcessService(new Repository<Process>(_unitOfWork));
            _webhookProcessService = new WebhookProcessService(new Repository<WebhookProcess>(_unitOfWork));
            _accountService = new AccountService(new Repository<Domain.Entities.Account>(_unitOfWork));
            _notificationService = new NotificationService(new Repository<Notification>(_unitOfWork));
            _recurlyInvoiceService = new RecurlyInvoiceService(new Repository<RecurlyInvoice>(_unitOfWork));
            _recurlySubscriptionService = new RecurlySubscriptionService(new Repository<RecurlySubscription>(_unitOfWork));
            _credentialService = new CredentialService(new Repository<Credential>(_unitOfWork));
            _recurlyPaymentService = new RecurlyPaymentService(new Repository<RecurlyPayment>(_unitOfWork));
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
            StringBuilder strResult = new StringBuilder();

            if (Monitor.TryEnter(thisLock))
            {
                try
                {
                    if (!executing && NotificationProcessEnabled && CAN_EXECUTE)
                    {
                        processJob.Running = true;
                        _processService.Update(processJob);
                        System.Diagnostics.Trace.TraceInformation(string.Format("[RecurlyJob_GenerateAccountStatement] Executing at {0}", DateTime.Now));
                        strResult.Append(string.Format("Executing at {0}", DateUtil.GetDateTimeNow()));

                        #region Implementar logica de negocio especifica
                        var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
                        var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                        //El proceso se ejecutara cada 4 del mes, a primera hora
                        //Validar a que hora se ejecutan los cobros con recurly.
                        //Todas las suscripciones deben cobrarse el mismo día 4
                        //Obtener los planes activo en recurly
                        var plans = RecurlyService.GetPlans(siteId, provider);

                        //Obtener la lista de usuarios activo
                        var accountsRecurly = _accountService.GetAccountRecurly();
                        var now = DateUtil.GetDateTimeNow();
                        var pastMonth = now.AddMonths(-1);
                        var firstDayOfMonth = new DateTime(pastMonth.Year, pastMonth.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddMinutes(-1);
                        var nextMonth = now.AddMonths(1);
                        DateTime nextBillDate = new DateTime(nextMonth.Year, nextMonth.Month, nextMonth.Day, 8, 0, 0);

                        if (accountsRecurly != null)
                        {
                            foreach (var acc in accountsRecurly)
                            {

                                var issuedInvoices = _invoicesIssuedService.FindBy(x => x.account.id == acc.id && x.status == "STAMPED"
                                    && x.invoicedAt >= firstDayOfMonth && x.invoicedAt <= lastDayOfMonth).OrderBy(x => x.invoicedAt);

                                var receivedInvoices = _invoicesReceivedService.FindBy(x => x.account.id == acc.id && x.status == "STAMPED"
                                    && x.invoicedAt >= firstDayOfMonth && x.invoicedAt <= lastDayOfMonth).OrderBy(x => x.invoicedAt);

                                bool isOldAccount = !string.IsNullOrEmpty(acc.planSchema) && acc.planSchema.StartsWith("OLD_SCHEMA");

                                var accountSupscriptions = RecurlyService.GetAccountSuscriptions(siteId, acc.idCredentialProvider);

                                if (!isOldAccount)
                                {
                                    if (acc.rfc.Length == 12)
                                    {
                                        var totalIssuedInvoices = issuedInvoices.Where(x => x.homemade && x.branchOffice != null).Count();

                                        var totalReceivedInvoices = receivedInvoices.Count();

                                        var totalInvoices = totalIssuedInvoices + totalReceivedInvoices;

                                        var planCode = totalInvoices <= 50 ? "plan_startup" :
                                                totalInvoices <= 125 ? "plan_basico" :
                                                totalInvoices <= 200 ? "plan_premium" : "plan_empresarial";

                                        if (accountSupscriptions.data != null && accountSupscriptions.data.Count > 0)
                                        {
                                            var currentSubscription = accountSupscriptions.data[0];
                                            if (currentSubscription.Plan.Code != planCode)
                                            {
                                                var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                                if (recurlyPlan != null)
                                                {
                                                    var subscriptionChangeModel = new SubscriptionChangeCreate()
                                                    {
                                                        PlanCode = recurlyPlan.code,
                                                        Timeframe = "bill_date"
                                                    };

                                                    var subscriptionChange = RecurlyService.CreateSubscriptionChange(subscriptionChangeModel, siteId, currentSubscription.Id, provider);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //create the suscription
                                            var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                            if (recurlyPlan != null)
                                            {
                                                var purchaseReq = new PurchaseCreate()
                                                {
                                                    Currency = "MXN",
                                                    Account = new AccountPurchase()
                                                    {
                                                        Code = acc.uuid.ToString(),

                                                    },
                                                    Subscriptions = new List<SubscriptionPurchase>()
                                                    {
                                                        new SubscriptionPurchase() {
                                                            PlanCode = recurlyPlan.code,
                                                            Quantity = 1,
                                                            NextBillDate = nextBillDate
                                                        }
                                                    }
                                                };

                                                var invoiceCollection = RecurlyService.CreatePurchase(purchaseReq, siteId, provider);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var recurlyPlan = plans.data.FirstOrDefault(x => x.code == "contigo");

                                        if (recurlyPlan != null)
                                        {
                                            var totalIssuedInvoices = issuedInvoices.Where(x => x.invoiceType != "E" && x.invoiceType != "P").Count();

                                            var totalReceivedInvoices = receivedInvoices.Where(x => x.invoiceType != "E" && x.invoiceType != "P").Count();

                                            var totalInvoices = totalIssuedInvoices + totalReceivedInvoices;

                                            int extraBillingInvoices = 0;

                                            if (totalInvoices > 30)
                                            {
                                                var allInvoices = issuedInvoices.Where(x => x.invoiceType != "E" && x.invoiceType != "P").Select(x => new Domain.Model.Invoice()
                                                {
                                                    issued = true,
                                                    id = x.id,
                                                    uuid = x.uuid,
                                                    createdAt = x.createdAt,
                                                    invoicedAt = x.invoicedAt,
                                                    invoiceType = x.invoiceType,
                                                    modifiedAt = x.modifiedAt,
                                                    status = x.status,
                                                    isHomeIssued = x.homemade && x.branchOffice != null
                                                });

                                                allInvoices = allInvoices.Concat(receivedInvoices.Where(x => x.invoiceType != "E" && x.invoiceType != "P").Select(x => new Domain.Model.Invoice()
                                                {
                                                    issued = false,
                                                    id = x.id,
                                                    uuid = x.uuid,
                                                    createdAt = x.createdAt,
                                                    invoicedAt = x.invoicedAt,
                                                    invoiceType = x.invoiceType,
                                                    modifiedAt = x.modifiedAt,
                                                    status = x.status
                                                }));

                                                allInvoices = allInvoices.OrderBy(x => x.invoicedAt);

                                                var extraInvoices = allInvoices.Skip(30);
                                                var totalExtraInvoices = extraInvoices.Count();
                                                var totalHomeIssuedInvoices = extraInvoices.Where(x => x.issued && x.isHomeIssued).Count();

                                                extraBillingInvoices = totalExtraInvoices - totalHomeIssuedInvoices;
                                            }

                                            var addonsQuantity = (totalInvoices > 30 ? 30 : totalInvoices) + extraBillingInvoices;
                                            //addonsQuantity = addonsQuantity > 0 ? addonsQuantity : 1;

                                            var haveSubscription = false;

                                            if (accountSupscriptions.data != null && accountSupscriptions.data.Count > 0)
                                            {
                                                haveSubscription = accountSupscriptions.data.Any(x => x.Plan.Code == "contigo");
                                            }

                                            if (!haveSubscription)
                                            {
                                                var purchaseReq = new PurchaseCreate()
                                                {
                                                    Currency = "MXN",
                                                    Account = new AccountPurchase()
                                                    {
                                                        Code = acc.uuid.ToString(),

                                                    },
                                                    Subscriptions = new List<SubscriptionPurchase>()
                                                    {
                                                        new SubscriptionPurchase() {
                                                            PlanCode = recurlyPlan.code,
                                                            Quantity = 1,
                                                            //UnitAmount = recurlyPlan.currencies[0].unit_amount,
                                                            NextBillDate = nextBillDate
                                                        }
                                                    }
                                                };

                                                if (extraBillingInvoices > 0)
                                                {
                                                    purchaseReq.Subscriptions[0].AddOns = new List<SubscriptionAddOnCreate>() {
                                                        new SubscriptionAddOnCreate()
                                                        {
                                                            Code = "factura_adicional_contigo",
                                                            Quantity = addonsQuantity,
                                                        }
                                                    };
                                                }

                                                var invoiceCollection = RecurlyService.CreatePurchase(purchaseReq, siteId, provider);
                                            }
                                            else
                                            {
                                                var currentSubscription = accountSupscriptions.data.FirstOrDefault(x => x.Plan.Code == "contigo");
                                                var currentSubAddonQuantity = currentSubscription.AddOns.FirstOrDefault(x => x.AddOn.Code == "factura_adicional_contigo")?.Quantity;

                                                if (currentSubAddonQuantity.GetValueOrDefault() != addonsQuantity)
                                                {
                                                    var subChangeReq = new SubscriptionChangeCreate
                                                    {
                                                        Timeframe = "bill_date",
                                                        AddOns = addonsQuantity > 0 ? new List <SubscriptionAddOnUpdate>
                                                        {
                                                            new SubscriptionAddOnUpdate
                                                            {
                                                                Code = "factura_adicional_contigo",
                                                                Quantity = addonsQuantity
                                                            }
                                                        } : null
                                                    };

                                                    var subChange = RecurlyService.CreateSubscriptionChange(subChangeReq, siteId, accountSupscriptions.data[0].Id, provider);
                                                }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    var haveSubscription = accountSupscriptions.data != null && accountSupscriptions.data.Count > 0;

                                    var planCode = "";
                                    Enum.TryParse(acc.planSchema, out SystemPlan planSchema);
                                    switch (planSchema)
                                    {
                                        case SystemPlan.OLD_SCHEMA_STARTUP:
                                            planCode = "pstartup_anterior";
                                            break;
                                        case SystemPlan.OLD_SCHEMA_BASICO:
                                            planCode = "pbasico_anterior";
                                            break;
                                        case SystemPlan.OLD_SCHEMA_PREMIUN:
                                            planCode = "ppremium_anterior";
                                            break;
                                        case SystemPlan.OLD_SCHEMA_EMPRESARIAL:
                                            planCode = "pempresarial_anterior";
                                            break;
                                    }

                                    if (haveSubscription)
                                    {
                                        var currentSubscription = accountSupscriptions.data[0];
                                        var currentSubscriptionCode = currentSubscription.Plan.Code;
                                        if (!string.IsNullOrEmpty(planCode) && planCode != currentSubscriptionCode)
                                        {
                                            var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                            if (recurlyPlan != null)
                                            {
                                                var subscriptionChangeModel = new SubscriptionChangeCreate()
                                                {
                                                    PlanCode = recurlyPlan.code,
                                                    Timeframe = "bill_date"
                                                };

                                                var subscriptionChange = RecurlyService.CreateSubscriptionChange(subscriptionChangeModel, siteId, currentSubscription.Id, provider);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                        if (recurlyPlan != null)
                                        {
                                            var purchaseReq = new PurchaseCreate()
                                            {
                                                Currency = "MXN",
                                                Account = new AccountPurchase()
                                                {
                                                    Code = acc.uuid.ToString(),

                                                },
                                                Subscriptions = new List<SubscriptionPurchase>()
                                                    {
                                                        new SubscriptionPurchase() {
                                                            PlanCode = recurlyPlan.code,
                                                            NextBillDate = nextBillDate
                                                        }
                                                    }
                                            };

                                            var invoiceCollection = RecurlyService.CreatePurchase(purchaseReq, siteId, provider);
                                        }
                                    }
                                }

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