using LogHubSDK.Models;
using MVC_Project.Domain.Services;
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
    public class BankController : Controller
    {
        private IAccountService _accountService;

        public BankController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBanks(JQueryDataTableParams param, string filtros, bool first)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                int totalDisplay = 0;
                int total = 0;
                var listResponse = new List<Object>();//List<CustomerList>();
                //if (!first)
                //{
                //    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                //    string rfc = filtersValues.Get("RFC").Trim();
                //    string businessName = filtersValues.Get("BusinessName").Trim();
                //    string email = filtersValues.Get("Email").Trim();

                //    var pagination = new BasePagination();
                //    var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                //    pagination.PageSize = param.iDisplayLength;
                //    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                //    if (rfc != "") filters.rfc = rfc;
                //    if (businessName != "") filters.businessName = businessName;
                //    if (email != "") filters.email = email;

                //    listResponse = _customerService.CustomerList(pagination, filters);

                //    //Corroborar los campos iTotalRecords y iTotalDisplayRecords

                //    if (listResponse.Count() > 0)
                //    {
                //        totalDisplay = listResponse[0].Total;
                //        total = listResponse.Count();
                //    }
                //}

                LogUtil.AddEntry(
                   "Lista de Bancos total: " + totalDisplay + ", totalDisplay: " + total,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

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

        //Llamadas para paybook
        [HttpGet, AllowAnonymous]
        public JsonResult GetToken()
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                #region Crear credencial el usuario de la cuenta rfc, si aun no ha sido creada
                //Crear usuario si aun no ha sido creado
                //Obtener idUsuario
                //Obtener token de paybook para retornar

                //Obtener usuario                 
                

                

                #endregion

                LogUtil.AddEntry(
                   "Obtener token del cliente",
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return Json(new
                {
                    Data = new { success = true, data = "" },
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

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