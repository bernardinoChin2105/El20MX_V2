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
using MVC_Project.FlashMessages;

namespace MVC_Project.WebBackend.Controllers
{
    public class BankController : Controller
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IBankService _bankService;
        private IBankCredentialService _bankCredentialService;
        private IBankAccountService _bankAccountService;
        private IBankTransactionService _bankTransactionService;

        public BankController(IAccountService accountService, ICredentialService credentialService, IBankService bankService,
            IBankCredentialService bankCredentialService, IBankAccountService bankAccountService, IBankTransactionService bankTransactionService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _bankService = bankService;
            _bankCredentialService = bankCredentialService;
            _bankAccountService = bankAccountService;
            _bankTransactionService = bankTransactionService;
        }

        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                var authUser = Authenticator.AuthenticatedUser;

                string token = (string)Session["token"];

                if (!string.IsNullOrEmpty(token) && PaybookService.GetVarifyToken(token))
                {
                    ViewBag.paybookT = token;
                }
                else
                {
                    //Crear credencial el usuario de la cuenta rfc, si aun no ha sido creada
                    var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());

                    if (credential == null)
                    {
                        var credentialPaybook = PaybookService.CreateUser(authUser.Account.RFC, authUser.Account.Uuid.ToString());

                        credential = new Credential()
                        {
                            uuid = Guid.NewGuid(),
                            account = new Account() { id = authUser.Account.Id },
                            provider = SystemProviders.SYNCFY.GetDisplayName(), //"Paybook",
                            idCredentialProvider = credentialPaybook.id_user,
                            statusProvider = SystemStatus.ACTIVE.ToString(),
                            createdAt = DateUtil.GetDateTimeNow(),
                            modifiedAt = DateUtil.GetDateTimeNow(),
                            status = SystemStatus.ACTIVE.ToString()
                        };

                        _credentialService.Create(credential);

                        LogUtil.AddEntry(
                           "Credencial creada - RFC: " + authUser.Account.RFC,
                           ENivelLog.Info,
                           authUser.Id,
                           authUser.Email,
                           EOperacionLog.ACCESS,
                           string.Format("Usuario {0} | Fecha {1} | RFC {2}", authUser.Email, DateUtil.GetDateTimeNow(), authUser.Account.RFC),
                           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                           JsonConvert.SerializeObject(credential)
                        );
                    }

