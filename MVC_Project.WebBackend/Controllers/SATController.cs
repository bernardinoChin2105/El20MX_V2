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
    public class SATController : Controller
    {
        private IAccountService _accountService;
        public SATController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        // GET: SAT
        public ActionResult Index()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var account = _accountService.GetById(userAuth.Account.Id);
            if (account == null)
                throw new Exception("");

            var model = new SATViewModel()
            {
                id = account.id,
                rfc=account.rfc,
                name = account.name,
                cerUrl = account.cer,
                keyUrl = account.key,
                eFirma = account.eFirma,
                ciec = account.ciec,
            };
            return View(model);
        }

        public ActionResult Edit(SATViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;

            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var account = _accountService.FirstOrDefault(x => x.id == model.id);
                if (account == null)
                    throw new Exception("No se encontró la regularización en el sistema");

                account.name = model.name;

                account.ciec = model.ciec;

                var storageEFirma = ConfigurationManager.AppSettings["StorageEFirma"];

                if (model.cer != null)
                {
                    var cer = AzureBlobService.UploadPublicFile(model.cer.InputStream, model.cer.FileName, storageEFirma, account.rfc);
                    account.cer = cer.Item1;
                }
                if (model.key != null)
                {
                    var key = AzureBlobService.UploadPublicFile(model.key.InputStream, model.key.FileName, storageEFirma, account.rfc);
                    account.key = key.Item1;
                }
                account.eFirma = model.eFirma;
                
                _accountService.Update(account);

                LogUtil.AddEntry(
                   "Edicion de sucursal: " + account.rfc,
                   ENivelLog.Info, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(account)
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
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
            }
            return RedirectToAction("Index");
        }
    }
}