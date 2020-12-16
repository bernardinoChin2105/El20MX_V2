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
using System.Configuration;
using System.IO;

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
            var authUser = Authenticator.AuthenticatedUser;
            try
            {

                string token = Authenticator.BankToken;

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        PaybookService.GetVerifyToken(token);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                        Authenticator.StoreBankToken(string.Empty);
                        token = Token();
                    }
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
                    Authenticator.StoreBankToken(token);
                    ViewBag.paybookT = token;
                }

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
                   ex.Message.ToString()
               );
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
            }
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBanks(JQueryDataTableParams param)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var provider = ConfigurationManager.AppSettings["BankProvider"];
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<BankCredentialsList>();
            var list = new List<BankCredentialsMV>();
            string error = string.Empty;
            DateTime todayDate = DateUtil.GetDateTimeNow();

            try
            {
                listResponse = _bankCredentialService.GetBankCredentials(userAuth.Account.Id);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    foreach (var bank in listResponse)
                    {
                        var bankMv = new BankCredentialsMV();
                        string token = Token();
                        //Obtener el listado de las cuentas de bancos
                        List<CredentialsPaybook> resultBank = PayBookServices.GetCredentials(bank.credentialProviderId, token, provider);

                        bankMv.id = bank.id;
                        bankMv.uuid = bank.uuid;
                        bankMv.credentialProviderId = bank.credentialProviderId;
                        bankMv.createdAt = bank.createdAt;
                        bankMv.modifiedAt = bank.modifiedAt;
                        bankMv.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), bank.status)).GetDisplayName();
                        bankMv.accountId = bank.accountId;
                        bankMv.banckId = bank.banckId;
                        bankMv.Name = bank.Name;
                        bankMv.NameSite = bank.nameSite;
                        bankMv.siteId = bank.providerSiteId;
                        bankMv.isTwofa = bank.isTwofa;
                        bankMv.code = resultBank[0].code;
                        bankMv.dateTimeAuthorized = resultBank[0].dt_authorized.HasValue ? DateUtil.UnixTimeToDateTime(resultBank[0].dt_authorized.Value).ToShortDateString() :
                            (bank.dateTimeAuthorized != null ? bank.dateTimeAuthorized.Value.ToShortDateString() : string.Empty);
                        bankMv.dateTimeRefresh = resultBank[0].dt_refresh.HasValue ? DateUtil.UnixTimeToDateTime(resultBank[0].dt_refresh.Value).ToShortDateString() : 
                            (bank.dateTimeRefresh != null ? bank.dateTimeRefresh.Value.ToShortDateString() : string.Empty);
                        if (bank.dateTimeRefresh != null && bankMv.code != 401)
                        {
                            if (bank.dateTimeAuthorized.Value.Date < todayDate.Date)
                                bankMv.code = 600;//código para actualizarlo manualmente
                        }
                        else if (bankMv.code != 401)
                            bankMv.code = 600; //código para actualizarlo manualmente

                        list.Add(bankMv);
                    }

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
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        private string Token()
        {
            var authUser = Authenticator.AuthenticatedUser;
            string token = Authenticator.BankToken;

            if (string.IsNullOrEmpty(token) || !PaybookService.GetVerifyToken(token))
            {
                var credential = _credentialService.FirstOrDefault(x => x.account.id == authUser.Account.Id && x.provider == SystemProviders.SYNCFY.GetDisplayName());
                token = PaybookService.CreateToken(credential.idCredentialProvider);
                Authenticator.StoreBankToken(token);
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

        //Utilizamos para crear o actualizar las cuentas de los bancos y transacciones
        [HttpGet, AllowAnonymous]
        public JsonResult CreateCredentialBank(string idCredential)
        {
            var authUser = Authenticator.AuthenticatedUser;
            var provider = ConfigurationManager.AppSettings["BankProvider"];
            try
            {
                string token = Token();
                //Obtener el listado de las cuentas de bancos
                List<CredentialsPaybook> paybookCredentials = PayBookServices.GetCredentials(idCredential, token, provider);

                if (!paybookCredentials.Any())
                    throw new Exception("No se encontró la credencial bancaria del usuario, comuniquese al área de soporte");

                DateTime todayDate = DateUtil.GetDateTimeNow();

                var paybookCredential = paybookCredentials.FirstOrDefault();

                //Buscar el banco
                Bank bank = _bankService.FirstOrDefault(x => x.providerSiteId == paybookCredential.id_site); //buscar por sitio

                if (bank == null)
                {
                    var paybookBanks = PaybookService.GetBanksSites(paybookCredential.id_site_organization, token);

                    if (!paybookBanks.Any())
                        throw new Exception("No se encontró la organización bancaria, comuniquese al área de soporte");

                    var paybookBank = paybookBanks.FirstOrDefault();
                    if (paybookBank.sites.Any())
                        throw new Exception("La organización bancaria no cuenta con sitios, comuniquese al área de soporte");

                    var site = paybookBank.sites.FirstOrDefault(x => x.id_site == paybookCredential.id_site);

                    if (site != null)
                        throw new Exception("El sitio bancario no se encuentra en la organización, comuniquese al área de soporte");

                    bank = new Bank
                    {
                        uuid = Guid.NewGuid(),
                        name = paybookBank.name,
                        providerId = paybookBank.id_site_organization,
                        nameSite = site.name,
                        providerSiteId = site.id_site,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString(),
                    };
                    _bankService.Create(bank);

                }

                BankCredential bankCredential = _bankCredentialService.FirstOrDefault(x => x.credentialProviderId == paybookCredential.id_credential && x.account.id == authUser.Account.Id && x.status == SystemStatus.ACTIVE.ToString());
                if (bankCredential == null)
                {
                    //Guardar los listado de bancos nuevos
                    bankCredential = new BankCredential()
                    {
                        uuid = Guid.NewGuid(),
                        account = new Account { id = authUser.Account.Id },
                        credentialProviderId = paybookCredential.id_credential,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = paybookCredential.is_authorized != null ? (paybookCredential.is_authorized.Value.ToString() == "1" ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()) : SystemStatus.INACTIVE.ToString(),
                        bank = bank,
                        //nuevos campos
                        isTwofa = Convert.ToBoolean(paybookCredential.is_twofa),
                    };
                }
                else
                {
                    bankCredential.modifiedAt = todayDate;
                    bankCredential.status = paybookCredential.is_authorized != null ? (paybookCredential.is_authorized.Value.ToString() == "1" ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()) : SystemStatus.INACTIVE.ToString();
                }

                if (paybookCredential.dt_authorized != null)
                    bankCredential.dateTimeAuthorized = DateUtil.UnixTimeToDateTime(paybookCredential.dt_authorized.Value);

                if (paybookCredential.dt_refresh != null)
                    bankCredential.dateTimeRefresh = DateUtil.UnixTimeToDateTime(paybookCredential.dt_refresh.Value);
                
                //Preguntarle por el guardado
                _bankCredentialService.CreateWithTransaction(bankCredential);

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

                return new JsonResult
                {
                    Data = new { success = true, data = "Conexión exitosa, recibirá una notificación al finalizar la sincronización de sus transacciones bancarias." },
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
                   "Credencial: " + idCredential
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
                credential.status = SystemStatus.CANCELLED.ToString();

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
                   "uuid: " + uuid
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
                   "Se actualizo la clave de la cuenta del banco: " + updateBankAccount.name,
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
                    JsonConvert.SerializeObject(param) + "; filtros:" + JsonConvert.SerializeObject(filter)
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
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(),
                MaxDate = DateUtil.GetDateTimeNow().ToString("dd/MM/yyyy")
            };
            try
            {
                //listados del banco de la cuenta registrada
                var listResponse = _bankCredentialService.GetBankCredentials(userAuth.Account.Id)
                    .Select(x => new SelectListItem() { Text = x.nameSite, Value = x.id.ToString() }).ToList();
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

        #region metodo para obtener los sitios de los bancos
        [HttpGet]
        public ActionResult UpdateSitesBank()
        {
            DateTime todayDate = DateUtil.GetDateTimeNow();
            using (StreamReader r = new StreamReader("C://Users//YelmyPech//Documents//Notas//SitioBancos.json"))
            {
                string json = r.ReadToEnd();
                List<AllBankSites> banks = JsonConvert.DeserializeObject<List<AllBankSites>>(json);


                foreach (AllBankSites item in banks)
                {
                    var bankP = _bankService.FirstOrDefault(x => x.providerId == item.id_site_organization);
                    if (bankP != null)
                    {
                        bankP.providerSiteId = item.sites[0].id_site;
                        bankP.nameSite = item.sites[0].name;
                        bankP.modifiedAt = todayDate;
                        _bankService.Update(bankP);

                        if (item.sites.Count() > 1)
                        {
                            for (int i = 1; i < item.sites.Count(); i++)
                            {
                                Bank newBank = new Bank()
                                {
                                    uuid = Guid.NewGuid(),
                                    name = bankP.name,
                                    providerId = bankP.providerId,
                                    nameSite = item.sites[i].name,
                                    providerSiteId = item.sites[i].id_site,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };
                                _bankService.Create(newBank);
                            }
                        }
                    }

                }
            }
            return View();
        }
        #endregion
    }
}