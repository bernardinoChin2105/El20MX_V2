using MVC_Project.Data.Helpers;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Web.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
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
        [HttpGet, Authorize]
        public JsonResult ObtenerRoles(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var roles = _roleService.ObtenerRoles(filtros);
                IList<RoleData> UsuariosResponse = new List<RoleData>();
                foreach (var rol in roles)
                {
                    RoleData userData = new RoleData();
                    userData.Name = rol.Name;
                    userData.Description = rol.Description;
                    userData.CreatedAt = rol.CreatedAt;
                    userData.UpdatedAt = rol.UpdatedAt;
                    userData.Status = rol.Status;
                    userData.Uuid = rol.Uuid;
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
            var roleCreateViewModel = new RoleCreateViewModel { Permissions = PopulatePermissions()};
            return View(roleCreateViewModel);
        }

        private IEnumerable<PermissionViewModel> PopulatePermissions()
        {
            var permissions = _permissionService.GetAll();
            var permissionsVM = new List<PermissionViewModel>();
            permissionsVM = permissions.Select(permission => new PermissionViewModel
            {
                Id = permission.Id,
                Description = permission.Description
            }).ToList();
            return permissionsVM;
        }

        // POST: Role/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Role/Edit/5
        public ActionResult Edit(string uuid)
        {
            Role role = new Role();
            role = _roleService.FindBy(x => x.Uuid == uuid).First();
            RoleEditViewModel model = new RoleEditViewModel();
            model.Id = role.Id;
            model.Name = role.Name;
            IEnumerable<PermissionViewModel> permisos = PopulatePermissions();
            foreach (PermissionViewModel permiso in permisos)
            {
                var match = role.Permissions.FirstOrDefault(stringToCheck => stringToCheck.Description.Contains(permiso.Description));
                if (match!=null)
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
                Role role = _roleService.FindBy(x => x.Id == model.Id).First();
                IList<Permission> permissions = role.Permissions;
                IList<PermissionViewModel> permisosNuevos = model.Permissions.Where(x=>x.Assigned==true).ToList();
                var query = permisosNuevos.Where(p => !permissions.Any(l => p.Id == l.Id)); //permisosNuevos.Where(x => permissions.Contains(x.Id));
                var query2 = permissions.Where(p => !permisosNuevos.Any(l => p.Id == l.Id));
                foreach (PermissionViewModel permisoNuevo in query)
                {
                    RolePermission rolePermission = new RolePermission();
                    rolePermission.Role = new Role { Id = role.Id };
                    rolePermission.Permission = new Permission { Id = permisoNuevo.Id };
                    _rolePermissionService.Create(rolePermission);
                }
                foreach (Permission permisoQuitado in query2)
                {
                    RolePermission rolePermission = _rolePermissionService.FindBy(x=>x.Role.Id == role.Id && x.Permission.Id == permisoQuitado.Id).FirstOrDefault();
                     _rolePermissionService.Delete(rolePermission.Id);
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
                var rol = _roleService.FindBy(x => x.Uuid == uuid).First();
                if (rol != null)
                {
                    if (rol.Status == true)
                    {
                        rol.Status = false;
                    }
                    else
                    {
                        rol.Status = true;
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
