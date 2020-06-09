using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Resources;
using MVC_Project.Utils;
using MVC_Project.WebBackend.App_Code;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.BackendWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using LogHubSDK.Models;
using static MVC_Project.Utils.Constants;

namespace MVC_Project.WebBackend.Controllers
{
    public class AuthController : BaseController
    {
        private IAuthService _authService;
        private IUserService _userService;
        private IPermissionService _permissionService;
        private IMembershipService _accountUserService;
        private IRoleService _roleService;
        private ISocialNetworkLoginService _socialNetworkLoginService;

        public AuthController(IAuthService authService, IUserService userService, IPermissionService permissionService,
            IMembershipService accountUserService, IRoleService roleService, ISocialNetworkLoginService socialNetworkLoginService)
        {
            _authService = authService;
            _userService = userService;
            _permissionService = permissionService;
            _accountUserService = accountUserService;
            _roleService = roleService;
            _socialNetworkLoginService = socialNetworkLoginService;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AuthViewModel model)
        {
            if (ModelState.IsValid)
            {
                Domain.Entities.User user = _authService.Authenticate(model.Email, SecurityUtil.EncryptPassword(model.Password));
                if (user != null)
                {
                    if (user.status != SystemStatus.ACTIVE.ToString())
                    {
                        ViewBag.Error = Resources.ErrorMessages.UserInactive;
                        return View(model);
                    }
                    //Asignar usuario en sesión
                    var authUser = GetValidateUserLogin(user);

                    if (user.passwordExpiration.HasValue)
                    {
                        DateTime passwordExpiration = user.passwordExpiration.Value;
                        DateTime todayDate = DateUtil.GetDateTimeNow();
                        if (user.passwordExpiration.Value.Date < todayDate.Date)
                        {
                            return RedirectToAction("ChangePassword", "Auth");
                        }
                        string daysBeforeExpireToNotifyConfig = ConfigurationManager.AppSettings["DaysBeforeExpireToNotify"];
                        int daysBeforeExpireToNotify = 0;
                        if (Int32.TryParse(daysBeforeExpireToNotifyConfig, out daysBeforeExpireToNotify))
                        {
                            int daysLeft = ((TimeSpan)(passwordExpiration.Date - todayDate.Date)).Days + 1;
                            if (daysLeft <= daysBeforeExpireToNotify)
                            {

                                string message = String.Format(ViewLabels.PASSWORD_EXPIRATION_MESSAGE, daysLeft);
                                ViewBag.Message = message;
                                MensajeFlashHandler.RegistrarMensaje(message, TiposMensaje.Info);
                            }
                        }
                    }

                    if (user.memberships.Count <= 0)//Rol invitado
                    {
                        var guestRole = _roleService.FindBy(x => x.code == SystemRoles.GUEST.ToString()).FirstOrDefault();
                        List<Permission> permissionsGest = guestRole.permissions.Select(p => new Permission
                        {
                            Action = p.action,
                            Controller = p.controller,
                            Module = p.module
                        }).ToList();

                        authUser.Role = new Role { Code = guestRole.code, Name = guestRole.name };
                        authUser.Permissions = permissionsGest;
                        Authenticator.StoreAuthenticatedUser(authUser);
                        return RedirectToAction("Index", "Account");
                    }
                    else if (user.memberships.Count == 1)
                    {
                        var uniqueMembership = user.memberships.First();
                        //var uniqueRole = _roleService.FindBy(x => x.code == uniqueMembership.role.code).FirstOrDefault();
                        List<Permission> permissionsUniqueMembership = uniqueMembership.mebershipPermissions.Select(p => new Permission
                        {
                            Action = p.permission.action,
                            Controller = p.permission.controller,
                            Module = p.permission.module
                        }).ToList();

                        authUser.Role = new Role { Code = uniqueMembership.role.code, Name = uniqueMembership.role.name };
                        authUser.Permissions = permissionsUniqueMembership;
                        authUser.Account = new Account { Name = uniqueMembership.account.name, RFC = uniqueMembership.account.rfc, Uuid = uniqueMembership.account.uuid };
                        Authenticator.StoreAuthenticatedUser(authUser);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        List<Permission> permissionsUser = new List<Permission>();
                        permissionsUser.Add(new Permission
                        {
                            Action = "Index",
                            Controller = "Account",
                            Module = "Account"
                        });
                        authUser.Permissions = permissionsUser;
                        Authenticator.StoreAuthenticatedUser(authUser);
                        return RedirectToAction("Index", "Account");
                        //Manda a seleccionar cuenta
                    }
                    //if (!string.IsNullOrEmpty(Request.Form["ReturnUrl"]))
                    //{
                    //    return Redirect(Request.Form["ReturnUrl"]);
                    //}

                }
                else
                {
                    ViewBag.Error = "El usuario no existe o contraseña inválida.";
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Authenticator.RemoveAuthenticatedUser();
            return RedirectToAction("Login", "Auth");
        }
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            ViewBag.mensajeError = string.Empty;
            return PartialView("_RequestPasswordModal");
        }
        [HttpPost, AllowAnonymous]
        public ActionResult ResetPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    issue = model,
                    errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                    .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
                });
            }
            try
            {
                var resultado = _userService.FindBy(e => e.name == model.Email).First();
                if (resultado != null)
                {
                    ViewBag.mensajeError = string.Empty;
                    resultado.tokenExpiration = System.DateTime.Now.AddDays(1);
                    string token = (resultado.uuid + "@" + DateTime.Now.AddDays(1).ToString());
                    token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                    resultado.token = token;
                    Dictionary<string, string> customParams = new Dictionary<string, string>();
                    string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
                    string link = urlAccion + "Auth/AccedeToken?token=" + token;
                    customParams.Add("param1", resultado.name);
                    customParams.Add("param2", link);
                    NotificationUtil.SendNotification(resultado.name, customParams, Constants.NOT_TEMPLATE_PASSWORDRECOVER);
                    _userService.Update(resultado);
                    //MensajesFlash.MensajeFlashHandler.RegistrarMensaje(ImpuestoPredial.Resource.Recursos.OperacionExitosa);
                    ViewBag.Message = "Solicitud realizada";
                    return View("Login");

                }
            }
            catch (Exception ex)
            {
                //ErrorController.SaveLogError(this, listAction.Update, "RecuperarContrasena", ex);
            }

            ModelState.AddModelError("Email", "No se encontró ninguna cuenta con el correo proporcionado. Verifique su información.");
            return Json(new
            {
                success = false,
                issue = model,
                errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
            });

        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccedeToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login");

                var desencriptaToken = EncryptorText.DataDecrypt(token.Replace("!!", "/").Replace("$", "+"));

                if (string.IsNullOrEmpty(desencriptaToken))
                    return RedirectToAction("Login");

                var elements = desencriptaToken.Split('@');
                Guid id = Guid.Parse(elements.First().ToString());
                var resultado = _userService.FindBy(e => e.uuid == id).First();
                int[] valores = new int[100];
                for (int a = 0; a < 100; a++)
                {
                    valores[a] = a++;
                }
                if (resultado != null && DateTime.Now <= resultado.tokenExpiration)
                {
                    ResetPassword model = new ResetPassword();
                    model.Uuid = resultado.uuid.ToString();
                    return View("ResetPassword", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Token de contraseña expirado";
                return View("Login");
                //ErrorController.SaveLogError(this, listAction.Update, "AccedeToken", ex);
            }
            ViewBag.Message = "Error en el token";
            return View("Login");
        }

        [HttpGet, AllowAnonymous]
        public ActionResult ChangePassword()
        {
            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;
            if (authenticatedUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View("ChangePassword");
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {

            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;

            if (authenticatedUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var user = _userService.FindBy(e => e.uuid == authenticatedUser.Uuid).First();

            if (!String.IsNullOrWhiteSpace(model.Password) && user != null)
            {
                string encriptedPass = SecurityUtil.EncryptPassword(model.Password);
                if (user.password == encriptedPass)
                {
                    ModelState.AddModelError("Password", "La contraseña ya ha sido utilizada");
                }
            }
            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    user.password = SecurityUtil.EncryptPassword(model.Password);
                    DateTime todayDate = DateUtil.GetDateTimeNow();
                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                    user.passwordExpiration = passwordExpiration;
                    _userService.Update(user);

                    var membership = user.memberships.First();
                    //var accountUser = _accountUserService.FindBy(x => x.user.id == user.id && x.account.id == account.id).First();

                    AuthUser authUser = new AuthUser
                    {
                        FirstName = user.profile.firstName,
                        LastName = user.profile.lastName,
                        Email = user.name,
                        Uuid = user.uuid,
                        PasswordExpiration = user.passwordExpiration,
                        Role = new Role
                        {
                            Code = membership.role.code,
                            Name = membership.role.name
                        },
                        Permissions = membership.mebershipPermissions.Select(p => new Permission
                        {
                            Action = p.permission.action,
                            Controller = p.permission.controller,
                            Module = p.permission.module
                        }).ToList()
                    };
                    Authenticator.StoreAuthenticatedUser(authUser);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View("ChangePassword", model);
        }
        [HttpPost, AllowAnonymous]
        public ActionResult Reset(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    issue = model,
                    errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                    .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
                });
            }
            try
            {
                var resultado = _userService.FindBy(e => e.uuid == Guid.Parse(model.Uuid)).First();
                if (resultado != null)
                {
                    resultado.password = SecurityUtil.EncryptPassword(model.Password);
                    DateTime todayDate = DateUtil.GetDateTimeNow();
                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                    resultado.passwordExpiration = passwordExpiration;
                    _userService.Update(resultado);

                    var membership = resultado.memberships.First();

                    AuthUser authUser = new AuthUser
                    {
                        FirstName = resultado.profile.firstName,
                        LastName = resultado.profile.lastName,
                        Uuid = resultado.uuid,
                        PasswordExpiration = resultado.passwordExpiration,
                        Email = resultado.name,
                        Role = new Role
                        {
                            Code = membership.role.code,
                            Name = membership.role.name
                        },
                        Permissions = membership.mebershipPermissions.Select(p => new Permission
                        {
                            Action = p.permission.action,
                            Controller = p.permission.controller,
                            Module = p.permission.module
                        }).ToList()
                    };
                    //UnitOfWork unitOfWork = new UnitOfWork();
                    //ISession session = unitOfWork.Session;
                    //Authenticator.StoreAuthenticatedUser(authUser);
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception ex)
            {
                //ErrorController.SaveLogError(this, listAction.Update, "RecuperarContrasena", ex);
            }

            ModelState.AddModelError("Password", "No se encontró ninguna cuenta con el correo proporcionado. Verifique su información.");
            return Json(new
            {
                success = false,
                issue = model,
                errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
            });

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChangeLanguage(string lang)
        {
            LanguageMngr.SetLanguage(lang);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ValidateLogin(AuthViewModel model)
        {
            string Error = string.Empty;
            Domain.Entities.User user = null;//new Domain.Entities.User();
            bool exist = false;
            string url = string.Empty;
            try
            {
                //es una red social
                if (model.RedSocial)
                {
                    //Existe usuario
                    //Domain.Entities.User user = _authService.Authenticate(model.Email, SecurityUtil.EncryptPassword(model.Password));
                    user = _authService.AuthenticateSocialNetwork(model.Email, SecurityUtil.EncryptPassword(model.Password),
                        model.TypeRedSocial, model.SocialId);
                    //_socialNe.GetUserByEmail(model.Email);
                    if (user != null)
                    {
                        if (user.status != SystemStatus.ACTIVE.ToString())
                        {
                            Error = Resources.ErrorMessages.UserInactive;
                        }
                        else
                        {
                            //Asignar usuario en sesión
                            var authUser = GetValidateUserLogin(user);

                            var session = Authenticator.AuthenticatedUser;
                            if (session != null)
                            {
                                exist = true;

                                if (user.memberships.Count <= 0)//Rol invitado
                                {
                                    var guestRole = _roleService.FindBy(x => x.code == SystemRoles.GUEST.ToString()).FirstOrDefault();
                                    List<Permission> permissionsGest = guestRole.permissions.Select(p => new Permission
                                    {
                                        Action = p.action,
                                        Controller = p.controller,
                                        Module = p.module
                                    }).ToList();

                                    authUser.Role = new Role { Code = guestRole.code, Name = guestRole.name };
                                    authUser.Permissions = permissionsGest;
                                    Authenticator.StoreAuthenticatedUser(authUser);
                                    url = "Account/Index";
                                }
                                else if (user.memberships.Count == 1)
                                {
                                    var uniqueMembership = user.memberships.First();
                                    //var uniqueRole = _roleService.FindBy(x => x.code == uniqueMembership.role.code).FirstOrDefault();
                                    List<Permission> permissionsUniqueMembership = uniqueMembership.mebershipPermissions.Select(p => new Permission
                                    {
                                        Action = p.permission.action,
                                        Controller = p.permission.controller,
                                        Module = p.permission.module
                                    }).ToList();

                                    authUser.Role = new Role { Code = uniqueMembership.role.code, Name = uniqueMembership.role.name };
                                    authUser.Permissions = permissionsUniqueMembership;
                                    authUser.Account = new Account { Name = uniqueMembership.account.name, RFC = uniqueMembership.account.rfc, Uuid = uniqueMembership.account.uuid };
                                    Authenticator.StoreAuthenticatedUser(authUser);                                    
                                    url = "Home/Index";
                                }
                                else
                                {
                                    List<Permission> permissionsUser = new List<Permission>();
                                    permissionsUser.Add(new Permission
                                    {
                                        Action = "Index",
                                        Controller = "Account",
                                        Module = "Account"
                                    });
                                    authUser.Permissions = permissionsUser;
                                    Authenticator.StoreAuthenticatedUser(authUser);
                                    url = "Account/Index";
                                    //Manda a seleccionar cuenta
                                }

                            }

                        }

                        //return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        Error = "El usuario no existe o contraseña inválida.";
                    }

                }
                else
                {
                    //no es una redsocial
                    Error = "El usuario no existe o contraseña inválida.";
                }

                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "response", error = Error }, data = user, Success = exist, Url = url },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };

            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = Error }, Success = exist },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            /*salida*/

        }

        //Validar el login e iniciar sesión
        public AuthUser GetValidateUserLogin(Domain.Entities.User user)
        {
            AuthUser authUser = null;
            try
            {
                user.lastLoginAt = DateTime.Now;
                _userService.Update(user);

                List<Permission> permissionsUser = _permissionService.GetAll().Select(p => new Permission
                {
                    Action = p.action,
                    Controller = p.controller,
                    Module = p.module
                }).ToList();

                authUser = new AuthUser
                {
                    Id = user.id,
                    FirstName = user.profile.firstName,
                    LastName = user.profile.lastName,
                    Email = user.name,
                    Language = user.profile.language,
                    Uuid = user.uuid,
                    PasswordExpiration = user.passwordExpiration,
                    Avatar = user.profile.avatar,
                    //Permissions = permissionsUser
                };

                //Set user in sesion
                Authenticator.StoreAuthenticatedUser(authUser);

                LogUtil.AddEntry(
                   "Ingresa al Sistema",
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                //Set Language
                LanguageMngr.SetDefaultLanguage();
                if (!string.IsNullOrEmpty(authUser.Language))
                {
                    LanguageMngr.SetLanguage(authUser.Language);
                }

            }
            catch (Exception Ex)
            {
                string Error = Ex.Message.ToString();
            }

            return authUser;
        }

        [HttpPost, AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult LoginAuth(AuthViewModel model)
        {
            //Falta validar si el resultado es falso

            var response = ValidateLogin(model);
            return RedirectToAction("Index", "Account");
        }
    }
}