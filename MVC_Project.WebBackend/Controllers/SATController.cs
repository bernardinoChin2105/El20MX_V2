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
    public class SATController : Controller
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private string _provider = ConfigurationManager.AppSettings["SATProvider"];
        public SATController(IAccountService accountService, ICredentialService credentialService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
        }

        // GET: SAT
        public ActionResult Index()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var account = _accountService.GetById(userAuth.Account.Id);
                if (account == null)
                    throw new Exception("La cuenta no existe en el sistema");

                var model = new SATViewModel()
                {
                    id = account.id,
                    uuid = account.uuid.ToString(),
                    rfc = account.rfc,
                    name = account.name,
                    cerUrl = account.cer,
                    keyUrl = account.key,
                    efirma = account.eFirma,
                    ciec = account.ciec,
                    avatar = account.avatar
                };

                var efirmaStatus = SystemStatus.INACTIVE.ToString();
                var ciecStatus = SystemStatus.INACTIVE.ToString();

                var credentials = _credentialService.FindBy(x => x.account.id == account.id && x.provider == _provider);
                if (credentials.Any())
                {
                    var ciec = credentials.FirstOrDefault(x => x.credentialType == SATCredentialType.CIEC.ToString());
                    if (ciec != null)
                    {
                        model.ciecUuid = ciec.uuid.ToString();
                        ciec.status = model.ciecStatus = SATService.GetCredentialStatusSat(ciec.idCredentialProvider, _provider);
                        ciec.modifiedAt = DateTime.Now;
                        _credentialService.Update(ciec);
                    }
                    var efirma = credentials.FirstOrDefault(x => x.credentialType == SATCredentialType.EFIRMA.ToString());
                    if (efirma != null)
                    {
                        model.efirmaUuid = efirma.uuid.ToString();
                        efirma.status = model.efirmaStatus = SATService.GetCredentialStatusSat(efirma.idCredentialProvider, _provider);
                        efirma.modifiedAt = DateTime.Now;
                        _credentialService.Update(efirma);
                    }
                }

                return View(model);
            }
            catch(Exception ex)
            {
                FlashMessages.MensajeFlashHandler.RegistrarMensaje("No se pudo obtener la información de la cuenta", TiposMensaje.Error);
                return View();
            }
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
                account.eFirma = model.efirma;

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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateCredential(string ciecUuid, string rfc, string ciec)
        {
            try
            {
                var credential = _credentialService.FirstOrDefault(x => x.uuid == Guid.Parse(ciecUuid));

                if (credential == null)
                    throw new Exception("Credencial inválida");

                var account = credential.account;
                account.ciec = ciec;
                account.modifiedAt = DateTime.Now;
                _accountService.Update(account);

                var satModel = SATService.CreateCredential(new CredentialRequest { rfc = rfc, ciec = ciec }, _provider);

                credential.idCredentialProvider = satModel.id;
                credential.statusProvider = satModel.status;
                credential.status = SystemStatus.PROCESSING.ToString();
                credential.modifiedAt = DateTime.Now;
                _credentialService.Update(credential);
                return Json(new { credential.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateEfirma(EfirmaViewModel data)
        {
            try
            {
                if(string.IsNullOrEmpty(data.efirma))
                    throw new Exception("Porporcione la efirma");
                
                var account = _accountService.FirstOrDefault(x => x.rfc == data.rfc);
                if (account == null)
                    throw new Exception("La cuenta no es válida");

                var storageEFirma = ConfigurationManager.AppSettings["StorageEFirma"];

                if (data.cer == null && string.IsNullOrEmpty(account.cer))
                    throw new Exception("Es necesario proporcionar un archivo .cer");

                if (data.key == null && string.IsNullOrEmpty(account.key))
                    throw new Exception("Es necesario proporcionar un archivo .key");

                string cerStr = string.Empty;
                if (data.cer != null)
                {
                    if (Path.GetExtension(data.cer.FileName) != ".cer")
                        throw new Exception("El archivo .cer no tiene el formato correcto");

                    var cer = AzureBlobService.UploadPublicFile(data.cer.InputStream, data.cer.FileName, storageEFirma, account.rfc);
                    account.cer = cer.Item1;
                    data.cer.InputStream.Position = 0;
                    byte[] result = null;
                    using (var streamReader = new MemoryStream())
                    {
                        data.cer.InputStream.CopyTo(streamReader);
                        result = streamReader.ToArray();
                    }
                    cerStr = Convert.ToBase64String(result);
                }
                else
                {
                    var cerName = Path.GetFileName(account.cer);
                    MemoryStream stream = AzureBlobService.DownloadFile(storageEFirma, account.rfc + "/" + cerName);
                    stream.Position = 0;
                    byte[] result = stream.ToArray();
                    cerStr = Convert.ToBase64String(result);
                }

                string keyStr = string.Empty;
                if (data.key != null)
                {
                    if (Path.GetExtension(data.key.FileName) != ".key")
                        throw new Exception("El archivo .key no tiene el formato correcto");

                    var key = AzureBlobService.UploadPublicFile(data.key.InputStream, data.key.FileName, storageEFirma, account.rfc);
                    account.key = key.Item1;
                    data.key.InputStream.Position = 0;
                    byte[] result = null;
                    using (var streamReader = new MemoryStream())
                    {
                        data.key.InputStream.CopyTo(streamReader);
                        result = streamReader.ToArray();
                    }
                    keyStr = Convert.ToBase64String(result);
                }
                else
                {
                    var keyName = Path.GetFileName(account.key);
                    MemoryStream stream = AzureBlobService.DownloadFile(storageEFirma, account.rfc + "/" + keyName);
                    stream.Position = 0;
                    byte[] result = stream.ToArray();
                    keyStr = Convert.ToBase64String(result);
                }

                account.eFirma = data.efirma;
                account.modifiedAt = DateTime.Now;
                _accountService.Update(account);

                var satModel = SATService.CreateCredentialEfirma(cerStr, keyStr, data.efirma, _provider);

                Domain.Entities.Credential credential = null;
                if (string.IsNullOrEmpty(data.efirmaUuid))
                {
                    credential = new Domain.Entities.Credential
                    {
                        account = account,
                        uuid = Guid.NewGuid(),
                        provider = _provider,
                        idCredentialProvider = satModel.id,
                        statusProvider = satModel.status,
                        createdAt = DateTime.Now,
                        modifiedAt = DateTime.Now,
                        status = SystemStatus.PROCESSING.ToString(),
                        credentialType = SATCredentialType.EFIRMA.ToString()
                    };
                    _credentialService.Create(credential);
                }
                else
                {
                    credential = _credentialService.FirstOrDefault(x => x.uuid == Guid.Parse(data.efirmaUuid));
                    credential.idCredentialProvider = satModel.id;
                    credential.statusProvider = satModel.status;
                    credential.status = SystemStatus.PROCESSING.ToString();
                    credential.modifiedAt = DateTime.Now;
                    _credentialService.Update(credential);
                }

                if (credential == null)
                    throw new Exception("Credencial inválida");
                
                return Json(new { credential.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public ActionResult CredentialStatus(string uuid)
        {
            try
            {
                var credential = _credentialService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));

                if (credential.status == SystemStatus.PROCESSING.ToString())
                    return Json(new { success = true, finish = false }, JsonRequestBehavior.AllowGet);
                else if (credential.status == SystemStatus.ACTIVE.ToString())
                    return Json(new { success = true, finish = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, finish = true, message = "No fue posible validar el RFC, credential " + credential.statusProvider }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, finish = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateImage(AvatarAccountViewMovel data)
        {
            try
            {
                var account = _accountService.FirstOrDefault(x => x.uuid == Guid.Parse(data.uuid));
                if (account == null)
                    throw new Exception("La cuenta no es válida");

                var StorageImages = ConfigurationManager.AppSettings["StorageImages"];

                if (data.image == null)
                    throw new Exception("No se proporcionó una imagen");

                var image = AzureBlobService.UploadPublicFile(data.image.InputStream, data.fileName, StorageImages, account.rfc);
                account.avatar = image.Item1;
                account.modifiedAt = DateTime.Now;
                _accountService.Update(account);
                
                return Json(new { account.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}