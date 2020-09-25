﻿using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
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
        private IAllianceService _allianceService;

        #region Métodos para las alianzas
        public AllianceController(IAllyService allyService, IAllianceService allianceService)
        {
            _allyService = allyService;
            _allianceService = allianceService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetAlliances(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<AllianceList>();
            var list = new List<AlliancesListVM>();
            string error = string.Empty;
            bool success = true;

            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string Name = filtersValues.Get("Name");
                string AllyName = filtersValues.Get("AllyName");

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);

                listResponse = _allianceService.GetAlliancesList(pagination, Name, AllyName);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();

                    list = listResponse.Select(x => new AlliancesListVM
                    {
                        id = x.id,
                        uuid = x.uuid,
                        name = x.name,
                        allyName = x.allyName,
                        allyCommisionPercent = x.allyCommisionPercent,
                        customerDiscountPercent = x.customerDiscountPercent,
                        createdAt = x.createdAt.ToShortDateString(),
                        status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), x.status)).GetDisplayName()
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

        [HttpPost]
        public ActionResult Delete(string uuid, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            bool success = true;
            string error = String.Empty;
            try
            {
                var aliance = _allianceService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (aliance != null)
                {
                    if (aliance.status == SystemStatus.ACTIVE.ToString())
                    {
                        aliance.status = SystemStatus.INACTIVE.ToString();
                    }
                    else
                    {
                        aliance.status = SystemStatus.ACTIVE.ToString();
                    }

                }
                _allianceService.Update(aliance);

                LogUtil.AddEntry(
                   "Actualización del status: " + JsonConvert.SerializeObject(uuid),
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                //return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message.ToString();
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
                //return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = success,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            AllianceViewModel model = new AllianceViewModel();
            try
            {
                var list = _allyService.GetAll();
                var allyList = list.Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() }).ToList();
                //allyList.Insert(0, new SelectListItem() { Text = "Todos", Value = "-1" });
                model.allyList = new SelectList(allyList);

                //model.allyMultList = new MultiSelectList(list, "id", "name");
                //model.allyId = new[];
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }
            return View(model);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(AllianceViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                if (_allianceService.FindBy(x => x.name == model.name).Any())
                    throw new Exception("Ya existe una alianza con el Nombre proporcionado");

                DateTime todayDate = DateUtil.GetDateTimeNow();

                Alliance alliance = new Alliance()
                {
                    uuid = Guid.NewGuid(),
                    name = model.name,
                    allyCommisionPercent = model.allyCommisionPercent,
                    customerDiscountPercent = model.customerDiscountPercent,
                    promotionCode = model.promotionCode,
                    allianceValidity = model.allianceValidity,
                    applyPeriod = model.applyPeriod,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString(),
                    ally = new Ally { id = model.allyId }
                };

                var ally = _allyService.FirstOrDefault(x => x.id == model.allyId);

                if (ally != null)
                {
                    var arrayS = model.promotionCode.Split('-');
                    var numCons = ally.consecutive + 1;
                    ally.consecutive = numCons;
                    ally.modifiedAt = todayDate;

                    if (arrayS.Count() != 3 || arrayS[2] != numCons.ToString())
                    {
                        arrayS[2] = numCons.ToString();
                        model.promotionCode = string.Join("-", arrayS);
                        ally.consecutive = numCons;
                    }
                }


                if (model.applyPeriod)
                {
                    alliance.initialPeriod = model.initialPeriod;
                    alliance.finalPeriod = model.finalPeriod;
                    alliance.finalAllyCommisionPercent = model.finalAllyCommisionPercent;
                }

                if (model.allianceValidity)
                {
                    alliance.finalDate = model.finalDate;
                    alliance.initialDate = model.finalDate;
                }

                _allianceService.Create(alliance);

                LogUtil.AddEntry(
                   "Se creo la alianza con id: " + alliance.id,
                   ENivelLog.Info,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var list = _allyService.GetAll();
                var allyList = list.Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() }).ToList();
                //allyList.Insert(0, new SelectListItem() { Text = "Todos", Value = "-1" });
                model.allyList = new SelectList(allyList);

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

                return View(model);
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPromotionCode(Int64 id)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            string code = string.Empty;
            string error = string.Empty;
            bool success = true;

            try
            {
                var ally = _allyService.FirstOrDefault(x => x.id == id);
                if (ally == null)
                    throw new Exception("El registro de Aliado no se encontró en la base de datos");

                code = ally.prefix + "-" + ally.id + "-" + (ally.consecutive + 1);

            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                success = false;
            }

            return Json(new
            {
                success = success,
                promotionCode = code,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Métodos para Aliados
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
            var listResponse = new List<AlliesList>();
            var list = new List<AlliesListVM>();
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
        #endregion
    }
}