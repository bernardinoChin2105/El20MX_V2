using MVC_Project.Domain.Services;
using MVC_Project.Integrations.SAT;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MVC_Project.Utils;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using System.Collections.Specialized;
using MVC_Project.FlashMessages;
using LogHubSDK.Models;
using System.Configuration;
using MVC_Project.Integrations.Storage;

namespace MVC_Project.WebBackend.Controllers
{
    public class DiagnosticController : Controller
    {
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IDiagnosticService _diagnosticService;
        private ICustomerService _customerService;
        private IProviderService _providerService;
        private IInvoiceIssuedService _invoicesIssuedService;
        private IInvoiceReceivedService _invoicesReceivedService;

        public DiagnosticController(IAccountService accountService, ICredentialService credentialService,
            IDiagnosticService diagnosticService, ICustomerService customerService, IProviderService providerService,
            IInvoiceIssuedService invoicesIssuedService, IInvoiceReceivedService invoicesReceivedService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _diagnosticService = diagnosticService;
            _customerService = customerService;
            _providerService = providerService;
            _invoicesIssuedService = invoicesIssuedService;
            _invoicesReceivedService = invoicesReceivedService;
        }

        // GET: Diagnostic
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Date = new
            {
                MinDate = DateTime.Now.AddDays(-10).ToString("dd-MM-yyyy"),
                MaxDate = DateTime.Now.ToString("dd-MM-yyyy")
            };
            return View();
        }

