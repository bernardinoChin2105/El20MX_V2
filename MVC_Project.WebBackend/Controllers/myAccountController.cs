using LogHubSDK.Models;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Integrations.Recurly.Models;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class MyAccountController : Controller
    {
        private ICredentialService _credentialService;
        //private IMembershipService _membership;
        //private IAccountService _accountService;
        //private IUserService _userService;  

        //IMembershipService accountUserService, IAccountService accountService, , IUserService userService
        public MyAccountController(ICredentialService credentialService)
        {
            _credentialService = credentialService;            
            //_membership = accountUserService;
            //_accountService = accountService;
            //_userService = userService;
        }

        // GET: myAccount
        public ActionResult Index()
        {
            MyAccountVM model = new MyAccountVM();
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(), //DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy"),
                MaxDate = DateTime.Now.ToString("dd/MM/yyyy")
            };
            SetCombos(ref model);

            #region Metodo para crear una nueva cuenta en recurly. Esta en una ubicación temporal mientras se logra ubicar el correcto.
            /*
             Creación temporal de la cuenta en Recurly, mientras le pido más información a William
             */
            //try
            //{
            //    var authUser = Authenticator.AuthenticatedUser;
            //    var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
            //    var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

            //    CreateAccountModel newAccount = new CreateAccountModel();
            //    DateTime todayDate = DateUtil.GetDateTimeNow();

            //    newAccount.code = authUser.Account.Uuid.ToString(); //me falta pensar cual poner
            //    newAccount.username = authUser.Email;
            //    newAccount.email = authUser.Email; //Preguntar si estos datos son los que iran
            //    newAccount.preferred_locale = "es-MX";
            //    newAccount.first_name = authUser.FirstName;
            //    newAccount.last_name = authUser.LastName;
            //    newAccount.company = authUser.Account.Name;

            //    //var accountModel = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(newAccount));
            //    var serilaizeJson = JsonConvert.SerializeObject(newAccount, Newtonsoft.Json.Formatting.None,
            //    new JsonSerializerSettings
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });
            //    dynamic accountRSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
            //    var accountRecurly = RecurlyService.CreateAccount(accountRSend, siteId, provider);

            //    if (accountRecurly != null)
            //    {
            //        var account = new Domain.Entities.Account { id = authUser.Account.Id };
            //        var credential = new Domain.Entities.Credential()
            //        {
            //            account = new Domain.Entities.Account { id = authUser.Account.Id },
            //            uuid = Guid.NewGuid(),
            //            provider = provider,
            //            idCredentialProvider = accountRecurly.id,
            //            statusProvider = accountRecurly.state,
            //            createdAt = todayDate,
            //            modifiedAt = todayDate,
            //            status = SystemStatus.ACTIVE.ToString(),
            //            credentialType = accountRecurly.hosted_login_token //Token para la pagina
            //        };

            //        _credentialService.Create(credential);
            //    }

            //    //Guardar las credenciales y las que son necesarias para mostrar la pantalla
            //}
            //catch (Exception ex)
            //{
            //    string error = ex.Message.ToString();
            //}
            #endregion



            return View(model);
        }

        //public ActionResult AcountInvoicing()
        //{
        //    return View();
        //}

        [HttpGet, AllowAnonymous]
        public JsonResult GetMyAccounts(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            //var listResponse = new List<AlliesList>();
            //var list = new List<AlliesListVM>();
            string error = string.Empty;
            bool success = true;

            object[] listResponse = new object[3];
            listResponse[0] = new { id = 15, month = "Noviembre", year = 2020, status = "Vigente", amountTotal = "$ 1,500.00", payInvoice = true };
            listResponse[1] = new { id = 15, month = "Octubre", year = 2020, status = "Vencida", amountTotal = "$ 1,500.00", payInvoice = true };
            listResponse[2] = new { id = 15, month = "Septiembre", year = 2020, status = "Pagada", amountTotal = "$ 1,500.00", payInvoice = true };

            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);

                //string rfc = filtersValues.Get("RFC").Trim();
                //string businessName = filtersValues.Get("BusinessName").Trim();
                //string email = filtersValues.Get("Email").Trim();

                //var pagination = new BasePagination();
                //var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                //pagination.PageSize = param.iDisplayLength;
                //pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                //if (rfc != "") filters.rfc = rfc;
                //if (businessName != "") filters.businessName = businessName;
                //if (email != "") filters.email = email;

                //listResponse = _customerService.CustomerList(pagination, filters);

                ////Corroborar los campos iTotalRecords y iTotalDisplayRecords

                //if (listResponse.Count() > 0)
                //{
                //    totalDisplay = listResponse[0].Total;
                //    total = listResponse.Count();
                //}


                //return Json(new
                //{
                //    success = true,
                //    sEcho = param.sEcho,
                //    iTotalRecords = total,
                //    iTotalDisplayRecords = totalDisplay,
                //    aaData = listResponse
                //}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                error = ex.Message.ToString();
                success = false;

                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
            }

            return Json(new
            {
                success = success,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult RedeemCoupon(string couponCode)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                if(string.IsNullOrWhiteSpace(couponCode))
                {
                    throw new ArgumentException("Código de cupón requerido.");
                }

                var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
                var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                var recurlyCredential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == provider && x.status == SystemStatus.ACTIVE.ToString());
                if (recurlyCredential == null)
                {
                    throw new ArgumentException("Cuenta no encontrada.");
                }

                var couponRequest = new CouponRedemptionCreate()
                {
                    CouponId = "code-" + couponCode
                };

                var redemptionResponse = RecurlyService.CreateCouponRedemption(couponRequest, siteId, recurlyCredential.idCredentialProvider, provider);

                LogUtil.AddEntry(
                   "Cupón canjeado: " + couponCode,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.AUTHORIZATION,
                   string.Format("Cupon {0} | Cuenta {1} | Fecha {2}", couponCode, recurlyCredential.idCredentialProvider, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(redemptionResponse)
                );

                return Json(new
                {
                    status = true,
                    message = "Cupón canjeado correctamente."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (RecurlyErrorException recurlyError)
            {
                var mainErrorMessage = "";
                var userMessage = "El cupón no pudo ser canjeado. Verifique e intentelo más tarde";
                mainErrorMessage = recurlyError.Error.Message;

                //Se trata de identificar los casos más comunes de error para mostrar un mensaje más significativo al usuario.
                if (mainErrorMessage.Contains("coupon_id"))
                {
                    userMessage = "Código no válido.";
                }
                else if (mainErrorMessage.Contains("has expired"))
                {
                    userMessage = "El cupón ha expirado.";
                }
                else if(mainErrorMessage.Contains("max redemptions"))
                {
                    userMessage = "El cupón ya ha sido canjeado.";
                }

                LogUtil.AddEntry(
                   "Error al canjear cupón: " + mainErrorMessage,
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.AUTHORIZATION,
                   string.Format("Usuario {0} | Fecha {1} | Cupon {2}", authUser.Email, DateUtil.GetDateTimeNow(), couponCode),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(recurlyError.Error)
                );

                return Json(new
                {
                    status = false,
                    error = userMessage
                }, JsonRequestBehavior.AllowGet);
            }
            catch (ArgumentException ex)
            {
                string error = ex.Message.ToString();
                LogUtil.AddEntry(
                   "Se encontro un error: " + error,
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.AUTHORIZATION,
                   string.Format("Usuario {0} | Fecha {1} | Cupon {2}", authUser.Email, DateUtil.GetDateTimeNow(), couponCode),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );

                return Json(new
                {
                    status = false,
                    error = error
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {                
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message,
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.AUTHORIZATION,
                   string.Format("Usuario {0} | Fecha {1} | Cupon {2}", authUser.Email, DateUtil.GetDateTimeNow(), couponCode),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );

                return Json(new
                {
                    status = false,
                    error = "El cupón no pudo ser canjeado. Verifique e intentelo más tarde."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private void SetCombos(ref MyAccountVM model)
        {
            model.ListStatusPayment = Enum.GetValues(typeof(StatusPayment)).Cast<StatusPayment>()
                   .Select(e => new SelectListItem
                   {
                       Value = e.ToString(),
                       Text = EnumUtils.GetDisplayName(e)
                   }).ToList();
        }

    }
}