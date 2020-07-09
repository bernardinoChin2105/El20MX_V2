using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class CustomerController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        //[HttpGet, AllowAnonymous]
        //public JsonResult ObtenerDiagnostic(JQueryDataTableParams param, string filtros)
        //{
        //    try
        //    {
        //        var userAuth = Authenticator.AuthenticatedUser;
        //        NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
        //        string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
        //        string FilterEnd = filtersValues.Get("FilterEndDate").Trim();

        //        var pagination = new BasePagination();
        //        pagination.PageSize = param.iDisplayLength;
        //        pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
        //        if (FilterStart != "") pagination.CreatedOnStart = Convert.ToDateTime(FilterStart);
        //        if (FilterEnd != "") pagination.CreatedOnEnd = Convert.ToDateTime(FilterEnd);

        //        var DiagnosticsResponse = _diagnosticService.DiagnosticList(userAuth.Account.Uuid.ToString(), pagination);

        //        //Corroborar los campos iTotalRecords y iTotalDisplayRecords
        //        int totalDisplay = 0;
        //        int total = 0;
        //        if (DiagnosticsResponse.Count() > 0)
        //        {
        //            totalDisplay = DiagnosticsResponse[0].Total;
        //            total = DiagnosticsResponse.Count();
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            sEcho = param.sEcho,
        //            iTotalRecords = total,
        //            iTotalDisplayRecords = totalDisplay,
        //            aaData = DiagnosticsResponse
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

        //public ActionResult Create()
        //{
        //    try
        //    {
        //        var roleCreateViewModel = new RoleCreateViewModel { Modules = PopulateModules() };
        //        return View(roleCreateViewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
        //        return RedirectToAction("Index");
        //    }
        //}

        //// POST: Role/Create
        ////[HttpPost]
        //[Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        //public ActionResult Create(RoleCreateViewModel roleCreateViewModel)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            throw new Exception("El modelo de entrada no es válido");

        //        var code = FormatUtil.ReplaceSpecialCharactersAndWhiteSpace(roleCreateViewModel.Name);
        //        var userAuth = Authenticator.AuthenticatedUser;

        //        if (_roleService.FindBy(x => x.code == code && x.account.id == userAuth.Account.Id).Any())
        //            throw new Exception("Ya existe un rol con el nombre proporcionado");

        //        DateTime todayDate = DateUtil.GetDateTimeNow();
        //        var role = new Role
        //        {
        //            uuid = Guid.NewGuid(),
        //            name = roleCreateViewModel.Name,
        //            code = code,
        //            description = roleCreateViewModel.Name,
        //            createdAt = todayDate,
        //            modifiedAt = todayDate,
        //            status = SystemStatus.ACTIVE.ToString(),
        //            account = new Account { id = userAuth.Account.Id }
        //        };

        //        List<RolePermission> rolesPermissions = new List<RolePermission>();
        //        foreach (var modules in roleCreateViewModel.Modules)
        //        {
        //            foreach (PermissionViewModel permisoNuevo in modules.Permissions)
        //            {
        //                RolePermission rolePermission = new RolePermission();
        //                rolePermission.role = role;
        //                rolePermission.permission = new Permission { id = permisoNuevo.Id };
        //                rolePermission.level = ((SystemLevelPermission)permisoNuevo.SystemAction).ToString();
        //                rolePermission.account = new Account { id = userAuth.Account.Id };
        //                rolesPermissions.Add(rolePermission);
        //            }
        //        }

        //        _roleService.CreateRole(role, rolesPermissions);
        //        MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
        //        return RedirectToAction("Index");

        //    }
        //    catch (Exception ex)
        //    {
        //        MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
        //        roleCreateViewModel.Modules = PopulateModules();
        //        return View(roleCreateViewModel);
        //    }
        //}

        //// GET: Role/Edit/5
        //public ActionResult Edit(string uuid)
        //{
        //    try
        //    {
        //        Role role = new Role();
        //        var userAuth = Authenticator.AuthenticatedUser;

        //        role = _roleService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
        //        if (role == null)
        //            throw new Exception("El rol no se encontró en la base de datos");

        //        RoleEditViewModel model = new RoleEditViewModel();
        //        model.Id = role.id;
        //        model.Name = role.name;
        //        model.Code = role.code;

        //        model.Modules = PopulateModulesEdit(role);
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
        //        return RedirectToAction("Index");
        //    }
        //}

        //// POST: Role/Edit/5
        //[HttpPost]
        //public ActionResult Edit(RoleEditViewModel model)
        //{
        //    var userAuth = Authenticator.AuthenticatedUser;
        //    Role role = _roleService.FirstOrDefault(x => x.id == model.Id && x.account.id == userAuth.Account.Id);
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            throw new Exception("El modelo de entrada no es válido");

        //        IList<RolePermission> rolePermissions = role.rolePermissions;

        //        List<PermissionViewModel> permissionsViewModels = new List<PermissionViewModel>();
        //        foreach (var module in model.Modules)
        //            permissionsViewModels.AddRange(module.Permissions);

        //        var news = permissionsViewModels.Where(pvm => !rolePermissions.Any(rp => pvm.Id == rp.permission.id));
        //        var newRolePermissions = news.Select(x => new RolePermission
        //        {
        //            role = new Role { id = role.id },
        //            permission = new Permission { id = x.Id },
        //            level = ((SystemLevelPermission)x.SystemAction).ToString()
        //        });

        //        var updates = from rp in rolePermissions
        //                      join pvm in permissionsViewModels on rp.permission.id equals pvm.Id
        //                      where rp.level != ((SystemLevelPermission)pvm.SystemAction).ToString()
        //                      select new { rolePermission = rp, level = ((SystemLevelPermission)pvm.SystemAction).ToString() };

        //        var updateRolePermissions = new List<RolePermission>();
        //        foreach (var u in updates)
        //        {
        //            var permision = u.rolePermission;
        //            permision.level = u.level;
        //            updateRolePermissions.Add(permision);
        //        }

        //        var oldRolePermissions = rolePermissions.Where(rp => !permissionsViewModels.Any(pvm => rp.permission.id == pvm.Id));

        //        _rolePermissionService.UpdateRolePermissions(newRolePermissions, updateRolePermissions, oldRolePermissions);
        //        MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
        //        model.Modules = PopulateModulesEdit(role);
        //        return View(model);
        //    }
        //}

        //// GET: Role/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Role/Delete/5
        //[HttpPost]
        //public ActionResult Delete(string uuid, FormCollection collection)
        //{
        //    try
        //    {
        //        var rol = _roleService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
        //        if (rol != null)
        //        {
        //            if (rol.status == SystemStatus.ACTIVE.ToString())
        //            {
        //                rol.status = SystemStatus.INACTIVE.ToString();
        //            }
        //            else
        //            {
        //                rol.status = SystemStatus.ACTIVE.ToString();
        //            }

        //        }
        //        _roleService.Update(rol);
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //}
    }
}