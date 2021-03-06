﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVC_Project.WebBackend.Models;
using MVC_Project.Utils;
using System.Configuration;
using MVC_Project.WebBackend.AuthManagement;
using System.Web;
using System.Collections.Specialized;
using MVC_Project.FlashMessages;
using LogHubSDK.Models;
using Newtonsoft.Json;

namespace MVC_Project.WebBackend.Controllers
{
    public class RoleController : BaseController
    {
        private IRoleService _roleService;
        private IPermissionService _permissionService;
        private IRolePermissionService _rolePermissionService;
        private IFeatureService _featureService;

        public RoleController(IRoleService roleService, IPermissionService permissionService, IRolePermissionService rolePermissionService,
            IFeatureService featureService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _rolePermissionService = rolePermissionService;
            _featureService = featureService;
        }

        // GET: Role
        public ActionResult Index()
        {
            RoleViewModel roles = new RoleViewModel();
            return View(roles);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult ObtenerRoles(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                IList<RoleData> roleResponse = new List<RoleData>();
                int totalDisplay = 0;
                
                Int64? accountId = userAuth.GetAccountId();

                var roles = _roleService.FilterBy(filtros, accountId, param.iDisplayStart, param.iDisplayLength);
                    totalDisplay = roles.Item2;
                foreach (var rol in roles.Item1)
                {
                    RoleData roleData = new RoleData();
                    roleData.Name = rol.name;
                    roleData.Description = rol.description;
                    roleData.CreatedAt = rol.createdAt;
                    roleData.UpdatedAt = rol.modifiedAt;
                    roleData.Status = rol.status == SystemStatus.ACTIVE.ToString();
                    roleData.Uuid = rol.uuid.ToString();
                    roleResponse.Add(roleData);
                }
                
                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = roleResponse.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = roleResponse
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

        // GET: Role/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Role/Create
        public ActionResult Create()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var roleCreateViewModel = new RoleCreateViewModel { Modules = PopulateModules() };
                return View(roleCreateViewModel);
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

        private List<FeatureViewModel> PopulateFeatures()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var features = _featureService.GetAll();
            var featuresViewModel = new List<FeatureViewModel>();
            var values = Enum.GetValues(typeof(SystemLevelPermission));
            var systemaActions = new List<SelectListItem>();
            foreach (int i in values)
            {
                systemaActions.Add(new SelectListItem
                {
                    Text = ((SystemLevelPermission)i).GetDisplayName(),
                    Value = i.ToString()
                });
            }
            foreach (var feature in features)
            {
                featuresViewModel.Add(new FeatureViewModel
                {
                    Id = feature.id,
                    Name = feature.name,
                    Permissions = feature.permissions.Select(x => new PermissionViewModel
                    {
                        Id = x.id,
                        Name = x.description,
                        SystemActions = systemaActions
                    }).ToList()
                });
            }
            
            return featuresViewModel;
        }

        private List<PermissionViewModel> PopulatePermissions()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var permissions = _permissionService.GetAll();
            var permissionsVM = new List<PermissionViewModel>();
            permissionsVM = permissions.Select(permission => new PermissionViewModel
            {
                Id = permission.id,
                Name = permission.description
            }).ToList();
            
            return permissionsVM;
        }

