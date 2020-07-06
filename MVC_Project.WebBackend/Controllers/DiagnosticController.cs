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

namespace MVC_Project.WebBackend.Controllers
{
    public class DiagnosticController : Controller
    {
        //private IUserService _userService;
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IDiagnosticService _diagnosticService;


        // IUserService userService,
        public DiagnosticController(IAccountService accountService, ICredentialService credentialService, IDiagnosticService diagnosticService)
        {
            //_userService = userService;
            _accountService = accountService;
            _credentialService = credentialService;
            _diagnosticService = diagnosticService;
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

                //Paso1 crear la extracción del periodo y retorna la información del contribuyente
                //Ajustar tiempo para que la información sea más precisa
                //Estos ajuste de horario solo funcion para satws, para otros proveedores dependera del horario que manejen
                //https://api.sandbox.sat.ws/extractions 
                DateTime today = DateTime.Now;
                DateTime dateFrom = today.AddMonths(-4);
                DateTime dateTo = today.AddMonths(-1);
                dateTo = new DateTime(dateTo.Year, dateTo.Month, DateTime.DaysInMonth(dateTo.Year, dateTo.Month));
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
                var responseCFDIS = SATws.CallServiceSATws("/taxpayers/" + authUser.Account.RFC + "/invoices", null, "Get");
                var modelInvoices = JsonConvert.DeserializeObject<List<InvoicesInfo>>(responseCFDIS);

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

                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();
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

            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }

            return View("DiagnosticDetail", diagn.uuid.ToString());
        }

        [HttpGet, AllowAnonymous]
        public JsonResult ObtenerDiagnostic(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;

                filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
                var filters = filtros.Split(',').ToList();

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (filters[0] != "") pagination.CreatedOnStart = Convert.ToDateTime(filters[0]);
                if (filters[1] != "") pagination.CreatedOnEnd = Convert.ToDateTime(filters[1]);

                var DiagnosticsResponse = _diagnosticService.DiagnosticList(userAuth.Account.Uuid.ToString(), pagination);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                int totalDisplay = DiagnosticsResponse[0].Total;

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = totalDisplay,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = DiagnosticsResponse
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
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

            string rfc = authUser.Account.RFC;
            //PageSize = Rotativa.Options.Size.Letter, 
            //return View(model);
            return new Rotativa.ViewAsPdf("DiagnosticZeroDownload", model) {FileName = rfc + "_D0.pdf" };
        }
    }
}