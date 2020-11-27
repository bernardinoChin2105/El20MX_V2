using LogHubSDK.Models;
using MVC_Project.Domain.Model;
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
    public class MyAccountController : Controller
    {
        // GET: myAccount
        public ActionResult Index()
        {
            MyAccountVM model = new MyAccountVM();
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(), //DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy"),
                MaxDate = DateTime.Now.ToString("dd/MM/yyyy")
            };
            SetCombos(ref model);
            return View(model);
        }

        //public ActionResult AcountInvoicing()
        //{
        //    return View();
        //}

        [HttpGet, AllowAnonymous]
        public JsonResult GetMyAccounts(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            //var listResponse = new List<AlliesList>();
            //var list = new List<AlliesListVM>();
            string error = string.Empty;
            bool success = true;

            object[] listResponse = new object[3];
            listResponse[0] = new { id = 15, month = "Noviembre", year = 2020, status = "Vigente", amountTotal = "$ 1,500.00", payInvoice = true };
            listResponse[1] = new { id = 15, month = "Octubre", year = 2020, status = "Vencida", amountTotal = "$ 1,500.00", payInvoice = true };
            listResponse[2] = new { id = 15, month = "Septiembre", year = 2020, status = "Pagada", amountTotal = "$ 1,500.00", payInvoice = true };
            
            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                       
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

                //listResponse = _customerService.CustomerList(pagination, filters);

                ////Corroborar los campos iTotalRecords y iTotalDisplayRecords

                //if (listResponse.Count() > 0)
                //{
                //    totalDisplay = listResponse[0].Total;
                //    total = listResponse.Count();
                //}


                //return Json(new
                //{
                //    success = true,
                //    sEcho = param.sEcho,
                //    iTotalRecords = total,
                //    iTotalDisplayRecords = totalDisplay,
                //    aaData = listResponse
                //}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                error = ex.Message.ToString();
                success = false;

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
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);

        }

        private void SetCombos(ref MyAccountVM model)
        {                                
            model.ListStatusPayment = Enum.GetValues(typeof(StatusPayment)).Cast<StatusPayment>()
                   .Select(e => new SelectListItem
                   {
                       Value = e.ToString(),
                       Text = EnumUtils.GetDisplayName(e)
                   }).ToList();
        }
    }
}