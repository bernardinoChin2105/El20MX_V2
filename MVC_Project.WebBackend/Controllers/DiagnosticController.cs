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
                    List<InvoicesGroup> invoicePeriod = diagDetails.GroupBy(x => new
                    {
                        x.year,
                        x.month,
                    })
                    .Select(b => new InvoicesGroup
                    {
                        year = b.Key.year,
                        month = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(b.Key.month),
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

        public ActionResult GenerateDx0()
        {
            Diagnostic diagn = new Diagnostic();
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();

                //Paso1 crear la extracción del periodo y retorna la información del contribuyente
                //Ajustar tiempo para que la información sea más precisa
                //Estos ajuste de horario solo funcion para satws, para otros proveedores dependera del horario que manejen
                //https://api.sandbox.sat.ws/extractions 
                //DateTime today = DateTime.Now;
                DateTime todayUTC = DateTime.UtcNow; // se utiliza esté tipo de horario porque SAT.ws maneja esos horarios en sus registros
                DateTime dateFrom = todayUTC.AddMonths(-4);
                DateTime dateTo = todayUTC.AddMonths(-1);
                dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month)).AddDays(1).AddMilliseconds(-1);
                dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);

                var modelInvoices = SATwsService.GetInformationByExtractions(authUser.Account.RFC, dateFrom, dateTo);
                if (!modelInvoices.Success)
                    throw new Exception(modelInvoices.Message);

                //crear clientes
                List<string> customerRfcs = modelInvoices.Customers.Select(c => c.rfc).Distinct().ToList();
                var ExistC = _customerService.ValidateRFC(customerRfcs, authUser.Account.Id);
                List<string> NoExistC = customerRfcs.Except(ExistC).ToList();

                if (NoExistC.Count > 0)
                {
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
                }

                //crear proveedores
                List<string> providersRfcs = modelInvoices.Providers.Select(c => c.rfc).Distinct().ToList();
                var ExistP = _providerService.ValidateRFC(providersRfcs, authUser.Account.Id);
                List<string> NoExistP = providersRfcs.Except(ExistP).ToList();

                if (NoExistP.Count > 0)
                {
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
                }

                #region Realizar el guardado de las facturas
                List<string> IdIssued = modelInvoices.Customers.Select(x => x.idInvoice).ToList();

                /*Obtener los CFDI's*/
                var customersCFDI = SATwsService.GetInvoicesCFDI(IdIssued);

                if (customersCFDI.Count > 0)
                {
                    List<InvoiceIssued> invoiceIssued = customersCFDI.Select(x => new InvoiceIssued
                    {
                        uuid = Guid.NewGuid(),
                        folio = x.Folio,
                        serie = x.Serie,
                        paymentMethod = x.MetodoPago,
                        paymentForm = x.FormaPago,
                        currency = x.Moneda,
                        amount = x.SubTotal,
                        iva = modelInvoices.Customers.FirstOrDefault(y => y.idInvoice == x.id).tax,
                        totalAmount = x.Total,
                        invoicedAt = x.Fecha,
                        xml = x.Xml,
                        createdAt = todayUTC,
                        modifiedAt = todayUTC,
                        status = SystemStatus.ACTIVE.ToString(),
                        account = account,
                        customer = _customerService.FirstOrDefault(y => y.rfc == x.Receptor.Rfc)
                    }).ToList();
                    _invoicesIssuedService.Create(invoiceIssued);
                }

                List<string> IdReceived = modelInvoices.Providers.Select(x => x.idInvoice).ToList();

                /*Obtener los CFDI's*/
                var providersCFDI = SATwsService.GetInvoicesCFDI(IdReceived);

                if (providersCFDI.Count > 0)
                {
                    List<InvoiceReceived> invoiceReceiveds = providersCFDI.Select(x => new InvoiceReceived
                    {
                        uuid = Guid.NewGuid(),
                        folio = x.Folio,
                        serie = x.Serie,
                        paymentMethod = x.MetodoPago,
                        paymentForm = x.FormaPago,
                        currency = x.Moneda,
                        amount = x.SubTotal,
                        iva = modelInvoices.Providers.FirstOrDefault(y => y.idInvoice == x.id).tax,
                        totalAmount = x.Total,
                        invoicedAt = x.Fecha,
                        xml = x.Xml,
                        createdAt = todayUTC,
                        modifiedAt = todayUTC,
                        status = SystemStatus.ACTIVE.ToString(),
                        account = account,
                        provider = _providerService.FirstOrDefault(y => y.rfc == x.Emisor.Rfc)
                    }).ToList();
                    _invoicesReceivedService.Create(invoiceReceiveds);
                }

                #endregion 

                //separar los cfdis por año, mes, recibidas, emitidas
                List<InvoicesGroup> invoicePeriod = modelInvoices.Invoices.GroupBy(x => new
                {
                    x.issuedAt.Year,
                    x.issuedAt.Month,
                })
                .Select(b => new InvoicesGroup
                {
                    year = b.Key.Year,
                    month = DateUtil.GetMonthName(new DateTime(b.Key.Year, b.Key.Month, 1), "es"),
                    issuer = new IssuerReceiverGroup()
                    {
                        type = TypeIssuerReceiver.ISSUER.ToString(),
                        amountTotal = b.Where(x => x.issuer.rfc == authUser.Account.RFC).Sum(x => x.total),
                        numberTotal = b.Where(x => x.issuer.rfc == authUser.Account.RFC).Count(),
                    },
                    receiver = new IssuerReceiverGroup()
                    {
                        type = TypeIssuerReceiver.RECEIVER.ToString(),
                        amountTotal = b.Where(x => x.receiver.rfc == authUser.Account.RFC).Sum(x => x.total),
                        numberTotal = b.Where(x => x.receiver.rfc == authUser.Account.RFC).Count(),
                    }
                }).ToList();

                //pendiente generara la información del CAD


                List<DiagnosticDetail> details = new List<DiagnosticDetail>();
                //Armado de modelos para guardar información
                diagn = new Diagnostic()
                {
                    uuid = Guid.NewGuid(),
                    account = account,
                    businessName = modelInvoices.Taxpayer.name,
                    commercialCAD = "",
                    plans = "",
                    createdAt = DateUtil.GetDateTimeNow()
                };

                //Obtener la constancia Fiscal
                try
                {
                    var taxStatusModel = SATwsService.GetTaxStatus(authUser.Account.RFC);
                    if (!taxStatusModel.Success)
                        throw new Exception(taxStatusModel.Message);

                    var taxStatus = taxStatusModel.TaxStatus
                        .Select(x => new DiagnosticTaxStatus
                        {
                            diagnostic = diagn,
                            createdAt = DateUtil.GetDateTimeNow(),
                            statusSAT = x.status,
                            businessName = x.person != null ? x.person.fullName : x.company.tradeName,
                            taxMailboxEmail = x.email,
                            taxRegime = x.taxRegimes.Count > 0 ? String.Join(",", x.taxRegimes.Select(y => y.name).ToArray()) : null,
                            economicActivities = x.economicActivities.Count > 0 ? String.Join(",", x.economicActivities.Select(y => y.name).ToArray()) : null
                                //fiscalObligations = ""
                            }).ToList();

                    diagn.taxStatus = taxStatus;
                }
                catch (Exception ex)
                {
                    string error = ex.Message.ToString();
                }

                foreach (var item in invoicePeriod)
                {
                    DiagnosticDetail detail = new DiagnosticDetail()
                    {
                        diagnostic = diagn,
                        year = item.year,
                        month = item.month,
                        typeTaxPayer = TypeIssuerReceiver.ISSUER.ToString(),
                        numberCFDI = item.issuer.numberTotal,
                        totalAmount = item.issuer.amountTotal,
                        createdAt = DateUtil.GetDateTimeNow()
                    };
                    diagn.details.Add(detail);

                    DiagnosticDetail detailR = new DiagnosticDetail()
                    {
                        diagnostic = diagn,
                        year = item.year,
                        month = item.month,
                        typeTaxPayer = TypeIssuerReceiver.RECEIVER.ToString(),
                        totalAmount = item.receiver.amountTotal,
                        numberCFDI = item.receiver.numberTotal,
                        createdAt = DateUtil.GetDateTimeNow()
                    };
                    diagn.details.Add(detailR);
                }

                _diagnosticService.Create(diagn);
                MensajeFlashHandler.RegistrarMensaje("¡Diagnóstico realizado!", TiposMensaje.Success);
                return RedirectToAction("DiagnosticDetail", new { id = diagn.uuid.ToString() });
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                //MensajeFlashHandler.RegistrarMensaje("¡Ocurrio un error en al realizar el diagnóstico!", TiposMensaje.Error);
                MensajeFlashHandler.RegistrarMensaje(error, TiposMensaje.Error);
                return RedirectToAction("Index");
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
                    Data = new { success = false, message = ex.Message },
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
                    List<InvoicesGroup> invoicePeriod = diagDetails.GroupBy(x => new
                    {
                        x.year,
                        x.month,
                    })
                    .Select(b => new InvoicesGroup
                    {
                        year = b.Key.year,
                        month = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(b.Key.month),
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

                MensajeFlashHandler.RegistrarMensaje("Descargando...", TiposMensaje.Success);
                string rfc = authUser.Account.RFC;
                //PageSize = Rotativa.Options.Size.Letter, 
                //return View(model);
                return new Rotativa.ViewAsPdf("DiagnosticZeroDownload", model) { FileName = rfc + "_D0.pdf" };
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                       "Error al descargar el diagnostico: " + ex.Message.ToString(),
                       ENivelLog.Error,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View();
            }

        }
    }
}