                    //Obtener token de paybook para retornar
                    token = PaybookService.CreateToken(credential.idCredentialProvider);
                    Session["token"] = token;
                    ViewBag.paybookT = token;
                }

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
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
                    listResponse.Select(c => { c.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), c.status)).GetDisplayName(); return c; }).ToList();

                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }

            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
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

            if (string.IsNullOrEmpty(token) || !PaybookService.GetVarifyToken(token))
            {
                var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());
                token = PaybookService.CreateToken(credential.idCredentialProvider);
                Session["token"] = token;
            }

            return token;
        }

        //Llamadas para paybook
        [HttpGet, AllowAnonymous]
        public JsonResult GetToken()
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                return new JsonResult
                {
                    Data = new { success = true, data = Token() },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
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
                        //var bank = _bankService.FirstOrDefault(x => x.providerId == itemBank.id_site);
                        Bank bank = _bankService.FirstOrDefault(x => x.providerId == itemBank.id_site_organization);
                        if (bank == null)
                            throw new Exception("El banco no se encuentra en el sistema, comuniquese al área de soporte");
                        
                        BankCredential bankCredential = _bankCredentialService.FirstOrDefault(x => x.credentialProviderId == itemBank.id_credential && x.account.id == authUser.Account.Id && x.status==SystemStatus.ACTIVE.ToString());
                        if (bankCredential == null)
                        {
                            //Guardar los listado de bancos nuevos
                            bankCredential = new BankCredential()
                            {
                                uuid = Guid.NewGuid(),
                                account = new Account { id = authUser.Account.Id },
                                credentialProviderId = itemBank.id_credential,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = itemBank.is_authorized != null ? (itemBank.is_authorized.Value.ToString() == "1" ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()) : SystemStatus.INACTIVE.ToString(),
                                bank = bank
                            };
                        }
                        else
                        {
                            bankCredential.modifiedAt = todayDate;
                            bankCredential.status = itemBank.is_authorized != null ? (itemBank.is_authorized.Value.ToString() == "1" ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()) : SystemStatus.INACTIVE.ToString();                                
                        }

                        //Obtener las cuentas de los bancos nuevos
                        var bankAccounts = PaybookService.GetAccounts(itemBank.id_credential, token);
                        foreach (var itemAccount in bankAccounts)
                        {
                            //long d_r = long.Parse();
                            DateTime date_refresh = DateUtil.UnixTimeToDateTime(itemAccount.dt_refresh);
                            BankAccount newBankAcc = _bankAccountService.FirstOrDefault(x => x.bankCredential.id == bankCredential.id && x.accountProviderId == itemAccount.id_account);
                            if (newBankAcc == null)
                            {
                                newBankAcc = new BankAccount()
                                {
                                    uuid = Guid.NewGuid(),
                                    bankCredential = bankCredential,
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
                            }
                            else
                            {
                                newBankAcc.balance = itemAccount.balance;
                                newBankAcc.modifiedAt = todayDate;
                            }

                            //buscar Transacciones
                            //--Obtener las transacciones de las cuentas nuevas
                            var transactions = PaybookService.GetTransactions(itemBank.id_credential, itemAccount.id_account, token);

                            foreach (var itemTransaction in transactions)
                            {
                                BankTransaction bankTransactions = _bankTransactionService.FirstOrDefault(x => x.transactionId == itemTransaction.id_transaction && x.bankAccount.id == newBankAcc.id);
                                if (bankTransactions == null)
                                {
                                    //long d_rt = itemTransaction.dt_refresh;
                                    DateTime date_refresht = DateUtil.UnixTimeToDateTime(itemTransaction.dt_refresh);
                                    DateTime date_transaction = DateUtil.UnixTimeToDateTime(itemTransaction.dt_transaction);

                                    bankTransactions = new BankTransaction()
                                    {
                                        uuid = Guid.NewGuid(),
                                        bankAccount = newBankAcc,
                                        transactionId = itemTransaction.id_transaction,
                                        description = itemTransaction.description,
                                        amount = itemTransaction.amount,
                                        currency = itemTransaction.currency,
                                        reference = itemTransaction.reference,
                                        transactionAt = date_transaction,
                                        createdAt = todayDate,
                                        modifiedAt = todayDate,
                                        status = SystemStatus.ACTIVE.ToString()
                                    };
                                    newBankAcc.bankTransaction.Add(bankTransactions);
                                }
                            }

                            bankCredential.bankAccount.Add(newBankAcc);
                        }

                        //Preguntarle por el guardado
                        _bankCredentialService.CreateWithTransaction(bankCredential);
                    }
                    LogUtil.AddEntry(
                       "Credencial bancaria para el rfc: " + authUser.Account.RFC,
                       ENivelLog.Info,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );
                }
                
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
            string message = string.Empty;
            bool success = true;

            try
            {
                string token = Token();

                BankCredential credential = _bankCredentialService.FirstOrDefault(x => x.uuid.ToString() == uuid);
                if (credential == null)
                    throw new Exception("No se encontro la credencial del banco en los registros.");

                var unlinkCredential = PaybookService.DeleteCredential(credential.credentialProviderId, "Delete", token);
                if (!unlinkCredential)
                    throw new Exception("Error al desvincular la credencial del banco con el servicio.");

                DateTime todayDate = DateUtil.GetDateTimeNow();
                credential.modifiedAt = todayDate;
                credential.status = "0";

                _bankCredentialService.Update(credential);

                LogUtil.AddEntry(
                   "Desvinculando la credencial de la cuenta: " + credential.id, // JsonConvert.SerializeObject(credential),
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                message = "Cuenta desvinculada exitosamente.";
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                success = false;
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

                //return new JsonResult
                //{
                //    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    MaxJsonLength = Int32.MaxValue
                //};
            }

            return new JsonResult
            {
                Data = new { success = success, message = message },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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
                   "Se actualizo la clave de la cuenta del banco: " + updateBankAccount.name ,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   "Clabe actualizada: " + updateBankAccount.clabe
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
                var bankCredential = _bankCredentialService.FirstOrDefault(x => x.uuid.ToString() == idBankCredential);
                //var bank = _bankService.FirstOrDefault(x => x.id == bankAccounts.b);
                ViewBag.IdBankCredential = idBankCredential;
                ViewBag.BankName = bankCredential.bank.name;
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
            char pad = '*';

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
                        //number = x.number,
                        number = !string.IsNullOrEmpty(x.number) ? x.number.PadLeft(10, pad) : string.Empty,
                        isDisable = x.isDisable,
                        refreshAt = x.refreshAt,
                        clabe = x.clabe,
                        bankCredentialId = x.bankCredentialId,
                        status = x.status
                    }).ToList();

                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }
                
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
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(), //DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy"),
                MaxDate = DateTime.Now.ToString("dd/MM/yyyy")
            };
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

                listTypes.Insert(0, new SelectListItem() { Text = "Todos", Value = "-1" });

                model.ListBanks = new SelectList(listResponse);
                model.ListMovements = new SelectList(listTypes);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
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

                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);

                string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                string FilterEnd = filtersValues.Get("FilterEndDate").Trim();

                if (first)
                {
                    FilterStart = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString();
                    FilterEnd = DateUtil.GetDateTimeNow().ToShortDateString();
                }

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
                if (bankAccount != "-1") filters.bankAccountId = Convert.ToInt64(bankAccount);
                if (Movements != "-1") filters.movements = Convert.ToInt64(Movements);

                listResponse = _bankCredentialService.GetBankTransactionList(pagination, filters);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                    char pad = '*';
                    double balanceA = 0;
                    //bool firstA = true;
                    Int64 bankAccountId = 0;
                    double amountB = 0;
                    foreach (var item in listResponse)
                    {
                        if (bankAccountId != item.bankAccountId)
                        {
                            bankAccountId = item.bankAccountId;
                            balanceA = item.balance;
                            amountB = (double)item.amount;
                        }
                        else
                        {
                            balanceA = balanceA - amountB;
                            amountB = (double)item.amount;
                        }


                        BankTransactionMV nuevo = new BankTransactionMV
                        {
                            id = item.id,
                            transactionId = item.transactionId,
                            description = item.description,
                            amountD = item.amount > 0 ? item.amount.ToString("C2") : "",
                            amountR = item.amount < 0 ? item.amount.ToString("C2") : "",
                            currency = item.currency,
                            transactionAt = item.transactionAt.ToShortDateString(),
                            balance = balanceA.ToString("C2"),
                            bankAccountName = item.bankAccountName + " " + (!string.IsNullOrEmpty(item.number) ? item.number.PadLeft(10, pad) : string.Empty),
                            number = (!string.IsNullOrEmpty(item.number) ? item.number.PadLeft(10, pad) : string.Empty),
                            bankName = item.bankName,
                            refreshAt = item.refreshAt.ToString()
                        };

                        list.Add(nuevo);
                    }

                    if (bankAccount != "-1" && bank != "-1")
                    {
                        totales = new BankTransactionTotalVM
                        {
                            currency = listResponse.FirstOrDefault().currency,
                            TotalAmount = listResponse.FirstOrDefault().balance.ToString("C2"),
                            TotalRetirement = listResponse.Where(x => x.amount < 0).Sum(x => x.amount).ToString("C2"),
                            TotalDeposits = listResponse.Where(x => x.amount > 0).Sum(x => x.amount).ToString("C2"),
                            TotalFinal = listResponse.FirstOrDefault().balance.ToString("C2")
                        };
                    }
                }

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
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBankAccountsFilter(Int64 credentialId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            bool success = true;
            string message = String.Empty;
            char pad = '*';
            try
            {
                list = _bankAccountService.FindBy(x => x.bankCredential.id == credentialId)
                    .Select(x => new SelectListItem() { Text = x.name + " " + (!String.IsNullOrEmpty(x.number) ? x.number.PadLeft(10, pad) : string.Empty), Value = x.id.ToString() }).ToList();
                list.Insert(0, new SelectListItem() { Text = "Todos", Value = "-1" });
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                success = false;
                message = error;
            }

            return new JsonResult
            {
                Data = new { success = success, message = message, data = list },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}