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
using MVC_Project.Domain.Model;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace MVC_Project.WebBackend.Controllers
{
    public class BankController : Controller
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IBankService _bankService;
        private IBankCredentialService _bankCredentialService;
        private IBankAccountService _bankAccountService;

        public BankController(IAccountService accountService, ICredentialService credentialService, IBankService bankService,
            IBankCredentialService bankCredentialService, IBankAccountService bankAccountService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _bankService = bankService;
            _bankCredentialService = bankCredentialService;
            _bankAccountService = bankAccountService;
        }

        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index()
        {
            var authUser = Authenticator.AuthenticatedUser;

            string token = (string)Session["token"];
            string tokenUser = string.Empty;
            if (token != null && token != "")
            {
                try
                {
                    if (PaybookService.GetVarifyToken(token))
                        tokenUser = token;
                }
                catch (Exception ex)
                {
                    token = null;
                }

                ViewBag.paybookT = tokenUser;
            }

            if (tokenUser == null || tokenUser == "")
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
                ViewBag.paybookT = tokenUser;

                LogUtil.AddEntry(
                   "Se Creo el token en el inicio: " + tokenUser,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                #endregion
            }
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBanks(JQueryDataTableParams param)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<BankCredentialsList>();
            string error = string.Empty;

            try
            {

                listResponse = _bankCredentialService.GetBankCredentials(userAuth.Account.Id);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }

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

                error = ex.Message.ToString();

                //return new JsonResult
                //{
                //    Data = new { success = false, message = ex.Message },
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    MaxJsonLength = Int32.MaxValue
                //};
            }

            return Json(new
            {
                success = true,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);
        }

        private string Token()
        {
            var authUser = Authenticator.AuthenticatedUser;
            string token = (string)Session["token"];
            string tokenUser = string.Empty;
            if (token != null && token != "")
            {
                try
                {
                    if (PaybookService.GetVarifyToken(token))
                        tokenUser = token;
                }
                catch (Exception ex)
                {
                    token = null;
                }
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
                //string token = (string)Session["token"];
                //string tokenUser = string.Empty;
                //if (token != null && token != "")
                //{
                //    tokenUser = PaybookService.GetVarifyToken(token) ? token : null;
                //}

                //if (tokenUser == null || tokenUser == "")
                //{
                //    #region Crear credencial el usuario de la cuenta rfc, si aun no ha sido creada
                //    //Crear usuario si aun no ha sido creado
                //    //Obtener idUsuario
                //    var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());
                //    DateTime todayDate = DateUtil.GetDateTimeNow();

                //    if (credential == null)
                //    {
                //        var credentialPaybook = PaybookService.CreateUser(authUser.Account.RFC, authUser.Account.Uuid.ToString());

                //        credential = new Domain.Entities.Credential()
                //        {
                //            account = new Domain.Entities.Account() { id = authUser.Account.Id },
                //            provider = SystemProviders.SYNCFY.GetDisplayName(), //"Paybook",
                //            idCredentialProvider = credentialPaybook.id_user,
                //            statusProvider = SystemStatus.ACTIVE.ToString(),
                //            createdAt = todayDate,
                //            modifiedAt = todayDate,
                //            status = SystemStatus.ACTIVE.ToString()
                //        };

                //        _credentialService.Create(credential);
                //    }

                //    //Obtener token de paybook para retornar
                //    tokenUser = PaybookService.CreateToken(credential.idCredentialProvider);
                //    Session["token"] = tokenUser;
                //    #endregion
                //}

                string token = Token();

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

                return new JsonResult
                {
                    Data = new { success = true, data = token },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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

        [HttpGet, AllowAnonymous]
        public JsonResult CreateCredentialBank(string idCredential)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                string token = Token();
                //Obtener el listado de las cuentas de bancos
                List<CredentialsPaybook> newBanks = PaybookService.GetCredentials(idCredential, token);

                DateTime todayDate = DateUtil.GetDateTimeNow();

                if (newBanks.Count() > 0)
                {
                    foreach (var itemBank in newBanks)
                    {
                        //Buscar el banco
                        var bank = _bankService.FirstOrDefault(x => x.providerId == itemBank.id_site);

                        //Guardar los listado de bancos nuevos
                        BankCredential newBankCred = new BankCredential()
                        {
                            uuid = Guid.NewGuid(),
                            account = new Account { id = authUser.Account.Id },
                            credentialProviderId = itemBank.id_credential,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = itemBank.is_authorized.ToString()
                        };

                        if (bank != null)
                        {
                            newBankCred.bank = new Bank { id = bank.id };
                        }

                        //Obtener las cuentas de los bancos nuevos
                        var bankAccounts = PaybookService.GetAccounts(itemBank.id_credential, token);
                        foreach (var itemAccount in bankAccounts)
                        {
                            //long d_r = long.Parse();
                            DateTime date_refresh = DateUtil.UnixTimeToDateTime(itemAccount.dt_refresh);

                            BankAccount newBankAcc = new BankAccount()
                            {
                                uuid = Guid.NewGuid(),
                                bankCredential = newBankCred,
                                accountProviderId = itemAccount.id_account,
                                accountProviderType = itemAccount.account_type,
                                name = itemAccount.name,
                                currency = itemAccount.currency,
                                balance = itemAccount.balance,
                                number = itemAccount.number,
                                isDisable = itemAccount.is_disable,
                                refreshAt = date_refresh,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = ((int)SystemStatus.ACTIVE).ToString()
                            };

                            //buscar Transacciones
                            //--Obtener las transacciones de las cuentas nuevas
                            var bankTransaction = PaybookService.GetTransactions(itemBank.id_credential, itemAccount.id_account, token);

                            foreach (var itemTransaction in bankTransaction)
                            {
                                //long d_rt = itemTransaction.dt_refresh;
                                DateTime date_refresht = DateUtil.UnixTimeToDateTime(itemTransaction.dt_refresh);

                                BankTransaction bt = new BankTransaction()
                                {
                                    uuid = Guid.NewGuid(),
                                    bankAccount = newBankAcc,
                                    transactionId = itemTransaction.id_transaction,
                                    description = itemTransaction.description,
                                    amount = itemTransaction.amount,
                                    currency = itemTransaction.currency,
                                    reference = itemTransaction.reference,
                                    transactionAt = date_refresht,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };
                                newBankAcc.bankTransaction.Add(bt);
                            }

                            newBankCred.bankAccount.Add(newBankAcc);
                        }

                        //Preguntarle por el guardado
                        _bankCredentialService.Create(newBankCred);
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

                return new JsonResult
                {
                    Data = new { success = true, data = "La conexión con el banco se realizó de manera exitosa." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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

        [HttpGet, AllowAnonymous]
        public JsonResult UnlinkBank(string uuid)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                string token = Token();

                BankCredential credential = _bankCredentialService.FirstOrDefault(x => x.uuid.ToString() == uuid);
                if (credential == null)
                    throw new Exception("No se encontro la credencial del banco en los registros.");

                var unlinkCredential = PaybookService.DeleteCredential(credential.credentialProviderId, token);
                if (!unlinkCredential)
                    throw new Exception("Error al desvincular la credencial del banco con el servicio.");

                DateTime todayDate = DateUtil.GetDateTimeNow();
                credential.modifiedAt = todayDate;
                credential.status = "0";

                _bankCredentialService.Update(credential);

                LogUtil.AddEntry(
                   "Desvinculando la credencial de la cuenta: " + JsonConvert.SerializeObject(credential),
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return new JsonResult
                {
                    Data = new { success = true, data = "" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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
        public JsonResult EditBankClabe(Int64 id, string clabe)
        {
            var authUser = Authenticator.AuthenticatedUser;
            string message = string.Empty;
            bool success = false;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();
                var updateBankAccount = _bankAccountService.FirstOrDefault(x => x.id == id);

                if (updateBankAccount == null)
                    throw new Exception("Error al intentar buscar la cuenta.");

                updateBankAccount.clabe = clabe;
                updateBankAccount.modifiedAt = todayDate;

                _bankAccountService.Update(updateBankAccount);

                LogUtil.AddEntry(
                   "Se actualizo la clave de la cuenta del banco: " + updateBankAccount.id + ", Clabe:" + updateBankAccount.clabe,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );
                success = true;
                message = "Clabe actualizada";
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

                message = ex.Message.ToString();
            }

            return new JsonResult
            {
                Data = new { success = success, mensaje = message },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }

        [AllowAnonymous]
        public ActionResult BanksAccount(string idBankCredential)
        {
            //var authUser = Authenticator.AuthenticatedUser;
            try
            {
                //var bankAccounts = _bankCredentialService.GetBanksAccounts(idBankCredential);
                ViewBag.IdBankCredential = idBankCredential;
                return View();
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBankAccounts(JQueryDataTableParams param, string filter)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<BankAccountsList>();
            var list = new List<BankAccountsVM>();
            string error = string.Empty;

            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filter);
                string filterCredential = filtersValues.Get("IdCredential").Trim();

                var bankCredential = _bankCredentialService.FirstOrDefault(x => x.uuid.ToString() == filterCredential);
                if (bankCredential == null)
                    throw new Exception("Error al traer registros de la cuenta.");

                listResponse = _bankCredentialService.GetBanksAccounts(bankCredential.id);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    list = listResponse.Select(x => new BankAccountsVM
                    {
                        id = x.id,
                        accountProviderId = x.accountProviderId,
                        accountProviderType = x.accountProviderType,
                        name = x.name,
                        balance = x.balance.ToString("C2"),
                        currency = x.currency,
                        number = x.number,
                        isDisable = x.isDisable,
                        refreshAt = x.refreshAt,
                        clabe = x.clabe,
                        bankCredentialId = x.bankCredentialId,
                        status = x.status
                    }).ToList();

                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }

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
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
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

                //return new JsonResult
                //{
                //    Data = new { success = false, message = ex.Message },
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    MaxJsonLength = Int32.MaxValue
                //};
            }

            return Json(new
            {
                success = true,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult BankTransaction()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            BankViewModel model = new BankViewModel();
            try
            {
                //listados del banco de la cuenta registrada
                var listResponse = _bankCredentialService.GetBankCredentials(userAuth.Account.Id)
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.id.ToString() }).ToList();
                listResponse.Insert(0, new SelectListItem() { Text = "Todos", Value = "-1" });

                var listTypes = Enum.GetValues(typeof(TypeMovements)).Cast<TypeMovements>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                model.ListBanks = new SelectList(listResponse);
                model.ListMovements = new SelectList(listTypes);

                //LogUtil.AddEntry(
                //   "Pantalla de movimientos bancarios, filtros: " + JsonConvert.SerializeObject(model),
                //   ENivelLog.Info,
                //   userAuth.Id,
                //   userAuth.Email,
                //   EOperacionLog.ACCESS,
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                //);
            }
            catch (Exception ex)
            {
                //LogUtil.AddEntry(
                //   "Se encontro un error: " + ex.Message.ToString(),
                //   ENivelLog.Error,
                //   userAuth.Id,
                //   userAuth.Email,
                //   EOperacionLog.ACCESS,
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                //);
            }
            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBankTransaction(JQueryDataTableParams param, string filtros, bool first)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<BankTransactionList>();
            var list = new List<BankTransactionMV>();
            string error = string.Empty;
            bool success = true;
            BankTransactionTotalVM totales = new BankTransactionTotalVM();

            try
            {
                if (first)
                {
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                    string FilterEnd = filtersValues.Get("FilterEndDate").Trim();
                    string bank = filtersValues.Get("BankName");
                    string bankAccount = filtersValues.Get("NumberBankAccount");
                    string Movements = filtersValues.Get("Movements");

                    var pagination = new BasePagination();
                    var filters = new BankTransactionFilter() { accountId = userAuth.Account.Id };
                    pagination.PageSize = param.iDisplayLength;
                    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                    if (FilterStart != "") pagination.CreatedOnStart = Convert.ToDateTime(FilterStart);
                    if (FilterEnd != "") pagination.CreatedOnEnd = Convert.ToDateTime(FilterEnd);
                    if (bank != "-1") filters.bankId = Convert.ToInt64(bank);
                    if (bankAccount != "") filters.bankAccountId = Convert.ToInt64(bankAccount);
                    if (Movements != "") filters.movements = Convert.ToInt64(Movements);

                    listResponse = _bankCredentialService.GetBankTransactionList(pagination, filters);

                    //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                    if (listResponse.Count() > 0)
                    {
                        totalDisplay = listResponse[0].Total;
                        total = listResponse.Count();

                        list = listResponse.Select(x => new BankTransactionMV
                        {
                        }).ToList();

                        //totales = 
                    }

                }

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
                totales = totales,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = error
            }, JsonRequestBehavior.AllowGet);
        }
    }
}