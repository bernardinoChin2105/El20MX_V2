using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
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
        private IUseCFDIService _useCFDIService;
        private ICustomerService _customerService;
        private IProviderService _providerService;

        public InvoicingController(IAccountService accountService, ICustomsService customsService, ICustomsPatentService customsPatentService,
            ICustomsRequestNumberService customsRequestNumberService, ITypeInvoiceService typeInvoiceService, IUseCFDIService useCFDIService,
            ITypeRelationshipService typeRelationshipService, ITypeVoucherService typeVoucherService, ICurrencyService currencyService,
            IPaymentFormService paymentFormService, IPaymentMethodService paymentMethodService, ICustomerService customerService, IProviderService providerService)
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
            _useCFDIService = useCFDIService;

            _customerService = customerService;
            _providerService = providerService;
        }

        /*                                                              
            ListExchangeRate = new SelectList(list);
            ListPaymentConditions = new SelectList(list);

            //receptor
            ListCustomerEmail = new SelectList(list);
            ListMunicipality = new SelectList(list);
            ListColony = new SelectList(list);
            ListState = new SelectList(list);
            ListCountry = new SelectList(list);

            //cuenta y sucursales
            ListBranchOffice = new SelectList(list);
            ListEmailIssued = new SelectList(list);
            ProductServices = new List<ProductServiceDescriptionView>();
             */

        // GET: Invoicing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Invoice()
        {
            InvoiceViewModel model = new InvoiceViewModel();
            try
            {
                /*
                 Obtener información de la cuenta para los datos del cliente a facturar
                 */
                #region Información de email y sucursales del cliente
                //model.ListBranchOffice = ;
                //model.ListEmailIssued = ;
                //model.SerieFolio = ;
                #endregion

                #region Información de los catalogos para las facturas y datos fiscales
                model.ListTypeInvoices = _typeVoucherService.GetAll().Select(x => new SelectListItem
                {
                    Text = "(" + x.code + ") " + x.Description.ToString(),
                    Value = x.id.ToString()
                }).ToList();
                //model.ListTypeRelationship = ;
                //model.ListTypeVoucher = ;
                //model.ListUseCFDI = ;
                //model.ListPaymentForm = ;
                //model.ListPaymentMethod = ;
                //model.ListCurrency = ;
                //model.ListExchangeRate = ;
                //model.ListCustomsPatent = ;
                //model.ListCustoms = ;
                //model.ListMotionNumber = ;
                //model.ListPaymentConditions = ;

                #endregion

                //    //stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                //    //Tipo de Comprobante
                //    var TypeVoucher = Enum.GetValues(typeof(TipoComprobante)).Cast<TipoComprobante>()
                //        .Select(e => new SelectListItem
                //        {
                //            Value = e.ToString(),
                //            Text = EnumUtils.GetDescription(e)
                //        }).Where(x => x.Value != "N" & x.Value != "T");

                //    //Forma de pago
                //    var PaymentForm = Enum.GetValues(typeof(MetodoPago)).Cast<MetodoPago>()
                //       .Select(e => new SelectListItem
                //       {
                //           Value = e.ToString(),
                //           Text = EnumUtils.GetDescription(e)
                //       });

                //    //Metodo de pago
                //    var PaymentMethod = Enum.GetValues(typeof(MetodoPago)).Cast<MetodoPago>()
                //      .Select(e => new SelectListItem
                //      {
                //          Value = e.ToString(),
                //          Text = EnumUtils.GetDescription(e)
                //      });

                //    //Uso de CFDI
                //    var UseCFDI = Enum.GetValues(typeof(UsoCFDI)).Cast<UsoCFDI>()
                //      .Select(e => new SelectListItem
                //      {
                //          Value = e.ToString(),
                //          Text = EnumUtils.GetDescription(e)
                //      });

                //    //Moneda
                //    var Currency = Enum.GetValues(typeof(TypeCurrency)).Cast<TypeCurrency>()
                //      .Select(e => new SelectListItem
                //      {
                //          Value = e.ToString(),
                //          Text = EnumUtils.GetDescription(e)
                //      });

            }
            catch (Exception ex)
            {
                //string error = ex.Message.ToString();
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }
    }
}