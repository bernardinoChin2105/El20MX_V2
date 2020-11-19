﻿using LogHubSDK.Models;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.Pipedrive;
using MVC_Project.Integrations.SAT;
using MVC_Project.Integrations.Storage;
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
        private IDiagnosticService _diagnosticService;
        private ISupervisorCADService _supervisorCADService;

        public AccountController(IMembershipService accountUserService, IAccountService accountService,
            ICredentialService credentialService, IRoleService roleService, IUserService userService, IPromotionService promotionService,
            IDiscountService discountService, ICADAccountService CADAccountService, IDiagnosticService diagnosticService, ISupervisorCADService supervisorCADService)
        {
            _membership = accountUserService;
            _accountService = accountService;
            _credentialService = credentialService;
            _roleService = roleService;
            _userService = userService;
            _promotionService = promotionService;
            _discountService = discountService;
            _CADAccountService = CADAccountService;
            _diagnosticService = diagnosticService;
            _supervisorCADService = supervisorCADService;
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
                if (authUser.Role.Code == SystemRoles.SYSTEM_ADMINISTRATOR.ToString() || authUser.Role.Code.Contains(SystemRoles.DIRECCION.ToString()))
                {
                    accounts = _accountService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString()).Select(x => new Account
                    {
                        Id = x.id,
                        Uuid = x.uuid,
                        Name = x.name,
                        RFC = x.rfc
                    }).ToList();
                }
                else if (authUser.Role.Code.Contains(SystemRoles.SUPERVISOR.ToString()))
                {
                    List<Int64> cadIds = _supervisorCADService.FindBy(x => x.supervisor.id == authUser.Id).Select(x => x.cad.id).ToList();
                    cadIds.Add(authUser.Id);

                    accounts = _CADAccountService.FindBy(x => cadIds.Contains(x.cad.id)).Select(x => new Account
                    {
                        Id = x.account.id,
                        Uuid = x.account.uuid,
                        Name = x.account.name,
                        RFC = x.account.rfc
                    }).ToList();
                }
                else
                {
                    accounts = _CADAccountService.FindBy(x => x.cad.id == authUser.Id).Select(x => new Account
                    {
                        Id = x.account.id,
                        Uuid = x.account.uuid,
                        Name = x.account.name,
                        RFC = x.account.rfc
                    }).ToList();
                }
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
                            credentialId = credential != null ? credential.idCredentialProvider : string.Empty,
                            //ciec=membership.account.ciec
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
                    if (account.status == SystemStatus.INACTIVE.ToString())
                    {
                        List<Permission> permissionsUser = new List<Permission>();
                        permissionsUser.Add(new Permission
                        {
                            Action = "Index",
                            Controller = "Account",
                            Module = "Account",
                            Level = SystemLevelPermission.FULL_ACCESS.ToString(),
                            isCustomizable = false
                        });
                        authUser.Permissions = permissionsUser;
                        Authenticator.StoreAuthenticatedUser(authUser);
                        return RedirectToAction("CreateAccount", new { uuid = account.uuid });
                    }

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
        public ActionResult CreateAccount(string uuid)
        {
            LoginSATViewModel model = new LoginSATViewModel();
            if (!string.IsNullOrEmpty(uuid))
            {
                var account = _accountService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (account != null)
                {
                    model.uuid = uuid;
                    model.RFC = account.rfc;
                    model.CIEC = account.ciec;
                }
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateCredential(LoginSATViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            DateTime todayDate = DateUtil.GetDateTimeNow();
            bool IsCRMEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["Pipedrive.Enabled"]);
            try
            {
                var provider = ConfigurationManager.AppSettings["SATProvider"];
                Domain.Entities.Account account = null;
                if (string.IsNullOrEmpty(model.uuid))
                {
                    var accountExist = _accountService.FirstOrDefault(x => x.rfc == model.RFC); //.ValidateRFC(model.RFC);

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
                        avatar = ConfigurationManager.AppSettings["Avatar.Account"],
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

                    // ESTO VA DONDE SE VAYA A INTEGRAR EL CRM
                    if (IsCRMEnabled)
                        CreatePripedrivePerson(user, account);

                }
                else
                {
                    account = _accountService.FirstOrDefault(x => x.uuid == Guid.Parse(model.uuid));
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
                            uuid = Guid.NewGuid(),
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

                    if (IsCRMEnabled)
                    {
                        var membership = _membership.FirstOrDefault(x => x.account.id == account.id && x.role.code == SystemRoles.ACCOUNT_OWNER.ToString() && x.status == SystemStatus.ACTIVE.ToString());
                        if (membership != null && membership.user != null)
                            CreatePripedrivePerson(membership.user, account);
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

        private void CreatePripedrivePerson(Domain.Entities.User user, Domain.Entities.Account account)
        {
            try
            {
                PipedriveResponse response = new PipedriveResponse();
                PipedriveClient pdClient = new PipedriveClient();
                if (user.pipedriveId > 0)
                {
                    response = pdClient.UpdatePerson(new PipedrivePerson()
                    {
                        Name = user.profile.firstName + " " + user.profile.lastName,
                        FirstName = user.profile.firstName,
                        LastName = user.profile.lastName,
                        Email = user.name,
                        RFC = account.rfc,
                        CIEC = account.ciec,
                        Phone = user.profile.phoneNumber
                    }, user.pipedriveId);
                }
                else
                {
                    response = pdClient.CreatePerson(new PipedrivePerson()
                    {
                        Name = user.profile.firstName + " " + user.profile.lastName,
                        FirstName = user.profile.firstName,
                        LastName = user.profile.lastName,
                        Email = user.name,
                        RFC = account.rfc,
                        CIEC = account.ciec,
                        Phone = user.profile.phoneNumber
                    });
                    if (response.Success)
                    {
                        user.pipedriveId = response.Data.Id;
                        _userService.Update(user);
                    }
                }

                LogUtil.AddEntry(
                   "Registro en Pipedrive del usuario" + account.rfc, ENivelLog.Info, user.id, user.name, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(response)
                );
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al registrar en Pipedrive al usuario" + user.name, ENivelLog.Info, user.id, user.name, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );
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
                    return Json(new { success = false, finish = true, message = "No fue posible validar el rfc (credential status " + credential.statusProvider + ")" }, JsonRequestBehavior.AllowGet);
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
                Domain.Entities.Membership membership = null;
                if (authUser.isBackOffice)
                    membership = _membership.FirstOrDefault(x => x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString());
                else
                    membership = _membership.FirstOrDefault(x => x.account.id == account.id && x.user.id == authUser.Id && x.status == SystemStatus.ACTIVE.ToString());

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
                MensajeFlashHandler.RegistrarMensaje("Se registró correctamente el rfc "+ account.rfc + ". Tu diagnóstico fiscal esta siendo procesado.", TiposMensaje.Success);
                LogUtil.AddEntry(
                   "Se registró correctamente el rfc " + account.rfc,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(account)
                );

                #region Generar diagnóstico Inicial
                if (bool.Parse(ConfigurationManager.AppSettings["InitialDiagnostic.Enable"]))
                    GenerateDx0();
                
                
                #endregion

                return RedirectToAction("Index", "SAT");
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

        #region DiagnosticoInicial

        public void GenerateDx0()
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                Domain.Entities.Account account = _accountService.FindBy(z => z.id == authUser.Account.Id).FirstOrDefault();

                var provider = ConfigurationManager.AppSettings["SATProvider"];
                DateTime dateFrom = DateTime.UtcNow.AddMonths(-3);
                DateTime dateTo = DateTime.UtcNow.AddMonths(-1);
                dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month)).AddDays(1).AddMilliseconds(-1);
                dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                string extractionId = SATService.GenerateExtractions(authUser.Account.RFC, dateFrom, dateTo, provider);

                var diagn = new Domain.Entities.Diagnostic()
                {
                    uuid = Guid.NewGuid(),
                    account = account,
                    businessName = account.name,
                    commercialCAD = "",
                    plans = "",
                    createdAt = DateUtil.GetDateTimeNow(),
                    status = SystemStatus.PENDING.ToString(),
                    processId = extractionId
                };

                _diagnosticService.Create(diagn);
                Session["InitialDiagnostic"] = diagn.uuid;
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error en diagnostico inicial: " + authUser.Account.RFC, ENivelLog.Error, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message + "" + (ex.InnerException != null ? ex.InnerException.Message : "")
                );
            }
        }

        [HttpGet]
        public ActionResult FinishExtraction(string uuid)
        {
            try
            {
                if (string.IsNullOrEmpty(uuid))
                {
                    if (Session["InitialDiagnostic"] != null)
                    {
                        uuid = Session["InitialDiagnostic"].ToString();
                    }
                }

                if (!string.IsNullOrEmpty(uuid))
                {
                    var diagnostic = _diagnosticService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                    if (diagnostic == null)
                        throw new Exception("No fue posible obtener el diagnostico");

                    if (diagnostic.status == SystemStatus.PENDING.ToString() || diagnostic.status == SystemStatus.PROCESSING.ToString())
                    {
                        return Json(new { success = true, finish = false, status = true }, JsonRequestBehavior.AllowGet);
                    }
                    else if (diagnostic.status == SystemStatus.ACTIVE.ToString())
                    {
                        Session["InitialDiagnostic"] = null;
                        return Json(new { success = true, finish = true, status = true }, JsonRequestBehavior.AllowGet);
                    }
                    else if (diagnostic.status == SystemStatus.FAILED.ToString())
                        return Json(new { success = false, finish = true, status = true, message = "Se generó un fallo durante la extracción" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = false, finish = true, status = true, message = "No fue posible generar el diagnostico, comuniquese al área de soporte" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, finish = false, status = false, message = "Sin diagnostico pendiente" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, finish = true }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateImage(string fileNameAccount, HttpPostedFileBase imageAccount)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                var account = _accountService.GetById(userAuth.Account.Id);
                if (account == null)
                    throw new Exception("La cuenta no es válida");

                var StorageImages = ConfigurationManager.AppSettings["StorageImages"];

                if (imageAccount == null)
                    throw new Exception("No se proporcionó una imagen");

                var image = AzureBlobService.UploadPublicFile(imageAccount.InputStream, fileNameAccount, StorageImages, account.rfc);
                account.avatar = image.Item1;
                account.modifiedAt = DateTime.Now;
                _accountService.Update(account);

                userAuth.Account.Image = image.Item1;
                Authenticator.RefreshAuthenticatedUser(userAuth);

                return Json(new { account.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}