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
    public class UserPermissionsController : BaseController
    {
        private RoleService _roleService;
        private PermissionService _permissionService;
        private UserService _userService;

        public UserPermissionsController(RoleService roleService, PermissionService permissionService, UserService userService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _userService = userService;
        }

        // GET: UserPermissions
        public ActionResult Index()
        {
            List<SelectListItem> listStatus = new List<SelectListItem>();
            listStatus.Add(new SelectListItem
            {
                Text = "Todos",
                Value = "2"
            });
            listStatus.Add(new SelectListItem
            {
                Text = "Activos",
                Value = "1",
                Selected = true
            });
            listStatus.Add(new SelectListItem
            {
                Text = "Inactivos",
                Value = "0"
            });
            ViewBag.OpcionesStatus = listStatus;
            return View();
        }

        // GET: UserPermissions/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserPermissions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserPermissions/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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

        // GET: UserPermissions/Edit/5
        public ActionResult Edit(string uuid)
        {
            User user = new User();
            Role role = new Role();
            user = _userService.FindBy(x => x.Uuid == uuid).First();
            role = _roleService.FindBy(x => x.Id == user.Role.Id).First();
            RoleEditViewModel model = new RoleEditViewModel();
            model.Name = role.Name;
            IEnumerable<PermissionViewModel> permisos = PopulatePermissions();
            foreach (PermissionViewModel permiso in permisos)
            {
                if (role.Permissions.Select(x => x.Description.Equals(permiso.Description)).First())
                {
                    permiso.Assigned = true;
                }
            }
            model.Permissions = permisos;
            return View(model);
        }

        // POST: UserPermissions/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: UserPermissions/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserPermissions/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //[HttpGet, Authorize]
        //public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        //{
        //    try
        //    {
        //        var users = _userService.FilterBy(filtros, param.iDisplayStart, param.iDisplayLength);
        //        IList<UserPermissionData> UsuariosResponse = new List<UserPermissionData>();
        //        foreach (var user in users)
        //        {
        //            UserPermissionData userData = new UserPermissionData();
        //            userData.Name = user.FirstName + " " + user.LastName;
        //            userData.Email = user.Email;
        //            userData.CreatedAt = user.CreatedAt;
        //            userData.UpdatedAt = user.UpdatedAt;
        //            userData.Status = user.Status;
        //            userData.Uuid = user.Uuid;
        //            UsuariosResponse.Add(userData);
        //        }
        //        return Json(new
        //        {
        //            success = true,
        //            param.sEcho,
        //            iTotalRecords = UsuariosResponse.Count(),
        //            iTotalDisplayRecords = 10,
        //            aaData = UsuariosResponse
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult
        //        {
        //            Data = new { Mensaje = new { title = "Error", message = ex.Message } },
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //            MaxJsonLength = Int32.MaxValue
        //        };
        //    }
        //}
    }
}