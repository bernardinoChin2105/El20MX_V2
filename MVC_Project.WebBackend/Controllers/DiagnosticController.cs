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
using Newtonsoft.Json.Linq;
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

        public DiagnosticController(IAccountService accountService, ICredentialService credentialService,
            IDiagnosticService diagnosticService, ICustomerService customerService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _diagnosticService = diagnosticService;
            _customerService = customerService;
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

                var diagTax = diagnostic.taxStatus.FirstOrDefault();
                if (diagTax != null)
                {
                    model.statusSAT = diagTax.statusSAT;
                    model.taxRegime = diagTax.taxRegime;
                    model.economicActivities = diagTax.economicActivities;
                    model.fiscalObligations = diagTax.fiscalObligations;
                    model.taxMailboxEmail = diagTax.taxMailboxEmail;
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
            }

            return View(model);
        }

        public ActionResult GenerateDx0()
        {
            Diagnostic diagn = new Diagnostic();
            try
            {
                var authUser = Authenticator.AuthenticatedUser;
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

                ExtractionsFilter filter = new ExtractionsFilter()
                {
                    taxpayer = "/taxpayers/" + authUser.Account.RFC,
                    extractor = "invoice",
                    periodFrom = dateFrom.ToString("s") + "Z",
                    periodTo = dateTo.ToString("s") + "Z"
                };

                var responseExtraction = SATws.CallServiceSATws("extractions", filter, "Post");
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseExtraction);
                var option = model.First(x => x.Key == "taxpayer").Value;
                JObject rItemValueJson = (JObject)option;
                TaxpayerInfo infoTaxpayer = JsonConvert.DeserializeObject<TaxpayerInfo>(rItemValueJson.ToString());


                //paso2 llamar los cfdis de los que voy a usar
                //https://api.sandbox.sat.ws/invoices/{cfdi} // retorna el item de la lista
                //https://api.sandbox.sat.ws/taxpayers/{id}/invoices  //lista de todos los cfdi's
                //https://api.sat.ws/invoices/{cfdi}/cfdi // retonna el cfdi original

                //2020-06-01 06:00:00&issuedAt[before]=2020-06-30 23:59:59
                string from = dateFrom.ToString("yyyy-MM-dd HH:mm:ss");
                string to = dateTo.ToString("yyyy-MM-dd HH:mm:ss");
                string url = "/taxpayers/" + authUser.Account.RFC + "/invoices?issuedAt[after]=" + from + "&issuedAt[before]=" + to;
                var responseInvoices = SATws.CallServiceSATws(url, null, "Get");
                var modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(responseInvoices);

                #region Obtener la información de los clientes y proveedores para guardarlos
                foreach (var item in modelInvoices)
                {
                    try
                    {
                        string cfdi = "/invoices/" + item.id + "/cfdi";
                        var responseCFDI = SATws.CallServiceSATws(cfdi, null, "Get");
                        var modelCFDI = JsonConvert.DeserializeObject<IDictionary<string, object>>(responseCFDI);

                        var zipCode = (String)modelCFDI["LugarExpedicion"];

                        //Estamos buscando mis clientes
                        if (item.issuer.rfc == authUser.Account.RFC)
                        {

                            Customer customer = new Customer()
                            {
                                uuid = Guid.NewGuid(),
                                account = account,
                                zipCode = zipCode,
                                createdAt = DateUtil.GetDateTimeNow(),
                                modifiedAt = DateUtil.GetDateTimeNow(),
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            //var myCustomer = (modelCFDI.GetType().GetProperty("Receptor").GetValue(modelCFDI, null));                        
                            var myCustomer = JsonConvert.DeserializeObject<IDictionary<string, object>>(modelCFDI["Receptor"].ToString());
                            customer.businessName = (String)myCustomer["Nombre"];
                            customer.rfc = (String)myCustomer["Rfc"];

                            var validRFC = _customerService.FindBy(x => x.rfc == customer.rfc).FirstOrDefault();
                            if (validRFC == null)
                            {
                                //Complemento -> Receptor
                                _customerService.Create(customer);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                    }


                    ////Estamos buscando mis proveedores
                    //if (item.receiver.rfc == authUser.Account.RFC)
                    //{
                    //    var myProvider = (modelCFDI.GetType().GetProperty("Emisor").GetValue(modelCFDI, null));
                    //    customer.regimen = (String)(myProvider.GetType().GetProperty("RegimenFiscal").GetValue(myProvider, null));
                    //    customer.businessName = (String)(myProvider.GetType().GetProperty("Nombre").GetValue(myProvider, null));
                    //    customer.rfc = (String)(myProvider.GetType().GetProperty("Rfc").GetValue(myProvider, null));
                    //}

                }
                #endregion

                //separar los cfdis por año, mes, recibidas, emitidas
                List<InvoicesGroup> invoicePeriod = modelInvoices.GroupBy(x => new
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
                    businessName = infoTaxpayer.name,
                    commercialCAD = "",
                    plans = "",
                    createdAt = DateUtil.GetDateTimeNow()
                };

                DiagnosticTaxStatus taxStatus = new DiagnosticTaxStatus()
                {
                    diagnostic = diagn,
                    businessName = infoTaxpayer.name,
                    createdAt = DateUtil.GetDateTimeNow()
                    //statusSAT = "",
                    //taxRegime = "",
                    //economicActivities = "",
                    //fiscalObligations = "",
                    //taxMailboxEmail = "",
                };

                diagn.taxStatus.Add(taxStatus);

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

                var diagTax = diagnostic.taxStatus.FirstOrDefault();
                if (diagTax != null)
                {
                    model.statusSAT = diagTax.statusSAT;
                    model.taxRegime = diagTax.taxRegime;
                    model.economicActivities = diagTax.economicActivities;
                    model.fiscalObligations = diagTax.fiscalObligations;
                    model.taxMailboxEmail = diagTax.taxMailboxEmail;
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