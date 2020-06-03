using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVC_Project.WebBackend.Models;
using MVC_Project.Utils;
using System.Configuration;

namespace MVC_Project.WebBackend.Controllers
{
    public class RoleController : BaseController
    {
        private IRoleService _roleService;
        private IPermissionService _permissionService;
        private IRolePermissionService _rolePermissionService;

        public RoleController(IRoleService roleService, IPermissionService permissionService, IRolePermissionService rolePermissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _rolePermissionService = rolePermissionService;
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
                var roles = _roleService.ObtenerRoles(filtros);
                IList<RoleData> UsuariosResponse = new List<RoleData>();
                foreach (var rol in roles)
                {
                    RoleData userData = new RoleData();
                    userData.Name = rol.name;
                    userData.Description = rol.description;
                    userData.CreatedAt = rol.createdAt;
                    userData.UpdatedAt = rol.modifiedAt;
                    userData.Status = rol.status==SystemStatus.ACTIVE.ToString();
                    userData.Uuid = rol.uuid.ToString();
                    UsuariosResponse.Add(userData);
                }
                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = UsuariosResponse.Count(),
                    iTotalDisplayRecords = 10,
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
            var roleCreateViewModel = new RoleCreateViewModel { Permissions = PopulatePermissions() };
            return View(roleCreateViewModel);
        }

        private IEnumerable<PermissionViewModel> PopulatePermissions()
        {
            var permissions = _permissionService.GetAll();
            var permissionsVM = new List<PermissionViewModel>();
            permissionsVM = permissions.Select(permission => new PermissionViewModel
            {
                Id = permission.id,
                Description = permission.description
            }).ToList();
            return permissionsVM;
        }

        // POST: Role/Create
        //[HttpPost]
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(RoleCreateViewModel roleCreateViewModel)
        {          
            if (ModelState.IsValid)
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();      
                var role = new Role
                {
                    uuid = Guid.NewGuid(),
                    name = roleCreateViewModel.Name,
                    code = roleCreateViewModel.Code,
                    description = roleCreateViewModel.Name,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString()
                };
                _roleService.Create(role);

                IList<PermissionViewModel> permisosNuevos = roleCreateViewModel.Permissions.Where(x => x.Assigned == true).ToList();                
                foreach (PermissionViewModel permisoNuevo in permisosNuevos)
                {
                    RolePermission rolePermission = new RolePermission();
                    rolePermission.role = new Role { id = role.id };
                    rolePermission.permission = new Permission { id = permisoNuevo.Id };
                    _rolePermissionService.Create(rolePermission);
                }
               
                return RedirectToAction("Index");
            }
            else
            {
                roleCreateViewModel = new RoleCreateViewModel { Permissions = PopulatePermissions() };
                return View("Create", roleCreateViewModel);
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(string uuid)
        {
            Role role = new Role();
            role = _roleService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
            RoleEditViewModel model = new RoleEditViewModel();
            model.Id = role.id;
            model.Name = role.name;
            model.Code = role.code;
            IEnumerable<PermissionViewModel> permisos = PopulatePermissions();
            foreach (PermissionViewModel permiso in permisos)
            {
                var match = role.permissions.FirstOrDefault(stringToCheck => stringToCheck.description.Contains(permiso.Description));
                if (match != null)
                {
                    permiso.Assigned = true;
                }
            }
            model.Permissions = permisos;
            return View(model);
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(RoleEditViewModel model)
        {
            try
            {
                Role role = _roleService.FindBy(x => x.id == model.Id).First();
                //role.Name = model.Name;
                //role.Code = model.Code;
                //_roleService.Update(role);

                IList<Permission> permissions = role.permissions;
                IList<PermissionViewModel> permisosNuevos = model.Permissions.Where(x => x.Assigned == true).ToList();
                var query = permisosNuevos.Where(p => !permissions.Any(l => p.Id == l.id)); //permisosNuevos.Where(x => permissions.Contains(x.Id));
                var query2 = permissions.Where(p => !permisosNuevos.Any(l => p.id == l.Id));
                foreach (PermissionViewModel permisoNuevo in query)
                {
                    RolePermission rolePermission = new RolePermission();
                    rolePermission.role = new Role { id = role.id };
                    rolePermission.permission = new Permission { id = permisoNuevo.Id };
                    _rolePermissionService.Create(rolePermission);
                }
                foreach (Permission permisoQuitado in query2)
                {
                    RolePermission rolePermission = _rolePermissionService.FindBy(x => x.role.id == role.id && x.permission.id == permisoQuitado.id).FirstOrDefault();
                    _rolePermissionService.Delete(rolePermission.id);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
