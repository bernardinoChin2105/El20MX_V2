using ExcelEngine;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.WebBackend.Models;
using MVC_Project.Web.Models.ExcelImport;
using MVC_Project.WebBackend.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.FlashMessages;
using LogHubSDK.Models;
using Newtonsoft.Json;

namespace MVC_Project.WebBackend.Controllers
{
    public class UserController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IMembershipService _accountUserService;
        private IFeatureService _featureService;
        private IProfileService _profileService;
        private IMembershipService _membershipService;
        public UserController(IUserService userService, IRoleService roleService, IMembershipService accountUserService, IFeatureService featureService,
            IProfileService profileService, IMembershipService membershipService)
        {
            _userService = userService;
            _roleService = roleService;
            _accountUserService = accountUserService;
            _featureService = featureService;
            _profileService = profileService;
            _membershipService = membershipService;
        }

        [Authorize]
        public ActionResult Index()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            UserViewModel model = new UserViewModel
            {
                UserList = new UserData(),
                Status = FilterStatusEnum.ALL.Id,
                Statuses = FilterStatusEnum.GetSelectListItems()
            };
            
            return View(model);
        }
        [Authorize]
        public ActionResult Import()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            UserImportViewModel model = new UserImportViewModel();
            LogUtil.AddEntry(
               "Importar usuarios: " + JsonConvert.SerializeObject(model),
               ENivelLog.Info,
               userAuth.Id,
               userAuth.Email,
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
            );
            return View("Import", model);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Import(UserImportViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            if (ModelState.IsValid)
            {
                ResultExcelImporter<UserImport> result = ExcelImporterMapper.ReadExcel<UserImport>(new ExcelFileInputData
                {
                    ContentLength = model.ImportedFile.ContentLength,
                    FileName = model.ImportedFile.FileName,
                    InputStream = model.ImportedFile.InputStream
                });
                model.ImportResult = new List<UserRowImportResultViewModel>();
                foreach (RowResult rowResult in result.ResultMapExcel.RowResults)
                {
                    UserRowImportResultViewModel userRowImportResultViewModel = new UserRowImportResultViewModel();
                    userRowImportResultViewModel.Email = rowResult.RowsValues.Email;
                    userRowImportResultViewModel.EmployeeNumber = rowResult.RowsValues.EmployeeNumber;
                    userRowImportResultViewModel.Name = rowResult.RowsValues.Name;
                    userRowImportResultViewModel.RowNumber = rowResult.Number;
                    userRowImportResultViewModel.Messages = new List<string>();
                    bool hasCustomError = false;
                    User existingUser = _userService.FindBy(x => x.name == userRowImportResultViewModel.Email).FirstOrDefault();
                    if (existingUser != null && !String.IsNullOrWhiteSpace(userRowImportResultViewModel.Email))
                    {
                        userRowImportResultViewModel.Messages.Add("El correo electrónico del usuario ya se encuentra registrado");
                    }
                    hasCustomError = userRowImportResultViewModel.Messages.Any();
                    if (rowResult.HasError)
                    {
                        userRowImportResultViewModel.Messages = userRowImportResultViewModel.Messages.Concat(rowResult.ErrorMessages).ToList();
                    }
                    if (!rowResult.HasError && !hasCustomError)
                    {

                        userRowImportResultViewModel.Messages.Add("Usuario registrado satisfactoriamente");
                    }
                }
            }

            LogUtil.AddEntry(
               "Importar usuarios: " + JsonConvert.SerializeObject(model),
               ENivelLog.Info,
               userAuth.Id,
               userAuth.Email,
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
            );

            return View("Import", model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                IList<UserData> dataResponse = new List<UserData>();
                int totalDisplay = 0;
                Int64? accountId = userAuth.GetAccountId();
                var results = _userService.FilterBy(filtersValues, accountId, param.iDisplayStart, param.iDisplayLength);
                totalDisplay = results.Item2;
                foreach (var user in results.Item1)
                {
                    var membership = user.memberships.FirstOrDefault(x => accountId.HasValue ? (x.account.id == accountId) : (true));
                    UserData userData = new UserData();
                    userData.Name = user.profile.firstName + " " + user.profile.lastName;
                    userData.Email = user.name;
                    userData.RoleName = membership.role.name;
                    userData.CreatedAt = user.createdAt;
                    userData.UpdatedAt = user.modifiedAt;
                    userData.Status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), membership.status)).GetDisplayName();
                    userData.Uuid = user.uuid.ToString();
                    userData.LastLoginAt = user.lastLoginAt;
                    userData.IsOwner = membership.role.code == SystemRoles.ACCOUNT_OWNER.ToString();
                    dataResponse.Add(userData);
                }
               
                return Json(new
                {
                    success = true,
                    param.sEcho,
                    iTotalRecords = dataResponse.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = dataResponse,
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

        [Authorize]
        public ActionResult Create()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var roles = PopulateRoles();
                if (!roles.Any())
                {
                    MensajeFlashHandler.RegistrarMensaje("Registre un rol antes de crear al usuario", TiposMensaje.Warning);
                    return RedirectToAction("Index");
                }
                var userCreateViewModel = new UserCreateViewModel { Roles = roles, isBackOffice = userAuth.isBackOfficeConfiguration() };
                
                return View(userCreateViewModel);
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
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        private IEnumerable<FeatureViewModel> PopulateFeatures(int roleId)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var role = _roleService.FindBy(x => x.id == roleId).FirstOrDefault();
            var features = _featureService.GetAll();
            var featuresViewModel = new List<FeatureViewModel>();
            var values = Enum.GetValues(typeof(SystemLevelPermission));
            var items = new List<SelectListItem>();
            foreach (var i in values)
            {
                items.Add(new SelectListItem
                {
                    Text = Enum.GetName(typeof(SystemLevelPermission), i),
                    Value = ((int)i).ToString()
                });
            }
            foreach (var feature in features)
            {
                var permisions = role.permissions.Where(x => x.feature.id == feature.id);
                featuresViewModel.Add(new FeatureViewModel
                {
                    Id = feature.id,
                    Name = feature.name,
                    Permissions = permisions.Select(x => new PermissionViewModel
                    {
                        Id = x.id,
                        Name = x.description,
                        SystemActions = items
                    }).ToList()
                });
            }

            return featuresViewModel;
        }

        private List<SelectListItem> PopulateRoles()
        {
            var userAuth = Authenticator.AuthenticatedUser;

            Int64? accountId = userAuth.GetAccountId();
            List<Role> availableRoles = _roleService.FindBy(x => x.account.id == accountId && x.status == SystemStatus.ACTIVE.ToString()).OrderBy(x => x.code).ToList();
            
            var rolesList = new List<SelectListItem>();
            rolesList = availableRoles.Select(role => new SelectListItem
            {
                Value = role.id.ToString(),
                Text = role.name
            }).ToList();

            return rolesList.ToList();
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(UserCreateViewModel userCreateViewModel)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                bool newUser = true;
                var user = _userService.FirstOrDefault(x => x.name == userCreateViewModel.Email);
                if (user != null)
                {
                    if (userAuth.isBackOfficeConfiguration())
                    {
                        throw new Exception("El usuario ya existe en el sistema");
                    }
                    else if (user.memberships.Any(x => x.account.id == userAuth.Account.Id))
                        throw new Exception("El usuario ya se encuentra relacionado a la cuenta");

                    newUser = false;
                }

                DateTime todayDate = DateUtil.GetDateTimeNow();

                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];

                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                if (newUser)
                {
                    user = new User
                    {
                        uuid = Guid.NewGuid(),
                        name = userCreateViewModel.Email,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.UNCONFIRMED.ToString(),
                        isBackOffice = userAuth.isBackOfficeConfiguration(),
                        profile = new Profile
                        {
                            uuid = Guid.NewGuid(),
                            firstName = userCreateViewModel.Name,
                            lastName = userCreateViewModel.Apellidos,
                            email = userCreateViewModel.Email,
                            phoneNumber = userCreateViewModel.MobileNumber,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString(),
                            avatar = ConfigurationManager.AppSettings["Avatar.User"]
                        }
                    };
                }
                var role = _roleService.GetById(userCreateViewModel.Role);
                Account account = userAuth.isBackOfficeConfiguration() ? null : new Account { id = userAuth.Account.Id };
                user.memberships.Add(new Membership
                {
                    account = account,
                    role = role,
                    user = user,
                    assignedBy = userAuth.Id,
                    acceptUser = (userCreateViewModel.AcceptUser || userAuth.isBackOfficeConfiguration()) ? 
                    TermsAndConditions.ACCEPT.ToString() : TermsAndConditions.DECLINE.ToString(),
                    status = SystemStatus.UNCONFIRMED.ToString()
                });

                if (newUser)
                    _userService.CreateUser(user);
                else
                    _userService.Update(user);

                if (userAuth.isBackOfficeConfiguration())
                {
                    SendWelcomeBackOffice(user);
                }    
                else
                {
                    SendWelcomeMail(user, userAuth.Account);
                }

                if (newUser)
                    MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                else
                    MensajeFlashHandler.RegistrarMensaje("Usuario agregado a la cuenta", TiposMensaje.Success);

                LogUtil.AddEntry(
                   "Crea nuevo usuario: " + JsonConvert.SerializeObject(user),
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                return RedirectToAction("Index");
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

                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                userCreateViewModel.Roles = PopulateRoles();
                return View(userCreateViewModel);
            }
        }

        private void SendWelcomeMail(User user, AuthManagement.Models.Account account)
        {
            string token = (user.uuid + "@" + account.Uuid + "@" + user.name);
            token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
            Dictionary<string, string> customParams = new Dictionary<string, string>();
            string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
            string link = urlAccion + "Register/NewUserVerify?token=" + token;
            customParams.Add("param1", user.profile.firstName);
            customParams.Add("param2", user.name);
            customParams.Add("param3", account.RFC);
            customParams.Add("param4", link);
            NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_NEW_USER);
        }

        private void SendWelcomeBackOffice(User user)
        {
            string token = (user.uuid + "@ @" + user.name);
            token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
            Dictionary<string, string> customParams = new Dictionary<string, string>();
            string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
            string link = urlAccion + "Register/NewUserVerify?token=" + token;
            customParams.Add("param1", user.profile.firstName);
            customParams.Add("param2", user.name);
            customParams.Add("param3", link);
            NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_NEW_BACKOFFICE_USER);
        }

        [Authorize]
        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                Int64? accountId = userAuth.GetAccountId();

                User user = _userService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (user == null)
                    throw new Exception("El usuario no se encontró en la base de datos");

                Membership membership = user.memberships.FirstOrDefault(x => accountId.HasValue ? x.account.id == accountId : true);

                UserEditViewModel model = new UserEditViewModel();
                model.Uuid = user.uuid.ToString();
                model.Name = user.profile.firstName;
                model.Apellidos = user.profile.lastName;
                model.Email = user.name;
                model.MobileNumber = user.profile.phoneNumber;
                model.Roles = PopulateRoles();
                if (!model.Roles.Any(x => x.Value == membership.role.id.ToString()))
                {
                    model.Roles.Add(new SelectListItem
                    {
                        Text = membership.role.name,
                        Value = membership.role.id.ToString()
                    });
                }
                model.Role = membership.role.id;
                model.Status = user.status;
                model.RoleCode = membership.role.code;
                
                return View(model);
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
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Edit(UserEditViewModel model, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                Int64? accountId = userAuth.GetAccountId();

                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                User user = _userService.FirstOrDefault(x => x.uuid == Guid.Parse(model.Uuid));

                if (_userService.FindBy(x => x.name == model.Email && x.id != user.id).Any())
                    throw new Exception("El email proporcionado se encuentra registrado en otro usuario");

                user.profile.firstName = model.Name;
                user.profile.lastName = model.Apellidos;
                user.profile.phoneNumber = model.MobileNumber;
                _profileService.Update(user.profile);

                Membership membership = user.memberships.FirstOrDefault(x => accountId.HasValue ? x.account.id == accountId : true);

                if (membership.role.id != model.Role)
                {
                    membership.role = new Role { id = model.Role };
                    _membershipService.Update(membership);
                }
                if (!user.name.Equals(model.Email))
                {
                    user.name = model.Email;
                    if (user.isBackOffice)
                        SendWelcomeBackOffice(user);
                    else
                        SendWelcomeMail(user, userAuth.Account);
                }
                _userService.Update(user);

                LogUtil.AddEntry(
                   "Usuario editado: " + user.name,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(user)
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
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
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                model.Roles = PopulateRoles();
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult EditPassword(string uuid)
        {
            var user = _userService.FindBy(e => e.uuid == Guid.Parse(uuid)).FirstOrDefault();
            if (user == null)
            {
                string message = Resources.ErrorMessages.UserNotAvailable;
                if (Request.IsAjaxRequest())
                {
                    LogUtil.AddEntry(
                       "Editar password: " + JsonConvert.SerializeObject(user),
                       ENivelLog.Info,
                       0,
                       "",
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                    );
                    return JsonStatusGone(message);
                }
                else
                {
                    LogUtil.AddEntry(
                       "Editar password: " + JsonConvert.SerializeObject(user),
                       ENivelLog.Info,
                       0,
                       "",
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                    );
                    FlashMessages.MensajeFlashHandler.RegistrarMensaje(message, FlashMessages.TiposMensaje.Error);
                    return RedirectToAction("Index");
                }
            }
            UserChangePasswordViewModel model = new UserChangePasswordViewModel
            {
                Uuid = uuid,
                Password = null,
                ConfirmPassword = null
            };
            if (Request.IsAjaxRequest())
            {
                LogUtil.AddEntry(
                   "Editar password: " + JsonConvert.SerializeObject(model),
                   ENivelLog.Info,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
                return PartialView(model);
            }

            LogUtil.AddEntry(
               "Editar password: " + JsonConvert.SerializeObject(model),
               ENivelLog.Info,
               0,
               "",
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
            );
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditPassword(UserChangePasswordViewModel model)
        {
            var user = _userService.FindBy(e => e.uuid == Guid.Parse(model.Uuid)).FirstOrDefault();
            if (user == null)
            {
                string message = Resources.ErrorMessages.UserNotAvailable;
                if (Request.IsAjaxRequest())
                {
                    LogUtil.AddEntry(
                       "Editar password: " + JsonConvert.SerializeObject(model),
                       ENivelLog.Info,
                       0,
                       "",
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                    );
                    return JsonStatusGone(message);
                }
                else
                {
                    LogUtil.AddEntry(
                        "Editar password: " + JsonConvert.SerializeObject(model),
                        ENivelLog.Info,
                        0,
                        "",
                        EOperacionLog.ACCESS,
                        string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                        ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                        string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                     );
                    FlashMessages.MensajeFlashHandler.RegistrarMensaje(message, FlashMessages.TiposMensaje.Error);
                    return RedirectToAction("Index");
                }
            }
            if (ModelState.IsValid)
            {
                user.password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                DateTime passwordExpiration = todayDate.AddDays(-1);
                user.passwordExpiration = passwordExpiration;
                _userService.Update(user);
                string successMessage = Resources.Messages.UserPasswordUpdated;
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        Message = successMessage
                    });
                }

                LogUtil.AddEntry(
                   "Editar password: " + JsonConvert.SerializeObject(model),
                   ENivelLog.Info,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
                FlashMessages.MensajeFlashHandler.RegistrarMensaje(successMessage, FlashMessages.TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            if (Request.IsAjaxRequest())
            {
                Response.StatusCode = 422;
                return Json(new
                {
                    issue = model,
                    errors = ModelState.Keys.Where(k => ModelState[k].Errors.Count > 0)
                    .Select(k => new { propertyName = k, errorMessage = ModelState[k].Errors[0].ErrorMessage })
                });
            }
            LogUtil.AddEntry(
               "Editar password: " + JsonConvert.SerializeObject(model),
               ENivelLog.Info,
               0,
               "",
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
            );
            return View(model);
        }

        [HttpPost, Authorize]
        public ActionResult ChangeStatus(string uuid, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var user = _userService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));

                if (user != null)
                {
                    var membership = _membershipService.FirstOrDefault(x => x.user.id == user.id && x.account.id == userAuth.Account.Id);
                    if (membership != null)
                    {
                        if (membership.status == SystemStatus.ACTIVE.ToString())
                            membership.status = SystemStatus.INACTIVE.ToString();
                        else if (membership.status == SystemStatus.INACTIVE.ToString())
                            membership.status = SystemStatus.ACTIVE.ToString();

                        _membershipService.Update(membership);
                        LogUtil.AddEntry(
                           "Cambio de estatus: " + JsonConvert.SerializeObject(membership),
                           ENivelLog.Info,
                           userAuth.Id,
                           userAuth.Email,
                           EOperacionLog.ACCESS,
                           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                        );
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
                LogUtil.AddEntry(
                   "Cambio de estatus no exitoso " + uuid,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
                return Json(false, JsonRequestBehavior.AllowGet);
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult ResendInvite(string uuid, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                Int64? accountId = userAuth.GetAccountId();
                var user = _userService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (user != null)
                {
                    var membership = user.memberships.FirstOrDefault(x => accountId.HasValue ? x.account.id == accountId : true);
                    if (membership != null && membership.status == SystemStatus.UNCONFIRMED.ToString())
                    {
                        if (userAuth.isBackOfficeConfiguration())
                            SendWelcomeBackOffice(user);
                        else
                            SendWelcomeMail(user, userAuth.Account);

                        LogUtil.AddEntry(
                           "Se reenvio la invitación al usuario: " + uuid,
                           ENivelLog.Info,
                           userAuth.Id,
                           userAuth.Email,
                           EOperacionLog.ACCESS,
                           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                        );

                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
                LogUtil.AddEntry(
                   "No se puso reenviar la invitación: " + JsonConvert.SerializeObject(uuid),
                   ENivelLog.Error,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
                return Json(false, JsonRequestBehavior.AllowGet);
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}