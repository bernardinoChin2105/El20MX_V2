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


            return View();
        }

        public ActionResult GenerateDx0()
        {
            try
            {
                //Información del sat datos del usuario
                //Obtener obtener información por periodos
                //obtener numero de cfdis emitidas(cliente)
                //Obtener numero de cfdis recibidas(proveedores)
                //Obtener el año y meses
                //Obtener montos de los cfdis

                var authUser = Authenticator.AuthenticatedUser;

                //paso1 Obtener los datos del cliente
                //https://api.sandbox.sat.ws/links
                //https://api.sandbox.sat.ws/credentials/{id}
                //https://api.sat.ws/links/90d3053f-2a35-4154-871d-bc4161de3f50
                //var responseInfoUser = SATws.CallServiceSATws("links/{id}", null, "Get");
                //var model = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);

                //Paso1 crear la extracción del periodo
                //https://api.sandbox.sat.ws/extractions //crear extracción
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

                //falta probar si esto mapea
                var option = model.First(x => x.Key == "taxpayer").Value;

                JObject rItemValueJson = (JObject)option;
                TaxpayerInfo infoTaxpayer = JsonConvert.DeserializeObject<TaxpayerInfo>(rItemValueJson.ToString());

                //TaxpayerInfo infoTaxpayer = new TaxpayerInfo();
                //infoTaxpayer.id = model.First(x => x.Key == "id").Value.ToString();

                //paso2 llamar los cfdis de los que voy a usar
                //https://api.sandbox.sat.ws/invoices/cbefca30-abc3-47ec-9744-4bb6e09b4092
                //https://api.sandbox.sat.ws/taxpayers/{id}/invoices  //lista de todos los cfdi's
                //https://api.sat.ws/invoices/8ce08c2e-6113-4c4a-890e-6459df90a337/cfdi
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

                //Preguntar como se generara la información del CAD

                Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();
                List<DiagnosticDetail> details = new List<DiagnosticDetail>();
                //Guardar información
                Diagnostic diagn = new Diagnostic()
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

            return View("DiagnosticDetail");
        }

        [HttpGet, AllowAnonymous]
        public JsonResult ObtenerDiagnostic(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                
                //Falta asignar los filtros en el modelo
                filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
                var filters = filtros.Split(',').ToList();

                var pagination = new BasePagination();                
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (filters[0] != "") pagination.CreatedOnStart = Convert.ToDateTime(filters[0]);
                if (filters[1] != "") pagination.CreatedOnEnd = Convert.ToDateTime(filters[1]);

                var DiagnosticsResponse = _diagnosticService.DiagnosticList(userAuth.Account.Uuid.ToString(), pagination);

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
    }
}