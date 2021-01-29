using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Integrations.Recurly.Models;
using MVC_Project.Jobs.Models;
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
    public class RecurlyAccountStatementJob
    {
        static bool executing = false;
        static readonly Object thisLock = new Object();
        static IProcessService _processService;
        static IWebhookProcessService _webhookProcessService;
        static IAccountService _accountService;
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

                        if (accountsRecurly != null)
                        {
                            foreach (var acc in accountsRecurly)
                            {
                                #region lo que se ejecuta dentro por cada cliente
                                var accountSupscriptions = RecurlyService.GetAccountSuscriptions(siteId, acc.idCredentialProvider);

                                var issuedInvoices = _invoicesIssuedService.FindBy(x => x.account.id == acc.id 
                                    && x.invoicedAt >= firstDayOfMonth && x.invoicedAt <= lastDayOfMonth).OrderBy(x => x.invoicedAt);

                                var receivedInvoices = _invoicesReceivedService.FindBy(x => x.account.id == acc.id 
                                    && x.invoicedAt >= firstDayOfMonth && x.invoicedAt <= lastDayOfMonth).OrderBy(x => x.invoicedAt);

                                bool isOldAccount = !string.IsNullOrEmpty(acc.planSchema) && acc.planSchema.StartsWith(SystemPlan.OLD_SCHEMA.ToString());

                                if (!isOldAccount)
                                {
                                    if (acc.rfc.Length == 12)
                                    {
                                        var excludedIssuedCount = issuedInvoices.Count(x => x.invoiceType == TipoComprobante.E.ToString() || x.invoiceType == TipoComprobante.P.ToString());
                                        var excludedReceivedCount = receivedInvoices.Count(x => x.invoiceType == TipoComprobante.E.ToString() || x.invoiceType == TipoComprobante.P.ToString());

                                        var totalIssuedInvoices = issuedInvoices.Count() - excludedIssuedCount;
                                        var totalReceivedInvoices = receivedInvoices.Count() - excludedReceivedCount;

                                        var totalInvoices = totalIssuedInvoices + totalReceivedInvoices;

                                        //var planCodeEnum = totalInvoices <= 50 ? SystemPlan.plan_startup :
                                        //        totalInvoices <= 125 ? SystemPlan.plan_basico :
                                        //        totalInvoices <= 200 ? SystemPlan.plan_premium : SystemPlan.plan_empresarial;
                                        //var planCode = planCodeEnum.GetDisplayName();

                                        var planCodeEnum = Constants.RecurlyPlanLimits.First(x => x.Value >= totalInvoices).Key;
                                        var planCode = planCodeEnum.ToString();

                                        if (!string.IsNullOrEmpty(acc.planFijo))
                                        {
                                            var planExists = Enum.TryParse(acc.planFijo, out SystemPlan fixedPlanEnum);
                                            if (planExists && Constants.RecurlyPlanLimits.ContainsKey(fixedPlanEnum))
                                            {
                                                var fixedPlanLimit = Constants.RecurlyPlanLimits[fixedPlanEnum];
                                                if (fixedPlanLimit >= totalInvoices)
                                                {
                                                    planCodeEnum = fixedPlanEnum;
                                                    planCode = acc.planFijo;
                                                }
                                            }
                                        }

                                        string receivedAddonCode = "";
                                        string issuedAddonCode = "";

                                        switch (planCodeEnum)
                                        {
                                            case SystemPlan.plan_startup:
                                                issuedAddonCode = RecurlyPlanAddons.STARTUP_FACTURA_EMITIDA.GetDisplayName();
                                                receivedAddonCode = RecurlyPlanAddons.STARTUP_FACTURA_RECIBIDA.GetDisplayName();
                                                break;
                                            case SystemPlan.plan_basico:
                                                issuedAddonCode = RecurlyPlanAddons.BASICO_FACTURA_EMITIDA.GetDisplayName();
                                                receivedAddonCode = RecurlyPlanAddons.BASICO_FACTURA_RECIBIDA.GetDisplayName();
                                                break;
                                            case SystemPlan.plan_premium:
                                                issuedAddonCode = RecurlyPlanAddons.PREMIUM_FACTURA_EMITIDA.GetDisplayName();
                                                receivedAddonCode = RecurlyPlanAddons.PREMIUM_FACTURA_RECIBIDA.GetDisplayName();
                                                break;
                                            case SystemPlan.plan_empresarial:
                                                issuedAddonCode = RecurlyPlanAddons.EMPRESARIAL_FACTURA_EMITIDA.GetDisplayName();
                                                receivedAddonCode = RecurlyPlanAddons.EMPRESARIAL_FACTURA_RECIBIDA.GetDisplayName();
                                                break;
                                        }

                                        var homeIssuedCount = issuedInvoices.Where(x => x.homemade && x.branchOffice != null).Count();

                                        List<SubscriptionAddOnCreate> addonsList = null;
                                        if (homeIssuedCount + totalReceivedInvoices > 0)
                                        {
                                            addonsList = new List<SubscriptionAddOnCreate>();

                                            if (homeIssuedCount > 0)
                                            {
                                                addonsList.Add(new SubscriptionAddOnCreate()
                                                {
                                                    Code = issuedAddonCode,
                                                    Quantity = homeIssuedCount,
                                                });
                                            }

                                            if (totalReceivedInvoices > 0)
                                            {
                                                addonsList.Add(new SubscriptionAddOnCreate()
                                                {
                                                    Code = receivedAddonCode,
                                                    Quantity = totalReceivedInvoices,
                                                });
                                            }
                                        }

                                        if (accountSupscriptions.data != null && accountSupscriptions.data.Count > 0)
                                        {
                                            var currentSubscription = accountSupscriptions.data[0];

                                            var currentSubIssuedAddonQuantity = currentSubscription.AddOns.FirstOrDefault(x => x.AddOn.Code == issuedAddonCode)?.Quantity;
                                            var currentSubReceivedAddonQuantity = currentSubscription.AddOns.FirstOrDefault(x => x.AddOn.Code == receivedAddonCode)?.Quantity;

                                            if (currentSubscription.Plan.Code != planCode)
                                            {
                                                var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                                if (recurlyPlan != null)
                                                {
                                                    var addonsUpdateLis = addonsList != null ? addonsList.Select(x => new SubscriptionAddOnUpdate
                                                    {
                                                        Code = x.Code,
                                                        Quantity = x.Quantity
                                                    }).ToList() : null;
                                                    var subscriptionChange = CreateSubscriptionChange(recurlyPlan.code, addonsUpdateLis, currentSubscription.Id, siteId, provider);
                                                    SaveSubscriptionChangeLog(subscriptionChange, acc);
                                                }
                                            }
                                            else if (currentSubIssuedAddonQuantity.GetValueOrDefault() != homeIssuedCount || currentSubReceivedAddonQuantity.GetValueOrDefault() != totalReceivedInvoices)
                                            {
                                                var addonsUpdateList = addonsList != null ? addonsList.Select(x => new SubscriptionAddOnUpdate
                                                {
                                                    Code = x.Code,
                                                    Quantity = x.Quantity
                                                }).ToList() : null;

                                                var subscriptionChange = CreateSubscriptionChange(null, addonsUpdateList, currentSubscription.Id, siteId, provider);
                                                SaveSubscriptionChangeLog(subscriptionChange, acc);
                                            }
                                        }
                                        else
                                        {
                                            //create the suscription
                                            var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                            if (recurlyPlan != null)
                                            {
                                                try
                                                {
                                                    var invoiceCollection = CreateSubscriptionPurchase(recurlyPlan.code, acc.uuid.ToString(), siteId, provider, addonsList);

                                                    InvoicesDTO invoices = new InvoicesDTO
                                                    {
                                                        extraBills = 0,
                                                        totalInvoice = totalInvoices,
                                                        totalInvoiceIssued = totalIssuedInvoices,
                                                        totalInvoiceReceived = totalReceivedInvoices
                                                    };

                                                    SavePurchaseLogs(invoiceCollection, recurlyPlan, acc, invoices);
                                                }
                                                catch (RecurlyErrorException recurlyException)
                                                {
                                                    //SaveErrorLog(recurlyException.Error, recurlyPlan, acc);
                                                    //Se mueve registro de error de pago a webhook
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var recurlyPlan = plans.data.FirstOrDefault(x => x.code == SystemPlan.contigo.GetDisplayName());

                                        if (recurlyPlan != null)
                                        {
                                            var totalIssuedInvoices = issuedInvoices.Where(x => x.invoiceType != TipoComprobante.E.ToString() && x.invoiceType != TipoComprobante.P.ToString()).Count();

                                            var totalReceivedInvoices = receivedInvoices.Where(x => x.invoiceType != TipoComprobante.E.ToString() && x.invoiceType != TipoComprobante.P.ToString()).Count();

                                            var totalInvoices = totalIssuedInvoices + totalReceivedInvoices;

                                            int extraBillingInvoices = 0;

                                            if (totalInvoices > 30)
                                            {
                                                var allInvoices = issuedInvoices.Where(x => x.invoiceType != TipoComprobante.E.ToString() && x.invoiceType != TipoComprobante.P.ToString())
                                                    .Select(x => new Domain.Model.Invoice()
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

                                                allInvoices = allInvoices.Concat(receivedInvoices.Where(x => x.invoiceType != TipoComprobante.E.ToString() && x.invoiceType != TipoComprobante.P.ToString())
                                                    .Select(x => new Domain.Model.Invoice()
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
                                                haveSubscription = accountSupscriptions.data.Any(x => x.Plan.Code == SystemPlan.contigo.GetDisplayName());
                                            }

                                            if (!haveSubscription)
                                            {
                                                List<SubscriptionAddOnCreate> addonsList = null;
                                                if (extraBillingInvoices > 0)
                                                {
                                                    addonsList = new List<SubscriptionAddOnCreate>() {
                                                        new SubscriptionAddOnCreate()
                                                        {
                                                            Code = RecurlyPlanAddons.CONTIGO_FACTURA_ADICIONAL.GetDisplayName(),
                                                            Quantity = addonsQuantity,
                                                        }
                                                    };
                                                }

                                                try
                                                {
                                                    var invoiceCollection = CreateSubscriptionPurchase(recurlyPlan.code, acc.uuid.ToString(), siteId, provider, addonsList);

                                                    InvoicesDTO invoices = new InvoicesDTO
                                                    {
                                                        extraBills = extraBillingInvoices,
                                                        totalInvoice = totalInvoices,
                                                        totalInvoiceIssued = totalIssuedInvoices,
                                                        totalInvoiceReceived = totalReceivedInvoices
                                                    };

                                                    SavePurchaseLogs(invoiceCollection, recurlyPlan, acc, invoices);
                                                }
                                                catch (RecurlyErrorException recurlyException)
                                                {
                                                    //SaveErrorLog(recurlyException.Error, recurlyPlan, acc);
                                                    //Se mueve registro de error de pago a webhook
                                                }

                                            }
                                            else
                                            {
                                                var currentSubscription = accountSupscriptions.data.FirstOrDefault(x => x.Plan.Code == SystemPlan.contigo.GetDisplayName());
                                                var currentSubAddonQuantity = currentSubscription.AddOns.FirstOrDefault(x => x.AddOn.Code == RecurlyPlanAddons.CONTIGO_FACTURA_ADICIONAL.GetDisplayName())?.Quantity;

                                                if (currentSubAddonQuantity.GetValueOrDefault() != addonsQuantity)
                                                {
                                                    List<SubscriptionAddOnUpdate> addonsList = addonsQuantity > 0 ? new List<SubscriptionAddOnUpdate>
                                                        {
                                                            new SubscriptionAddOnUpdate
                                                            {
                                                                Code = RecurlyPlanAddons.CONTIGO_FACTURA_ADICIONAL.GetDisplayName(),
                                                                Quantity = addonsQuantity
                                                            }
                                                        } : null;

                                                    var subscriptionChange = CreateSubscriptionChange(null, addonsList, currentSubscription.Id, siteId, provider);
                                                    SaveSubscriptionChangeLog(subscriptionChange, acc);
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
                                            planCode = SystemPlan.OLD_SCHEMA_STARTUP.GetDisplayName();
                                            break;
                                        case SystemPlan.OLD_SCHEMA_BASICO:
                                            planCode = SystemPlan.OLD_SCHEMA_BASICO.GetDisplayName();
                                            break;
                                        case SystemPlan.OLD_SCHEMA_PREMIUN:
                                            planCode = SystemPlan.OLD_SCHEMA_PREMIUN.GetDisplayName();
                                            break;
                                        case SystemPlan.OLD_SCHEMA_EMPRESARIAL:
                                            planCode = SystemPlan.OLD_SCHEMA_EMPRESARIAL.GetDisplayName();
                                            break;
                                    }

                                    if (!string.IsNullOrEmpty(acc.planFijo))
                                    {
                                        planCode = acc.planFijo;
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
                                                var subscriptionChange = CreateSubscriptionChange(recurlyPlan.code, null, currentSubscription.Id, siteId, provider);
                                                SaveSubscriptionChangeLog(subscriptionChange, acc);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var recurlyPlan = plans.data.FirstOrDefault(x => x.code == planCode);
                                        if (recurlyPlan != null)
                                        {
                                            try
                                            {
                                                var invoiceCollection = CreateSubscriptionPurchase(recurlyPlan.code, acc.uuid.ToString(), siteId, provider);

                                                InvoicesDTO invoices = new InvoicesDTO
                                                {
                                                    extraBills = 0,
                                                    totalInvoice = 0,
                                                    totalInvoiceIssued = 0,
                                                    totalInvoiceReceived = 0
                                                };

                                                SavePurchaseLogs(invoiceCollection, recurlyPlan, acc, invoices);
                                            }
                                            catch (RecurlyErrorException recurlyException)
                                            {
                                                //SaveErrorLog(recurlyException.Error, recurlyPlan, acc);
                                                //Se mueve registro de error de pago a webhook
                                            }
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

        private static InvoiceCollection CreateSubscriptionPurchase(string planCode, string accontCode, string siteId, string provider, List<SubscriptionAddOnCreate> addons = null)
        {
            var purchaseReq = new PurchaseCreate()
            {
                Currency = TypeCurrency.MXN.ToString(),
                Account = new AccountPurchase()
                {
                    Code = accontCode,

                },
                Subscriptions = new List<SubscriptionPurchase>()
                {
                    new SubscriptionPurchase() {
                        PlanCode = planCode,
                    }
                },
                //CollectionMethod = "manual",
                //NetTerms = 0
            };

            if (addons != null && addons.Count > 0)
            {
                purchaseReq.Subscriptions[0].AddOns = addons;
            }

            var invoiceCollection = RecurlyService.CreatePurchase(purchaseReq, siteId, provider);

            return invoiceCollection;
        }

        private static SubscriptionChange CreateSubscriptionChange(string planCode, List<SubscriptionAddOnUpdate> addons, string currentSubscriptionId, string siteId, string provider)
        {
            var subscriptionChangeModel = new SubscriptionChangeCreate()
            {
                Timeframe = RecurlyChangeTimeframe.BILL_DATE.GetDisplayName(),
                AddOns = addons
            };

            if (!string.IsNullOrEmpty(planCode))
            {
                subscriptionChangeModel.PlanCode = planCode;
            }

            var subscriptionChange = RecurlyService.CreateSubscriptionChange(subscriptionChangeModel, siteId, currentSubscriptionId, provider);

            return subscriptionChange;
        }

        private static void SavePurchaseLogs(InvoiceCollection invoiceCollection, PlanDataModel recurlyPlan, AccountCredentialModel accountCredential, InvoicesDTO invoicesCount)
        {
            var now = DateUtil.GetDateTimeNow();

            var recurlySubscription = new RecurlySubscription
            {
                account = new Domain.Entities.Account
                {
                    id = accountCredential.id
                },
                createdAt = now,
                modifiedAt = now,
                planCode = invoiceCollection.ChargeInvoice.LineItems.Data[0].PlanCode,
                planId = invoiceCollection.ChargeInvoice.LineItems.Data[0].PlanId,
                state = invoiceCollection.ChargeInvoice.State,
                status = SystemStatus.ACTIVE.ToString(),
                planName = recurlyPlan.name,
                subscriptionId = invoiceCollection.ChargeInvoice.LineItems.Data[0].SubscriptionId,
                uuid = Guid.NewGuid()
            };
            _recurlySubscriptionService.Create(recurlySubscription);

            var storedPaymentInvoice = _recurlyInvoiceService.FirstOrDefault(x => x.invoiceNumber == invoiceCollection.ChargeInvoice.Number);
            if (storedPaymentInvoice == null)
            {
                var recurlyInvoice = new RecurlyInvoice
                {
                    createdAt = now,
                    transactionAt = invoiceCollection.ChargeInvoice.Transactions.First().CreatedAt.GetValueOrDefault(),
                    mounth = invoiceCollection.ChargeInvoice.CreatedAt.GetValueOrDefault().Month.ToString(),
                    year = invoiceCollection.ChargeInvoice.CreatedAt.GetValueOrDefault().Year.ToString(),
                    uuid = Guid.NewGuid(),
                    subscription = recurlySubscription,
                    totalInvoice = invoicesCount.totalInvoice,
                    totalInvoiceIssued = invoicesCount.totalInvoiceIssued,
                    totalInvoiceReceived = invoicesCount.totalInvoiceReceived,
                    extraBills = invoicesCount.extraBills,
                    invoiceNumber = invoiceCollection.ChargeInvoice.Number,
                    account = new Domain.Entities.Account { id = accountCredential.id },
                };
                _recurlyInvoiceService.Create(recurlyInvoice);
            }
            else
            {
                storedPaymentInvoice.subscription = recurlySubscription;
                _recurlyInvoiceService.Update(storedPaymentInvoice);
            }

            var transactionId = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault().Uuid;
            var storedPaymentTransaction = _recurlyPaymentService.FirstOrDefault(x => x.transactionId == transactionId);
            if (storedPaymentTransaction == null)
            {
                var recurlyPayment = new RecurlyPayment
                {
                    createdAt = now,
                    transactionAt = invoiceCollection.ChargeInvoice.Transactions.First().CreatedAt.GetValueOrDefault(),
                    subscription = recurlySubscription,
                    subtotal = Convert.ToDecimal(invoiceCollection.ChargeInvoice.Subtotal.GetValueOrDefault()),
                    total = Convert.ToDecimal(invoiceCollection.ChargeInvoice.Total.GetValueOrDefault()),
                    paymentGateway = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault()?.PaymentGateway.Name,
                    customerMessage = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault()?.CustomerMessage,
                    statusCode = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault()?.Status,
                    statusMessage = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault()?.StatusMessage,
                    transactionId = transactionId,
                    email = invoiceCollection.ChargeInvoice.Transactions.FirstOrDefault().Account.Email,
                    invoiceNumber = invoiceCollection.ChargeInvoice.Number,
                    account = new Domain.Entities.Account { id = accountCredential.id },
                };
                _recurlyPaymentService.Create(recurlyPayment);
            }
            else
            {
                storedPaymentTransaction.subscription = recurlySubscription;
                _recurlyPaymentService.Update(storedPaymentTransaction);
            }
        }

        public static void SaveSubscriptionChangeLog(SubscriptionChange subscriptionChange, AccountCredentialModel accountCredential)
        {
            var now = DateUtil.GetDateTimeNow();
            var storedSubscription = _recurlySubscriptionService.FirstOrDefault(x => x.subscriptionId == subscriptionChange.SubscriptionId);
            if (storedSubscription != null)
            {
                storedSubscription.modifiedAt = now;
                storedSubscription.state = SystemStatus.PROCESSING.ToString();
                storedSubscription.planCode = subscriptionChange.Plan.Code;
                storedSubscription.planName = subscriptionChange.Plan.Name;
                storedSubscription.planId = subscriptionChange.Plan.Id;
                _recurlySubscriptionService.Update(storedSubscription);
            }
            else
            {
                var recurlySubscription = new RecurlySubscription
                {
                    account = new Domain.Entities.Account
                    {
                        id = accountCredential.id
                    },
                    createdAt = now,
                    modifiedAt = now,
                    planCode = subscriptionChange.Plan.Code,
                    planId = subscriptionChange.Plan.Id,
                    planName = subscriptionChange.Plan.Name,
                    state = SystemStatus.PROCESSING.ToString(),
                    status = SystemStatus.ACTIVE.ToString(),
                    subscriptionId = subscriptionChange.SubscriptionId,
                    uuid = Guid.NewGuid()
                };
                _recurlySubscriptionService.Create(recurlySubscription);
            }
        }

        public static void SaveErrorLog(Error error, PlanDataModel recurlyPlan, AccountCredentialModel accountCredential)
        {
            var now = DateUtil.GetDateTimeNow();
            var failedStatus = SystemStatus.FAILED.ToString();

            var recurlySubscription = new RecurlySubscription
            {
                account = new Domain.Entities.Account
                {
                    id = accountCredential.id
                },
                subscriptionId = failedStatus,
                createdAt = now,
                modifiedAt = now,
                planCode = recurlyPlan.code,
                planId = recurlyPlan.id,
                state = failedStatus,
                status = failedStatus,
                planName = recurlyPlan.name,
                uuid = Guid.NewGuid()
            };
            _recurlySubscriptionService.Create(recurlySubscription);

            var recurlyPayment = new RecurlyPayment
            {
                createdAt = now,
                subscription = recurlySubscription,
                statusCode = error.Type,
                statusMessage = error.Message
            };
            _recurlyPaymentService.Create(recurlyPayment);
        }
    }
}