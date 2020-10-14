using LogHubSDK.Models;
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
        public BranchOfficeController(BranchOfficeService branchOfficeService)
        {
            _branchOfficeService = branchOfficeService;
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

                var branchOffices = _branchOfficeService.GetBranchOffice(filtros, param.iDisplayStart, param.iDisplayLength);
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
                    cerUrl =branchOffice.cer,
                    keyUrl = branchOffice.key,
                    fiel=branchOffice.fiel,
                    ciec = branchOffice.ciec,
                    
                };

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

                var storageQuotation = ConfigurationManager.AppSettings["StorageQuotation"];
                
                if (model.ciec != null)
                {
                    var upload = AzureBlobService.UploadPublicFile(model.cer.InputStream, model.cer.FileName, storageQuotation, branchOffice.account.rfc);
                }

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

                return View(model);
            }
        }
    }
}