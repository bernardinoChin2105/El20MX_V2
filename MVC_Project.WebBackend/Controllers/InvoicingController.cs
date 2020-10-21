﻿using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class InvoicingController : Controller
    {
        private IAccountService _accountService;
        private ICurrencyService _currencyService;
        private ICustomsService _customsService;
        private ICustomsPatentService _customsPatentService;
        private ICustomsRequestNumberService _customsRequestNumberService;
        private IPaymentFormService _paymentFormService;
        private IPaymentMethodService _paymentMethodService;
        private ITypeInvoiceService _typeInvoiceService;
        private ITypeRelationshipService _typeRelationShipService;
        private ITypeVoucherService _typeVoucherService;
        private ITaxRegimeService _taxRegimeService;
        private IUseCFDIService _useCFDIService;
        private ICustomerService _customerService;
        private IProviderService _providerService;
        private IBranchOfficeService _branchOfficeService;

        public InvoicingController(IAccountService accountService, ICustomsService customsService, ICustomsPatentService customsPatentService,
            ICustomsRequestNumberService customsRequestNumberService, ITypeInvoiceService typeInvoiceService, IUseCFDIService useCFDIService,
            ITypeRelationshipService typeRelationshipService, ITypeVoucherService typeVoucherService, ICurrencyService currencyService,
            IPaymentFormService paymentFormService, IPaymentMethodService paymentMethodService, ICustomerService customerService,
            IProviderService providerService, IBranchOfficeService branchOfficeService, ITaxRegimeService taxRegimeService)
        {
            _accountService = accountService;
            _currencyService = currencyService;
            _customsService = customsService;
            _customsPatentService = customsPatentService;
            _customsRequestNumberService = customsRequestNumberService;
            _paymentFormService = paymentFormService;
            _paymentMethodService = paymentMethodService;
            _typeInvoiceService = typeInvoiceService;
            _typeRelationShipService = typeRelationshipService;
            _typeVoucherService = typeVoucherService;
            _taxRegimeService = taxRegimeService;
            _useCFDIService = useCFDIService;

            _customerService = customerService;
            _providerService = providerService;
            _branchOfficeService = branchOfficeService;
        }

        // GET: Invoicing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Invoice()
        {
            InvoiceViewModel model = new InvoiceViewModel();
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                //obtener información de mi emisor                
                var account = authUser.Account;
                model.IssuingRFC = account.RFC;
                model.BusinessName = account.Name;
                model.IssuingTaxEmail = authUser.Email; //Preguntar si este sería o buscaar el fiscal
                //model.IssuingTaxRegime = ""; //Faltan estos datos del cliente
                //model.IssuingTaxRegimeId = "";//Faltan estos datos del cliente

                //Obtener listas de los combos
                SetCombos(ref model);
            }
            catch (Exception ex)
            {
                //string error = ex.Message.ToString();
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }

        private void SetCombos(ref InvoiceViewModel model)
        {
            model.ListTaxRegime = _taxRegimeService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListTypeInvoices = _typeVoucherService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListTypeRelationship = _typeRelationShipService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListBranchOffice = _branchOfficeService.GetAll().Select(x => new SelectListItem
            {
                Text = x.name.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListUseCFDI = _useCFDIService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListPaymentForm = _paymentFormService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListPaymentMethod = _paymentMethodService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListCurrency = _currencyService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.id.ToString()
            }).ToList();
        }

        public JsonResult GetSerieFolio(Int64 sucursalId)
        {
            try
            {
                var branchOffice = _branchOfficeService.GetById(sucursalId);
                if (branchOffice == null)
                    throw new Exception("Sucursal no encontrada en el sistema");

                return Json(new { success = true, serie = branchOffice.serie, folio = branchOffice.folio }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region
        //Busca a los cliente o proveedores por rfc o razon social
        public JsonResult GetSearchReceiver(string field, string value, string typeInvoice)
        {
            bool success = false;
            string message = string.Empty;
            List<ListCustomersAC> result = new List<ListCustomersAC>();
            try
            {
                ReceiverFilter filters = new ReceiverFilter();
                if (typeInvoice != "-1") filters.typeInvoice = typeInvoice;

                if (value != "")
                {
                    if (field == "RFC")
                    {
                        filters.rfc = value;
                    }
                    else if (field == "Name")
                    {
                        filters.businessName = value;
                    }
                }

                result = _customerService.ReceiverSearchList(filters);
                if (result != null)
                {
                    success = true;
                }       
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message.ToString();
            }

            return Json(new { success = success, message = message, data = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReceiverInformation(Int64 AccountId)
        {
            bool success = false;
            string message = string.Empty;
            try
            {
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message.ToString();

            }
            return Json(new { success = success, message = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}