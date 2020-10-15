﻿using LogHubSDK.Models;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class BranchOfficeController : Controller
    {
        BranchOfficeService _branchOfficeService;
        StateService _stateService;
        public BranchOfficeController(BranchOfficeService branchOfficeService, StateService stateService)
        {
            _branchOfficeService = branchOfficeService;
            _stateService = stateService;
        }
        // GET: BranchOffice
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetBranchOffices(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                IList<BranchOfficeData> response = new List<BranchOfficeData>();
                int totalDisplay = 0;


                Int64? accountId = userAuth.GetAccountId();

                var branchOffices = _branchOfficeService.GetBranchOffice(filtros, accountId, param.iDisplayStart, param.iDisplayLength);
                totalDisplay = branchOffices.Item2;
                foreach (var branchOffice in branchOffices.Item1)
                {
                    BranchOfficeData data = new BranchOfficeData();
                    data.uuid = branchOffice.uuid.ToString();
                    data.id = branchOffice.id;
                    data.name = branchOffice.name;
                    data.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), branchOffice.status)).GetDisplayName();
                    response.Add(data);
                }

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = response.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = response
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, message = ex.Message },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var branchOffice = _branchOfficeService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (branchOffice == null)
                    throw new Exception("La regularización no se encontró en la base de datos");

                var model = new BranchOfficeViewModel()
                {
                    id = branchOffice.id,
                    name = branchOffice.name,
                    cerUrl = branchOffice.cer,
                    keyUrl = branchOffice.key,
                    eFirma = branchOffice.eFirma,
                    ciec = branchOffice.ciec,
                    folio = branchOffice.folio,
                    serie = branchOffice.serie,
                    street = branchOffice.street,
                    outdoorNumber = branchOffice.outdoorNumber,
                    interiorNumber = branchOffice.interiorNumber,
                    zipCode = branchOffice.zipCode,
                    colony = branchOffice.colony != null ? branchOffice.colony.id : 0,
                    municipality = branchOffice.municipality != null ? branchOffice.municipality.id : 0,
                    state = branchOffice.state != null ? branchOffice.state.id : 0,
                    country = branchOffice.country != null ? branchOffice.country.id : 0
                };

                SetCombos(branchOffice.zipCode, ref model);
                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(BranchOfficeViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;

            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var branchOffice = _branchOfficeService.FirstOrDefault(x => x.id == model.id);
                if (branchOffice == null)
                    throw new Exception("No se encontró la regularización en el sistema");

                branchOffice.name = model.name;

                branchOffice.ciec = model.ciec;

                var storageEFirma = ConfigurationManager.AppSettings["StorageEFirma"];
                
                if (model.cer != null)
                {
                    var cer = AzureBlobService.UploadPublicFile(model.cer.InputStream, model.cer.FileName, storageEFirma, branchOffice.account.rfc);
                    branchOffice.cer = cer.Item1;
                }
                if (model.key != null)
                {
                    var key = AzureBlobService.UploadPublicFile(model.key.InputStream, model.key.FileName, storageEFirma, branchOffice.account.rfc);
                    branchOffice.key = key.Item1;
                }
                branchOffice.eFirma = model.eFirma;

                branchOffice.street = model.street;
                branchOffice.outdoorNumber = model.outdoorNumber;
                branchOffice.interiorNumber = model.interiorNumber;
                branchOffice.zipCode = model.zipCode;
                branchOffice.colony = new Domain.Entities.Settlement { id = model.colony };
                branchOffice.municipality = new Domain.Entities.Municipality { id = model.municipality};
                branchOffice.state = new Domain.Entities.State { id = model.state };
                branchOffice.country = new Domain.Entities.Country { id = model.country };

                _branchOfficeService.Update(branchOffice);

                LogUtil.AddEntry(
                   "Edicion de sucursal: " + branchOffice.account.rfc,
                   ENivelLog.Info, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(branchOffice)
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al editar la sucursal con id " + model.id,
                   ENivelLog.Error, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                SetCombos(model.zipCode, ref model);
                return View(model);
            }
        }

        private void SetCombos(string zipCode, ref BranchOfficeViewModel model)
        {
            var stateList = _stateService.GetAll().Select(x => new SelectListItem { Text = x.nameState, Value = x.id.ToString() }).ToList();
            stateList.Insert(0, (new SelectListItem { Text = "Seleccione...", Value = "-1" }));
            model.listState = stateList;

            if (!string.IsNullOrEmpty(zipCode))
            {
                var listResponse = _stateService.GetLocationList(zipCode);

                var countries = listResponse.Select(x => new { id = x.countryId, name = x.nameCountry }).Distinct();
                model.listCountry = countries.Select(x => new SelectListItem
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                }).Distinct().ToList();

                var municipalities = listResponse.Select(x => new { id = x.municipalityId, name = x.nameMunicipality }).Distinct();
                model.listMunicipality = municipalities.Select(x => new SelectListItem
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                }).Distinct().ToList();

                model.listColony = listResponse.Select(x => new SelectListItem
                {
                    Text = x.nameSettlement,
                    Value = x.id.ToString(),
                }).Distinct().ToList();
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