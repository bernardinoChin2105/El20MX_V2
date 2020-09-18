using LogHubSDK.Models;
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
    public class AllianceController : Controller
    {
        private IAllyService _allyService;

        public AllianceController(IAllyService allyService)
        {
            _allyService = allyService;
        }

        // GET: Alliance
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult AllyIndex()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetAllies(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<AlliesList>(); //cambiar modelo
            var list = new List<AlliesListVM>(); //cambiar modelo
            string error = string.Empty;
            bool success = true;

            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string Name = filtersValues.Get("Name");

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);

                listResponse = _allyService.GetAlliesList(pagination, Name);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();

                    list = listResponse.Select(x => new AlliesListVM
                    {
                        id = x.id,
                        uuid = x.uuid,
                        name = x.name,
                        createdAt = x.createdAt.ToShortDateString(),
                        modifiedAt = x.createdAt.ToShortDateString(),
                        status = x.status
                    }).ToList();
                }

                LogUtil.AddEntry(
                   "Lista de Aliados total: " + totalDisplay + ", totalDisplay: " + total,
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
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult AllyCreate()
        {
            return View();
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult AllyCreate(AllyFilterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var authUser = Authenticator.AuthenticatedUser;

                if (_allyService.FindBy(x => x.name == model.Name).Any())
                    throw new Exception("Ya existe un Aliado con el Nombre proporcionado");

                DateTime todayDate = DateUtil.GetDateTimeNow();

                Ally ally = new Ally()
                {
                    uuid = Guid.NewGuid(),
                    name = model.Name,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString(),
                };

                _allyService.Create(ally);
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("AllyIndex");
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }

        public ActionResult AllyEdit(string uuid)
        {
            try
            {
                AllyFilterViewModel model = new AllyFilterViewModel();
                var userAuth = Authenticator.AuthenticatedUser;

                var ally = _allyService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (ally == null)
                    throw new Exception("El registro de Aliado no se encontró en la base de datos");

                model.Id = ally.id;
                model.Name = ally.name;
                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult AllyEdit(AllyFilterViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var allyData = _allyService.FirstOrDefault(x => x.id == model.Id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                DateTime todayDate = DateUtil.GetDateTimeNow();

                allyData.name = model.Name;
                allyData.modifiedAt = todayDate;
                allyData.status = SystemStatus.ACTIVE.ToString();

                _allyService.Update(allyData);
                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("AllyIndex");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }
    }
}