using MVC_Project.Domain.Services;
//using MVC_Project.FlashMessages;
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
using Newtonsoft.Json;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Pipedrive;

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
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var user = _authService.Authenticate(model.Email, SecurityUtil.EncryptPassword(model.Password));

                if (user == null)
                    throw new Exception("El usuario no existe o contraseña inválida.");

                if (user.status != SystemStatus.ACTIVE.ToString())
                {
                    string msgReactivation = "";
                    if (user.status == SystemStatus.UNCONFIRMED.ToString())
                    {
                        //Enviar notificación para activar el correo si no es por red social
                        string token = (user.uuid + "@");
                        token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                        //user.token = token;
                        Dictionary<string, string> customParams = new Dictionary<string, string>();
                        string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
                        string link = urlAccion + "Register/VerifyUser?token=" + token;
                        customParams.Add("param1", user.profile.firstName);
                        customParams.Add("param2", link);
                        NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_WELCOME);

                        msgReactivation = ". Se ha enviado un email de confirmación.";                       
                    }
                    //Sino confirmado se reenviara el correo de confirmación
                    throw new Exception(ErrorMessages.UserInactive + msgReactivation);
                }

                //Asignar usuario en sesión
                var authUser = GetValidateUserLogin(user);


                #region Se comenta para evitar validar la expiración del password

                //if (user.passwordExpiration.HasValue)
                //{
                //    DateTime passwordExpiration = user.passwordExpiration.Value;
                //    DateTime todayDate = DateUtil.GetDateTimeNow();
                //    if (user.passwordExpiration.Value.Date < todayDate.Date)
                //    {
                //        return RedirectToAction("ChangePassword", "Auth", new { userUuid = user.uuid });
                //    }
                //    string daysBeforeExpireToNotifyConfig = ConfigurationManager.AppSettings["DaysBeforeExpireToNotify"];
                //    int daysBeforeExpireToNotify = 0;
                //    if (Int32.TryParse(daysBeforeExpireToNotifyConfig, out daysBeforeExpireToNotify))
                //    {
                //        int daysLeft = ((TimeSpan)(passwordExpiration.Date - todayDate.Date)).Days + 1;
                //        if (daysLeft <= daysBeforeExpireToNotify)
                //        {
                //            string message = String.Format(ViewLabels.PASSWORD_EXPIRATION_MESSAGE, daysLeft);
                //            MensajeFlashHandler.RegistrarMensaje(message, TiposMensaje.Info);
                //        }
                //    }
                //}

                #endregion

                var memberships = user.memberships.Where(x => x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());

                if (user.isBackOffice)
                {
                    var uniqueMembership = memberships.First();
                    List<Permission> permissionsUniqueMembership = uniqueMembership.role.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                    {
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level,
                        isCustomizable = p.permission.isCustomizable
                    }).ToList();

                    authUser.Role = new Role { Id = uniqueMembership.role.id, Code = uniqueMembership.role.code, Name = uniqueMembership.role.name };
                    authUser.Permissions = permissionsUniqueMembership;
                    
                    Authenticator.StoreAuthenticatedUser(authUser);
                    //MensajeFlashHandler.RegistrarMensaje("Sesión iniciada", TiposMensaje.Success);

                    LogUtil.AddEntry("Sesión iniciada", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );

                    return RedirectToAction("Index", "Account");
                }
                else if (memberships.Count() <= 0)//Rol invitado
                {
                    var guestRole = _roleService.FirstOrDefault(x => x.code == SystemRoles.LEAD.ToString());
                    List<Permission> permissionsGest = guestRole.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                    {
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level,
                        isCustomizable = p.permission.isCustomizable
                    }).ToList();

                    authUser.Role = new Role { Id = guestRole.id, Code = guestRole.code, Name = guestRole.name };
                    authUser.Permissions = permissionsGest;
                    Authenticator.StoreAuthenticatedUser(authUser);
                    
                    LogUtil.AddEntry( "Sesión iniciada", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );

                    return RedirectToAction("Index", "Account");
                }
                else if(memberships.Count() == 1)
                {
                    var uniqueMembership = memberships.First();
                    if (uniqueMembership.account.status == SystemStatus.INACTIVE.ToString())
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

                        LogUtil.AddEntry("Sesión iniciada", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                           string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                           string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                        );

                        return RedirectToAction("Index", "Account");
                    }
                    List<Permission> permissionsUniqueMembership = uniqueMembership.role.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                    {
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level,
                        isCustomizable = p.permission.isCustomizable
                    }).ToList();

                    authUser.Role = new Role { Id = uniqueMembership.role.id, Code = uniqueMembership.role.code, Name = uniqueMembership.role.name };
                    authUser.Permissions = permissionsUniqueMembership;
                    authUser.Account = new Account { Id = uniqueMembership.account.id, Name = uniqueMembership.account.name, RFC = uniqueMembership.account.rfc, Uuid = uniqueMembership.account.uuid, Image = uniqueMembership.account.avatar };
                    Authenticator.StoreAuthenticatedUser(authUser);

                    LogUtil.AddEntry("Sesión iniciada", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );
                    var start = permissionsUniqueMembership.FirstOrDefault(x => x.isCustomizable && x.Level != SystemLevelPermission.NO_ACCESS.ToString());
                    return RedirectToAction("Index", start.Controller);
                }
                else
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

                    LogUtil.AddEntry("Sesión iniciada", ENivelLog.Info, authUser.Id, authUser.Email, EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );

                    return RedirectToAction("Index", "Account");
                }
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            var authUser = Authenticator.AuthenticatedUser;
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
                //return Json(new
                //{
                //    success = false,
                //    issue = model,
                //    errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                //    .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
                //});

                var list = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                    .Select(k => ModelState[k].Errors[0].ErrorMessage);
                string errors = string.Join(",", list);

                //ModelState.AddModelError("Email", "No se encontró ninguna cuenta con el correo proporcionado. Verifique su información.");
                MensajeFlashHandler.RegistrarMensaje("Error: "+errors, TiposMensaje.Error);

                //return View("Login");
                return RedirectToAction("Login", "Auth");
            }
            try
            {
                var resultado = _userService.FindBy(e => e.name == model.Email).First();
                if (resultado != null)
                {
                    ViewBag.mensajeError = string.Empty;
                    var expirationDate = DateTime.Now.AddDays(1);
                    resultado.tokenExpiration = expirationDate;
                    string token = (resultado.uuid + "@" + expirationDate.ToString());
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

                    LogUtil.AddEntry("Cambio de contraseña", ENivelLog.Info, 0, "", EOperacionLog.UPDATE,
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                    );

                    //return View("Login");
                    return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
            }

            ModelState.AddModelError("Email", "No se encontró ninguna cuenta con el correo proporcionado. Verifique su información.");
            MensajeFlashHandler.RegistrarMensaje("No se encontró ninguna cuenta con el correo proporcionado. Verifique su información.", TiposMensaje.Error);
            
            //return View("Login");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult AccedeToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    throw new Exception("El token no es válido");

                var desencriptaToken = EncryptorText.DataDecrypt(token.Replace("!!", "/").Replace("$", "+"));

                if (string.IsNullOrEmpty(desencriptaToken))
                    throw new Exception("El token no es válido");

                var elements = desencriptaToken.Split('@');
                Guid id = new Guid();

                if (!Guid.TryParse(elements.First().ToString(), out id))
                    throw new Exception("El token no es válido");

                var user = _userService.FindBy(e => e.uuid == id).First();

                if (user == null)
                    throw new Exception("Usuario no encontrado en el sistema");

                if (DateTime.Now > user.tokenExpiration)
                    throw new Exception("El token ha expirado");

                ResetPassword model = new ResetPassword();
                model.Uuid = user.uuid;

                return View("ResetPassword", model);

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View("Login");
            }
        }

        [HttpGet, AllowAnonymous]
        public ActionResult ChangePassword(Guid userUuid)
        {
            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;
            //if (authenticatedUser == null)
            //{
            //    return RedirectToAction("Login", "Auth");
            //}
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            model.Uuid = userUuid;            
            return View("ChangePassword", model);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada es incorrecto");

                var user = _userService.FirstOrDefault(e => e.uuid == model.Uuid);

                if (user == null)
                    throw new Exception("El usuario no se encuentra registrado en el sistema");

                user.password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                user.passwordExpiration = passwordExpiration;
                _userService.Update(user);

                MensajeFlashHandler.RegistrarMensaje("Contraseña actualizada", TiposMensaje.Success);

                LogUtil.AddEntry("Actualización: " + JsonConvert.SerializeObject(model), ENivelLog.Info, 0, "", EOperacionLog.UPDATE,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );

                return RedirectToAction("Login", "Auth");

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return View("ChangePassword", model);
            }

        }

        [HttpPost, AllowAnonymous]
        public ActionResult Reset(ResetPassword model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada es incorrecto");

                var user = _userService.FirstOrDefault(e => e.uuid == model.Uuid);

                if (user == null)
                    throw new Exception("El usuario no se encuentra registrado en el sistema");

                user.password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                user.passwordExpiration = passwordExpiration;
                _userService.Update(user);

                MensajeFlashHandler.RegistrarMensaje("Contraseña actualizada", TiposMensaje.Success);

                LogUtil.AddEntry("Contraseña actualizada: " + JsonConvert.SerializeObject(model), ENivelLog.Info, 0, "", EOperacionLog.UPDATE,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );

                return RedirectToAction("Login", "Auth");

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View("ResetPassword", model);
            }
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
            Domain.Entities.User user = null;
            bool exist = false;
            string url = string.Empty;
            try
            {
                //es una red social
                if (model.RedSocial)
                {
                    user = _authService.AuthenticateSocialNetwork(model.Email, SecurityUtil.EncryptPassword(model.Password),
                    model.TypeRedSocial, model.SocialId);
                    
                    if (user != null)
                    {
                        if (user.status != SystemStatus.ACTIVE.ToString())
                        {
                            Error = Resources.ErrorMessages.UserInactive;
                        }
                        else
                        {
                            //Es un usuario con red social activa
                            //Asignar usuario en sesión
                            var authUser = GetValidateUserLogin(user);

                            if (authUser != null)
                            {
                                exist = true;

                                var memberships = user.memberships.Where(x => x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());

                                if (memberships.Count() <= 0)//Rol invitado
                                {
                                    var guestRole = _roleService.FindBy(x => x.code == SystemRoles.LEAD.ToString()).FirstOrDefault();
                                    List<Permission> permissionsGest = guestRole.rolePermissions.Select(p => new Permission
                                    {
                                        Controller = p.permission.controller,
                                        Module = p.permission.module,
                                        Level = p.level,
                                        isCustomizable = p.permission.isCustomizable
                                    }).ToList();

                                    authUser.Role = new Role { Id = guestRole.id, Code = guestRole.code, Name = guestRole.name };
                                    authUser.Permissions = permissionsGest;
                                    Authenticator.StoreAuthenticatedUser(authUser);
                                    url = "/Account/Index";
                                }
                                else if(memberships.Count()==1)
                                {
                                    var uniqueMembership = memberships.First();
                                    List<Permission> permissionsUniqueMembership = uniqueMembership.role.rolePermissions.Select(p => new Permission
                                    {
                                        Controller = p.permission.controller,
                                        Module = p.permission.module,
                                        Level = p.level,
                                        isCustomizable = p.permission.isCustomizable
                                    }).ToList();

                                    authUser.Role = new Role { Id = uniqueMembership.role.id, Code = uniqueMembership.role.code, Name = uniqueMembership.role.name };
                                    authUser.Permissions = permissionsUniqueMembership;
                                    authUser.Account = new Account { Name = uniqueMembership.account.name, RFC = uniqueMembership.account.rfc, Uuid = uniqueMembership.account.uuid, Image = uniqueMembership.account.avatar, Id = uniqueMembership.account.id };
                                    Authenticator.StoreAuthenticatedUser(authUser);
                                    var start = permissionsUniqueMembership.FirstOrDefault(x => x.isCustomizable && x.Level != SystemLevelPermission.NO_ACCESS.ToString());
                                    url = "/" + start.Controller + "/Index";
                                }
                                else
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
                                    url = "/Account/Index";
                                }
                            }
                            else
                            {
                                Error = "El usuario esta inactivo";
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

                LogUtil.AddEntry( "Validación: "+Error + ", URL: "+url+", Existe: "+exist, ENivelLog.Info, 0, "", EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );

                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "response", error = Error }, data = user.id, Success = exist, Url = url },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };

            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message }, Success = false, Url = "" },
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

            user.lastLoginAt = DateTime.Now;
            _userService.Update(user);

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
                isBackOffice = user.isBackOffice
            };

            //Set Language
            LanguageMngr.SetDefaultLanguage();
            if (!string.IsNullOrEmpty(authUser.Language))
            {
                LanguageMngr.SetLanguage(authUser.Language);
            }
            
            return authUser;
        }

        [AllowAnonymous]
        public ActionResult LoginAuth()
        {
            //Falta validar si el resultado es falso

            AuthViewModel model = (AuthViewModel)Session["modelNW"];

            var response = ValidateLogin(model);

            var data = response.Data;
            var url = "/Auth/Login";
            if (data != null)
            {
                url = (String)(data.GetType().GetProperty("Url").GetValue(data, null));
            }

            return Redirect(url);
        }

        //Para mantener la sesión
        [AllowAnonymous]
        public ActionResult KeepAlive()
        {
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}