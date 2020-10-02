using MVC_Project.Domain.Services;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class QuotationController : Controller
    {

        private QuotationService _quotationService;

        public QuotationController(QuotationService quotationService)
        {
            _quotationService = quotationService;
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
                    data.id = quotation.id;
                    data.account = quotation.account.name + "( "+ quotation.account.rfc +" )";
                    data.total = quotation.total;
                    data.partialitiesNumber = quotation.partialitiesNumber;
                    data.status = quotation.status;
                    data.quoteLink = quotation.quoteLink;
                    data.quoteName = quotation.quoteName;
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

    }
}