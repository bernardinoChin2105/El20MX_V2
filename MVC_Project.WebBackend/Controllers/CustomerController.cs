using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class CustomerController : Controller
    {
        private IAccountService _accountService;
        private ICustomerService _customerService;
        private IStateService _stateService;

        public CustomerController(IAccountService accountService, ICustomerService customerService, IStateService stateService)
        {
            _accountService = accountService;
            _customerService = customerService;
            _stateService = stateService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCustomers(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string rfc = filtersValues.Get("RFC").Trim();
                string businessName = filtersValues.Get("BusinessName").Trim();
                string email = filtersValues.Get("Email").Trim();

                var pagination = new BasePagination();
                var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (rfc != "") filters.rfc = rfc;
                if (businessName != "") filters.businessName = businessName;
                if (email != "") filters.email = email;

                var listResponse = _customerService.CustomerList(pagination, filters);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                int totalDisplay = 0;
                int total = 0;
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResponse
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            try
            {
                var createCustomer = new CustomerViewModel(); 
                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    });

                createCustomer.ListRegimen = new SelectList(regimenList);
                createCustomer.ListState = new SelectList(stateList);
                return View(createCustomer);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
            // return View();
        }

        // POST: Customer/Create
        //[HttpPost]
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(CustomerViewModel model)
        {
            MensajeFlashHandler.RegistrarMensaje("¡Registro de Cliente realizado!", TiposMensaje.Success);
            //try
            //{
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var authUser = Authenticator.AuthenticatedUser;

                if (_customerService.FindBy(x => x.rfc == model.RFC && x.account.id == authUser.Account.Id).Any())
                    throw new Exception("Ya existe un Cliente con el RFC proporcionado");
                
                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();

            //    DateTime todayDate = DateUtil.GetDateTimeNow();
            //    Customer customer = new Customer()
            //    {

            //        public virtual Int64 id { get; set; }
            //        public virtual Guid uuid { get; set; }
            //        public virtual Account account { get; set; }
            //        public virtual string firstName { get; set; }
            //        public virtual string lastName { get; set; }
            //        public virtual string rfc { get; set; }
            //        public virtual string curp { get; set; }
            //        public virtual string businessName { get; set; }
            //        public virtual string taxRegime { get; set; }
            //        public virtual string street { get; set; }
            //        public virtual string interiorNumber { get; set; }
            //        public virtual string outdoorNumber { get; set; }
            //        public virtual Int64 colony { get; set; }
            //        public virtual string zipCode { get; set; }
            //        public virtual Int64 municipality { get; set; }
            //        public virtual Int64 state { get; set; }
            //        public virtual bool deliveryAddress { get; set; }
            //        public virtual DateTime createdAt { get; set; }
            //        public virtual DateTime modifiedAt { get; set; }
            //        public virtual string status { get; set; }
            //    };

            //    var role = new Role
            //    {
            //        uuid = Guid.NewGuid(),
            //        name = roleCreateViewModel.Name,
            //        code = code,
            //        description = roleCreateViewModel.Name,
            //        createdAt = todayDate,
            //        modifiedAt = todayDate,
            //        status = SystemStatus.ACTIVE.ToString(),
            //        account = new Account { id = userAuth.Account.Id }
            //    };

            //    /*public string FistName { get; set; }
            //    public string LastName { get; set; }
            //    public string RFC { get; set; }
            //    public string CURP { get; set; }
            //    public string BusinessName { get; set; }
            //    public string taxRegime { get; set; }
            //    public string Street { get; set; }        
            //    public string OutdoorNumber { get; set; }        
            //    public string InteriorNumber { get; set; }
            //    public int? Colony { get; set; }
            //    public string ZipCode { get; set; }
            //    public int? Municipality { get; set; }
            //    public int? State { get; set; }        
            //    public bool DeliveryAddress { get; set; }

            //    public List<CustomerContact> Emails { get; set; }
            //    public List<CustomerContact> Phones { get; set; }*/



            //    List<RolePermission> rolesPermissions = new List<RolePermission>();
            //    foreach (var modules in roleCreateViewModel.Modules)
            //    {
            //        foreach (PermissionViewModel permisoNuevo in modules.Permissions)
            //        {
            //            RolePermission rolePermission = new RolePermission();
            //            rolePermission.role = role;
            //            rolePermission.permission = new Permission { id = permisoNuevo.Id };
            //            rolePermission.level = ((SystemLevelPermission)permisoNuevo.SystemAction).ToString();
            //            rolePermission.account = new Account { id = userAuth.Account.Id };
            //            rolesPermissions.Add(rolePermission);
            //        }
            //    }

            //    _roleService.CreateRole(role, rolesPermissions);
            //    MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
            //    return RedirectToAction("Index");

            //}
            //catch (Exception ex)
            //{
            //    MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
            //    roleCreateViewModel.Modules = PopulateModules();
            //    return View(roleCreateViewModel);
            //}
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetLocations(string zipCode)
        {
            try
            {
                var listResponse = _stateService.GetLocationList(zipCode);

                return Json(new
                {
                    Data = new { success = true, data = listResponse },
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(string uuid)
        {
            try
            {
                CustomerViewModel model = new CustomerViewModel();
                //        var userAuth = Authenticator.AuthenticatedUser;

                //        role = _roleService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
                //        if (role == null)
                //            throw new Exception("El rol no se encontró en la base de datos");

                //        RoleEditViewModel model = new RoleEditViewModel();
                //        model.Id = role.id;
                //        model.Name = role.name;
                //        model.Code = role.code;

                //        model.Modules = PopulateModulesEdit(role);
                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(CustomerViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            CustomerViewModel customer = new CustomerViewModel(); //_customerService.FirstOrDefault(x => x.id == model.Id && x.account.id == userAuth.Account.Id);
            try
            {
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                //        model.Modules = PopulateModulesEdit(role);
                return View(model);
            }
        }

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