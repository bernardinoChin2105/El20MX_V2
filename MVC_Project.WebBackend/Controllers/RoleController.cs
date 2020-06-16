using MVC_Project.Domain.Entities;
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
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                IList<RoleData> UsuariosResponse = new List<RoleData>();
                int totalDisplay = 0;
                if (userAuth.Account != null)
                {
                    var roles = _roleService.FilterBy(filtros, userAuth.Account.Id, param.iDisplayStart, param.iDisplayLength);
                    totalDisplay = roles.Item2;
                    foreach (var rol in roles.Item1)
                    {
                        RoleData userData = new RoleData();
                        userData.Name = rol.name;
                        userData.Description = rol.description;
                        userData.CreatedAt = rol.createdAt;
                        userData.UpdatedAt = rol.modifiedAt;
                        userData.Status = rol.status == SystemStatus.ACTIVE.ToString();
                        userData.Uuid = rol.uuid.ToString();
                        UsuariosResponse.Add(userData);
                    }
                }
                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = UsuariosResponse.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = UsuariosResponse
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

        // GET: Role/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Role/Create
        public ActionResult Create()
        {
            try
            {
                var roleCreateViewModel = new RoleCreateViewModel { Modules = PopulateModules() };
                return View(roleCreateViewModel);
            }
            catch(Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        private List<FeatureViewModel> PopulateFeatures()
        {
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
            var permissions = _permissionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString()).ToList();
            var permissionsVM = new List<ModuleViewModel>();
            permissionsVM = permissions.GroupBy(x => x.module).Select(g => new ModuleViewModel
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
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var code = FormatUtil.ReplaceSpecialCharactersAndWhiteSpace(roleCreateViewModel.Name);
                var userAuth = Authenticator.AuthenticatedUser;

                if (_roleService.FindBy(x => x.code == code && x.account.id == userAuth.Account.Id).Any())
                    throw new Exception("Ya existe un rol con el nombre proporcionado");

                DateTime todayDate = DateUtil.GetDateTimeNow();
                var role = new Role
                {
                    uuid = Guid.NewGuid(),
                    name = roleCreateViewModel.Name,
                    code = code,
                    description = roleCreateViewModel.Name,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString(),
                    account = new Account { id = userAuth.Account.Id }
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
                        rolePermission.account = new Account { id = userAuth.Account.Id };
                        rolesPermissions.Add(rolePermission);
                    }
                }

                _roleService.CreateRole(role, rolesPermissions);
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                roleCreateViewModel.Modules = PopulateModules();
                return View(roleCreateViewModel);
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(string uuid)
        {
            try
            {
                Role role = new Role();
                var userAuth = Authenticator.AuthenticatedUser;

                role = _roleService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
                if (role == null)
                    throw new Exception("El rol no se encontró en la base de datos");

                RoleEditViewModel model = new RoleEditViewModel();
                model.Id = role.id;
                model.Name = role.name;
                model.Code = role.code;
                
                model.Modules = PopulateModulesEdit(role);
                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
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
                    var match = role.rolePermissions.FirstOrDefault(x => x.permission.id == permission.Id && x.account.id == userAuth.Account.Id);
                    if (match != null)
                    {
                        int level = (int)(Enum.Parse(typeof(SystemLevelPermission), match.level));
                        permission.SystemAction = level;
                        var action = permission.SystemActions.First(x => x.Value == level.ToString());
                        action.Selected = true;
                    }
                }
            }

            return modules;
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(RoleEditViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            Role role = _roleService.FirstOrDefault(x => x.id == model.Id && x.account.id == userAuth.Account.Id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");
                
                IList<RolePermission> rolePermissions = role.rolePermissions;

                List<PermissionViewModel> permissionsViewModels = new List<PermissionViewModel>();
                foreach (var module in model.Modules)
                    permissionsViewModels.AddRange(module.Permissions);

                var news = permissionsViewModels.Where(pvm => !rolePermissions.Any(rp => pvm.Id == rp.permission.id));
                var newRolePermissions = news.Select(x => new RolePermission
                {
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
                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
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
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
