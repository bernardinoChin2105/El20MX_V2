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
        private IDiagnosticDetailService _diagnosticDetailService;
        private IDiagnosticTaxStatusService _diagnosticTaxStatusService;
        private IWebhookProcessService _webhookProcessService;

        public DiagnosticController(IAccountService accountService, ICredentialService credentialService,
            IDiagnosticService diagnosticService, ICustomerService customerService, IProviderService providerService,
            IInvoiceIssuedService invoicesIssuedService, IInvoiceReceivedService invoicesReceivedService, 
            IDiagnosticDetailService diagnosticDetailService, IDiagnosticTaxStatusService diagnosticTaxStatusService,
            IWebhookProcessService webhookProcessService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _diagnosticService = diagnosticService;
            _customerService = customerService;
            _providerService = providerService;
            _invoicesIssuedService = invoicesIssuedService;
            _invoicesReceivedService = invoicesReceivedService;
            _diagnosticDetailService = diagnosticDetailService;
            _diagnosticTaxStatusService = diagnosticTaxStatusService;
            _webhookProcessService = webhookProcessService;
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
            var authUser = Authenticator.AuthenticatedUser;
            var process = _webhookProcessService.FindBy(x => x.reference == authUser.Account.Uuid.ToString()).OrderByDescending(x => x.id).FirstOrDefault();
            if (process != null)
            {
                ViewBag.HasInvoiceSync = true;
                ViewBag.InvoiceSyncDate = process.createdAt;
            }
            else
            {
                ViewBag.HasInvoiceSync = false;
            }
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
                        economicActivities = x.economicActivities != null ? x.economicActivities.Split(',').ToList() : new List<string>(),
                        fiscalObligations = !string.IsNullOrEmpty(x.fiscalObligations) ? x.fiscalObligations : null,
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

                var diagnostic = new Diagnostic()
                {
                    uuid = Guid.NewGuid(),
                    account = account,
                    businessName = account.name,
                    commercialCAD = "",
                    plans = "",
                    createdAt = DateUtil.GetDateTimeNow(),
                    status = SystemStatus.ACTIVE.ToString(),
                };

                _diagnosticService.Create(diagnostic);
                
                List<DiagnosticDetail> details = new List<DiagnosticDetail>();

                DateTime dateFrom = DateTime.UtcNow.AddMonths(-3);
                DateTime dateTo = DateTime.UtcNow.AddMonths(-1);
                dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month)).AddDays(1).AddMilliseconds(-1);
                dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                var invoicesIssued = _invoicesIssuedService.FindBy(x => x.account.id == account.id && x.invoicedAt >= dateFrom && x.invoicedAt <= dateTo).ToList();

                details.AddRange(invoicesIssued
                    .GroupBy(x => new
                    {
                        x.invoicedAt.Year,
                        x.invoicedAt.Month,
                    })
                .Select(b => new DiagnosticDetail()
                {
                    diagnostic = diagnostic,
                    year = b.Key.Year,
                    month = b.Key.Month,
                    typeTaxPayer = TypeIssuerReceiver.ISSUER.ToString(),
                    numberCFDI = b.Count(),
                    totalAmount = b.Sum(y => y.total),
                    createdAt = DateUtil.GetDateTimeNow()
                }));

                var invoicesReceived = _invoicesReceivedService.FindBy(x => x.account.id == account.id && x.invoicedAt >= dateFrom && x.invoicedAt <= dateTo).ToList();

                details.AddRange(invoicesReceived
                    .Where(x => x.invoiceType != TipoComprobante.N.ToString()) //Si es solo ingreso en las factura descomentar esta linea
                    .GroupBy(x => new
                    {
                        x.invoicedAt.Year,
                        x.invoicedAt.Month,
                    })
                .Select(b => new DiagnosticDetail()
                {
                    diagnostic = diagnostic,
                    year = b.Key.Year,
                    month = b.Key.Month,
                    typeTaxPayer = TypeIssuerReceiver.RECEIVER.ToString(),
                    numberCFDI = b.Count(),
                    totalAmount = b.Sum(y => y.total),
                    createdAt = DateUtil.GetDateTimeNow()
                }));
                
                _diagnosticDetailService.Create(details);

                var taxStatusModel = SATService.GetTaxStatus(account.rfc, provider);
                if (!taxStatusModel.Success)
                    throw new Exception(taxStatusModel.Message);

                var taxStatus = taxStatusModel.TaxStatus
                    .Select(x => new DiagnosticTaxStatus
                    {
                        diagnostic = diagnostic,
                        createdAt = DateUtil.GetDateTimeNow(),
                        statusSAT = x.status,
                        businessName = x.person != null ? x.person.fullName :
                            (x.company != null ? (!string.IsNullOrEmpty(x.company.tradeName) ? x.company.tradeName : x.company.legalName) : string.Empty),
                        taxMailboxEmail = x.email,
                        taxRegime = x.taxRegimes.Count > 0 ? String.Join(",", x.taxRegimes.Select(y => y.name).ToArray()) : null,
                        economicActivities = x.economicActivities != null && x.economicActivities.Any() ? String.Join(",", x.economicActivities.Select(y => y.name).ToArray()) : null,
                        fiscalObligations = x.obligations != null && x.obligations.Any() ? String.Join(",", x.obligations.Select(y => y.description).ToArray()) : null,
                    }).ToList();
                _diagnosticTaxStatusService.Create(taxStatus);

                return Json(new { uuid = diagnostic.uuid, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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
                    return Json(new { success = false, finish = true, message = "Se generó un fallo durante la extracción" }, JsonRequestBehavior.AllowGet);
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
                        fiscalObligations = x.fiscalObligations != null ? x.fiscalObligations : null,
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
                       "Se descarga el diagnóstico del cliente " + authUser.Account.RFC,
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

        //Tiene detalles con el paginador de la extracción diaria, trae muchos registros y no solo lo del rango que se solicito
        [HttpGet]
        public ActionResult ExtractionDay()
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();

                var provider = ConfigurationManager.AppSettings["SATProvider"];
                DateTime dateFrom = DateTime.UtcNow.AddDays(-1);
                DateTime dateTo = DateTime.UtcNow;
                //dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month)).AddDays(1).AddMilliseconds(-1);
                //dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                
                //Se obtiene la extracción del día.
                string extractionId = SATService.GenerateExtractions(authUser.Account.RFC, dateFrom, dateTo, provider);

                var modelInvoices = SATService.GetInvoicesByExtractions(authUser.Account.RFC, dateFrom.ToString(), dateTo.ToString(), provider);

                List<string> customerRfcs = modelInvoices.Customers.Select(c => c.rfc).Distinct().ToList();
                var ExistC = _customerService.ValidateRFC(customerRfcs, account.id);
                List<string> NoExistC = customerRfcs.Except(ExistC).ToList();

                List<Customer> customers = modelInvoices.Customers
                    .Where(x => NoExistC.Contains(x.rfc)).GroupBy(x => new { x.rfc, x.businessName })
                    .Select(x => new Customer
                    {
                        uuid = Guid.NewGuid(),
                        account = account,
                        zipCode = x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault() != null ? x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault().zipCode : null,
                        businessName = x.Key.businessName,
                        rfc = x.Key.rfc,
                        createdAt = DateUtil.GetDateTimeNow(),
                        modifiedAt = DateUtil.GetDateTimeNow(),
                        status = SystemStatus.ACTIVE.ToString()
                    }).ToList();

                _customerService.Create(customers);

                //Crear proveedores
                List<string> providersRfcs = modelInvoices.Providers.Select(c => c.rfc).Distinct().ToList();
                var ExistP = _providerService.ValidateRFC(providersRfcs, account.id);
                List<string> NoExistP = providersRfcs.Except(ExistP).ToList();

                List<Provider> providers = modelInvoices.Providers
                    .Where(x => NoExistP.Contains(x.rfc)).GroupBy(x => new { x.rfc, x.businessName })
                    .Select(x => new Provider
                    {
                        uuid = Guid.NewGuid(),
                        account = account,
                        zipCode = x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault() != null ? x.Where(b => b.rfc == x.Key.rfc).FirstOrDefault().zipCode : null,
                        businessName = x.Key.businessName,
                        rfc = x.Key.rfc,
                                //taxRegime = 
                                createdAt = DateUtil.GetDateTimeNow(),
                        modifiedAt = DateUtil.GetDateTimeNow(),
                        status = SystemStatus.ACTIVE.ToString()
                    }).Distinct().ToList();

                _providerService.Create(providers);                

                List<string> IdIssued = modelInvoices.Customers.Select(x => x.idInvoice).ToList();
                var invoicesIssuedExist = _invoicesIssuedService.FindBy(x => x.account.id == account.id).Select(x => x.uuid.ToString()).ToList();

                IdIssued = IdIssued.Except(invoicesIssuedExist).ToList();

                /*Obtener los CFDI's*/
                var customersCFDI = SATService.GetCFDIs(IdIssued, provider);
                var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                List<InvoiceIssued> invoiceIssued = new List<InvoiceIssued>();
                foreach (var cfdi in customersCFDI)
                {
                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(cfdi.Xml);
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                    var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesIssued, account.rfc);

                    invoiceIssued.Add(new InvoiceIssued
                    {
                        uuid = Guid.Parse(cfdi.id),
                        folio = cfdi.Folio,
                        serie = cfdi.Serie,
                        paymentMethod = cfdi.MetodoPago,
                        paymentForm = cfdi.FormaPago,
                        currency = cfdi.Moneda,
                        iva = modelInvoices.Customers.FirstOrDefault(y => y.idInvoice == cfdi.id).tax,
                        invoicedAt = cfdi.Fecha,
                        xml = upload.Item1,
                        createdAt = DateTime.Now,
                        modifiedAt = DateTime.Now,
                        status = IssueStatus.STAMPED.ToString(),
                        account = account,
                        customer = _customerService.FirstOrDefault(y => y.rfc == cfdi.Receptor.Rfc),
                        invoiceType = cfdi.TipoDeComprobante,
                        subtotal = cfdi.SubTotal,
                        total = cfdi.Total,
                        homemade = false
                    });
                }

                _invoicesIssuedService.Create(invoiceIssued);

                List<string> IdReceived = modelInvoices.Providers.Select(x => x.idInvoice).ToList();
                var invoicesReceivedExist = _invoicesReceivedService.FindBy(x => x.account.id == account.id).Select(x => x.uuid.ToString()).ToList();

                IdReceived = IdReceived.Except(invoicesReceivedExist).ToList();

                /*Obtener los CFDI's*/
                var providersCFDI = SATService.GetCFDIs(IdReceived, provider);
                var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                List<InvoiceReceived> invoiceReceiveds = new List<InvoiceReceived>();
                foreach (var cfdi in providersCFDI)
                {
                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(cfdi.Xml);
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                    var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesReceived, account.rfc);

                    invoiceReceiveds.Add(new InvoiceReceived
                    {
                        uuid = Guid.Parse(cfdi.id),
                        folio = cfdi.Folio,
                        serie = cfdi.Serie,
                        paymentMethod = cfdi.MetodoPago,
                        paymentForm = cfdi.FormaPago,
                        currency = cfdi.Moneda,
                        iva = modelInvoices.Providers.FirstOrDefault(y => y.idInvoice == cfdi.id).tax,
                        invoicedAt = cfdi.Fecha,
                        xml = upload.Item1,
                        createdAt = DateTime.Now,
                        modifiedAt = DateTime.Now,
                        status = IssueStatus.STAMPED.ToString(),
                        account = account,
                        provider = _providerService.FirstOrDefault(y => y.rfc == cfdi.Emisor.Rfc),
                        invoiceType = cfdi.TipoDeComprobante,
                        subtotal = cfdi.SubTotal,
                        total = cfdi.Total,
                        homemade = false
                    });
                }

                _invoicesReceivedService.Create(invoiceReceiveds);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }

            return RedirectToAction("Invoice/InvoicesIssued");
        }
    }
}