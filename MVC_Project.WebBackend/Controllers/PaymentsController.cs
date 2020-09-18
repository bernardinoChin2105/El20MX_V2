using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Payments;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{

    public class PaymentsController : BaseController
    {
        private IPaymentService _paymentService;
        private IUserService _userService;
        private IPaymentServiceProvider paymentProviderService;
        private int TransferExpirationDays;
        private bool UseSelective3DSecure;
        private string GlobalClientId;
        private string SecureVerificationURL;
        private string OpenpayWebhookKey;
        private string AppKey;

        public PaymentsController(IPaymentService paymentService, IUserService userService)
        {
            _paymentService = paymentService;
            _userService = userService;

            AppKey = System.Configuration.ConfigurationManager.AppSettings["Payments.DefaultAppKey"];

            TransferExpirationDays = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Payments.TransferExpirationDays"]);
            UseSelective3DSecure = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["Payments.UseSelective3DSecure"]);
            GlobalClientId = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayGeneralClientId"];
            SecureVerificationURL = System.Configuration.ConfigurationManager.AppSettings["Payments.SecureVerificationURL"];
            OpenpayWebhookKey = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayWebhookKey"];
            paymentProviderService = new OpenPayService();
        }

        [Authorize]
        public ActionResult Index()
        {
            PaymentViewModel model = new PaymentViewModel();

            return View(model);
        }

        [Authorize]
        public ActionResult Checkout()
        {
            PaymentViewModel model = new PaymentViewModel();

            //Simular datos
            model.OrderId = Guid.NewGuid().ToString().Substring(24);
            model.Amount = Convert.ToInt32((new Random().NextDouble() * 10000));

            return View(model);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutProceed(PaymentViewModel model)
        {
            //Esto puede hacerse dinamico
            if (model.PaymentMethod == Integrations.Payments.PaymentMethod.BANK_ACCOUNT)
            {
                model.DueDate = DateUtil.GetDateTimeNow().AddDays(TransferExpirationDays);
                return CreateSPEI(model);
                //return View("CreateSPEI", model);
            }
            if (model.PaymentMethod == Integrations.Payments.PaymentMethod.CARD)
            {
                //Setear variables del conector
                PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(AppKey);
                model.MerchantId = paymentApp.MerchantId;
                model.PublicKey = paymentApp.PublicKey;
                return View("CreateTDC", model);
            }
            return RedirectToAction("Index", "Error");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSPEI(PaymentViewModel model)
        {
            //Setear variables del conector
            PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(AppKey);
            paymentProviderService.MerchantId = paymentApp.MerchantId;
            paymentProviderService.PublicKey = paymentApp.PublicKey;
            paymentProviderService.PrivateKey = paymentApp.PrivateKey;
            paymentProviderService.DashboardURL = paymentApp.DashboardURL;

            PaymentModel payment = new PaymentModel()
            {
                ClientId = paymentApp.ClientId,
                OrderId = model.OrderId,
                Amount = model.Amount,
                DueDate = DateUtil.GetDateTimeNow().AddDays(TransferExpirationDays),
                Description = String.Format("Pago de orden {0}", model.OrderId),
            };

            model.PaymentMethod = Integrations.Payments.PaymentMethod.BANK_ACCOUNT;
            payment = paymentProviderService.CreateBankTransferPayment(payment);
            model.ChargeSuccess = payment.ChargeSuccess;

            if (payment.ChargeSuccess)
            {
                //Primero guardar en BD
                Payment paymentBO = new Payment();
                paymentBO.CreationDate = DateUtil.GetDateTimeNow();
                paymentBO.User = new User() { id = Authenticator.AuthenticatedUser.Id };
                paymentBO.Amount = model.Amount;
                paymentBO.OrderId = model.OrderId;
                paymentBO.ConfirmationEmail = model.ConfirmationEmail;
                paymentBO.ProviderId = payment.Id;
                paymentBO.Status = payment.Status;
                paymentBO.DueDate = payment.DueDate;
                paymentBO.Method = Integrations.Payments.PaymentMethod.BANK_ACCOUNT;
                paymentBO.TransactionType = PaymentType.CHARGE;

                paymentBO.ConfirmationDate = null;

                _paymentService.Create(paymentBO);

                model.Id = payment.Id;
                model.Description = payment.Description;
                model.JsonData = payment.ResultData;
                model.DueDate = payment.DueDate;
                model.PaymentCardURL = payment.PaymentCardURL;
                model.BankName = payment.PaymentMethod.BankName;
                model.Clabe = payment.PaymentMethod.Clabe;
                model.Reference = payment.PaymentMethod.Reference;
                model.Name = payment.PaymentMethod.Name;
                model.Agreement = payment.PaymentMethod.Agreement;
            }
            else
            {
                model.Description = payment.ResultData;
            }

            //return View("CreateSPEI", model);
            return View("CheckoutSuccess", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTDC(PaymentViewModel model)
        {
            //Setear variables del conector
            PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(AppKey);
            paymentProviderService.MerchantId = paymentApp.MerchantId;
            paymentProviderService.PublicKey = paymentApp.PublicKey;
            paymentProviderService.PrivateKey = paymentApp.PrivateKey;
            paymentProviderService.DashboardURL = paymentApp.DashboardURL;

            PaymentModel payment = new PaymentModel()
            {
                ClientId = paymentApp.ClientId,
                OrderId = model.OrderId,
                Amount = model.Amount,
                TokenId = model.TokenId,
                DeviceSessionId = model.DeviceSessionId,
                Description = String.Format("Pago de orden {0}", model.OrderId),
                RedirectUrl = SecureVerificationURL,
                //Use3DSecure = true
            };

            //Primero en BD
            Payment paymentBO = new Payment();
            paymentBO.CreationDate = DateUtil.GetDateTimeNow();
            paymentBO.User = new User() { id = Authenticator.AuthenticatedUser.Id };
            paymentBO.Amount = model.Amount;
            paymentBO.OrderId = model.OrderId;
            paymentBO.ConfirmationEmail = model.ConfirmationEmail;
            paymentBO.Status = PaymentStatus.IN_PROGRESS;
            paymentBO.Method = Integrations.Payments.PaymentMethod.CARD;
            paymentBO.TransactionType = PaymentType.CHARGE;

            paymentBO.ConfirmationDate = null;
            _paymentService.Create(paymentBO);

            //Luego cobrar
            model.PaymentMethod = Integrations.Payments.PaymentMethod.CARD;
            payment = paymentProviderService.CreateTDCPayment(payment);

            //Si hubiera reintento, probar Antifraude
            if (UseSelective3DSecure && !payment.ChargeSuccess & payment.ResultCode == PaymentError.ANTI_FRAUD)
            {
                payment.Use3DSecure = true;
                payment = paymentProviderService.CreateTDCPayment(payment);
            }

            model.ChargeSuccess = payment.ChargeSuccess;

            if (payment.ChargeSuccess)
            {
                //Luego actualizar
                paymentBO.ProviderId = payment.Id;
                paymentBO.Status = payment.Status;
                paymentBO.DueDate = payment.DueDate;
                paymentBO.LogData = payment.ResultData;
                _paymentService.Update(paymentBO);

                model.Id = payment.Id;
                model.Description = payment.Description;
                model.JsonData = payment.ResultData;
                model.DueDate = payment.DueDate;
                model.PaymentCardURL = payment.PaymentCardURL;
            }
            else
            {
                paymentBO.Status = PaymentStatus.ERROR;
                paymentBO.LogData = payment.ResultData;
                _paymentService.Update(paymentBO);
                model.Description = payment.ResultData;
            }

            //
            Session.Add("Payments.PaymentModel", model);

            if (payment.PaymentMethod != null && !string.IsNullOrEmpty(payment.PaymentMethod.RedirectUrl))
            {
                return Redirect(payment.PaymentMethod.RedirectUrl);
            }

            //return View("CreateTDC", model);
            return View("CheckoutSuccess", model);
        }


        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult SecureVerification(string id)
        {
            Payment payment = _paymentService.GetByProviderId(id);
            PaymentViewModel model = (PaymentViewModel)Session["Payments.PaymentModel"];
            return View("CheckoutSuccess", model);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        {
            try
            {
                AuthUser authUser = Authenticator.AuthenticatedUser;

                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                filtersValues["UserId"] = "-1";
                if (authUser.Role.Code != Constants.ROLE_ADMIN && authUser.Role.Code != Constants.ROLE_IT_SUPPORT)
                {
                    filtersValues["UserId"] = Convert.ToString(authUser.Id);
                }

                var payments = _paymentService.FilterBy(filtersValues, param.iDisplayStart, param.iDisplayLength);

                IList<PaymentViewModel> dataResponse = new List<PaymentViewModel>();
                foreach (var payment in payments.Item1)
                {
                    PaymentViewModel resultData = new PaymentViewModel
                    {
                        Id = Convert.ToString(payment.id),
                        OrderId = payment.OrderId,
                        Amount = payment.Amount,
                        PaymentMethod = payment.Method,
                        ProviderId = payment.ProviderId,
                        Status = payment.Status,
                        CreationDate = payment.CreationDate.ToString(Constants.DATE_FORMAT_CALENDAR),
                        ConfirmationDate = payment.ConfirmationDate.HasValue ? payment.ConfirmationDate.Value.ToString(Constants.DATE_FORMAT_CALENDAR) : "",
                        User = payment.User.name
                    };
                    dataResponse.Add(resultData);
                }
                return Json(new
                {
                    success = true,
                    param.sEcho,
                    iTotalRecords = dataResponse.Count(),
                    iTotalDisplayRecords = payments.Item2,
                    aaData = dataResponse
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Tracking()
        {

            string urlKey = Request.QueryString["k"];
            if (string.IsNullOrWhiteSpace(urlKey) || urlKey != OpenpayWebhookKey)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            System.IO.StreamReader reader = new System.IO.StreamReader(HttpContext.Request.InputStream);
            string rawJSON = reader.ReadToEnd();
            System.Diagnostics.Trace.TraceInformation("PaymentsController [rawJSON] : " + rawJSON); // For debugging to the Azure Streaming logs

            PaymentEventModel paymentEvent = JsonConvert.DeserializeObject<PaymentEventModel>(rawJSON);
            if (paymentEvent != null)
            {
                if (!string.IsNullOrWhiteSpace(paymentEvent.type))
                {
                    System.Diagnostics.Trace.TraceInformation("\tTransaction: " + paymentEvent.transaction);
                    if (paymentEvent.transaction != null)
                    {
                        System.Diagnostics.Trace.TraceInformation("\t\t Transaction Id: " + paymentEvent.transaction.id);
                        System.Diagnostics.Trace.TraceInformation("\t\t Order Id: " + paymentEvent.transaction.order_id);
                        System.Diagnostics.Trace.TraceInformation("\t\t Authorization: " + paymentEvent.transaction.authorization);

                        Payment paymentBO = _paymentService.GetByOrderId(paymentEvent.transaction.order_id);
                        User user = paymentBO.User;//_userService.GetById(payment.User.Id);

                        if (paymentBO != null)
                        {
                            System.Diagnostics.Trace.TraceInformation("\t\t Payment BO ID: " + paymentBO.id);
                            paymentBO.Status = paymentEvent.transaction.status;
                            paymentBO.LogData = rawJSON;
                            if (paymentEvent.type == PaymentEventStatus.CHARGE_SUCCEEDED)
                            {
                                paymentBO.ConfirmationDate = DateUtil.GetDateTimeNow(); //lo tomamos cuando llega el evento
                                paymentBO.AuthorizationCode = paymentEvent.transaction.authorization;

                                Dictionary<string, string> customParams = new Dictionary<string, string>();
                                customParams.Add("param1", user.profile.firstName);
                                customParams.Add("param2", paymentEvent.transaction.order_id);
                                customParams.Add("param3", paymentBO.AuthorizationCode);
                                customParams.Add("param4", paymentEvent.transaction.id);
                                customParams.Add("param5", paymentBO.ConfirmationDate.Value.ToString(Constants.DATE_FORMAT));
                                customParams.Add("param6", string.Format("{0:#.00}", paymentBO.Amount));
                                customParams.Add("param7", paymentBO.Method);
                                string confirmationEmail = !string.IsNullOrWhiteSpace(paymentBO.ConfirmationEmail) ? paymentBO.ConfirmationEmail : user.name;
                                NotificationUtil.SendNotification(confirmationEmail, customParams, Constants.NOT_TEMPLATE_CHARGESUCCESS);
                            }
                            _paymentService.Update(paymentBO);
                            System.Diagnostics.Trace.TraceInformation("\t\t Payment BO Status Updated: " + paymentBO.Status);
                        }
                    }
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //[Authorize]
        //public ActionResult CreatePayment(PaymentViewModel model)
        //{
        //    OpenPayService paymentProviderService = new OpenPayService();
        //    PaymentModel payment = new PaymentModel()
        //    {
        //        ClientId = "avfwrv0q9x2binx9odgf",
        //        OrderId = model.OrderId,
        //        Amount = model.Amount,
        //        Description = String.Format("Payment for Order Id # {0}", model.OrderId),
        //        TokenId = model.TokenId,
        //        DeviceSessionId = model.DeviceSessionId
        //    };
        //    model.ChargeSuccess = false;

        //    #region Pagos con SPEI
        //    if (model.PaymentMethod == PaymentMethod.BANK_ACCOUNT)
        //    {
        //        payment = paymentProviderService.CreateBankTransferPayment(payment);
        //        model.ChargeSuccess = payment.ChargeSuccess;
        //        if (payment.ChargeSuccess)
        //        {
        //            //Primero guardar en BD
        //            Payment paymentBO = new Payment();
        //            paymentBO.CreationDate = DateUtil.GetDateTimeNow();
        //            paymentBO.User = new User() { Id = Authenticator.AuthenticatedUser.Id };
        //            paymentBO.Amount = model.Amount;
        //            paymentBO.OrderId = model.OrderId;
        //            paymentBO.ProviderId = payment.Id;
        //            paymentBO.Status = payment.Status;
        //            paymentBO.DueDate = payment.DueDate;
        //            paymentBO.Method = PaymentMethod.BANK_ACCOUNT;
        //            paymentBO.TransactionType = PaymentType.CHARGE;

        //            paymentBO.ConfirmationDate = null;

        //            _paymentService.Create(paymentBO);

        //            model.Id = payment.Id;
        //            model.Description = payment.Description;
        //            model.JsonData = payment.ResultData;
        //            model.DueDate = payment.DueDate;
        //            model.PaymentCardURL = payment.PaymentCardURL;
        //            model.BankName = payment.PaymentMethod.BankName;
        //            model.Clabe = payment.PaymentMethod.Clabe;
        //            model.Reference = payment.PaymentMethod.Reference;
        //            model.Name = payment.PaymentMethod.Name;
        //            model.Agreement = payment.PaymentMethod.Agreement;
        //        }

        //    }
        //    #endregion

        //    #region Pagos con Tarjeta
        //    if (model.PaymentMethod == PaymentMethod.CARD)
        //    {
        //        //Primero en BD
        //        Payment paymentBO = new Payment();
        //        paymentBO.CreationDate = DateUtil.GetDateTimeNow();
        //        paymentBO.User = new User() { Id = Authenticator.AuthenticatedUser.Id };
        //        paymentBO.Amount = model.Amount;
        //        paymentBO.OrderId = model.OrderId;
        //        paymentBO.Status = PaymentStatus.IN_PROGRESS;
        //        paymentBO.Method = PaymentMethod.CARD;
        //        paymentBO.TransactionType = PaymentType.CHARGE;

        //        paymentBO.ConfirmationDate = null;
        //        _paymentService.Create(paymentBO);

        //        //Luego cobrar
        //        payment = paymentProviderService.CreateTDCPayment(payment);
        //        model.ChargeSuccess = payment.ChargeSuccess;

        //        if (payment.ChargeSuccess)
        //        {
        //            //Luego actualizar
        //            paymentBO.ProviderId = payment.Id;
        //            paymentBO.Status = payment.Status;
        //            paymentBO.DueDate = payment.DueDate;
        //            paymentBO.LogData = payment.ResultData;
        //            _paymentService.Update(paymentBO);

        //            model.Id = payment.Id;
        //            model.Description = payment.Description;
        //            model.JsonData = payment.ResultData;
        //            model.DueDate = payment.DueDate;
        //            model.PaymentCardURL = payment.PaymentCardURL;
        //        }
        //        else
        //        {
        //            paymentBO.Status = PaymentStatus.ERROR;
        //            paymentBO.LogData = payment.ResultData;
        //            _paymentService.Update(paymentBO);
        //            model.Description = payment.ResultData;
        //        }
        //    }
        //    #endregion

        //    if (!model.ChargeSuccess)
        //    {
        //        model.Description = payment.ResultData;
        //    }

        //    return View("CheckoutSuccess", model);
        //}


        //[Authorize]
        //[ValidateAntiForgeryToken]
        //[HttpGet]
        //public ActionResult CreateTDC()
        //{
        //    PaymentViewModel model = new PaymentViewModel();

        //    return View(model);
        //}

        //[Authorize]
        //[HttpGet]
        //public ActionResult CreateSPEI()
        //{
        //    PaymentViewModel model = new PaymentViewModel();
        //    model.OrderId = Guid.NewGuid().ToString().Substring(24);
        //    return View(model);
        //}

        // GET: Payments

    }
}