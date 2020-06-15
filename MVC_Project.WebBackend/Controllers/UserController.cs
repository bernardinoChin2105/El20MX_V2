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

namespace MVC_Project.WebBackend.Controllers
{
    public class UserController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IMembershipService _accountUserService;
        private IFeatureService _featureService;

        public UserController(IUserService userService, IRoleService roleService, IMembershipService accountUserService, IFeatureService featureService)
        {
            _userService = userService;
            _roleService = roleService;
            _accountUserService = accountUserService;
            _featureService = featureService;
        }

        [Authorize]
        public ActionResult Index()
        {
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
            UserImportViewModel model = new UserImportViewModel();
            return View("Import", model);
        }
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Import(UserImportViewModel model)
        {
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
                    if(!rowResult.HasError && !hasCustomError)
                    {
                        
                        userRowImportResultViewModel.Messages.Add("Usuario registrado satisfactoriamente");
                    }
                }
            }
            return View("Import", model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                IList<UserData> dataResponse = new List<UserData>();
                int totalDisplay = 0;
                if (userAuth.Account != null)
                {
                    var results = _userService.FilterBy(filtersValues, userAuth.Account.Id, param.iDisplayStart, param.iDisplayLength);
                    totalDisplay = results.Item2;
                    foreach (var user in results.Item1)
                    {
                        var accountUser = user.memberships.First();
                        UserData userData = new UserData();
                        userData.Name = user.profile.firstName + " " + user.profile.lastName;
                        userData.Email = user.name;
                        userData.RoleName = accountUser.role.name;
                        userData.CreatedAt = user.createdAt;
                        userData.UpdatedAt = user.modifiedAt;
                        userData.Status = user.status == SystemStatus.ACTIVE.ToString();
                        userData.Uuid = user.uuid.ToString();
                        userData.LastLoginAt = user.lastLoginAt;
                        dataResponse.Add(userData);
                    }
                }
                return Json(new
                {
                    success = true,
                    param.sEcho,
                    iTotalRecords = dataResponse.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = dataResponse,
                    error = "Test error"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [Authorize]
        public ActionResult Create()
        {
            var roles = PopulateRoles();
            var userCreateViewModel = new UserCreateViewModel { Roles = roles, Features = PopulateFeatures(int.Parse(roles.First().Value)) };
            return View(userCreateViewModel);
        }

        private IEnumerable<FeatureViewModel> PopulateFeatures(int roleId)
        {
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

        private IEnumerable<SelectListItem> PopulateRoles()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var availableRoles = _roleService.FindBy(x => x.account.id == userAuth.Id).OrderBy(x => x.code);
            var rolesList = new List<SelectListItem>();
            rolesList = availableRoles.Select(role => new SelectListItem
            {
                Value = role.id.ToString(),
                Text = role.name
            }).ToList();
            return rolesList;
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(UserCreateViewModel userCreateViewModel)
        {
            if(!String.IsNullOrWhiteSpace(userCreateViewModel.ConfirmPassword) 
                && !String.IsNullOrWhiteSpace(userCreateViewModel.Password))
            {
                if(!userCreateViewModel.Password.Equals(userCreateViewModel.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
                }
            }
            if (ModelState.IsValid)
            {
                DateTime todayDate =  DateUtil.GetDateTimeNow();

                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                var user = new User
                {
                    uuid = Guid.NewGuid(),
                    //firstName = userCreateViewModel.Name,
                    //lastName = userCreateViewModel.Apellidos,
                    name = userCreateViewModel.Email,
                    //phoneNumber = userCreateViewModel.MobileNumber,
                    password = SecurityUtil.EncryptPassword(userCreateViewModel.Password),
                    passwordExpiration = passwordExpiration,
                    //role = new Role { id = userCreateViewModel.Role },
                    //userName = userCreateViewModel.Username,
                    //language = userCreateViewModel.Language,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString()
                };
                var role = _roleService.GetById(userCreateViewModel.Role);
                //foreach (var permission in role.permissions)
                //{
                //    user.permissions.Add(permission);
                //}
                _userService.Create(user);

                /*AuthUser usuario = (AuthUser)Authenticator.AuthenticatedUser;
                LogUtil.AddEntry(
                   "Ingresa al Sistema",
                   ENivelLog.Info,
                   usuario.Id,
                   usuario.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", usuario.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", usuario.Email, DateUtil.GetDateTimeNow())
                );*/

                return RedirectToAction("Index");
            }
            else
            {
                userCreateViewModel.Roles = PopulateRoles();
                return View("Create", userCreateViewModel);
            }
        }

        [Authorize]
        public ActionResult Edit(string uuid)
        {
            User user = _userService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();

            var accountUser = user.memberships.First();
            //var accountUser = _accountUserService.FindBy(x => x.user.id == user.id && x.account.id == account.id).First();

            UserEditViewModel model = new UserEditViewModel();
            model.Uuid = user.uuid.ToString();
            model.Name = user.profile.firstName;
            model.Apellidos = user.profile.lastName;
            model.Email = user.name;
            model.MobileNumber = user.profile.phoneNumber;
            model.Roles = PopulateRoles();
            model.Role = accountUser.role.id;
            return View(model);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Edit(UserEditViewModel model, FormCollection collection)
        {
            try
            {
                User user = _userService.FindBy(x => x.uuid == Guid.Parse(model.Uuid)).First();
                user.name = model.Name;
                //user.lastName = model.Apellidos;
                //user.name = model.Email;
                //user.phoneNumber = model.MobileNumber;
                //user.userName = model.Username;
                //user.language = model.Language;
                _userService.Update(user);
                return RedirectToAction("Index");
            }
            catch
            {
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
                    return JsonStatusGone(message);
                }
                else
                {
                    FlashMessages.MensajeFlashHandler.RegistrarMensaje(message, FlashMessages.TiposMensaje.Error);
                    return RedirectToAction("Index");
                }
            }
            UserChangePasswordViewModel model = new UserChangePasswordViewModel {
                Uuid = uuid,
                Password = null,
                ConfirmPassword = null
            };
            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }

            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditPassword(UserChangePasswordViewModel model)
        {
            var user = _userService.FindBy(e => e.uuid == Guid.Parse(model.Uuid)).FirstOrDefault();
            if(user == null)
            {
                string message = Resources.ErrorMessages.UserNotAvailable;
                if (Request.IsAjaxRequest())
                {
                    return JsonStatusGone(message);
                }
                else
                {
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
                    return Json(new {
                        Message = successMessage
                    });
                }
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
            return View(model);
        }
        
        [HttpPost, Authorize]
        public ActionResult ChangeStatus(string uuid, FormCollection collection)
        {
            try
            {
                var user = _userService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (user != null)
                {
                    user.status = user.status;
                    _userService.Update(user);
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}