        private List<ModuleViewModel> PopulateModules()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            List<Permission> pp = new List<Permission>();
            if (userAuth.isBackOffice && userAuth.Account == null)
                pp = _permissionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString() 
                && x.isCustomizable).ToList();
            else
                pp = _permissionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString() 
                && x.isCustomizable && x.applyTo != SystemPermissionApply.ONLY_BACK_OFFICE.ToString()).ToList();
            
            var permissionsVM = new List<ModuleViewModel>();
            permissionsVM = pp.GroupBy(x => x.module).Select(g => new ModuleViewModel
            {
                Id = (int)Enum.Parse(typeof(SystemModules), g.Key),
                Name = ((SystemModules)Enum.Parse(typeof(SystemModules), g.Key)).GetDisplayName(),
                Permissions = g.Select(s => new PermissionViewModel
                {
                    Id = s.id,
                    Name = s.description,
                    SystemActions = PopulateActions()
                }).ToList()
            }).ToList();
            
            return permissionsVM;
        }

        public List<SelectListItem> PopulateActions()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var values = Enum.GetValues(typeof(SystemLevelPermission));
            var systemaActions = new List<SelectListItem>();
            foreach (int i in values)
            {
                systemaActions.Add(new SelectListItem
                {
                    Text = ((SystemLevelPermission)i).GetDisplayName(),
                    Value = i.ToString()
                });
            }

            return systemaActions;
        }

        // POST: Role/Create
        //[HttpPost]
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(RoleCreateViewModel roleCreateViewModel)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var code = FormatUtil.ReplaceSpecialCharactersAndWhiteSpace(roleCreateViewModel.Name);
                if (userAuth.isBackOfficeConfiguration())
                {
                    if (_roleService.FindBy(x => x.code == code && x.account == null).Any())
                        throw new Exception("Ya existe un rol con el nombre proporcionado");
                }
                else
                {
                    if (_roleService.FindBy(x => x.code == code && x.account.id == userAuth.Account.Id).Any())
                        throw new Exception("Ya existe un rol con el nombre proporcionado");
                }

                DateTime todayDate = DateUtil.GetDateTimeNow();
                Account account = userAuth.isBackOfficeConfiguration() ? null : new Account { id = userAuth.Account.Id };
                var role = new Role
                {
                    uuid = Guid.NewGuid(),
                    name = roleCreateViewModel.Name,
                    code = code,
                    description = roleCreateViewModel.Name,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString(),
                    account = account
                };

                List<RolePermission> rolesPermissions = new List<RolePermission>();
                foreach (var modules in roleCreateViewModel.Modules)
                {
                    foreach (PermissionViewModel permisoNuevo in modules.Permissions)
                    {
                        RolePermission rolePermission = new RolePermission();
                        rolePermission.role = role;
                        rolePermission.permission = new Permission { id = permisoNuevo.Id };
                        rolePermission.level = ((SystemLevelPermission)permisoNuevo.SystemAction).ToString();
                        rolePermission.account = account;
                        rolesPermissions.Add(rolePermission);
                    }
                }

                var noCustomizables = _permissionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString() && !x.isCustomizable).ToList();
                foreach (var noCustomizable in noCustomizables)
                {
                    RolePermission rolePermission = new RolePermission();
                    rolePermission.role = role;
                    rolePermission.permission = new Permission { id = noCustomizable.id };
                    rolePermission.level = SystemLevelPermission.FULL_ACCESS.ToString();
                    rolePermission.account = account;
                    rolesPermissions.Add(rolePermission);
                }
                _roleService.CreateRole(role, rolesPermissions);

                LogUtil.AddEntry(
                   "Crea rol: " + JsonConvert.SerializeObject(role) + ", rolespermisos: " + JsonConvert.SerializeObject(rolesPermissions),
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
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
                roleCreateViewModel.Modules = PopulateModules();
                return View(roleCreateViewModel);
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                Role role = _roleService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));

                if (role == null)
                    throw new Exception("El rol no se encontró en la base de datos");

                RoleEditViewModel model = new RoleEditViewModel();
                model.Id = role.id;
                model.Name = role.name;
                model.Code = role.code;

                model.Modules = PopulateModulesEdit(role);

                LogUtil.AddEntry(
                   "Modelo de edicion: " + JsonConvert.SerializeObject(model),
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

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

        private List<ModuleViewModel> PopulateModulesEdit(Role role)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            List<ModuleViewModel> modules = PopulateModules();

            foreach (var module in modules)
            {
                foreach (var permission in module.Permissions)
                {
                    RolePermission  match = role.rolePermissions.FirstOrDefault(x => x.permission.id == permission.Id);
                    if (match != null)
                    {
                        int level = (int)(Enum.Parse(typeof(SystemLevelPermission), match.level));
                        permission.SystemAction = level;
                        var action = permission.SystemActions.First(x => x.Value == level.ToString());
                        action.Selected = true;
                    }
                }
            }
            LogUtil.AddEntry(
               "Edicion de modulos populares: " + JsonConvert.SerializeObject(modules),
               ENivelLog.Info,
               userAuth.Id,
               userAuth.Email,
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
            );

            return modules;
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(RoleEditViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            Role role = _roleService.FirstOrDefault(x => x.id == model.Id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                IList<RolePermission> rolePermissions = role.rolePermissions.Where(x => x.permission.isCustomizable).ToList();

                List<PermissionViewModel> permissionsViewModels = new List<PermissionViewModel>();
                foreach (var module in model.Modules)
                    permissionsViewModels.AddRange(module.Permissions);

                var news = permissionsViewModels.Where(pvm => !rolePermissions.Any(rp => pvm.Id == rp.permission.id));

                Account account = userAuth.isBackOfficeConfiguration() ? null : new Account { id = userAuth.Account.Id };

                var newRolePermissions = news.Select(x => new RolePermission
                {
                    account = account,
                    role = new Role { id = role.id },
                    permission = new Permission { id = x.Id },
                    level = ((SystemLevelPermission)x.SystemAction).ToString()
                });

                var updates = from rp in rolePermissions
                              join pvm in permissionsViewModels on rp.permission.id equals pvm.Id
                              where rp.level != ((SystemLevelPermission)pvm.SystemAction).ToString()
                              select new { rolePermission = rp, level = ((SystemLevelPermission)pvm.SystemAction).ToString() };

                var updateRolePermissions = new List<RolePermission>();
                foreach (var u in updates)
                {
                    var permision = u.rolePermission;
                    permision.level = u.level;
                    updateRolePermissions.Add(permision);
                }

                var oldRolePermissions = rolePermissions.Where(rp => !permissionsViewModels.Any(pvm => rp.permission.id == pvm.Id));

                _rolePermissionService.UpdateRolePermissions(newRolePermissions, updateRolePermissions, oldRolePermissions);

                LogUtil.AddEntry(
                   "Edicion de rol " + role.name,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   "Nuevos permisos: " + JsonConvert.SerializeObject(newRolePermissions)
                   + ", actualizacion de permisos: " + JsonConvert.SerializeObject(updateRolePermissions)
                   + ", antiguos permisos: " + JsonConvert.SerializeObject(oldRolePermissions)
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
                model.Modules = PopulateModulesEdit(role);
                return View(model);
            }
        }

        // GET: Role/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Role/Delete/5
        [HttpPost]
        public ActionResult Delete(string uuid, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var rol = _roleService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (rol != null)
                {
                    if (rol.status == SystemStatus.ACTIVE.ToString())
                    {
                        rol.status = SystemStatus.INACTIVE.ToString();
                    }
                    else
                    {
                        rol.status = SystemStatus.ACTIVE.ToString();
                    }

                }
                _roleService.Update(rol);

                LogUtil.AddEntry(
                   "Eliminación de rol: " + JsonConvert.SerializeObject(rol),
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
