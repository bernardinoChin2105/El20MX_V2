using LogHubSDK.Models;
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
    public class PlansController : Controller
    {
        private IPlanService _planService;

        public PlansController(IPlanService planeService)
        {
            _planService = planeService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPlans(JQueryDataTableParams param, string filtros, bool first)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            bool success = true;
            string message = string.Empty;
            var listResponse = new List<PlansViewModel>();

            try
            {
                if (!first)
                {
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);                    
                    string Name = filtersValues.Get("Name").Trim();

                    var pagination = new BasePagination();                   
                    pagination.PageSize = param.iDisplayLength;
                    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                    //if (Name != "") filters.businessName = businessName;

                    listResponse = _planService.GetPlans(pagination, Name);

                    //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                    if (listResponse.Count() > 0)
                    {
                        totalDisplay = listResponse[0].Total;
                        total = listResponse.Count();
                    }
                }

                LogUtil.AddEntry(
                   "Lista de clientes totalDisplay: " + totalDisplay + ", total: " + total,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message.ToString();
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
            }

            return Json(new
            {
                success = success,
                message = message,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                //var createCustomer = new CustomerViewModel();
                //var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                //stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                //var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                //    .Select(e => new SelectListItem
                //    {
                //        Value = e.ToString(),
                //        Text = EnumUtils.GetDisplayName(e)
                //    }).ToList();

                //createCustomer.ListRegimen = new SelectList(regimenList);
                //createCustomer.ListState = new SelectList(stateList);

                //LogUtil.AddEntry(
                //   "Crea nuevo cliente: " + JsonConvert.SerializeObject(createCustomer),
                //   ENivelLog.Info,
                //   userAuth.Id,
                //   userAuth.Email,
                //   EOperacionLog.ACCESS,
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                //);

                return View();
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                //LogUtil.AddEntry(
                //   "Se encontro un error: " + ex.Message.ToString(),
                //   ENivelLog.Error,
                //   userAuth.Id,
                //   userAuth.Email,
                //   EOperacionLog.ACCESS,
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                //);
                return RedirectToAction("Index");
            }
            // return View();
        }

        //public ActionResult Edit(string uuid)
        //{
        //    var userAuth = Authenticator.AuthenticatedUser;
        //    try
        //    {
        //        CustomerViewModel model = new CustomerViewModel();

        //        var customer = _customerService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
        //        if (customer == null)
        //            throw new Exception("El registro de Cliente no se encontró en la base de datos");

        //        model.Id = customer.id;
        //        model.FistName = customer.firstName;
        //        model.LastName = customer.lastName;
        //        model.RFC = customer.rfc;
        //        model.CURP = customer.curp;
        //        model.BusinessName = customer.businessName;
        //        model.Street = customer.street;
        //        model.OutdoorNumber = customer.outdoorNumber;
        //        model.InteriorNumber = customer.interiorNumber;
        //        model.ZipCode = customer.zipCode;
        //        model.Colony = customer.colony;
        //        model.Municipality = customer.municipality;
        //        model.State = customer.state;
        //        model.Country = customer.country;
        //        model.DeliveryAddress = customer.deliveryAddress;
        //        if (customer.taxRegime != null)
        //            model.taxRegime = ((TypeTaxRegimen)Enum.Parse(typeof(TypeTaxRegimen), customer.taxRegime)).ToString();

        //        var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
        //        stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

        //        var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
        //            .Select(e => new SelectListItem
        //            {
        //                Value = e.ToString(),
        //                Text = EnumUtils.GetDisplayName(e)
        //            }).ToList();

        //        model.ListRegimen = new SelectList(regimenList);
        //        model.ListState = new SelectList(stateList);

        //        var emails = customer.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
        //                    .Select(x => new CustomerContactsViewModel
        //                    {
        //                        Id = x.id,
        //                        TypeContact = x.typeContact,
        //                        EmailOrPhone = x.emailOrPhone
        //                    }).ToList();

        //        if (emails.Count() > 0)
        //        {
        //            model.Emails = emails;
        //        }

        //        var phones = customer.customerContacts.Where(x => x.typeContact == TypeContact.PHONE.ToString() && x.status == SystemStatus.ACTIVE.ToString())
        //                    .Select(x => new CustomerContactsViewModel
        //                    {
        //                        Id = x.id,
        //                        TypeContact = x.typeContact,
        //                        EmailOrPhone = x.emailOrPhone
        //                    }).ToList();
        //        if (phones.Count() > 0)
        //        {
        //            model.Phones = phones;
        //        }

        //        LogUtil.AddEntry(
        //           "Editar Cliente: " + JsonConvert.SerializeObject(model),
        //           ENivelLog.Info,
        //           userAuth.Id,
        //           userAuth.Email,
        //           EOperacionLog.ACCESS,
        //           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
        //           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
        //           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
        //        );

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
        //        LogUtil.AddEntry(
        //           "Se encontro un error: " + ex.Message.ToString(),
        //           ENivelLog.Error,
        //           userAuth.Id,
        //           userAuth.Email,
        //           EOperacionLog.ACCESS,
        //           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
        //           ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
        //           string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
        //        );
        //        return RedirectToAction("Index");
        //    }
        //}
    }
}