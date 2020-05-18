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

namespace MVC_Project.WebBackend.Controllers
{
    public class UserController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;

        public UserController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
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
                    User existingUser = _userService.FindBy(x => x.Email == userRowImportResultViewModel.Email).FirstOrDefault();
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

        [HttpGet, Authorize]
        public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        {
            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                var results = _userService.FilterBy(filtersValues, param.iDisplayStart, param.iDisplayLength);
                IList<UserData> dataResponse = new List<UserData>();
                foreach (var user in results.Item1)
                {
                    UserData userData = new UserData();
                    userData.Name = user.FirstName + " " + user.LastName;
                    userData.Email = user.Email;
                    userData.RoleName = user.Role.Name;
                    userData.CreatedAt = user.CreatedAt;
                    userData.UpdatedAt = user.UpdatedAt;
                    userData.Status = user.Status;
                    userData.Uuid = user.Uuid;
                    userData.LastLoginAt = user.LastLoginAt;
                    dataResponse.Add(userData);
                }
                return Json(new
                {
                    success = true,
                    param.sEcho,
                    iTotalRecords = dataResponse.Count(),
                    iTotalDisplayRecords = results.Item2,
                    aaData = dataResponse
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
            var userCreateViewModel = new UserCreateViewModel { Roles = PopulateRoles() };
            return View(userCreateViewModel);
        }

        private IEnumerable<SelectListItem> PopulateRoles()
        {
            var availableRoles = _roleService.GetAll();
            var rolesList = new List<SelectListItem>();
            rolesList = availableRoles.Select(role => new SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name
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
                    Uuid = Guid.NewGuid().ToString(),
                    FirstName = userCreateViewModel.Name,
                    LastName = userCreateViewModel.Apellidos,
                    Email = userCreateViewModel.Email,
                    MobileNumber = userCreateViewModel.MobileNumber,
                    Password = SecurityUtil.EncryptPassword(userCreateViewModel.Password),
                    PasswordExpiration = passwordExpiration,
                    Role = new Role { Id = userCreateViewModel.Role },
                    Username = userCreateViewModel.Username,
                    Language = userCreateViewModel.Language,
                    CreatedAt = todayDate,
                    UpdatedAt = todayDate,
                    Status = true
                };
                var role = _roleService.GetById(user.Role.Id);
                foreach (var permission in role.Permissions)
                {
                    user.Permissions.Add(permission);
                }
                _userService.Create(user);
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
            User user = _userService.FindBy(x => x.Uuid == uuid).First();
            UserEditViewModel model = new UserEditViewModel();
            model.Uuid = user.Uuid;
            model.Name = user.FirstName;
            model.Apellidos = user.LastName;
            model.Email = user.Email;
            model.MobileNumber = user.MobileNumber;
            model.Roles = PopulateRoles();
            model.Role = user.Role.Id;
            return View(model);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Edit(UserEditViewModel model, FormCollection collection)
        {
            try
            {
                User user = _userService.FindBy(x => x.Uuid == model.Uuid).First();
                user.FirstName = model.Name;
                user.LastName = model.Apellidos;
                user.Email = model.Email;
                user.MobileNumber = model.MobileNumber;
                user.Username = model.Username;
                user.Language = model.Language;
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
            var user = _userService.FindBy(e => e.Uuid == uuid).FirstOrDefault();
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
            var user = _userService.FindBy(e => e.Uuid == model.Uuid).FirstOrDefault();
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
                user.Password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                DateTime passwordExpiration = todayDate.AddDays(-1);
                user.PasswordExpiration = passwordExpiration;
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
                var user = _userService.FindBy(x => x.Uuid == uuid).First();
                if (user != null)
                {
                    user.Status = !user.Status;
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