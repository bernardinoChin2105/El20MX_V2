using MVC_Project.Domain.Model;
using MVC_Project.Domain.Entities;
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
using Newtonsoft.Json;
using MVC_Project.Integrations.SAT;
using System.Configuration;

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
        private IInvoiceIssuedService _invoiceIssuedService;
        private IInvoiceReceivedService _invoiceReceivedService;
        private IProductServiceKeyService _productServiceKeyService;
        private ICountryService _countryService;
        private IStateService _stateService;
        private IDriveKeyService _driveKeyService;

        public InvoicingController(IAccountService accountService, ICustomsService customsService, ICustomsPatentService customsPatentService,
            ICustomsRequestNumberService customsRequestNumberService, ITypeInvoiceService typeInvoiceService, IUseCFDIService useCFDIService,
            ITypeRelationshipService typeRelationshipService, ITypeVoucherService typeVoucherService, ICurrencyService currencyService,
            IPaymentFormService paymentFormService, IPaymentMethodService paymentMethodService, ICustomerService customerService,
            IProviderService providerService, IBranchOfficeService branchOfficeService, ITaxRegimeService taxRegimeService,
            IInvoiceIssuedService invoiceIssuedService, IInvoiceReceivedService invoiceReceivedService, IDriveKeyService driveKeyService,
            IProductServiceKeyService productServiceKeyService, ICountryService countryService, IStateService stateService)

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
            _invoiceIssuedService = invoiceIssuedService;
            _invoiceReceivedService = invoiceReceivedService;
            _productServiceKeyService = productServiceKeyService;
            _countryService = countryService;
            _stateService = stateService;
            _driveKeyService = driveKeyService;
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

        #region Guardar información para timbrar 
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Invoice(InvoiceViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();


                if (model.TypeInvoice != "E")
                {
                    //Validar que se exista el receptor
                    if (model.CustomerId > 0)
                    {
                        //Poder guardar el cliente si no existe
                    }

                    InvoiceIssued invoice = new InvoiceIssued()
                    {
                        uuid = Guid.NewGuid(),
                        folio = model.Folio,
                        serie = model.Serie,
                        paymentMethod = model.PaymentMethod,
                        paymentForm = model.PaymentForm,
                        currency = model.Currency,
                        invoiceType = model.TypeInvoice,
                        total = model.Total,
                        subtotal = model.Subtotal,
                        account = new Account() { id = authUser.Account.Id },
                        customer = new Customer() { id = model.CustomerId },

                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        json = JsonConvert.SerializeObject(model),
                        status = IssueStatus.SAVED.ToString(), // para saber si esta guardado
                    };
                    //validar que exista la el iva para guardar
                    //iva = model.TaxWithheldIVA,

                    _invoiceIssuedService.Create(invoice);
                }
                else
                {
                    if (model.CustomerId > 0)
                    {
                        //Poder guardar el cliente si no existe
                    }

                    InvoiceReceived invoice = new InvoiceReceived()
                    {
                        uuid = Guid.NewGuid(),
                        folio = model.Folio,
                        serie = model.Serie,
                        paymentMethod = model.PaymentMethod,
                        paymentForm = model.PaymentForm,
                        currency = model.Currency,
                        invoiceType = model.TypeInvoice,
                        total = model.Total,
                        subtotal = model.Subtotal,
                        account = new Account() { id = authUser.Account.Id },
                        provider = new Provider() { id = model.CustomerId },

                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        json = JsonConvert.SerializeObject(model),
                        status = IssueStatus.SAVED.ToString(), // para saber si esta guardado
                    };
                    //validar que exista la el iva para guardar
                    //iva = model.TaxWithheldIVA,

                    _invoiceReceivedService.Create(invoice);
                }

                MensajeFlashHandler.RegistrarMensaje("Factura Guardada", TiposMensaje.Success);
                return RedirectToAction("InvoicesSaved");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }
        #endregion

        private void SetCombos(ref InvoiceViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;

            model.ListTaxRegime = _taxRegimeService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListTypeInvoices = _typeVoucherService.GetAll().Where(x => x.code != "N").Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListTypeRelationship = _typeRelationShipService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListBranchOffice = _branchOfficeService.FindBy(x => x.account.id == authUser.Account.Id).Select(x => new SelectListItem
            {
                Text = x.name.ToString(),
                Value = x.id.ToString()
            }).ToList();

            model.ListUseCFDI = _useCFDIService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListPaymentForm = _paymentFormService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListPaymentMethod = _paymentMethodService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.Description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListCurrency = _currencyService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListCustoms = _customsService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListCustomsPatent = _customsPatentService.GetAll().Select(x => new SelectListItem
            {
                Text = x.code.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListMotionNumber = _customsRequestNumberService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.patent.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListWithholdings = Enum.GetValues(typeof(TypeRetention)).Cast<TypeRetention>()
                   .Select(e => new SelectListItem
                   {
                       Value = e.ToString(),
                       Text = EnumUtils.GetDescription(e)
                   }).ToList();

            model.ListValuation = Enum.GetValues(typeof(TypeValuation)).Cast<TypeValuation>()
                   .Select(e => new SelectListItem
                   {
                       Value = ((int)e).ToString(),
                       Text = EnumUtils.GetDescription(e)
                   }).ToList();

            model.ListTransferred = Enum.GetValues(typeof(TypeTransferred)).Cast<TypeTransferred>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDescription(e)
                    }).ToList();

            model.ListTransferred = Enum.GetValues(typeof(TypeTransferred)).Cast<TypeTransferred>()
                   .Select(e => new SelectListItem
                   {
                       Value = e.ToString(),
                       Text = EnumUtils.GetDescription(e)
                   }).ToList();

            model.ListCountry = _countryService.GetAll().Select(x => new SelectListItem
            {
                Text = x.nameCountry.ToString(),
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

        #region busquedas de información 
        //Busca a los cliente o proveedores por rfc o razon social
        public JsonResult GetSearchReceiver(string field, string value, string typeInvoice)
        {
            var authUser = Authenticator.AuthenticatedUser;
            bool success = false;
            string message = string.Empty;
            List<ListCustomersProvider> result = new List<ListCustomersProvider>();
            try
            {
                ReceiverFilter filters = new ReceiverFilter()
                {
                    uuid = authUser.Account.Id.ToString()
                };

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
                    //Validar que los datos no sean vacios por los nombres
                    result = result.Select(c =>
                    {
                        c.businessName = (c.first_name != null || c.last_name != null ? c.first_name + " " + c.last_name : c.businessName);
                        return c;
                    }).ToList();

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

        //obtenet la información del cliente
        public JsonResult GetReceiverInformation(Int64 id, string type)
        {
            bool success = false;
            string message = string.Empty;
            CustomerViewModel receiver = new CustomerViewModel();
            //Object receiver = new object();
            try
            {
                if (type == "customer")
                {
                    var customer = _customerService.FirstOrDefault(x => x.id == id);

                    if (customer != null)
                    {
                        receiver = new CustomerViewModel()
                        {
                            Id = customer.id,
                            BusinessName = customer.businessName != null ? customer.businessName : customer.firstName + " " + customer.lastName,
                            RFC = customer.rfc,
                            Street = customer.street,
                            OutdoorNumber = customer.outdoorNumber,
                            InteriorNumber = customer.interiorNumber,
                            Colony = customer.colony,
                            ZipCode = customer.zipCode,
                            Municipality = customer.municipality,
                            State = customer.state,
                            Country = customer.country,
                            Emails = customer.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new CustomerContactsViewModel
                            {
                                Id = x.id,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList()
                        };
                    }
                }
                else
                {
                    var provider = _providerService.FirstOrDefault(x => x.id == id);

                    if (provider != null)
                    {
                        receiver = new CustomerViewModel()
                        {
                            Id = provider.id,
                            BusinessName = provider.businessName,
                            RFC = provider.rfc,
                            Street = provider.street,
                            OutdoorNumber = provider.outdoorNumber,
                            InteriorNumber = provider.interiorNumber,
                            Colony = provider.colony,
                            ZipCode = provider.zipCode,
                            Municipality = provider.municipality,
                            State = provider.state,
                            Country = provider.country,
                            Emails = provider.providerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new CustomerContactsViewModel
                            {
                                Id = x.id,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList()
                        };
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message.ToString();

            }
            return Json(new { success = success, message = message, data = receiver }, JsonRequestBehavior.AllowGet);
        }

        //Obtener listado de productos por codigo
        public JsonResult GetProdServSAT(string Concept)
        {
            bool success = false;
            string message = string.Empty;
            List<ProductServiceKeyViewModel> result = new List<ProductServiceKeyViewModel>();
            try
            {
                result = _productServiceKeyService.GetProdServ(Concept);

                if (result != null)
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }

            return Json(new { success = success, message = message, data = result }, JsonRequestBehavior.AllowGet);
        }

        //Obtener listado de unidad del SAT
        public JsonResult GetUnitSAT(string Clave)
        {
            bool success = false;
            string message = string.Empty;
            List<DriveKeyViewModel> result = new List<DriveKeyViewModel>();
            try
            {
                result = _driveKeyService.GetDriveKey(Clave);

                if (result != null)
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }

            return Json(new { success = success, message = message, data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetLocations(string zipCode)
        {
            try
            {
                var listResponse = _stateService.GetLocationList(zipCode);

                return Json(new
                {
                    Data = new { success = true, data = listResponse },
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        public ActionResult IssueIncomeInvoice(InvoiceViewModel model)
        {
            bool success = false;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();
                var provider = ConfigurationManager.AppSettings["SATProvider"];

                var issuer = new InvoiceIssuer
                {
                    Rfc = model.IssuingRFC,
                    Nombre = model.BusinessName,
                    RegimenFiscal = model.IssuingTaxRegime,
                };

                var receiver = new InvoiceReceiver
                {
                    Rfc = model.RFC,
                    Nombre = model.CustomerName,
                    //ResidenciaFiscal = model.Street, //tengo dudas de que dato va
                    //NumRegIdTrib = model, // que dato va
                    UsoCFDI = model.UseCFDI
                };

                var conceptos = new List<ConceptsData>();
                foreach (var item in model.ProductServices)
                {
                    var conceptsData = new ConceptsData
                    {
                        ClaveProdServ = item.SATCode.ToString(),
                        //NoIdentificacion = que dato es?
                        Cantidad = item.Quantity,
                        ClaveUnidad = item.SATUnit,
                        //Unidad  = que dato es?
                        Descripcion = item.ProductServiceDescription,
                        ValorUnitario = item.UnitPrice,
                        Descuento = item.DiscountRateProServ,
                        Importe = item.Subtotal,
                        CuentaPredial =  new CuentaPredial { Numero = model.PropertyAccountNumber },

                        //dudas por el llenado de datos

                        //public List<Traslados> Traslados { get; set; }
                        //public List<Retenciones> Retenciones { get; set; }
                        //public List<InformacionAduanera> InformacionAduanera { get; set; }
                        //public List<Parte> Parte { get; set; }
                    };
                    conceptos.Add(conceptsData);
                }

                if (model.TypeInvoice == "PAYMENT")
                {
                    //tengo dudas de los complementos
                    List<Pagos> pagos = new List<Pagos>();
                    //foreach (var item in model.varios)
                    //{
                    //    var pago = new Pagos
                    //    {
                    //        //FechaPago //dudas de que dato se agrega
                    //        FormaDePagoP = model.PaymentForm,
                    //        MonedaP = model.Currency,
                    //        //TipoCambioP { get; set; }
                    //        Monto = model.Total.ToString(),
                    //        //List<DoctoRelacionado> DoctoRelacionado { get; set; }
                    //    };
                    //}
                    var invoiceComplementData = new InvoiceComplementData
                    {
                        Serie = model.Serie,
                        Folio = Convert.ToInt32(model.Folio),
                        Fecha = todayDate,
                        Moneda = model.Currency,
                        //TipoCambio = model.ExchangeRate.,
                        TipoDeComprobante = model.TypeInvoice,
                        LugarExpedicion = model.ZipCode,
                        //Complemento = new Complemento() { pagos = pagos },
                        Emisor = issuer,
                        Receptor = receiver,
                        Conceptos = conceptos
                    };

                    var invoice = new InvoiceComplementJson
                    {
                        data = invoiceComplementData
                    };

                    var result = SATService.PostIssuePaymentInvoices(invoice, provider);
                    if (result != null)
                    {
                        success = true;
                    }
                }
                else
                {
                    var invoiceData = new InvoiceData
                    {
                        Serie = model.Serie,
                        Folio = Convert.ToInt32(model.Folio),
                        Fecha = todayDate,
                        Moneda = model.Currency,
                        //TipoCambio = model.ExchangeRate.,
                        TipoDeComprobante = model.TypeInvoice,
                        CondicionesDePago = model.PaymentConditions,
                        FormaPago = model.PaymentForm,
                        MetodoPago = model.PaymentMethod,
                        //Confirmacion = model.
                        LugarExpedicion = model.ZipCode,
                        Emisor = issuer,
                        Receptor = receiver,
                        Conceptos = conceptos
                    };
                    var invoice = new InvoiceJson
                    {
                        data = invoiceData
                    };

                    var result = SATService.PostIssueIncomeInvoices(invoice, provider);
                    if (result != null)
                    {
                        success = true;
                    }
                }

                if (!success)
                {
                    throw new Exception("Error al crear la factura");
                }

                MensajeFlashHandler.RegistrarMensaje("Se creo la factura", TiposMensaje.Success);
                return RedirectToAction("InvoicesSaved");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View();
        }
        #endregion



        public ActionResult InvoicesSaved()
        {
            InvoicesSavedViewModel model = new InvoicesSavedViewModel();
            try
            {
                model.ListTypeInvoices = _typeVoucherService.GetAll().Select(x => new SelectListItem
                {
                    Text = "(" + x.code + ") " + x.Description.ToString(),
                    Value = x.id.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetInvoicesSaved(JQueryDataTableParams param, string filtros)
        {
            var listResponse = new List<InvoiceIssued>();
            var list = new List<InvoicesSavedList>();

            try
            {
                listResponse = _invoiceIssuedService.GetAll().ToList();

                list = listResponse.Select(x => new InvoicesSavedList
                {
                    id = x.id,
                    folio = x.folio,
                    serie = x.serie,
                    paymentMethod = x.paymentMethod,
                    paymentForm = x.paymentForm,
                    currency = x.currency,
                    iva = x.iva.ToString("C2"),
                    invoicedAt = x.invoicedAt.ToShortDateString(),
                    invoiceType = x.invoiceType,
                    total = x.total.ToString("C2"),
                    subtotal = x.subtotal.ToString("C2"),
                    xml = x.xml
                }).ToList();

                return Json(new
                {
                    success = true,
                    param.sEcho,
                    iTotalRecords = list.Count(),
                    iTotalDisplayRecords = list,
                    aaData = list
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