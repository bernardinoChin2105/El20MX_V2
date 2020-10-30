using LogHubSDK.Models;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.SAT;
using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class BranchOfficeController : Controller
    {
        private string _provider = ConfigurationManager.AppSettings["SATProvider"];

        IBranchOfficeService _branchOfficeService;
        IStateService _stateService;
        IAccountService _accountService;
        public BranchOfficeController(IBranchOfficeService branchOfficeService, IStateService stateService, IAccountService accountService)
        {
            _branchOfficeService = branchOfficeService;
            _stateService = stateService;
            _accountService = accountService;
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

        public ActionResult Create()
        {
            var model = new BranchOfficeViewModel();
            model.folio = 1;
            model.serie = "A";
            SetCombos(string.Empty, ref model);
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(BranchOfficeViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;

            try
            {
                var account = _accountService.FirstOrDefault(x => x.id == userAuth.Account.Id);
                if (account == null)
                    throw new Exception("La cuenta no es válida");

                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");
                
                var branchOffice = new Domain.Entities.BranchOffice
                {
                    uuid = Guid.NewGuid(),
                    name = model.name,
                    account = account,
                    folio = model.folio,
                    serie = model.serie,
                    street = model.street,
                    outdoorNumber = model.outdoorNumber,
                    interiorNumber = model.interiorNumber,
                    zipCode = model.zipCode,
                    colony = new Domain.Entities.Settlement { id = model.colony },
                    municipality = new Domain.Entities.Municipality { id = model.municipality },
                    state = new Domain.Entities.State { id = model.state },
                    country = new Domain.Entities.Country { id = model.country },
                    createdAt = DateTime.Now,
                    status = SystemStatus.ACTIVE.ToString(),
                };
                _branchOfficeService.Create(branchOffice);

                if (model.cer != null && model.key != null && !string.IsNullOrEmpty(model.password))
                {
                    if (Path.GetExtension(model.cer.FileName) == ".cer" && Path.GetExtension(model.key.FileName) == ".key")
                    {
                        try
                        {
                            var storageEFirma = ConfigurationManager.AppSettings["StorageEFirma"];

                            model.cer.InputStream.Position = 0;
                            byte[] result = null;
                            using (var streamReader = new MemoryStream())
                            {
                                model.cer.InputStream.CopyTo(streamReader);
                                result = streamReader.ToArray();
                            }
                            string cerStr = Convert.ToBase64String(result);

                            model.key.InputStream.Position = 0;
                            result = null;
                            using (var streamReader = new MemoryStream())
                            {
                                model.key.InputStream.CopyTo(streamReader);
                                result = streamReader.ToArray();
                            }
                            string keyStr = Convert.ToBase64String(result);

                            var satModel = SATService.CreateCertificates(cerStr, keyStr, model.password, _provider);

                            branchOffice.password = model.password;
                            branchOffice.certificateId = satModel.id;

                            model.cer.InputStream.Position = 0;
                            var cer = AzureBlobService.UploadPublicFile(model.cer.InputStream, model.cer.FileName, storageEFirma, account.rfc + "/csd_sucursal_" + branchOffice.id);
                            branchOffice.cer = cer.Item1;
                            model.key.InputStream.Position = 0;
                            var key = AzureBlobService.UploadPublicFile(model.key.InputStream, model.key.FileName, storageEFirma, account.rfc + "/csd_sucursal_" + branchOffice.id);
                            branchOffice.key = key.Item1;

                            _branchOfficeService.Update(branchOffice);
                        }
                        catch (Exception ex)
                        {
                            MensajeFlashHandler.RegistrarMensaje("No se pudo completar la carga de los archivos csd. " + ex.Message, TiposMensaje.Warning);
                            return RedirectToAction("Edit", new { uuid = branchOffice.uuid });
                        }
                    }
                }

                LogUtil.AddEntry(
                   "Creacion de sucursal: " + branchOffice.account.rfc,
                   ENivelLog.Info, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(branchOffice)
                );

                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Edit", new { uuid = branchOffice.uuid });
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al crear la sucursal para el rfc " + userAuth.Account.RFC,
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
                    uuid = branchOffice.uuid.ToString(),
                    name = branchOffice.name,
                    folio = branchOffice.folio,
                    serie = branchOffice.serie,
                    street = branchOffice.street,
                    outdoorNumber = branchOffice.outdoorNumber,
                    interiorNumber = branchOffice.interiorNumber,
                    zipCode = branchOffice.zipCode,
                    colony = branchOffice.colony != null ? branchOffice.colony.id : 0,
                    municipality = branchOffice.municipality != null ? branchOffice.municipality.id : 0,
                    state = branchOffice.state != null ? branchOffice.state.id : 0,
                    country = branchOffice.country != null ? branchOffice.country.id : 0,
                    logo = branchOffice.logo,
                    cerUrl = branchOffice.cer,
                    keyUrl = branchOffice.key,
                    password = branchOffice.password
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
                    throw new Exception("No se encontró la sucursal en el sistema");

                branchOffice.name = model.name;
                branchOffice.street = model.street;
                branchOffice.outdoorNumber = model.outdoorNumber;
                branchOffice.interiorNumber = model.interiorNumber;
                branchOffice.zipCode = model.zipCode;
                branchOffice.colony = new Domain.Entities.Settlement { id = model.colony };
                branchOffice.municipality = new Domain.Entities.Municipality { id = model.municipality};
                branchOffice.state = new Domain.Entities.State { id = model.state };
                branchOffice.country = new Domain.Entities.Country { id = model.country };

                _branchOfficeService.Update(branchOffice);

                if (model.cer != null && model.key != null && !string.IsNullOrEmpty(model.password))
                {
                    if (Path.GetExtension(model.cer.FileName) == ".cer" && Path.GetExtension(model.key.FileName) == ".key")
                    {
                        try
                        {
                            var storageEFirma = ConfigurationManager.AppSettings["StorageEFirma"];

                            if (!string.IsNullOrEmpty(branchOffice.certificateId))
                                SATService.DeleteCertificates(branchOffice.certificateId, _provider);

                            model.cer.InputStream.Position = 0;
                            byte[] result = null;
                            using (var streamReader = new MemoryStream())
                            {
                                model.cer.InputStream.CopyTo(streamReader);
                                result = streamReader.ToArray();
                            }
                            string cerStr = Convert.ToBase64String(result);

                            model.key.InputStream.Position = 0;
                            result = null;
                            using (var streamReader = new MemoryStream())
                            {
                                model.key.InputStream.CopyTo(streamReader);
                                result = streamReader.ToArray();
                            }
                            string keyStr = Convert.ToBase64String(result);

                            var satModel = SATService.CreateCertificates(cerStr, keyStr, model.password, _provider);

                            branchOffice.password = model.password;
                            branchOffice.certificateId = satModel.id;

                            model.cer.InputStream.Position = 0;
                            var cer = AzureBlobService.UploadPublicFile(model.cer.InputStream, model.cer.FileName, storageEFirma, branchOffice.account.rfc + "/csd_sucursal_" + branchOffice.id);
                            branchOffice.cer = cer.Item1;
                            model.key.InputStream.Position = 0;
                            var key = AzureBlobService.UploadPublicFile(model.key.InputStream, model.key.FileName, storageEFirma, branchOffice.account.rfc + "/csd_sucursal_" + branchOffice.id);
                            branchOffice.key = key.Item1;

                            _branchOfficeService.Update(branchOffice);
                        }
                        catch (Exception ex)
                        {
                            MensajeFlashHandler.RegistrarMensaje("No se pudo completar la carga de los archivos csd. " + ex.Message, TiposMensaje.Warning);
                            return RedirectToAction("Edit", new { uuid = branchOffice.uuid });
                        }
                    }
                }

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
            if (!string.IsNullOrEmpty(zipCode))
            {
                var listResponse = _stateService.GetLocationList(zipCode);

                var countries = listResponse.Select(x => new { id = x.countryId, name = x.nameCountry }).Distinct();
                model.listCountry = countries.Select(x => new SelectListItem
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                }).Distinct().ToList();

                var states = listResponse.Select(x => new { id = x.stateId, name = x.nameState }).Distinct();
                model.listState = states.Select(x => new SelectListItem
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
            else
            {
                model.listCountry = new List<SelectListItem>();
                model.listState = new List<SelectListItem>();
                model.listMunicipality = new List<SelectListItem>();
                model.listColony = new List<SelectListItem>();
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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateLogo(LogoBranchOfficeViewModel data)
        {
            try
            {
                var branchOffice = _branchOfficeService.FirstOrDefault(x => x.uuid == Guid.Parse(data.uuid));
                if (branchOffice == null)
                    throw new Exception("La cuenta no es válida");

                var StorageImages = ConfigurationManager.AppSettings["StorageImages"];

                if (data.image == null)
                    throw new Exception("No se proporcionó una imagen");

                var image = AzureBlobService.UploadPublicFile(data.image.InputStream, data.fileName, StorageImages, branchOffice.account.rfc + "/sucursal_" + branchOffice.id);
                branchOffice.logo = image.Item1;
                branchOffice.modifiedAt = DateTime.Now;
                _branchOfficeService.Update(branchOffice);

                return Json(new { branchOffice.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}