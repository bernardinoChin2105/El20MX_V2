using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Payments;
using MVC_Project.Utils;
using MVC_Project.Web.AuthManagement;
using MVC_Project.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
{
    [AllowAnonymous]
    public class CheckoutController : BaseController
    {

        private IPaymentService _paymentService;
        private IUserService _userService;
        private IPaymentServiceProvider paymentProviderService;
        private int TransferExpirationDays;
        private bool UseSelective3DSecure;
        //private string GlobalClientId;
        private string SecureVerificationURL;
        private string OpenpayWebhookKey;

        public CheckoutController(IPaymentService paymentService, IUserService userService)
        {
            _paymentService = paymentService;
            _userService = userService;
            TransferExpirationDays = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Payments.TransferExpirationDays"]);
            UseSelective3DSecure = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["Payments.UseSelective3DSecure"]);
            //GlobalClientId = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayGeneralClientId"];
            SecureVerificationURL = System.Configuration.ConfigurationManager.AppSettings["Payments.SecureVerificationURL"];
            OpenpayWebhookKey = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayWebhookKey"];
            paymentProviderService = new OpenPayService();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Test()
        {
            //Simular datos
            String appKey = "96700712-ba90-4c68-8a9a-0f51b158f745";
            String newOrderId = Guid.NewGuid().ToString().Substring(24);
            PaymentViewModel model = new PaymentViewModel
            {
                AppKey = appKey,
                OrderId = newOrderId,
                Amount = Convert.ToInt32((new Random().NextDouble() * 10000)),
                Description = String.Format("Pago de orden # {0}", newOrderId)
            };
            return View("Test", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(PaymentViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AppKey))
            {
                return RedirectToAction("ShowError", "Error", new { errorMessage = "Error en los parámetros enviados: Parámetro AppKey no enviado" });
            }
            if (string.IsNullOrWhiteSpace(model.OrderId))
            {
                return RedirectToAction("ShowError", "Error", new { errorMessage = "Error en los parámetros enviados: Parámetro OrderId no enviado" });
            }
            if (model.Amount <= 0)
            {
                return RedirectToAction("ShowError", "Error", new { errorMessage = "Error en los parámetros enviados: Amount debe ser decimal" });
            }
            PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(model.AppKey);
            if (paymentApp == null)
            {
                return RedirectToAction("ShowError", "Error", new { errorMessage = string.Format("Error, no se encuentra la aplicación registrada con el appKey={0}", model.AppKey) });
            }

            model.AppName = paymentApp.Name;
            model.MerchantId = paymentApp.MerchantId;
            model.PublicKey = paymentApp.PublicKey;

            if (model.PaymentMethod == PaymentMethod.CARD)
            {
                return View("Index", model);
            }
            if (model.PaymentMethod == PaymentMethod.BANK_ACCOUNT)
            {
                return ProceedSPEI(model);
            }
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProceedSPEI(PaymentViewModel model)
        {
            //Setear variables del conector
            PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(model.AppKey);
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
                Description = model.Description,
            };

            model.PaymentMethod = PaymentMethod.BANK_ACCOUNT;
            payment = paymentProviderService.CreateBankTransferPayment(payment);
            model.ChargeSuccess = payment.ChargeSuccess;
            if (payment.ChargeSuccess)
            {
                //Primero guardar en BD
                Payment paymentBO = new Payment();
                paymentBO.CreationDate = DateUtil.GetDateTimeNow();
                paymentBO.User = paymentApp.User;
                paymentBO.Amount = model.Amount;
                paymentBO.OrderId = model.OrderId;
                paymentBO.ConfirmationEmail = model.ConfirmationEmail;
                paymentBO.ProviderId = payment.Id;
                paymentBO.Status = payment.Status;
                paymentBO.DueDate = payment.DueDate;
                paymentBO.Method = PaymentMethod.BANK_ACCOUNT;
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

                return RedirectPermanent(payment.PaymentCardURL);

            }
            else
            {
                model.Description = payment.ResultData;
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProceedTDC(PaymentViewModel model)
        {
            //Setear variables del conector
            PaymentApplication paymentApp = _paymentService.GetPaymentApplicationByKey(model.AppKey);
            paymentProviderService.MerchantId = paymentApp.MerchantId;
            paymentProviderService.PublicKey = paymentApp.PublicKey;
            paymentProviderService.PrivateKey = paymentApp.PrivateKey;

            //Crear modelo
            PaymentModel payment = new PaymentModel()
            {
                ClientId = paymentApp.ClientId,
                OrderId = model.OrderId,
                Amount = model.Amount,
                Description = model.Description,
                TokenId = model.TokenId,
                DeviceSessionId = model.DeviceSessionId,
                RedirectUrl = SecureVerificationURL,
                //Use3DSecure = true
            };

            //Primero en BD
            Payment paymentBO = new Payment();
            paymentBO.CreationDate = DateUtil.GetDateTimeNow();
            paymentBO.User = paymentApp.User;
            paymentBO.Amount = model.Amount;
            paymentBO.OrderId = model.OrderId;
            paymentBO.ConfirmationEmail = model.ConfirmationEmail;
            paymentBO.Status = PaymentStatus.IN_PROGRESS;
            paymentBO.Method = PaymentMethod.CARD;
            paymentBO.TransactionType = PaymentType.CHARGE;
            paymentBO.ConfirmationDate = null;
            _paymentService.Create(paymentBO);

            //Luego cobrar
            model.PaymentMethod = PaymentMethod.CARD;
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

            //if (payment.PaymentMethod != null && !string.IsNullOrEmpty(payment.PaymentMethod.RedirectUrl))
            //{
            //    return Redirect(payment.PaymentMethod.RedirectUrl);
            //}
            if (paymentApp.ReturnURL != null && !string.IsNullOrEmpty(paymentApp.ReturnURL))
            {
                FormCollection formCollection = new FormCollection
                {
                    { "payment_id", paymentBO.ProviderId },
                    { "status", paymentBO.Status }
                };
                
                //SendPostRequest(paymentApp.ReturnURL, formCollection);
                return Content("<form action='" + paymentApp.ReturnURL + "' id='frmReturnURL' method='POST'>" +
                    "<input type='hidden' name='Id' value='" + paymentBO.Id + "' />" +
                    "<input type='hidden' name='ProviderId' value='" + paymentBO.ProviderId + "' />" +
                    "<input type='hidden' name='CreationDate' value='" + paymentBO.CreationDate + "' />" +
                    "<input type='hidden' name='Status' value='" + paymentBO.Status + "' />" +
                    "<input type='hidden' name='Amount' value='" + paymentBO.Amount + "' />" +
                    "<input type='hidden' name='OrderId' value='" + paymentBO.OrderId + "' />" +
                    "<input type='hidden' name='Method' value='" + paymentBO.Method + "' />" +
                    "<input type='hidden' name='TransactionType' value='" + paymentBO.TransactionType + "' />" +
                    "</form>" +
                    "<script>document.getElementById('frmReturnURL').submit();</script>");
            }

            return View("Test");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult TestResult(FormCollection formCollection)
        {
            Dictionary<string, string> formValues = formCollection.AllKeys.ToDictionary(k => k, v => formCollection[v]);
            ViewData["ResultValues"] = formValues;
            return View();
        }
        
    }
}