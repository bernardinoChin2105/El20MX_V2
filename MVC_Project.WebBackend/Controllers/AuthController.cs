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
//using LogHubSDK.Models;

namespace MVC_Project.WebBackend.Controllers
{
    public class AuthController : BaseController
    {
        private IAuthService _authService;
        private IUserService _userService;
        private IPermissionService _permissionService;
      
        public AuthController(IAuthService authService, IUserService userService, IPermissionService permissionService)
        {
            _authService = authService;
            _userService = userService;
            _permissionService = permissionService;
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
                var user = _authService.Authenticate(model.Email, SecurityUtil.EncryptPassword(model.Password));
                if (user != null)
                {
                    if (!user.Status)
                    {
                        ViewBag.Error = Resources.ErrorMessages.UserInactive;
                        return View(model);
                    }
                    user.LastLoginAt = DateTime.Now;
                    _userService.Update(user);

                    //Permissions by role
                    List<Permission> permissionsUser = user.Role.Permissions.Select(p => new Permission
                    {
                        Action = p.Action,
                        Controller = p.Controller,
                        Module = p.Module
                    }).ToList();

                    //IF SUPPORT, SET ALL PERMISSIONS
                    if (user.Role.Code == Constants.ROLE_IT_SUPPORT)
                    {
                        permissionsUser = _permissionService.GetAll().Select(p => new Permission
                        {
                            Action = p.Action,
                            Controller = p.Controller,
                            Module = p.Module
                        }).ToList();
                    }

                    AuthUser authUser = new AuthUser
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Language = user.Language,
                        Uuid = user.Uuid,
                        PasswordExpiration = user.PasswordExpiration,
                        Role = new Role
                        {
                            Code = user.Role.Code,
                            Name = user.Role.Name
                        },
                        Permissions = permissionsUser
                    };
                    
                    //Set user in sesion
                    Authenticator.StoreAuthenticatedUser(authUser);

                    //LogUtil.AddEntry(
                    //   "Ingresa al Sistema",
                    //   ENivelLog.Info,
                    //   authUser.Id,
                    //   authUser.Email,
                    //   EOperacionLog.ACCESS,
                    //   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                    //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                    //   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    //);

                    //Set Language
                    LanguageMngr.SetDefaultLanguage();
                    if (!string.IsNullOrEmpty(authUser.Language))
                    {
                        LanguageMngr.SetLanguage(authUser.Language);
                    }

                    if (user.PasswordExpiration.HasValue)
                    {
                        DateTime passwordExpiration = user.PasswordExpiration.Value;
                        DateTime todayDate = DateUtil.GetDateTimeNow();
                        if (user.PasswordExpiration.Value.Date < todayDate.Date)
                        {
                            return RedirectToAction("ChangePassword", "Auth");
                        }
                        string daysBeforeExpireToNotifyConfig = ConfigurationManager.AppSettings["DaysBeforeExpireToNotify"];
                        int daysBeforeExpireToNotify = 0;
                        if (Int32.TryParse(daysBeforeExpireToNotifyConfig, out daysBeforeExpireToNotify))
                        {

                            int daysLeft = ((TimeSpan)(passwordExpiration.Date - todayDate.Date)).Days + 1;
                            if(daysLeft <= daysBeforeExpireToNotify)
                            {
                                string message = String.Format(ViewLabels.PASSWORD_EXPIRATION_MESSAGE, daysLeft);
                                MensajeFlashHandler.RegistrarMensaje(message, TiposMensaje.Info);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(Request.Form["ReturnUrl"]))
                    {
                        return Redirect(Request.Form["ReturnUrl"]);
                    }
                    return RedirectToAction("Index", "Home");
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
                var resultado = _userService.FindBy(e => e.Email == model.Email).First();
                if (resultado != null)
                {
                    ViewBag.mensajeError = string.Empty;
                    resultado.ExpiraToken = System.DateTime.Now.AddDays(1);
                    string token = (resultado.Uuid + "@" + DateTime.Now.AddDays(1).ToString());
                     token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                    resultado.Token = token;
                    Dictionary<string, string> customParams = new Dictionary<string, string>();
                    string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
                    string link = urlAccion + "Auth/AccedeToken?token=" + token;
                    customParams.Add("param1", resultado.Email);
                    customParams.Add("param2", link);
                    NotificationUtil.SendNotification(resultado.Email, customParams, Constants.NOT_TEMPLATE_PASSWORDRECOVER );
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
                string id = elements.First().ToString();
                var resultado = _userService.FindBy(e => e.Uuid == id).First();
                int[] valores = new int[100];
                for(int a=0;a<100; a++)
                {
                    valores[a] = a++;
                }
                if (resultado != null && DateTime.Now <= resultado.ExpiraToken)
                {
                    ResetPassword model = new ResetPassword();
                    model.Uuid = resultado.Uuid.ToString();
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
            var user = _userService.FindBy(e => e.Uuid == authenticatedUser.Uuid).First();

            if (!String.IsNullOrWhiteSpace(model.Password) && user != null)
            {
                string encriptedPass = SecurityUtil.EncryptPassword(model.Password);
                if (user.Password == encriptedPass)
                {
                    ModelState.AddModelError("Password", "La contraseña ya ha sido utilizada");
                }
            }
            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    user.Password = SecurityUtil.EncryptPassword(model.Password);
                    DateTime todayDate = DateUtil.GetDateTimeNow();
                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                    user.PasswordExpiration = passwordExpiration;
                    _userService.Update(user);
                    AuthUser authUser = new AuthUser
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Uuid = user.Uuid,
                        PasswordExpiration = user.PasswordExpiration,
                        Role = new Role
                        {
                            Code = user.Role.Code,
                            Name = user.Role.Name
                        },
                        Permissions = user.Permissions.Select(p => new Permission
                        {
                            Action = p.Action,
                            Controller = p.Controller,
                            Module = p.Module
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
                var resultado = _userService.FindBy(e => e.Uuid == model.Uuid).First();
                if (resultado != null)
                {
                    resultado.Password = SecurityUtil.EncryptPassword(model.Password);
                    DateTime todayDate = DateUtil.GetDateTimeNow();
                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                    resultado.PasswordExpiration = passwordExpiration;
                    _userService.Update(resultado);
                    AuthUser authUser = new AuthUser
                    {
                        FirstName = resultado.FirstName,
                        LastName = resultado.LastName,
                        Uuid = resultado.Uuid,
                        PasswordExpiration = resultado.PasswordExpiration,
                        Email = resultado.Email,
                        Role = new Role
                        {
                            Code = resultado.Role.Code,
                            Name = resultado.Role.Name
                        },
                        Permissions = resultado.Permissions.Select(p => new Permission
                        {
                            Action = p.Action,
                            Controller = p.Controller,
                            Module = p.Module
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
    }
}