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
using System.IO;
using MVC_Project.Integrations.Storage;
using System.Xml;

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
                SetCombos(null, ref model);
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

                //if (model.TypeInvoice != "E")
                //{
                //Validar que se exista el receptor
                Customer customer = new Customer();
                if ((model.CustomerId != 0 && model.TypeInvoice == "E") || model.CustomerId != 0)
                {
                    //Poder guardar el cliente si no existe       
                    customer = new Customer()
                    {
                        uuid = Guid.NewGuid(),
                        account = new Account { id = authUser.Account.Id },
                        businessName = model.CustomerName,
                        rfc = model.RFC,
                        //taxRegime = model.taxRegime,
                        street = model.Street,
                        interiorNumber = model.InteriorNumber,
                        outdoorNumber = model.OutdoorNumber,
                        zipCode = model.ZipCode,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString()
                    };

                    if (model.Colony.Value > 0)
                        customer.colony = model.Colony.Value;
                    if (model.Municipality.Value > 0)
                        customer.municipality = model.Municipality.Value;
                    if (model.State.Value > 0)
                        customer.state = model.State.Value;
                    if (model.Country.Value > 0)
                        customer.country = model.Country.Value;
                }
                else
                    customer = new Customer() { id = model.CustomerId };

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
                    customer = customer,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    json = JsonConvert.SerializeObject(model),
                    status = IssueStatus.SAVED.ToString(), // para saber si esta guardado
                };
                //validar que exista la el iva para guardar
                //iva = model.TaxWithheldIVA,

                //_invoiceIssuedService.Create(invoice);
                _invoiceIssuedService.SaveInvoice(invoice, null, customer);
                //}
                //else
                //{
                //    Provider provider = new Provider();
                //    if (model.CustomerId == 0)
                //    {
                //        //Poder guardar el cliente si no existe        
                //        provider = new Provider()
                //        {
                //            uuid = Guid.NewGuid(),
                //            account = new Account { id = authUser.Account.Id },
                //            businessName = model.BusinessName,
                //            rfc = model.RFC,
                //            //taxRegime = model.taxRegime,
                //            street = model.Street,
                //            interiorNumber = model.InteriorNumber,
                //            outdoorNumber = model.OutdoorNumber,
                //            zipCode = model.ZipCode,
                //            createdAt = todayDate,
                //            modifiedAt = todayDate,
                //            status = SystemStatus.ACTIVE.ToString()
                //        };

                //        if (model.Colony.Value > 0)
                //            provider.colony = model.Colony.Value;
                //        if (model.Municipality.Value > 0)
                //            provider.municipality = model.Municipality.Value;
                //        if (model.State.Value > 0)
                //            provider.state = model.State.Value;
                //        if (model.Country.Value > 0)
                //            provider.country = model.Country.Value;
                //    }
                //    else
                //        provider = new Provider() { id = model.CustomerId };

                //    InvoiceReceived invoice = new InvoiceReceived()
                //    {
                //        uuid = Guid.NewGuid(),
                //        folio = model.Folio,
                //        serie = model.Serie,
                //        paymentMethod = model.PaymentMethod,
                //        paymentForm = model.PaymentForm,
                //        currency = model.Currency,
                //        invoiceType = model.TypeInvoice,
                //        total = model.Total,
                //        subtotal = model.Subtotal,
                //        account = new Account() { id = authUser.Account.Id },
                //        provider = provider,
                //        createdAt = todayDate,
                //        modifiedAt = todayDate,
                //        json = JsonConvert.SerializeObject(model),
                //        status = IssueStatus.SAVED.ToString(), // para saber si esta guardado
                //    };
                //    //validar que exista la el iva para guardar
                //    //iva = model.TaxWithheldIVA,

                //    _invoiceReceivedService.SaveInvoice(invoice, provider, null);
                //}

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

        #region Realizar timbrado de factura
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
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
                        CuentaPredial = new CuentaPredial { Numero = model.PropertyAccountNumber },

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

        private void SetCombos(string zipCode, ref InvoiceViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;

            model.ListTaxRegime = _taxRegimeService.GetAll().Select(x => new SelectListItem
            {
                Text = "(" + x.code + ") " + x.description.ToString(),
                Value = x.code.ToString()
            }).ToList();

            model.ListTypeInvoices = _typeVoucherService.GetAll().Where(x => x.code != "N" && x.code != "T").Select(x => new SelectListItem
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
            model.Currency = model.ListCurrency.Where(x => x.Value == "MXN").FirstOrDefault().Value;

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

            var stateList = _stateService.GetAll().Select(x => new SelectListItem { Text = x.nameState, Value = x.id.ToString() }).ToList();
            stateList.Insert(0, (new SelectListItem { Text = "Seleccione...", Value = "-1" }));
            model.ListState = stateList;

            if (!string.IsNullOrEmpty(zipCode))
            {
                var listResponse = _stateService.GetLocationList(zipCode);

                var countries = listResponse.Select(x => new { id = x.countryId, name = x.nameCountry }).Distinct();
                model.ListCountry = countries.Select(x => new SelectListItem
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                }).Distinct().ToList();

                var municipalities = listResponse.Select(x => new { id = x.municipalityId, name = x.nameMunicipality }).Distinct();
                model.ListMunicipality = municipalities.Select(x => new SelectListItem
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                }).Distinct().ToList();

                model.ListColony = listResponse.Select(x => new SelectListItem
                {
                    Text = x.nameSettlement,
                    Value = x.id.ToString(),
                }).Distinct().ToList();
            }
            else
            {
                model.ListCountry = new List<SelectListItem>();
                model.ListMunicipality = new List<SelectListItem>();
                model.ListColony = new List<SelectListItem>();
            }
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
        #endregion

        #region Generar PDF
        [HttpGet, AllowAnonymous]
        public ActionResult GetDownloadPDF(Int64 id)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                var invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");

                if (invoice.xml == null)
                    throw new Exception("El registro no cuenta con el xml de la factura emitida");

                MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoicesIssued, authUser.Account.RFC + "/" + invoice.uuid + ".xml");
                stream.Position = 0;

                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
                XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
                nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3"); ;

                //Accedemos a nodo "Comprobante"
                XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

                //Obtener Folio, Serie, SubTotal y Total
                string varFolio = nodeComprobante.Attributes["Folio"].Value;
                string varSerie = nodeComprobante.Attributes["Serie"].Value;
                string varSubTotal = nodeComprobante.Attributes["SubTotal"].Value;
                string varTotal = nodeComprobante.Attributes["Total"].Value;
                string varTipoComprobante = nodeComprobante.Attributes["TipoDeComprobante"].Value;
                string varCertificado = nodeComprobante.Attributes["Certificado"].Value;
                string varNoCertificado = nodeComprobante.Attributes["NoCertificado"].Value;
                string varSello = nodeComprobante.Attributes["Sello"].Value;
                string varFormaPago = nodeComprobante.Attributes["FormaPago"].Value;
                string varMetodoPago = nodeComprobante.Attributes["MetodoPago"].Value;
                string varLugarExpedicion = nodeComprobante.Attributes["LugarExpedicion"].Value;
                string varFecha = nodeComprobante.Attributes["Fecha"].Value;
                string varMoneda = nodeComprobante.Attributes["Moneda"].Value;
                string varDescuento1 = nodeComprobante.Attributes["Descuento"] != null ? nodeComprobante.Attributes["Descuento"].Value : string.Empty;

                MonedaUtils formatoTexto = new MonedaUtils();
                var fecha = varFecha != null || varFecha != "" ? Convert.ToDateTime(varFecha).ToString("yyyy-MM-dd HH:mm:ss") : varFecha;

                InvoicesVM cfdipdf = new InvoicesVM()
                {
                    Folio = varFolio,
                    Serie = varSerie,
                    SubTotal = varSubTotal,
                    Total = varTotal,
                    TipoDeComprobante = ((TipoComprobante)Enum.Parse(typeof(TipoComprobante), varTipoComprobante, true)).GetDescription(),
                    Certificado = varCertificado,
                    NoCertificado = varNoCertificado,
                    Sello = varSello,
                    FormaPago = varFormaPago,
                    MetodoPago = ((MetodoPago)Enum.Parse(typeof(MetodoPago), varMetodoPago, true)).GetDescription(),
                    LugarExpedicion = varLugarExpedicion,
                    Fecha = fecha,
                    //Moneda = varMoneda,
                    Moneda = formatoTexto.Convertir(varTotal.ToString(), true),
                    Descuento = varDescuento1
                };


                XmlNode nodeEmisor = nodeComprobante.SelectSingleNode("cfdi:Emisor", nsm);
                if (nodeEmisor != null)
                {
                    string varNombre = nodeEmisor.Attributes["Nombre"].Value;
                    string varRfc = nodeEmisor.Attributes["Rfc"].Value;
                    string varRegimenFiscal = nodeEmisor.Attributes["RegimenFiscal"].Value;
                    string varRegimenFiscalText = string.Empty;

                    try
                    {
                        varRegimenFiscalText = ((RegimenFiscal)Enum.Parse(typeof(RegimenFiscal), "RegimenFiscal" + varRegimenFiscal, true)).GetDescription();
                    }
                    catch (Exception ex)
                    {
                    }

                    Models.Emisor emisor = new Models.Emisor()
                    {
                        Nombre = varNombre,
                        Rfc = varRfc,
                        RegimenFiscal = varRegimenFiscal,
                        RegimenFiscalTexto = varRegimenFiscalText != null ? varRegimenFiscalText : varRegimenFiscal
                    };

                    cfdipdf.Emisor = emisor;
                }

                XmlNode nodeReceptor = nodeComprobante.SelectSingleNode("cfdi:Receptor", nsm);
                if (nodeReceptor != null)
                {
                    string varNombre = nodeReceptor.Attributes["Nombre"].Value;
                    string varRfc = nodeReceptor.Attributes["Rfc"].Value;
                    string varUsoCFDI = nodeReceptor.Attributes["UsoCFDI"].Value;
                    string varUsoCFDIText = string.Empty;
                    try
                    {
                        varUsoCFDIText = ((UsoCFDI)Enum.Parse(typeof(UsoCFDI), varUsoCFDI, true)).GetDescription();
                    }
                    catch (Exception ex)
                    {
                    }

                    Models.Receptor receptor = new Models.Receptor()
                    {
                        Nombre = varNombre,
                        Rfc = varRfc,
                        UsoCFDI = varUsoCFDI,
                        UsoCFDITexto = varUsoCFDIText != null ? varUsoCFDIText : varUsoCFDI
                    };

                    cfdipdf.Receptor = receptor;
                }

                GeneradorQR generador = new GeneradorQR();
                string selloQR = cfdipdf.Sello.Substring((cfdipdf.Sello.Length - 8), 8);
                string urlQR = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id=" + cfdipdf.Folio + "&re=" + cfdipdf.Emisor.Rfc + "&rr=" + cfdipdf.Receptor.Rfc + "&tt=" + cfdipdf.Total + "&fe=" + selloQR;
                var byteImage = generador.CrearCodigo(urlQR);
                cfdipdf.QR = "data:image/png;base64," + Convert.ToBase64String(byteImage);

                XmlNode nodeConceptos = nodeComprobante.SelectSingleNode("cfdi:Conceptos", nsm);
                if (nodeConceptos != null)
                {
                    XmlNode nodeConcepto = nodeConceptos.SelectSingleNode("cfdi:Concepto", nsm);
                    if (nodeConcepto != null)
                    {
                        string varNoIdentificacion = nodeConcepto.Attributes["NoIdentificacion"].Value;
                        string varClaveProdServ = nodeConcepto.Attributes["ClaveProdServ"].Value;
                        string varCantidad = nodeConcepto.Attributes["Cantidad"].Value;
                        string varClaveUnidad = nodeConcepto.Attributes["ClaveUnidad"].Value;
                        string varDescripcion = nodeConcepto.Attributes["Descripcion"].Value;
                        string varDescuento = nodeConcepto.Attributes["Descuento"] != null ? nodeConcepto.Attributes["Descuento"].Value : string.Empty;
                        string varImporte = nodeConcepto.Attributes["Importe"].Value;
                        string varValorUnitario = nodeConcepto.Attributes["ValorUnitario"].Value;

                        Concepto concepto = new Concepto()
                        {
                            ClaveProdServ = varClaveProdServ,
                            NoIdentificacion = varNoIdentificacion,
                            Cantidad = varCantidad,
                            ClaveUnidad = varClaveUnidad,
                            Descripcion = varDescripcion,
                            Descuento = varDescuento,
                            Importe = varImporte,
                            ValorUnitario = varValorUnitario
                            //Unidad 
                        };
                        cfdipdf.Conceptos.Concepto = concepto;
                    }
                }

                XmlNode nodoComplemento = nodeComprobante.SelectSingleNode("cfdi:Complemento", nsm);
                if (nodoComplemento != null)
                {
                    nsm.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

                    XmlNode nodoTimbrado = nodoComplemento.SelectSingleNode("tfd:TimbreFiscalDigital", nsm);
                    if (nodoTimbrado != null)
                    {
                        string varUUID = nodoTimbrado.Attributes["UUID"].Value;
                        string varFechaTimbrado = nodoTimbrado.Attributes["FechaTimbrado"].Value;
                        string varSelloCFD = nodoTimbrado.Attributes["SelloCFD"].Value;
                        string varNoCertificadoSAT = nodoTimbrado.Attributes["NoCertificadoSAT"].Value;
                        string varSelloSAT = nodoTimbrado.Attributes["SelloSAT"].Value;
                        string varRfcProvCertif = nodoTimbrado.Attributes["RfcProvCertif"].Value;
                        string varVersion = nodoTimbrado.Attributes["Version"].Value;

                        var fechaTimbrado = varFechaTimbrado != null || varFechaTimbrado != "" ? Convert.ToDateTime(varFechaTimbrado).ToString("yyyy-MM-dd HH:mm:ss") : varFechaTimbrado;

                        TimbreFiscalDigital timbre = new TimbreFiscalDigital()
                        {
                            UUID = varUUID,
                            FechaTimbrado = fechaTimbrado,
                            NoCertificadoSAT = varNoCertificadoSAT,
                            RfcProvCertif = varRfcProvCertif,
                            SelloCFD = varSelloCFD,
                            SelloSAT = varSelloSAT,
                            Version = varVersion
                        };

                        cfdipdf.Complemento.TimbreFiscalDigital = timbre;
                    }
                }

                MensajeFlashHandler.RegistrarMensaje("Descargando...", TiposMensaje.Success);
                string rfc = authUser.Account.RFC;
                return new Rotativa.ViewAsPdf("InvoiceDownloadPDF", cfdipdf) { FileName = invoice.uuid + ".pdf" };
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                //return View("InvoicesIssued", model);
                return RedirectToAction("InvoicesIssued");
            }

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