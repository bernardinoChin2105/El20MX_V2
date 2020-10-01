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
    public class PromotionController : Controller
    {
        // GET: Promotion
        #region Promociones (Descuentos)
        public ActionResult Index()
        {
            PromotionFilterViewModel model = new PromotionFilterViewModel();

            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos", Value = "-1" });

            var types = Enum.GetValues(typeof(TypePromotions)).Cast<TypePromotions>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = EnumUtils.GetDisplayName(e)
                }).ToList();
            list.AddRange(types);

            model.typeList = new SelectList(list);

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPromotions(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<AlliesList>();
            var list = new List<AlliesListVM>();
            string error = string.Empty;
            bool success = true;

            try
            {
                //NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                //string Name = filtersValues.Get("Name");

                //var pagination = new BasePagination();
                //pagination.PageSize = param.iDisplayLength;
                //pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);

                //listResponse = _allyService.GetAlliesList(pagination, Name);

                ////Corroborar los campos iTotalRecords y iTotalDisplayRecords
                //if (listResponse.Count() > 0)
                //{
                //    totalDisplay = listResponse[0].Total;
                //    total = listResponse.Count();

                //    list = listResponse.Select(x => new AlliesListVM
                //    {
                //        id = x.id,
                //        uuid = x.uuid,
                //        name = x.name,
                //        createdAt = x.createdAt.ToShortDateString(),
                //        modifiedAt = x.createdAt.ToShortDateString(),
                //        status = x.status
                //    }).ToList();
                //}

                //LogUtil.AddEntry(
                //   "Lista de Aliados total: " + totalDisplay + ", totalDisplay: " + total,
                //   ENivelLog.Info,
                //   userAuth.Id,
                //   userAuth.Email,
                //   EOperacionLog.ACCESS,
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                //   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                //);
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                success = false;

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
            }

            return Json(new
            {
                success = success,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}