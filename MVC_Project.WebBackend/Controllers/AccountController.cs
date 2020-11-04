using LogHubSDK.Models;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.SAT;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class AccountController : Controller
    {
        private IMembershipService _membership;
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IRoleService _roleService;
        private IUserService _userService;
        private IPromotionService _promotionService;
        private IDiscountService _discountService;
        private ICADAccountService _CADAccountService;

        public AccountController(IMembershipService accountUserService, IAccountService accountService,
            ICredentialService credentialService, IRoleService roleService, IUserService userService, IPromotionService promotionService,
            IDiscountService discountService, ICADAccountService CADAccountService)
        {
            _membership = accountUserService;
            _accountService = accountService;
            _credentialService = credentialService;
            _roleService = roleService;
            _userService = userService;
            _promotionService = promotionService;
            _discountService = discountService;
            _CADAccountService = CADAccountService;
        }

        // GET: Account
        public ActionResult Index()
        {
            var authUser = Authenticator.AuthenticatedUser;
            if (authUser == null)
            {
                ViewBag.Error = "Sesion del usuario inválida";
                return RedirectToAction("Index", "Auth");
            }
            
            return View();
        }

        [AllowAnonymous]
        public ActionResult SelectAccount()
        {
            var authUser = Authenticator.AuthenticatedUser;
            var provider = ConfigurationManager.AppSettings["SATProvider"];
            if (authUser.isBackOffice)
            {
                var accounts = new List<Account>();
                if (authUser.Role.Code == SystemRoles.SYSTEM_ADMINISTRATOR.ToString())
                    accounts = _accountService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString()).Select(x => new Account
                    {
                        Id = x.id,
                        Uuid = x.uuid,
                        Name = x.name,
                        RFC = x.rfc
                    }).ToList();
                else
                    accounts = _CADAccountService.FindBy(x => x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString()).Select(x => new Account
                    {
                        Id = x.account.id,
                        Uuid = x.account.uuid,
                        Name = x.account.name,
                        RFC = x.account.rfc
                    }).ToList();
                
                
                var accountViewModel = new AccountSelectViewModel { accountListItems = new List<SelectListItem>() };
                accountViewModel.accountListItems = accounts.Select(x => new SelectListItem
                {
                    Text = x.Name + " ( " + x.RFC+ " )",
                    Value = x.Uuid.ToString()
                }).ToList();
                return PartialView("_SelectAccountBackOfficeModal", accountViewModel);
            }
            else
            {
                var accountViewModel = new AccountSelectViewModel { accountListViewModels = new List<AccountListViewModel>() };
                var memberships = _membership.FindBy(x => x.user.id == authUser.Id && x.account != null && x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());
                if (memberships.Any())
                {
                    foreach (var membership in memberships)
                    {
                        var credential = _credentialService.FirstOrDefault(x => x.account.id == membership.account.id && x.provider == provider && x.credentialType == SATCredentialType.CIEC.ToString());
                        accountViewModel.accountListViewModels.Add(new AccountListViewModel
                        {
                            uuid = membership.account.uuid,
                            name = membership.account.name,
                            rfc = membership.account.rfc,
                            role = membership.role.name,
                            accountId = membership.account.id,
                            imagen = membership.account.avatar,
                            credentialStatus = credential != null ? credential.status : SystemStatus.INACTIVE.ToString(),
                            accountStatus = credential != null ? membership.account.status : SystemStatus.PENDING.ToString(),
                            credentialId = credential != null ? credential.idCredentialProvider : string.Empty
                        });
                    }
                }
                //ValidarSat(ref accountViewModel);
                accountViewModel.count = accountViewModel.accountListViewModels.Count;

                return PartialView("_SelectAccountModal", accountViewModel);
            }
        }

        [AllowAnonymous]
        public ActionResult SetAccount(Guid? uuid)
        {
            var authUser = Authenticator.AuthenticatedUser;
            if (authUser.isBackOffice)
            {
                if (!uuid.HasValue)
                {
                    var membership = _membership.FirstOrDefault(x => x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());

                    authUser.Permissions = membership.role.rolePermissions
                    .Where(x => x.permission.status == SystemStatus.ACTIVE.ToString() && x.permission.applyTo != SystemPermissionApply.ONLY_ACCOUNT.ToString())
                    .Select(p => new Permission
                    {
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level,
                        isCustomizable = p.permission.isCustomizable
                    }).ToList();

                    authUser.Role = new Role { Id = membership.role.id, Code = membership.role.code, Name = membership.role.name };
                    authUser.Account = null;
                    Authenticator.RefreshAuthenticatedUser(authUser);
                }
                else
                {
                    var account = _accountService.FindBy(x => x.uuid == uuid).FirstOrDefault();
                    authUser.Account = new Account { Id = account.id, Uuid = account.uuid, Name = account.name, RFC = account.rfc, Image = account.avatar };

                    var membership = _membership.FirstOrDefault(x => x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());

                    authUser.Permissions = membership.role.rolePermissions
                    .Where(x => x.permission.status == SystemStatus.ACTIVE.ToString() && x.permission.applyTo != SystemPermissionApply.ONLY_BACK_OFFICE.ToString())
                    .Select(p => new Permission
                    {
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level,
                        isCustomizable = p.permission.isCustomizable
                    }).ToList();

                    authUser.Role = new Role { Id = membership.role.id, Code = membership.role.code, Name = membership.role.name };

                    Authenticator.RefreshAuthenticatedUser(authUser);
                }
                var inicio = authUser.Permissions.FirstOrDefault(x => x.isCustomizable && x.Level != SystemLevelPermission.NO_ACCESS.ToString());
                return RedirectToAction("Index", inicio.Controller);
            }
            else
            {
                var account = _accountService.FindBy(x => x.uuid == uuid).FirstOrDefault();

                if (account != null)
                {
                    var membership = _membership.FindBy(x => x.account.id == account.id && x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString()).FirstOrDefault();

                    if (membership != null)
                    {
                        var permissions = membership.role.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                        {
                            Controller = p.permission.controller,
                            Module = p.permission.module,
                            Level = p.level,
                            isCustomizable = p.permission.isCustomizable
                        }).ToList();

                        authUser.Role = new Role { Id = membership.role.id, Code = membership.role.code, Name = membership.role.name };
                        authUser.Account = new Account { Id = account.id, Uuid = account.uuid, Name = account.name, RFC = account.rfc, Image = account.avatar };
                        authUser.Permissions = permissions;

                        Authenticator.RefreshAuthenticatedUser(authUser);

                        LogUtil.AddEntry("Acceso a la cuenta: " + account.rfc, ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                           string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                           string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                        );
                        var inicio = permissions.FirstOrDefault(x => x.isCustomizable && x.Level != SystemLevelPermission.NO_ACCESS.ToString());
                        return RedirectToAction("Index", inicio.Controller);
                    }
                }
            }

            MensajeFlashHandler.RegistrarMensaje("No es posible acceder a la cuenta", TiposMensaje.Warning);
            return RedirectToAction("Index", "Account");
        }

        private void SetMembership(Domain.Entities.Membership membership)
        {


        }

        [AllowAnonymous]
        public ActionResult CreateAccountModal()
        {
            return PartialView("_CreateAccountModal");
        }

        [AllowAnonymous]
        public ActionResult CreateAccount(string uuid, string rfc)
        {
            LoginSATViewModel model = new LoginSATViewModel { uuid = uuid, RFC = rfc };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateCredential(LoginSATViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            DateTime todayDate = DateUtil.GetDateTimeNow();
            try
            {
                var provider = ConfigurationManager.AppSettings["SATProvider"];
                Domain.Entities.Account account = null;
                if (string.IsNullOrEmpty(model.uuid))
                {
                    var accountExist = _accountService.ValidateRFC(model.RFC);

                    if (accountExist != null)
                        throw new Exception("Existe una cuenta registrada con este RFC");

                    var satModel = SATService.CreateCredential(new CredentialRequest { rfc = model.RFC, ciec = model.CIEC }, provider);
                    
                    account = new Domain.Entities.Account()
                    {
                        uuid = Guid.NewGuid(),
                        name = authUser.FirstName + " " + authUser.LastName,
                        rfc = model.RFC,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        avatar = "/Images/p1.jpg",
                        status = SystemStatus.PENDING.ToString(),
                        ciec = model.CIEC
                    };
                    var role = _roleService.FirstOrDefault(x => x.code == SystemRoles.ACCOUNT_OWNER.ToString());
                    var user = _userService.FirstOrDefault(x => x.uuid == authUser.Uuid);

                    var membership = new Domain.Entities.Membership
                    {
                        account = account,
                        user = user,
                        role = role,
                        status = SystemStatus.ACTIVE.ToString()
                    };

                    account.memberships.Add(membership);

                    var credential = new Domain.Entities.Credential()
                    {
                        account = account,
                        uuid = Guid.NewGuid(),
                        provider = provider,
                        idCredentialProvider = satModel.id,
                        statusProvider = satModel.status,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.PROCESSING.ToString(),
                        credentialType = SATCredentialType.CIEC.ToString()
                    };
                    
                    _credentialService.Create(credential, account);
                }
                else
                {
                    account = _accountService.FirstOrDefault(x=>x.uuid == Guid.Parse(model.uuid));
                    if (account == null)
                        throw new Exception("El id de la cuenta es invalida");

                    if (_accountService.FindBy(x => x.rfc == model.RFC && x.id != account.id).Any())
                        throw new Exception("Existe una cuenta registrada con este RFC");

                    account.rfc = model.RFC;
                    account.ciec = model.CIEC;
                    account.modifiedAt = todayDate;
                    _accountService.Update(account);
                    
                    var satModel = SATService.CreateCredential(new CredentialRequest { rfc = model.RFC, ciec = model.CIEC }, provider);

                    var credential = _credentialService.FirstOrDefault(x => x.account.id == account.id && x.provider == provider && x.credentialType == SATCredentialType.CIEC.ToString());
                    if (credential == null)
                    {
                        credential = new Domain.Entities.Credential()
                        {
                            account = account,
                            uuid=Guid.NewGuid(),
                            provider = provider,
                            idCredentialProvider = satModel.id,
                            statusProvider = satModel.status,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.PROCESSING.ToString(),
                            credentialType = SATCredentialType.CIEC.ToString()
                        };
                        _credentialService.Create(credential);
                    }
                    else
                    {

                        credential.idCredentialProvider = satModel.id;
                        credential.statusProvider = satModel.status;
                        credential.status = SystemStatus.PROCESSING.ToString();
                        credential.modifiedAt = todayDate;
                        _credentialService.Update(credential);
                    }
                }
                
                LogUtil.AddEntry(
                   "RFC " + account.rfc + " por validar", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(account)
                );

                return Json(new { account.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(), ENivelLog.Error, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public ActionResult AccountValidation(string uuid)
        {
            try
            {
                var provider = ConfigurationManager.AppSettings["SATProvider"];
                var account = _accountService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (account == null)
                    throw new Exception("No fue posible obtener la cuenta");

                var credential = _credentialService.FirstOrDefault(x => x.account.id == account.id && x.provider == provider && x.credentialType == SATCredentialType.CIEC.ToString());

                if (credential.status == SystemStatus.PROCESSING.ToString())
                    return Json(new { success = true, finish = false }, JsonRequestBehavior.AllowGet);
                else if (credential.status == SystemStatus.ACTIVE.ToString())
                    return Json(new { success = true, finish = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, finish = true, message = "No fue posible validar el RFC, credential " + credential.statusProvider }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, finish = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateFinish(string uuid)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();
                
                var account = _accountService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (account == null)
                    throw new Exception("No fue posible obtener la cuenta");

                account.status = SystemStatus.ACTIVE.ToString();
                _accountService.Update(account);
                
                var promotion = _promotionService.GetValidityPromotion(TypePromotions.INITIAL_DISCOUNT.ToString());

                if (promotion != null)
                {
                    var discount = new Domain.Entities.Discount
                    {
                        type = promotion.type,
                        discount = promotion.discount,
                        discountRate = promotion.discountRate,
                        hasPeriod = promotion.hasPeriod,
                        periodInitial = promotion.periodInitial,
                        periodFinal = promotion.periodFinal,
                        hasValidity = promotion.hasValidity,
                        validityInitialAt = promotion.validityInitialAt,
                        validityFinalAt = promotion.validityInitialAt,
                        createdAt = DateTime.Now,
                        modifiedAt = DateTime.Now,
                        status = SystemStatus.ACTIVE.ToString(),
                        account = account,
                        promotion = promotion
                    };
                    _discountService.Create(discount);
                }

                var membership = _membership.FirstOrDefault(x => x.account.id == account.id && x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString());
                var permissions = membership.role.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                {
                    Controller = p.permission.controller,
                    Module = p.permission.module,
                    Level = p.level,
                    isCustomizable = p.permission.isCustomizable
                }).ToList();

                authUser.Role = new Role { Id = membership.role.id, Code = membership.role.code, Name = membership.role.name };
                authUser.Account = new Account { Id = account.id, Uuid = account.uuid, Name = account.name, RFC = account.rfc, Image = account.avatar };
                authUser.Permissions = permissions;

                Authenticator.RefreshAuthenticatedUser(authUser);
                MensajeFlashHandler.RegistrarMensaje("RFC " + account.rfc + " registrado correctamente", TiposMensaje.Success);
                LogUtil.AddEntry(
                   "RFC " + account.rfc + " registrado correctamente",
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(account)
                );

                var inicio = authUser.Permissions.FirstOrDefault(x => x.isCustomizable && x.Level != SystemLevelPermission.NO_ACCESS.ToString());
                return RedirectToAction("Index", inicio.Controller);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(), ENivelLog.Error, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );
                return View("CreateAccount");
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult SelectLastAccount()
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                bool result = false;
                string message = string.Empty;
                string typeNoti = string.Empty;
                Guid uuid = new Guid();
                var accountViewModel = new AccountSelectViewModel { accountListViewModels = new List<AccountListViewModel>() };
                var memberships = _membership.FindBy(x => x.user.id == authUser.Id && x.account != null).OrderByDescending(x => x.account.id).FirstOrDefault();
                if (memberships != null)
                {
                    accountViewModel.accountListViewModels.Add(new AccountListViewModel
                    {
                        uuid = memberships.account.uuid,
                        name = memberships.account.name,
                        rfc = memberships.account.rfc,
                        role = memberships.role.name,
                        accountId = memberships.account.id,
                        imagen = memberships.account.avatar
                    });

                    //accountViewModel = ValidarSat(accountViewModel);
                    if (accountViewModel.accountListViewModels[0].credentialStatus == "valid")
                    {
                        result = true;
                    }
                    message = "Cuenta con estatus: " + accountViewModel.accountListViewModels[0].credentialStatus;
                    uuid = accountViewModel.accountListViewModels[0].uuid;
                }

                LogUtil.AddEntry(
                   message,
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
                    Data = new { Success = result, Mensaje = message, uuid, Type = typeNoti },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
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

        private void ValidarSat(ref AccountSelectViewModel accountViewModel)
        {
            var authUser = Authenticator.AuthenticatedUser;
            foreach (var item in accountViewModel.accountListViewModels)
            {
                if (!string.IsNullOrEmpty(item.credentialId))
                {
                    try
                    {
                        var model = SATwsService.GetCredentialSat(item.credentialId);

                        switch (model.status)
                        {
                            case "pending":
                                break;
                            case "valid":
                                item.credentialStatus = SystemStatus.ACTIVE.ToString();
                                break;
                            case "invalid":
                                item.credentialStatus = SystemStatus.INACTIVE.ToString();
                                break;
                            case "deactivated":
                                item.credentialStatus = SystemStatus.INACTIVE.ToString();
                                break;
                            case "error":
                                item.credentialStatus = SystemStatus.INACTIVE.ToString();
                                break;
                            default:
                                item.credentialStatus = SystemStatus.INACTIVE.ToString();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                    
                }
        }
    }
}