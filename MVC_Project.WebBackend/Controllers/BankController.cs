using LogHubSDK.Models;
using MVC_Project.Domain.Services;
using MVC_Project.Domain.Entities;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class BankController : Controller
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IBankService _bankService;

        public BankController(IAccountService accountService, ICredentialService credentialService, IBankService bankService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _bankService = bankService;
        }

        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBanks(JQueryDataTableParams param, string filtros, bool first)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                int totalDisplay = 0;
                int total = 0;
                var listResponse = new List<Object>();//List<CustomerList>();
                //if (!first)
                //{
                //    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                //    string rfc = filtersValues.Get("RFC").Trim();
                //    string businessName = filtersValues.Get("BusinessName").Trim();
                //    string email = filtersValues.Get("Email").Trim();

                //    var pagination = new BasePagination();
                //    var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                //    pagination.PageSize = param.iDisplayLength;
                //    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                //    if (rfc != "") filters.rfc = rfc;
                //    if (businessName != "") filters.businessName = businessName;
                //    if (email != "") filters.email = email;

                //    listResponse = _customerService.CustomerList(pagination, filters);

                //    //Corroborar los campos iTotalRecords y iTotalDisplayRecords

                //    if (listResponse.Count() > 0)
                //    {
                //        totalDisplay = listResponse[0].Total;
                //        total = listResponse.Count();
                //    }
                //}

                LogUtil.AddEntry(
                   "Lista de Bancos total: " + totalDisplay + ", totalDisplay: " + total,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResponse
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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

                return new JsonResult
                {
                    Data = new { success = false, message = ex.Message },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        private string Token()
        {
            var authUser = Authenticator.AuthenticatedUser;
            string token = (string)Session["token"];
            string tokenUser = string.Empty;
            if (token != null)
            {
                tokenUser = PaybookService.GetVarifyToken(token);
            }

            if (tokenUser == null)
            {
                var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());
                tokenUser = PaybookService.CreateToken(credential.idCredentialProvider);
                Session["token"] = tokenUser;
            }

            return tokenUser;
        }

        //Llamadas para paybook
        [HttpGet, AllowAnonymous]
        public JsonResult GetToken()
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                string token = (string)Session["token"];
                string tokenUser = string.Empty;
                if (token != null)
                {
                    tokenUser = PaybookService.GetVarifyToken(token);
                }

                if (tokenUser == null)
                {
                    #region Crear credencial el usuario de la cuenta rfc, si aun no ha sido creada
                    //Crear usuario si aun no ha sido creado
                    //Obtener idUsuario
                    var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());
                    DateTime todayDate = DateUtil.GetDateTimeNow();

                    if (credential == null)
                    {
                        var credentialPaybook = PaybookService.CreateUser(authUser.Account.RFC, authUser.Account.Uuid.ToString());

                        credential = new Domain.Entities.Credential()
                        {
                            account = new Domain.Entities.Account() { id = authUser.Account.Id },
                            provider = SystemProviders.SYNCFY.GetDisplayName(), //"Paybook",
                            idCredentialProvider = credentialPaybook.id_user,
                            statusProvider = SystemStatus.ACTIVE.ToString(),
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };

                        _credentialService.Create(credential);
                    }

                    //Obtener token de paybook para retornar
                    tokenUser = PaybookService.CreateToken(credential.idCredentialProvider);
                    Session["token"] = tokenUser;
                    #endregion
                }

                LogUtil.AddEntry(
                   "Obtener token del cliente",
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return Json(new
                {
                    Data = new { success = true, data = tokenUser },
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return new JsonResult
                {
                    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult CreateCredentialBank(string idCredential)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                string token = Token();
                //Obtener el listado de las cuentas de bancos
                List<CredentialsPaybook> newBanks = PaybookService.GetCredentials(idCredential, token);
                //List<string> idCredentialsNews = newBanks.Select(x => x.id_credential).ToList();
                //List<string> idCredSaved = _bankService.ValidateBankCredentials(idCredentialsNews, authUser.Account.Id);
                //List<string> NoExist = idCredentialsNews.Except(idCredSaved).ToList();
                DateTime todayDate = DateUtil.GetDateTimeNow();

                if (newBanks.Count() > 0)
                {
                    foreach (var itemBank in newBanks)
                    {
                        //Guardar los listado de bancos nuevos
                        BankCredential newBankCred = new BankCredential()
                        {
                            uuid = Guid.NewGuid(),
                            account = new Account { id = authUser.Account.Id },
                            credentailProviderId = itemBank.id_credential,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = itemBank.is_authorized.ToString(),
                            bank = new Bank { id = 0 } // Cambiar por los bancos del catalogo
                        };

                        //Preguntarle por el guardado

                        //Obtener las cuentas de los bancos nuevos
                        var bankAccounts = PaybookService.GetAccounts(itemBank.id_credential, token);
                        foreach (var itemAccount in bankAccounts)
                        {
                            BankAccount newBankAcc = new BankAccount()
                            {
                                uuid = Guid.NewGuid(),

                                accountProviderId = itemAccount.id_account,
                                accountProviderType = itemAccount.account_type,
                                name = itemAccount.name,
                                currency = itemAccount.currency,
                                //refreshAt = itemAccount.dt_refresh,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = itemAccount.is_disable.ToString(),
                                //bankAccount = newBankCred, //Falta este dato 
                                //Account account { get; set; } //Preguntarle porque esta relación
                                //Bank bank { get; set; }
                            };
                        }

                        //--Obtener las transacciones de las cuentas nuevas
                    }
                }

                LogUtil.AddEntry(
                   "Obtener token del cliente",
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return Json(new
                {
                    Data = new { success = true, data = "" },
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return new JsonResult
                {
                    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        public ActionResult AccountList(string idBankCredential)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                var bankAccounts = _bankService.GetBanksAccounts(idBankCredential);
                return View(bankAccounts);
            }
            catch(Exception ex)
            {
                return View("Index");
            }
        }
    }
}