        [AllowAnonymous]
        public ActionResult DiagnosticDetail(string id)
        {
            var authUser = Authenticator.AuthenticatedUser;
            DiagnosticViewModel model = new DiagnosticViewModel();
            var diagnostic = _diagnosticService.FindBy(x => x.uuid.ToString() == id).FirstOrDefault();

            if (diagnostic != null)
            {
                model.id = id;
                model.rfc = diagnostic.account.rfc;
                model.businessName = diagnostic.businessName;
                model.commercialCAD = diagnostic.commercialCAD;
                model.plans = diagnostic.plans;
                model.email = authUser.Email;
                model.createdAt = diagnostic.createdAt;

                var diagTax = diagnostic.taxStatus;
                if (diagTax != null)
                {
                    model.diagnosticTaxStatus = new List<DiagnosticTaxStatusViewModel>();
                    var taxStatus = diagTax.Select(x => new DiagnosticTaxStatusViewModel
                    {
                        businessName = x.businessName,
                        statusSAT = x.statusSAT,
                        taxRegime = x.taxRegime != null ? x.taxRegime.Split(',').ToList() : null,
                        economicActivities = x.economicActivities != null ? x.economicActivities.Split(',').ToList() : null,
                        fiscalObligations = x.fiscalObligations != null ? x.fiscalObligations.Split(',').ToList() : null,
                        taxMailboxEmail = x.taxMailboxEmail
                    });

                    model.diagnosticTaxStatus = taxStatus.ToList();
                }

                var diagDetails = diagnostic.details;

                if (diagDetails != null)
                {
                    model.diagnosticDetails = new List<InvoicesGroup>();

                    string date = DateUtil.GetMonthName(DateTime.Now, "es");

                    //Asignar datos de diagnostico
                    List<InvoicesGroup> invoicePeriod = diagDetails.OrderBy(x => x.year).ThenBy(x => x.month).GroupBy(x => new
                    {
                        x.year,
                        x.month,
                    })
                    .Select(b => new InvoicesGroup
                    {
                        year = b.Key.year,
                        month = DateUtil.GetMonthName(new DateTime(b.Key.year, b.Key.month, 1), "es"),
                        //month = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(b.Key.month),
                        issuer = new IssuerReceiverGroup()
                        {
                            type = TypeIssuerReceiver.ISSUER.ToString(),
                            amountTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.ISSUER.ToString()).Select(x => x.totalAmount).FirstOrDefault(),
                            numberTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.ISSUER.ToString()).Select(x => x.numberCFDI).FirstOrDefault(),
                        },
                        receiver = new IssuerReceiverGroup()
                        {
                            type = TypeIssuerReceiver.RECEIVER.ToString(),
                            amountTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.RECEIVER.ToString()).Select(x => x.totalAmount).FirstOrDefault(),
                            numberTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.RECEIVER.ToString()).Select(x => x.numberCFDI).FirstOrDefault(),
                        }
                    }).ToList();

                    model.diagnosticDetails = invoicePeriod;
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult GenerateDx0()
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();

                var provider = ConfigurationManager.AppSettings["SATProvider"];
                DateTime dateFrom = DateTime.UtcNow.AddMonths(-3);
                DateTime dateTo = DateTime.UtcNow.AddMonths(-1);
                dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month)).AddDays(1).AddMilliseconds(-1);
                dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                //Se obtiene la extracción de los 3 meses completos. Sin los días del mes actual
                string extractionId = SATService.GenerateExtractions(authUser.Account.RFC, dateFrom, dateTo, provider);
                
                var diagn = new Diagnostic()
                {
                    uuid = Guid.NewGuid(),
                    account = account,
                    businessName = account.name,
                    commercialCAD = "",
                    plans = "",
                    createdAt = DateUtil.GetDateTimeNow(),
                    status = SystemStatus.PENDING.ToString(),
                    processId = extractionId
                };

                _diagnosticService.Create(diagn);
                //MensajeFlashHandler.RegistrarMensaje("¡Diagnóstico realizado!", TiposMensaje.Success);
                //return RedirectToAction("DiagnosticDetail", new { id = diagn.uuid.ToString() });
                return Json(new { diagn.uuid, success=true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //string error = ex.Message.ToString();
                //MensajeFlashHandler.RegistrarMensaje("¡Ocurrio un error en al realizar el diagnóstico!", TiposMensaje.Error);
                //MensajeFlashHandler.RegistrarMensaje(error, TiposMensaje.Error);
                //return RedirectToAction("Index");
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult FinishExtraction(string uuid)
        {
            try
            {
                var diagnostic = _diagnosticService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (diagnostic == null)
                    throw new Exception("No fue posible obtener el diagnostico");

                if (diagnostic.status == SystemStatus.PENDING.ToString() || diagnostic.status == SystemStatus.PROCESSING.ToString())
                    return Json(new { success = true, finish = false }, JsonRequestBehavior.AllowGet);
                else if (diagnostic.status == SystemStatus.ACTIVE.ToString())
                    return Json(new { success = true, finish = true }, JsonRequestBehavior.AllowGet);
                else if (diagnostic.status == SystemStatus.FAILED.ToString())
                    return Json(new { success = false, finish = true, message= "Se generó un fallo durante la extracción" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, finish = true, message = "No fue posible generar el diagnostico, comuniquese al área de soporte" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, finish = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult ObtenerDiagnostic(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                string FilterEnd = filtersValues.Get("FilterEndDate").Trim();

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (FilterStart != "") pagination.CreatedOnStart = Convert.ToDateTime(FilterStart);
                if (FilterEnd != "") pagination.CreatedOnEnd = Convert.ToDateTime(FilterEnd);

                var DiagnosticsResponse = _diagnosticService.DiagnosticList(userAuth.Account.Uuid.ToString(), pagination);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                int totalDisplay = 0;
                int total = 0;
                if (DiagnosticsResponse.Count() > 0)
                {
                    totalDisplay = DiagnosticsResponse[0].Total;
                    total = DiagnosticsResponse.Count();
                }

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = DiagnosticsResponse
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new {
                        success = false,
                        message = ex.Message,
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<DiagnosticsList>()
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [HttpGet, AllowAnonymous]
        public ActionResult GetDiagnosticDownload(string id)
        {
            var authUser = Authenticator.AuthenticatedUser;
            DiagnosticViewModel model = new DiagnosticViewModel();

            try
            {
                var diagnostic = _diagnosticService.FindBy(x => x.uuid.ToString() == id).FirstOrDefault();

                if (diagnostic == null)
                    throw new Exception("No se encontro el registro del diagnóstico");

                model.id = id;
                model.rfc = diagnostic.account.rfc;
                model.businessName = diagnostic.businessName;
                model.commercialCAD = diagnostic.commercialCAD;
                model.plans = diagnostic.plans;
                model.email = authUser.Email;
                model.createdAt = diagnostic.createdAt;

                var diagTax = diagnostic.taxStatus;
                if (diagTax != null)
                {
                    model.diagnosticTaxStatus = new List<DiagnosticTaxStatusViewModel>();
                    var taxStatus = diagTax.Select(x => new DiagnosticTaxStatusViewModel
                    {
                        businessName = x.businessName,
                        statusSAT = x.statusSAT,
                        taxRegime = x.taxRegime != null ? x.taxRegime.Split(',').ToList() : null,
                        economicActivities = x.economicActivities != null ? x.economicActivities.Split(',').ToList() : null,
                        fiscalObligations = x.fiscalObligations != null ? x.fiscalObligations.Split(',').ToList() : null,
                        taxMailboxEmail = x.taxMailboxEmail
                    });

                    model.diagnosticTaxStatus = taxStatus.ToList();
                }

                var diagDetails = diagnostic.details;

                if (diagDetails != null)
                {
                    //List<DiagnosticDetailsViewModel> details = new List<DiagnosticDetailsViewModel>();
                    model.diagnosticDetails = new List<InvoicesGroup>();

                    string date = DateUtil.GetMonthName(DateTime.Now, "es");

                    //Asignar datos de diagnostico
                    List<InvoicesGroup> invoicePeriod = diagDetails.OrderBy(x => x.year).ThenBy(x => x.month).GroupBy(x => new
                    {
                        x.year,
                        x.month,
                    })
                    .Select(b => new InvoicesGroup
                    {
                        year = b.Key.year,
                        month = DateUtil.GetMonthName(new DateTime(b.Key.year, b.Key.month, 1), "es"),
                        issuer = new IssuerReceiverGroup()
                        {
                            type = TypeIssuerReceiver.ISSUER.ToString(),
                            amountTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.ISSUER.ToString()).Select(x => x.totalAmount).FirstOrDefault(),
                            numberTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.ISSUER.ToString()).Select(x => x.numberCFDI).FirstOrDefault(),
                        },
                        receiver = new IssuerReceiverGroup()
                        {
                            type = TypeIssuerReceiver.RECEIVER.ToString(),
                            amountTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.RECEIVER.ToString()).Select(x => x.totalAmount).FirstOrDefault(),
                            numberTotal = b.Where(x => x.typeTaxPayer == TypeIssuerReceiver.RECEIVER.ToString()).Select(x => x.numberCFDI).FirstOrDefault(),
                        }
                    }).ToList();

                    model.diagnosticDetails = invoicePeriod;
                }


                LogUtil.AddEntry(
                       "Se descarga el Dx0 del cliente",
                       ENivelLog.Info,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );

                return new Rotativa.ViewAsPdf("DiagnosticZeroDownload", model) { FileName = authUser.Account.RFC + "_D0.pdf" };
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }

        }
    }
}