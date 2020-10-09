using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
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
    public class QuotationController : Controller
    {

        private QuotationService _quotationService;
        private AccountService _accountService;

        public QuotationController(QuotationService quotationService, AccountService accountService)
        {
            _quotationService = quotationService;
            _accountService = accountService;
        }
        // GET: Quotation
        public ActionResult Index()
        {
            var model = new QuotationViewModel();
            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetQuotations(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                IList<QuotationData> response = new List<QuotationData>();
                int totalDisplay = 0;

                Int64? accountId = userAuth.GetAccountId();

                var quotations = _quotationService.GetQuotation(filtros, param.iDisplayStart, param.iDisplayLength);
                totalDisplay = quotations.Item2;
                foreach (var quotation in quotations.Item1)
                {
                    QuotationData data = new QuotationData();
                    data.uuid = quotation.uuid.ToString();
                    data.id = quotation.id;
                    data.account = quotation.account.name + "( " + quotation.account.rfc + " )";
                    data.total = quotation.total;
                    data.partialitiesNumber = quotation.partialitiesNumber;
                    data.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), quotation.status)).GetDisplayName();
                    data.quoteLink = quotation.quoteLink;
                    data.quoteName = quotation.quoteName;
                    data.startedAt = quotation.startedAt;
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
            try
            {
                var model = new QuotationCreate();
                model.accounts = PopulateAccounts();
                model.partialitiesNumber = 1;
                model.statusQuotation = PopulateStatus();
                return View(model);
            }
            catch (Exception ex)
            {
                
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        private List<SelectListItem> PopulateStatus()
        {
            List<SelectListItem> quoteStatus = new List<SelectListItem>();
            quoteStatus.Add(new SelectListItem
            {
                Value = SystemStatus.PROCESSING.ToString(),
                Text = EnumUtils.GetDisplayName(SystemStatus.PROCESSING)
            });
            quoteStatus.Add(new SelectListItem
            {
                Value = SystemStatus.APPROVED.ToString(),
                Text = EnumUtils.GetDisplayName(SystemStatus.APPROVED)
            });

            quoteStatus.Add(new SelectListItem
            {
                Value = SystemStatus.CANCELLED.ToString(),
                Text = EnumUtils.GetDisplayName(SystemStatus.CANCELLED)
            });

            return quoteStatus;

        }

        private List<SelectListItem> PopulateAccounts()
        {
            var accountList = new List<SelectListItem>();
            accountList = _accountService.GetAll().Select(g => new SelectListItem
            {
               Value = g.id.ToString(),
               Text = g.name + "( " + g.rfc + " )"
            }).ToList();
            return accountList;
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(QuotationCreate model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");
                var account = _accountService.GetById(model.accountId);
                var storageQuotation = ConfigurationManager.AppSettings["StorageQuotation"];
                var upload = AzureBlobService.UploadPublicFile(model.file.InputStream, model.file.FileName, storageQuotation, account.rfc);

                var quotation = new Quotation()
                {
                    uuid = Guid.NewGuid(),
                    account = account,
                    startedAt = model.startedAt,
                    total = model.total,
                    hasDeferredPayment = model.hasDeferredPayment,
                    partialitiesNumber = model.partialitiesNumber,
                    advancePayment = model.advancePayment,
                    monthlyCharge = model.monthlyCharge,
                    quoteLink = upload.Item1,
                    quoteName = upload.Item2,
                    status = model.quoteStatus,
                    createdAt = DateTime.Now,
                };

                _quotationService.Create(quotation);

                LogUtil.AddEntry(
                   "Creacion de regularizcion para el cliente: " + account.rfc, ENivelLog.Info, userAuth.Id, userAuth.Email,
                   EOperacionLog.ACCESS, string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(quotation)
                );

                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al crear la regularización",
                   ENivelLog.Error,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                model.accounts = PopulateAccounts();
                model.statusQuotation = PopulateStatus();
                return View(model);
            }
        }

        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var quotation = _quotationService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (quotation == null)
                    throw new Exception("La regularización no se encontró en la base de datos");

                var model = new QuotationCreate()
                {
                    id = quotation.id,
                    accountId = quotation.account.id,
                    accounts = PopulateAccounts(),
                    startedAt = quotation.startedAt,
                    total = Math.Round(quotation.total),
                    hasDeferredPayment = quotation.hasDeferredPayment,
                    advancePayment = Math.Round(quotation.advancePayment, 2),
                    partialitiesNumber = quotation.partialitiesNumber,
                    monthlyCharge = Math.Round(quotation.monthlyCharge, 2),
                    statusQuotation = PopulateStatus(),
                    quoteStatus = quotation.status,
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
        public ActionResult Edit(QuotationCreate model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            var quotation = _quotationService.FirstOrDefault(x => x.id == model.id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                quotation.startedAt = model.startedAt;
                quotation.total = model.total;
                quotation.hasDeferredPayment = model.hasDeferredPayment;
                quotation.partialitiesNumber = model.partialitiesNumber;
                quotation.advancePayment = model.advancePayment;
                quotation.monthlyCharge = model.monthlyCharge;
                quotation.status = model.quoteStatus;

                _quotationService.Update(quotation);

                LogUtil.AddEntry(
                   "Edicion de regularización del cliente: " + quotation.account.rfc,
                   ENivelLog.Info, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(quotation)
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al editar la regularización con id " + model.id,
                   ENivelLog.Error, userAuth.Id, userAuth.Email, EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   ex.Message
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                model.accounts = PopulateAccounts();
                model.statusQuotation = PopulateStatus();
                return View(model);
            }
        }

    }
}