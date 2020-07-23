using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class ProviderController : Controller
    {
        private IAccountService _accountService;
        private ICustomerService _customerService;
        private ICustomerContactService _customerContactService;
        private IStateService _stateService;

        public ProviderController(IAccountService accountService, IStateService stateService)
        {
            _accountService = accountService;
            //_customerService = customerService;
            //_customerContactService = customerContactService;
            _stateService = stateService;
        }

        // GET: Provider
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
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
                    }).ToList();

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

        public ActionResult Edit(string uuid)
        {
            try
            {
                CustomerViewModel model = new CustomerViewModel();
                var userAuth = Authenticator.AuthenticatedUser;


                //var customer = _customerService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
                //if (customer == null)
                //    throw new Exception("El registro de Cliente no se encontró en la base de datos");

                //model.Id = customer.id;
                //model.FistName = customer.firstName;
                //model.LastName = customer.lastName;
                //model.RFC = customer.rfc;
                //model.CURP = customer.curp;
                //model.BusinessName = customer.businessName;
                //model.Street = customer.street;
                //model.OutdoorNumber = customer.outdoorNumber;
                //model.InteriorNumber = customer.interiorNumber;
                //model.ZipCode = customer.zipCode;
                //model.Colony = customer.colony;
                //model.Municipality = customer.municipality;
                //model.State = customer.state;
                //model.DeliveryAddress = customer.deliveryAddress;
                //if (customer.taxRegime != null)
                //    model.taxRegime = ((TypeTaxRegimen)Enum.Parse(typeof(TypeTaxRegimen), customer.taxRegime)).ToString();

                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                model.ListRegimen = new SelectList(regimenList);
                model.ListState = new SelectList(stateList);

                //var emails = customer.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                //            .Select(x => new CustomerContactsViewModel
                //            {
                //                Id = x.id,
                //                TypeContact = x.typeContact,
                //                EmailOrPhone = x.emailOrPhone
                //            }).ToList();

                //if (emails.Count() > 0)
                //{
                //    model.Emails = emails;
                //}

                //var phones = customer.customerContacts.Where(x => x.typeContact == TypeContact.PHONE.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                //            .Select(x => new CustomerContactsViewModel
                //            {
                //                Id = x.id,
                //                TypeContact = x.typeContact,
                //                EmailOrPhone = x.emailOrPhone
                //            }).ToList();
                //if (phones.Count() > 0)
                //{
                //    model.Phones = phones;
                //}

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetProviders(JQueryDataTableParams param, string filtros, bool first)
        {
            try
            {
                int totalDisplay = 0;
                int total = 0;
                var listResponse = new List<CustomerList>();
                if (!first)
                {
                    var userAuth = Authenticator.AuthenticatedUser;
                    //NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    //string rfc = filtersValues.Get("RFC").Trim();
                    //string businessName = filtersValues.Get("BusinessName").Trim();
                    //string email = filtersValues.Get("Email").Trim();

                    //var pagination = new BasePagination();
                    //var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                    //pagination.PageSize = param.iDisplayLength;
                    //pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                    //if (rfc != "") filters.rfc = rfc;
                    //if (businessName != "") filters.businessName = businessName;
                    //if (email != "") filters.email = email;

                    listResponse = new List<CustomerList>(); //_customerService.CustomerList(pagination, filters);

                    //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                    //if (listResponse.Count() > 0)
                    //{
                    //    totalDisplay = listResponse[0].Total;
                    //    total = listResponse.Count();
                    //}
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
                    Data =
                    new
                    {
                        success = true,
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<CustomerList>()
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
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
    }
}