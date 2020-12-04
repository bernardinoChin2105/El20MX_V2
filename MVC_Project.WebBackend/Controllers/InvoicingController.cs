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
using Microsoft.Ajax.Utilities;
using System.Collections.Specialized;
using LogHubSDK.Models;

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
        private IRateFeeService _rateFeeService;
        private ITaxService _taxService;
        private IMembershipService _membershipService;

        public InvoicingController(IAccountService accountService, ICustomsService customsService, ICustomsPatentService customsPatentService,
            ICustomsRequestNumberService customsRequestNumberService, ITypeInvoiceService typeInvoiceService, IUseCFDIService useCFDIService,
            ITypeRelationshipService typeRelationshipService, ITypeVoucherService typeVoucherService, ICurrencyService currencyService,
            IPaymentFormService paymentFormService, IPaymentMethodService paymentMethodService, ICustomerService customerService,
            IProviderService providerService, IBranchOfficeService branchOfficeService, ITaxRegimeService taxRegimeService,
            IInvoiceIssuedService invoiceIssuedService, IInvoiceReceivedService invoiceReceivedService, IDriveKeyService driveKeyService,
            IProductServiceKeyService productServiceKeyService, ICountryService countryService, IStateService stateService, IRateFeeService rateFeeService,
            ITaxService taxService, IMembershipService membershipService)

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
            _rateFeeService = rateFeeService;
            _taxService = taxService;
            _membershipService = membershipService;
        }

        // GET: Invoicing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Invoice(string customer)
        {
            InvoiceViewModel model = new InvoiceViewModel();
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                ViewBag.Date = new
                {
                    MinDate = DateUtil.GetDateTimeNow(),
                    MaxDate = DateUtil.GetDateTimeNow()
                };

                //obtener información de mi emisor                
                var account = authUser.Account;
                string email = authUser.Email;
                var membership = _membershipService.FirstOrDefault(x => x.account.id == account.Id && x.role.code == SystemRoles.ACCOUNT_OWNER.ToString() && x.status == SystemStatus.ACTIVE.ToString() && x.role.status == SystemStatus.ACTIVE.ToString());

                if (membership != null)
                {
                    email = membership.user.name;
                }
                //obtener información de mi emisor                    
                model.IssuingRFC = account.RFC;
                model.BusinessName = account.Name;
                model.IssuingTaxEmail = email;
                //model.IssuingTaxRegime = ""; //Faltan estos datos del cliente
                //model.IssuingTaxRegimeId = "";//Faltan estos datos del cliente  
                string zipCode = string.Empty;

                if (!string.IsNullOrEmpty(customer))
                {
                    CustomerViewModel customerModel = GetCustomerReceiver(customer, account.Id);
                    model.CustomerId = customerModel.Id;
                    model.CustomerName = customerModel.BusinessName;
                    model.RFC = customerModel.RFC;
                    model.Street = customerModel.Street;
                    model.OutdoorNumber = customerModel.OutdoorNumber;
                    model.InteriorNumber = customerModel.InteriorNumber;
                    model.Colony = customerModel.Colony;
                    model.ZipCode = customerModel.ZipCode;
                    model.Municipality = customerModel.Municipality;
                    model.State = customerModel.State;
                    model.Country = customerModel.Country;
                    //model.TypeReceptor = customerModel.taxRegime;
                    model.ReceiverType = "";
                    model.CustomerEmail = customerModel.Emails.Count() > 0 ? customerModel.Emails[0].EmailOrPhone : string.Empty;
                    zipCode = customerModel.ZipCode;
                }

                //Obtener listas de los combos
                SetCombos(zipCode, ref model);
            }
            catch (Exception ex)
            {
                //string error = ex.Message.ToString();
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }

        private CustomerViewModel GetCustomerReceiver(string uuid, Int64 accountId)
        {
            CustomerViewModel receiver = new CustomerViewModel();
            try
            {
                var provider = _providerService.FirstOrDefault(x => x.uuid.ToString() == uuid && x.account.id == accountId);
                if (provider != null)
                {
                    var customerProv = _customerService.FirstOrDefault(x => x.rfc == provider.rfc && x.account.id == accountId);
                    if (customerProv != null)
                    {
                        receiver = new CustomerViewModel()
                        {
                            Id = customerProv.id,
                            BusinessName = customerProv.businessName,
                            RFC = customerProv.rfc,
                            Street = customerProv.street,
                            OutdoorNumber = customerProv.outdoorNumber,
                            InteriorNumber = customerProv.interiorNumber,
                            Colony = customerProv.colony,
                            ZipCode = customerProv.zipCode,
                            Municipality = customerProv.municipality,
                            State = customerProv.state,
                            Country = customerProv.country,
                            Emails = customerProv.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                        .Select(x => new CustomerContactsViewModel
                        {
                            Id = x.id,
                            EmailOrPhone = x.emailOrPhone
                        }).ToList()
                        };
                    }
                    else
                    {
                        receiver = new CustomerViewModel()
                        {
                            Id = 0,
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
                else
                {
                    var customer = _customerService.FirstOrDefault(x => x.uuid.ToString() == uuid && x.account.id == accountId);

                    if (customer != null)
                    {
                        receiver = new CustomerViewModel()
                        {
                            Id = customer.id,
                            BusinessName = customer.businessName, // != null ? customer.businessName : customer.firstName + " " + customer.lastName,
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
            }
            catch (Exception ex)
            {

            }
            return receiver;
        }

        #region Guardar información para timbrar 
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Invoice(InvoiceViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();

                //Validar que se exista el receptor
                Customer customer = GetObjetCustomer(model);

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
                var office = _branchOfficeService.FirstOrDefault(x => x.id.ToString() == model.BranchOffice);
                office.folio++;

                var saved = _invoiceIssuedService.SaveInvoice(invoice, customer, office);

                MensajeFlashHandler.RegistrarMensaje("Factura Guardada", TiposMensaje.Success);
                return RedirectToAction("InvoicesSaved");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }

        private Customer GetObjetCustomer(InvoiceViewModel model)
        {
            Customer customer = new Customer();
            DateTime todayDate = DateUtil.GetDateTimeNow();
            var authUser = Authenticator.AuthenticatedUser;

            if (model.TypeReceptor == "provider" || model.CustomerId == 0)
            {
                //Poder guardar el cliente si no existe       
                customer = new Customer()
                {
                    uuid = Guid.NewGuid(),
                    account = new Account { id = authUser.Account.Id },
                    businessName = model.CustomerName,
                    rfc = model.RFC,
                    taxRegime = model.RFC.Count() > 12 ? TypeTaxRegimen.PERSONA_MORAL.ToString() : TypeTaxRegimen.PERSONA_FISICA.ToString(),
                    street = model.Street,
                    interiorNumber = model.InteriorNumber,
                    outdoorNumber = model.OutdoorNumber,
                    zipCode = model.ZipCode,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString()
                };

                if (model.Colony != null)
                    customer.colony = model.Colony.Value;
                if (model.Municipality != null)
                    customer.municipality = model.Municipality.Value;
                if (model.State != null)
                    customer.state = model.State.Value;
                if (model.Country != null)
                    customer.country = model.Country.Value;

                if (model.ListCustomerEmail.Count() > 0)
                {
                    for (int i = 0; i < model.ListCustomerEmail.Count(); i++)
                    {
                        if (model.ListCustomerEmail[i] != null && model.ListCustomerEmail[i].Trim() != "")
                        {
                            CustomerContact email = new CustomerContact()
                            {
                                emailOrPhone = model.ListCustomerEmail[i],
                                typeContact = TypeContact.EMAIL.ToString(),
                                customer = customer,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            customer.customerContacts.Add(email);
                        }
                    }
                }
            }
            else
                customer = new Customer() { id = model.CustomerId };

            return customer;
        }
        #endregion

        #region Realizar timbrado de factura
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult IssueIncomeInvoice(InvoiceViewModel model)
        {
            bool success = false;
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();
                var provider = ConfigurationManager.AppSettings["SATProvider"];
                dynamic invoiceModel = (dynamic)null;

                var issuer = new InvoiceIssuer
                {
                    Rfc = model.IssuingRFC,
                    Nombre = model.BusinessName,
                    RegimenFiscal = model.IssuingTaxRegimeId,
                };

                var receiver = new InvoiceReceiver
                {
                    Rfc = model.RFC,
                    Nombre = model.CustomerName,
                    //ResidenciaFiscal = model.Street, //tengo dudas de que dato va
                    //NumRegIdTrib = model, // que dato va
                    UsoCFDI = model.UseCFDI
                };

                var taxes = _taxService.GetAll().ToList();

                var conceptos = new List<ConceptsData>();
                foreach (var item in model.ProductServices)
                {
                    var conceptsData = new ConceptsData
                    {
                        ClaveProdServ = item.SATCode.ToString(),
                        //NoIdentificacion = que dato es?
                        Cantidad = item.Quantity,
                        ClaveUnidad = item.SATUnit,
                        Descripcion = item.ProductServiceDescription,
                        ValorUnitario = item.UnitPrice,
                        Importe = item.Subtotal,
                        //public List<Parte> Parte { get; set; }
                    };

                    if (model.TypeInvoice != TipoComprobante.P.ToString() && item.Unit.Count() <= 20)
                        conceptsData.Unidad = item.Unit;

                    if (item.DiscountRateProServ > 0)
                        conceptsData.Descuento = item.DiscountRateProServ;

                    if (model.InternationalChk && !string.IsNullOrEmpty(model.MotionNumber))
                    {
                        Integrations.SAT.InformacionAduanera infoAduanera = new Integrations.SAT.InformacionAduanera()
                        {
                            NumeroPedimento = model.MotionNumber
                        };

                        conceptsData.InformacionAduanera.Add(infoAduanera);
                    }

                    if (item.TaxesGeneral != null)
                    {
                        var taxesG = item.TaxesGeneral.Replace("},{", "};{");
                        taxesG = item.TaxesGeneral.Replace("'", "\"");
                        string[] taxArray = taxesG.Split(';');

                        if (taxArray.Count() > 0)
                        {
                            List<Integrations.SAT.Traslados> Traslados = new List<Integrations.SAT.Traslados>();
                            List<Integrations.SAT.Retenciones> Retenciones = new List<Integrations.SAT.Retenciones>();
                            foreach (var itemTax in taxArray)
                            {
                                TaxesAll imp = JsonConvert.DeserializeObject<TaxesAll>(itemTax.ToString());
                                if (imp.Tipo == "Retención")
                                {
                                    Integrations.SAT.Retenciones ret = new Integrations.SAT.Retenciones()
                                    {
                                        Base = (conceptsData.ValorUnitario * conceptsData.Cantidad).ToString(),
                                        Impuesto = taxes.FirstOrDefault(x => x.description == imp.Impuesto).code,
                                        TipoFactor = "Tasa",
                                        TasaOCuota = (Convert.ToDecimal(imp.Porcentaje) / 100).ToString("N6"),
                                        Importe = (Math.Round((Convert.ToDecimal(imp.Porcentaje) / 100) * (conceptsData.ValorUnitario * conceptsData.Cantidad))).ToString()
                                    };
                                    Retenciones.Add(ret);
                                }
                                else
                                {
                                    if (imp.Porcentaje != "Exento")
                                    {
                                        Integrations.SAT.Traslados tras = new Integrations.SAT.Traslados()
                                        {
                                            Base = (conceptsData.ValorUnitario * conceptsData.Cantidad).ToString(),
                                            Impuesto = taxes.FirstOrDefault(x => x.description == imp.Impuesto).code,
                                            TipoFactor = "Tasa",
                                            TasaOCuota = (Convert.ToDecimal(imp.Porcentaje) / 100).ToString("N6"),
                                            //TasaOCuota = "0.160000",
                                            Importe = (Math.Round((Convert.ToDecimal(imp.Porcentaje) / 100) * (conceptsData.ValorUnitario * conceptsData.Cantidad), 2)).ToString()
                                        };
                                        Traslados.Add(tras);
                                    }
                                    else
                                    {
                                        Integrations.SAT.Traslados tras = new Integrations.SAT.Traslados()
                                        {
                                            Base = (conceptsData.ValorUnitario * conceptsData.Cantidad).ToString(),
                                            TipoFactor = imp.Porcentaje,
                                            Impuesto = taxes.FirstOrDefault(x => x.description == imp.Impuesto).code,
                                        };
                                        Traslados.Add(tras);
                                    }
                                }
                            }
                            if (Traslados.Count() > 0)
                                conceptsData.Traslados = Traslados;

                            if (Retenciones.Count() > 0)
                                conceptsData.Retenciones = Retenciones;
                        }
                    }


                    if (!string.IsNullOrEmpty(model.PropertyAccountNumber))
                        conceptsData.CuentaPredial = new CuentaPredial { Numero = model.PropertyAccountNumber };
                    conceptos.Add(conceptsData);
                }

                //Obtengo la entidad de la oficina
                var office = _branchOfficeService.FirstOrDefault(x => x.id.ToString() == model.BranchOffice);
                InvoicesInfo result = new InvoicesInfo();
                string serilaizeJson = string.Empty;

                if (model.TypeInvoice == TipoComprobante.P.ToString())
                {
                    //invoiceModel = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(invoice));
                    //tengo dudas de los complementos
                    List<Integrations.SAT.Pagos> payments = new List<Integrations.SAT.Pagos>();
                    foreach (var item in model.payment)
                    {
                        DateTime dateP = Convert.ToDateTime(item.startedAt);

                        var pago = new Integrations.SAT.Pagos
                        {
                            FechaPago = dateP.ToString("s"), //item.startedAt,
                            FormaDePagoP = item.PaymentFormCFDI,
                            MonedaP = item.CurrencyCFDI,
                            //TipoCambioP = item.ExchangeRateCFDI.ToString(),
                            Monto = item.AmountCFDI.ToString(),
                            NumOperacion = item.NumOperationCFDI,
                            DoctoRelacionado = new Integrations.SAT.DoctoRelacionado()
                            {
                                IdDocumento = item.uuid,
                                MonedaDR = item.currency,
                                MetodoDePagoDR = item.method,
                                NumParcialidad = item.numberPartialities.ToString(),
                                ImpSaldoAnt = item.previousBalance.ToString(),
                                ImpSaldoInsoluto = item.outstanding.ToString(),
                                Serie = item.serie,
                                Folio = item.folio
                            }
                        };

                        if (item.CurrencyCFDI != "MXN")
                            pago.TipoCambioP = item.ExchangeRateCFDI.ToString();

                        payments.Add(pago);
                    }

                    InvoiceComplementData invoiceComplementData = new InvoiceComplementData
                    {
                        Serie = model.Serie,
                        Folio = Convert.ToInt32(model.Folio),
                        Fecha = todayDate.ToString("s"),
                        Moneda = "XXX", //model.Currency,
                        TipoDeComprobante = model.TypeInvoice,
                        LugarExpedicion = office.zipCode,
                        Complemento = new Integrations.SAT.Complemento() { Pagos = payments },
                        Emisor = issuer,
                        Receptor = receiver,
                        Conceptos = conceptos
                    };

                    if (Convert.ToDecimal(model.ExchangeRate) > 0)
                        invoiceComplementData.TipoCambio = Convert.ToDecimal(model.ExchangeRate);
                    else
                        invoiceComplementData.TipoCambio = null;

                    #region Convertir el objeto en un objeto sin valores nulos

                    var invoice = new InvoiceComplementJson
                    {
                        data = invoiceComplementData
                    };

                    invoiceModel = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(invoice));

                    serilaizeJson = JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    #endregion

                    //dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                    ////result = SATService.PostIssuePaymentInvoices(invoiceSend, provider);                    

                }
                else if (model.TypeInvoice == TipoComprobante.E.ToString())
                {
                    InvoiceRefundData invoiceData = invoiceData = new InvoiceRefundData
                    {
                        Serie = model.Serie,
                        Folio = Convert.ToInt32(model.Folio),
                        Fecha = todayDate.ToString("s"),
                        Moneda = model.Currency,
                        TipoDeComprobante = model.TypeInvoice,
                        CondicionesDePago = model.PaymentConditions,
                        FormaPago = model.PaymentForm,
                        MetodoPago = model.PaymentMethod,
                        LugarExpedicion = office.zipCode,
                        Emisor = issuer,
                        Receptor = receiver,
                        Conceptos = conceptos
                    };

                    if (Convert.ToDecimal(model.ExchangeRate) > 0)
                        invoiceData.TipoCambio = Convert.ToDecimal(model.ExchangeRate);
                    else
                        invoiceData.TipoCambio = null;

                    invoiceData.CfdiRelacionados = new List<Integrations.SAT.CfdiRelacionados>();
                    if (model.invoicesUuid.Count() > 0)
                    {
                        foreach (var item in model.invoicesUuid)
                        {
                            Integrations.SAT.CfdiRelacionados cfdi = new Integrations.SAT.CfdiRelacionados()
                            {
                                TipoRelacion = item.typeRelationship,
                                CfdiRelacionado = new Integrations.SAT.CfdiRelacionado()
                                {
                                    UUID = item.uuid,
                                }
                            };
                            invoiceData.CfdiRelacionados.Add(cfdi);
                        }
                    }

                    #region Convertir el objeto en un objeto sin valores nulos
                    var invoice = new InvoiceRefundJson
                    {
                        data = invoiceData
                    };

                    invoiceModel = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(invoice));

                    serilaizeJson = JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    #endregion

                    //dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                    ////result = SATService.PostIssueRefundInvoices(invoiceSend, provider);                    
                }
                else
                {
                    InvoiceData invoiceData = invoiceData = new InvoiceData
                    {
                        Serie = model.Serie,
                        Folio = Convert.ToInt32(model.Folio),
                        Fecha = todayDate.ToString("s"),
                        Moneda = model.Currency,
                        TipoDeComprobante = model.TypeInvoice,
                        CondicionesDePago = model.PaymentConditions,
                        FormaPago = model.PaymentForm,
                        MetodoPago = model.PaymentMethod,
                        LugarExpedicion = office.zipCode,
                        Emisor = issuer,
                        Receptor = receiver,
                        Conceptos = conceptos
                    };

                    if (Convert.ToDecimal(model.ExchangeRate) > 0)
                        invoiceData.TipoCambio = Convert.ToDecimal(model.ExchangeRate);
                    else
                        invoiceData.TipoCambio = null;

                    #region Convertir el objeto en un objeto sin valores nulos
                    var invoice = new InvoiceJson
                    {
                        data = invoiceData
                    };

                    invoiceModel = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(invoice));

                    serilaizeJson = JsonConvert.SerializeObject(invoice, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    #endregion

                    //dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                    //result = SATService.PostIssueIncomeInvoices(invoiceSend, provider);
                }

                //Envio de la factura al satws para timbrado
                dynamic invoiceSend = JsonConvert.DeserializeObject<dynamic>(serilaizeJson);
                result = SATService.PostIssueIncomeInvoices(invoiceSend, provider);

                if (result != null)
                {
                    //hasta aquí ya se realizo el timbrado
                    office.folio++;
                    _branchOfficeService.Update(office);

                    try
                    {
                        //Se inicializa el objeto del cliente
                        Customer customer = new Customer();
                        customer = GetObjetCustomer(model);
                        List<string> IdIssued = new List<string>();

                        IdIssued.Add(result.uuid.ToString());

                        /*Obtener los CFDI's*/
                        var customersCFDI = SATService.GetCFDIs(IdIssued, provider);
                        var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];

                        List<InvoiceIssued> invoiceIssued = new List<InvoiceIssued>();

                        //List<InvoiceIssued> invoiceIssued = _invoiceIssuedService.FindBy(x => x.account.id == authUser.Account.Id).ToList();

                        foreach (var cfdi in customersCFDI)
                        {
                            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(cfdi.Xml);
                            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                            var upload = AzureBlobService.UploadPublicFile(stream, cfdi.id + ".xml", StorageInvoicesIssued, model.IssuingRFC);

                            invoiceIssued.Add(new InvoiceIssued
                            {
                                uuid = Guid.Parse(cfdi.id),
                                folio = cfdi.Folio,
                                serie = cfdi.Serie,
                                paymentMethod = cfdi.MetodoPago,
                                paymentForm = cfdi.FormaPago,
                                currency = cfdi.Moneda,
                                iva = result.tax.Value,
                                invoicedAt = cfdi.Fecha,
                                xml = upload.Item1,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = IssueStatus.STAMPED.ToString(),
                                account = new Account() { id = authUser.Account.Id },
                                customer = customer,
                                invoiceType = cfdi.TipoDeComprobante,
                                subtotal = cfdi.SubTotal,
                                total = cfdi.Total,
                                homemade = true,
                                branchOffice = office
                            });
                        }

                        var resultSaved = _invoiceIssuedService.SaveInvoice(invoiceIssued[0], customer);
                        if (resultSaved != null)
                        {
                            success = true;
                            if (!string.IsNullOrEmpty(model.ListCustomerEmail[0]))
                            {
                                try
                                {
                                    var byteArrayPdf = GetInvoicingPDF(invoiceIssued[0].id);
                                    System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArrayPdf);
                                    var uploadPDF = AzureBlobService.UploadPublicFile(stream, invoiceIssued[0].uuid + ".pdf", StorageInvoicesIssued, model.IssuingRFC);

                                    SendInvoice(model.ListCustomerEmail[0], model.RFC, model.CustomerName, model.Comments, invoiceIssued[0].xml, uploadPDF.Item1);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Factura timbrada con éxito, pero hubo un error al generar la factura en formato pdf.");
                                }
                            }
                        }
                        else
                            throw new Exception("Factura timbrada con éxito, pero hubo un error al guardar los datos del cliente.");

                    }
                    catch (Exception ex)
                    {
                        string message = "Factura timbrada con éxito, pero hubo un error: " + ex.Message.ToString();
                        throw new Exception("Factura timbrada con éxito, pero hubo un error al guardar los datos del cliente.");
                    }

                }
                else
                    throw new Exception("Error al crear la factura");

                LogUtil.AddEntry(
                                                   "Factura timbrada con éxito.",
                                                   ENivelLog.Info,
                                                   authUser.Id,
                                                   authUser.Email,
                                                   EOperacionLog.ACCESS,
                                                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                                                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                                                   JsonConvert.SerializeObject(invoiceModel)
                                               );
                MensajeFlashHandler.RegistrarMensaje("Factura timbrada con éxito.", TiposMensaje.Success);
                return RedirectToAction("Invoice");
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Error al realizar la facturación:" + ex.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(model)
               );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                //SetCombos(model.ZipCode, ref model);
                //return View("Invoice", model);
                //return View("Invoice");
                return RedirectToAction("Invoice");
            }
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
            //model.ListCustoms = _customsService.GetAll().Select(x => new SelectListItem
            //{
            //    Text = "(" + x.code + ") " + x.description.ToString(),
            //    Value = x.code.ToString()
            //}).ToList();

            //model.ListCustomsPatent = _customsPatentService.GetAll().Select(x => new SelectListItem
            //{
            //    Text = x.code.ToString(),
            //    Value = x.code.ToString()
            //}).ToList();

            //model.ListMotionNumber = _customsRequestNumberService.GetAll().Select(x => new SelectListItem
            //{
            //    Text = "(" + x.code + ") " + x.patent.ToString(),
            //    Value = x.code.ToString()
            //}).ToList();

            //model.ListTransferred = Enum.GetValues(typeof(TypeTransferred)).Cast<TypeTransferred>()
            //       .Select(e => new SelectListItem
            //       {
            //           Value = e.ToString(),
            //           Text = EnumUtils.GetDescription(e)
            //       }).ToList();

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

                var states = listResponse.Select(x => new { id = x.stateId, name = x.nameState }).Distinct();
                model.ListState = states.Select(x => new SelectListItem
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
                model.ListState = new List<SelectListItem>();
                model.ListMunicipality = new List<SelectListItem>();
                model.ListColony = new List<SelectListItem>();
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetSerieFolio(Int64 sucursalId)
        {
            try
            {
                var branchOffice = _branchOfficeService.GetById(sucursalId);
                if (branchOffice == null)
                    throw new Exception("Sucursal no encontrada en el sistema");

                //Validar si la oficina tiene sellos cargados.
                bool sello = false;
                if (!string.IsNullOrEmpty(branchOffice.certificateId))
                {
                    sello = true;
                }

                return Json(new { success = true, serie = branchOffice.serie, folio = branchOffice.folio, logo = branchOffice.logo, sello = sello }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region busquedas de información 
        //Busca a los cliente o proveedores por rfc o razon social
        [HttpGet, AllowAnonymous]
        public JsonResult GetSearchReceiver(string value, string typeInvoice)
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
                filters.businessName = value;

                result = _customerService.ReceiverSearchList(filters);
                if (result != null)
                {
                    //Validar que los datos no sean vacios por los nombres
                    //result = result.Select(c =>
                    //{
                    //    c.businessName = (c.first_name != null || c.last_name != null ? c.first_name + " " + c.last_name : c.businessName);
                    //    return c;
                    //}).ToList();

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
        [HttpGet, AllowAnonymous]
        public JsonResult GetReceiverInformation(Int64 id, string type)
        {
            bool success = false;
            string message = string.Empty;
            var authUser = Authenticator.AuthenticatedUser;
            CustomerViewModel receiver = new CustomerViewModel();
            //Object receiver = new object();
            try
            {
                var accountId = authUser.Account.Id;
                if (type == "customer")
                {
                    var customer = _customerService.FirstOrDefault(x => x.id == id && x.account.id == accountId);

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
                    var provider = _providerService.FirstOrDefault(x => x.id == id && x.account.id == accountId);

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
        [HttpGet, AllowAnonymous]
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

        //Busqueda de CFDI para complementos
        //Obtener listado de unidad del SAT
        [HttpGet, AllowAnonymous]
        public JsonResult GetCFDIId(string uuid, string typeInvoice)
        {
            bool success = false;
            string message = string.Empty;
            var authUser = Authenticator.AuthenticatedUser;
            InvoiceIssued invoice = new InvoiceIssued();
            InvoicesVM CFDI = new InvoicesVM();
            //List<DriveKeyViewModel> result = new List<DriveKeyViewModel>();
            try
            {
                if (typeInvoice == TipoComprobante.P.ToString())
                    invoice = _invoiceIssuedService.FirstOrDefault(x => x.uuid.ToString() == uuid && x.account.id == authUser.Account.Id && x.paymentMethod == MetodoPago.PPD.ToString());
                else
                    invoice = _invoiceIssuedService.FirstOrDefault(x => x.uuid.ToString() == uuid && x.account.id == authUser.Account.Id);
                //invoice = _invoiceIssuedService.FirstOrDefault(x => x.uuid.ToString() == uuid && x.paymentMethod == MetodoPago.PPD.ToString());
                if (invoice != null)
                {
                    var StorageInvoices = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
                    MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoices, authUser.Account.RFC + "/" + invoice.uuid + ".xml");
                    //MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoices, "CAYW880502FK4/" + invoice.uuid + ".xml");

                    stream.Position = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);

                    //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
                    XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
                    nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3"); ;

                    //Accedemos a nodo "Comprobante"
                    XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);


                    string varFolio = nodeComprobante.Attributes["Folio"] != null ? nodeComprobante.Attributes["Folio"].Value : string.Empty;
                    string varSerie = nodeComprobante.Attributes["Serie"] != null ? nodeComprobante.Attributes["Serie"].Value : string.Empty;
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
                    string varMoneda = nodeComprobante.Attributes["Moneda"] != null ? nodeComprobante.Attributes["Moneda"].Value : string.Empty;
                    string varDescuento1 = nodeComprobante.Attributes["Descuento"] != null ? nodeComprobante.Attributes["Descuento"].Value : string.Empty;
                    string varTipoCambio = nodeComprobante.Attributes["TipoCambio"] != null ? nodeComprobante.Attributes["TipoCambio"].Value : "1";

                    MonedaUtils formatoTexto = new MonedaUtils();
                    var fecha = varFecha != null || varFecha != "" ? Convert.ToDateTime(varFecha).ToString("yyyy-MM-dd HH:mm:ss") : varFecha;

                    CFDI = new InvoicesVM()
                    {
                        Folio = varFolio,
                        Serie = varSerie,
                        SubTotal = varSubTotal,
                        Total = varTotal,
                        TipoDeComprobante = _typeVoucherService.FirstOrDefault(x => x.code == varTipoComprobante).Description,
                        Certificado = varCertificado,
                        NoCertificado = varNoCertificado,
                        Sello = varSello,
                        FormaPago = varFormaPago,
                        MetodoPago = varMetodoPago,
                        LugarExpedicion = varLugarExpedicion,
                        Fecha = fecha,
                        Moneda = varMoneda,
                        TipoCambio = varTipoCambio,
                        TotalTexto = formatoTexto.Convertir(varTotal.ToString(), true),
                        Descuento = varDescuento1,
                    };


                    invoice.account = null;
                    invoice.customer = null;
                    invoice.branchOffice = null;
                    CFDI.Emisor = null;
                    CFDI.Receptor = null;
                    CFDI.Complemento = null;
                    CFDI.Conceptos = null;
                    CFDI.Impuestos = null;
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }

            return Json(new { success = success, message = message, data = new { factura = invoice, xml = CFDI } }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetRates(string value, string typeTaxes)
        {
            bool success = false;
            string message = string.Empty;
            var authUser = Authenticator.AuthenticatedUser;
            List<RateFee> rate = new List<RateFee>();

            try
            {
                rate = _rateFeeService.FindBy(x => x.taxes == value && x.type == "Fijo")
                    .Select(c =>
                    {
                        c.maximumValue = (c.maximumValue * 100) == 10.667 ? 10.66667 : (c.maximumValue * 100);
                        return c;
                    }).OrderBy(x => x.maximumValue).ToList();
                if (typeTaxes == "Retención")
                {
                    rate = rate.Where(x => x.retention == true).ToList();
                }
                else
                {
                    rate = rate.Where(x => x.transfer == true).ToList();
                }

                if (rate != null)
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
            }

            return Json(new { success = success, message = message, data = rate }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Generar PDF
        //[HttpGet, AllowAnonymous]
        public byte[] GetInvoicingPDF(Int64 id)
        {
            var authUser = Authenticator.AuthenticatedUser;

            //try
            //{
            var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];
            var invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);

            string typeInvoicing = TypeInvoicing.ISSUED.GetDisplayName();
            string logo = invoice.branchOffice != null ? invoice.branchOffice.logo : string.Empty;
            InvoicesVM cfdipdf = GetGenerateFilePDF(typeInvoicing, invoice, logo);

            //MensajeFlashHandler.RegistrarMensaje("Descargando...", TiposMensaje.Success);
            //string rfc = authUser.Account.RFC;
            //return new Rotativa.ViewAsPdf("InvoiceDownloadPDF", cfdipdf) {
            //    FileName = invoice.uuid + ".pdf"

            //};
            var actionPDF = new Rotativa.ViewAsPdf("InvoiceDownloadPDF", cfdipdf)
            {
                FileName = invoice.uuid + ".pdf"
            };
            byte[] applicationPDFData = actionPDF.BuildPdf(ControllerContext);
            return applicationPDFData;
            //}
            //catch (Exception ex)
            //{
            //    //MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            //    //return View("InvoicesIssued", model);
            //    //return RedirectToAction("InvoicesIssued");
            //}

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

        #region Facturas Emitidas
        [AllowAnonymous]
        public ActionResult InvoicesIssued()
        {
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(),
                MaxDate = DateTime.Now.ToString("dd/MM/yyyy")
            };
            try
            {
                InvoicesFilter model = new InvoicesFilter();
                var initial = new SelectListItem() { Text = "Todos...", Value = "-1" };

                var currencyList = _currencyService.GetAll().Select(x => new SelectListItem() { Text = x.code, Value = x.code }).ToList();
                var parmentFormList = _paymentFormService.GetAll().Select(x => new SelectListItem() { Text = x.code + "-" + x.Description, Value = x.code }).ToList();
                var parmentMethodList = _paymentMethodService.GetAll().Select(x => new SelectListItem() { Text = x.code, Value = x.code }).ToList();

                currencyList.Insert(0, (initial));
                parmentFormList.Insert(0, (initial));
                parmentMethodList.Insert(0, (initial));

                model.ListCurrency = new SelectList(currencyList);
                model.ListPaymentForm = new SelectList(parmentFormList);
                model.ListPaymentMethod = new SelectList(parmentMethodList);

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetInvoices(JQueryDataTableParams param, string filtros, bool first)
        {
            int totalDisplay = 0;
            int total = 0;
            string error = string.Empty;
            bool success = true;
            var userAuth = Authenticator.AuthenticatedUser;
            var listResponse = new List<InvoicesIssuedList>();
            var list = new List<InvoicesIssuedListVM>();

            try
            {

                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                string FilterEnd = filtersValues.Get("FilterEndDate").Trim();

                if (first)
                {
                    FilterStart = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString();
                    FilterEnd = DateUtil.GetDateTimeNow().ToShortDateString();
                }

                string Folio = filtersValues.Get("Folio").Trim();
                string rfc = filtersValues.Get("RFC").Trim();
                string PaymentMethod = filtersValues.Get("PaymentMethod").Trim();
                string PaymentForm = filtersValues.Get("PaymentForm").Trim();
                string Currency = filtersValues.Get("Currency").Trim();
                string serie = filtersValues.Get("Serie").Trim();
                string nombreRazonSocial = filtersValues.Get("NombreRazonSocial").Trim();

                var pagination = new BasePagination();
                var filters = new CustomerCFDIFilter() { accountId = userAuth.Account.Id };
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (FilterStart != "") pagination.CreatedOnStart = Convert.ToDateTime(FilterStart);
                if (FilterEnd != "") pagination.CreatedOnEnd = Convert.ToDateTime(FilterEnd);
                if (Folio != "") filters.folio = Folio;
                if (rfc != "") filters.rfc = rfc;
                if (PaymentForm != "-1") filters.paymentForm = PaymentForm;
                if (PaymentMethod != "-1") filters.paymentMethod = PaymentMethod;
                if (Currency != "-1") filters.currency = Currency;
                if (!string.IsNullOrEmpty(serie)) filters.serie = serie;
                if (!string.IsNullOrEmpty(nombreRazonSocial)) filters.nombreRazonSocial = nombreRazonSocial;

                listResponse = _customerService.CustomerCDFIList(pagination, filters);

                list = listResponse.Select(x => new InvoicesIssuedListVM
                {
                    id = x.id,
                    folio = x.folio,
                    serie = x.serie,
                    paymentMethod = x.paymentMethod,
                    paymentForm = x.paymentForm,
                    currency = x.currency,
                    amount = x.subtotal.ToString("C2"),
                    iva = x.iva.ToString("C2"),
                    totalAmount = x.total.ToString("C2"),
                    invoicedAt = x.invoicedAt.ToShortDateString(),
                    rfc = x.rfc,
                    businessName = (x.rfc.Count() == 12 ? x.businessName : x.first_name + " " + x.last_name),
                    //first_name = x.first_name,
                    //last_name = x.last_name,
                    paymentFormDescription = x.paymentFormDescription
                }).ToList();

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords

                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }


            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message.ToString();
            }

            return Json(new
            {
                success = success,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Facturas Recibidas

        /*Funciones para las facturas*/
        [AllowAnonymous]
        public ActionResult InvoicesReceived()
        {
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString(), //DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy"),
                MaxDate = DateTime.Now.ToString("dd/MM/yyyy")
            };
            try
            {
                InvoicesFilter model = new InvoicesFilter();
                var initial = new SelectListItem() { Text = "Todos...", Value = "-1" };

                var currencyList = _currencyService.GetAll().Select(x => new SelectListItem() { Text = x.code, Value = x.code }).ToList();
                var parmentFormList = _paymentFormService.GetAll().Select(x => new SelectListItem() { Text = x.code + "-" + x.Description, Value = x.code }).ToList();
                var parmentMethodList = _paymentMethodService.GetAll().Select(x => new SelectListItem() { Text = x.code, Value = x.code }).ToList();

                currencyList.Insert(0, (initial));
                parmentFormList.Insert(0, (initial));
                parmentMethodList.Insert(0, (initial));

                model.ListCurrency = new SelectList(currencyList);
                model.ListPaymentForm = new SelectList(parmentFormList);
                model.ListPaymentMethod = new SelectList(parmentMethodList);

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetInvoicesProvider(JQueryDataTableParams param, string filtros, bool first)
        {
            int totalDisplay = 0;
            int total = 0;
            string error = string.Empty;
            bool success = true;
            var userAuth = Authenticator.AuthenticatedUser;
            var listResponse = new List<InvoicesReceivedList>();
            var list = new List<InvoicesReceivedListVM>();

            try
            {

                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                string FilterEnd = filtersValues.Get("FilterEndDate").Trim();

                if (first)
                {
                    FilterStart = DateUtil.GetFirstDateTimeOfMonth(DateUtil.GetDateTimeNow()).ToShortDateString();
                    FilterEnd = DateUtil.GetDateTimeNow().ToShortDateString();
                }

                string Folio = filtersValues.Get("Folio").Trim();
                string rfc = filtersValues.Get("RFCP").Trim();
                string PaymentMethod = filtersValues.Get("PaymentMethod").Trim();
                string PaymentForm = filtersValues.Get("PaymentForm").Trim();
                string Currency = filtersValues.Get("Currency").Trim();

                string serie = filtersValues.Get("Serie").Trim();
                string nombreRazonSocial = filtersValues.Get("NombreRazonSocial").Trim();

                var pagination = new BasePagination();
                var filters = new CustomerCFDIFilter() { accountId = userAuth.Account.Id };
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                if (FilterStart != "") pagination.CreatedOnStart = Convert.ToDateTime(FilterStart);
                if (FilterEnd != "") pagination.CreatedOnEnd = Convert.ToDateTime(FilterEnd);
                if (Folio != "") filters.folio = Folio;
                if (rfc != "") filters.rfc = rfc;
                if (PaymentForm != "-1") filters.paymentForm = PaymentForm;
                if (PaymentMethod != "-1") filters.paymentMethod = PaymentMethod;
                if (Currency != "-1") filters.currency = Currency;
                if (!string.IsNullOrEmpty(serie)) filters.serie = serie;
                if (!string.IsNullOrEmpty(nombreRazonSocial)) filters.nombreRazonSocial = nombreRazonSocial;

                listResponse = _providerService.ProviderCDFIList(pagination, filters);

                list = listResponse.Select(x => new InvoicesReceivedListVM
                {
                    id = x.id,
                    folio = x.folio,
                    serie = x.serie,
                    paymentMethod = x.paymentMethod,
                    paymentForm = x.paymentForm,
                    currency = x.currency,
                    amount = x.subtotal.ToString("C2"),
                    iva = x.iva.ToString("C2"),
                    totalAmount = x.total.ToString("C2"),
                    invoicedAt = x.invoicedAt.ToShortDateString(),
                    rfc = x.rfc,
                    businessName = (x.rfc.Count() == 12 ? x.businessName : x.first_name + " " + x.last_name),
                    //first_name = x.first_name,
                    //last_name = x.last_name,
                    paymentFormDescription = x.paymentFormDescription
                }).ToList();

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords

                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }


            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message.ToString();
            }

            return Json(new
            {
                success = success,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion



        [HttpGet, AllowAnonymous]
        public JsonResult GetAutoComplite()
        {
            List<string> list = new List<string>();
            try
            {
                var authUser = Authenticator.AuthenticatedUser;
                var listRFC = _customerService.ListCustomerAutoComplete(authUser.Account.Id);
                list = listRFC.Select(x => x.rfc).ToList();
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }

            return Json(new
            {
                Data = list,
            }, JsonRequestBehavior.AllowGet);
        }

        #region modulo para envio de correo
        private void SendInvoice(string email, string rfc, string businessName, string comments, string linkXml, string linkPdf)
        {
            Dictionary<string, string> customParams = new Dictionary<string, string>();
            string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
            //string link = urlAccion + "Auth/Login";
            customParams.Add("param_rfc", rfc);
            customParams.Add("param_razon_social", businessName);
            customParams.Add("param_comentarios", comments);

            customParams.Add("param_link_xml", linkXml);
            customParams.Add("param_link_pdf", linkPdf);
            //customParams.Add("param_asunto", "");
            NotificationUtil.SendNotification(email, customParams, Constants.NOt_TEMPLATE_INVOICING);
        }
        #endregion

        #region Metodos genericos para Facturas recibidas/emitidas

        [HttpGet, AllowAnonymous]
        public ActionResult GetDownloadPDF(Int64 id, string type)
        {
            var authUser = Authenticator.AuthenticatedUser;

            string typeInvoicing = ((TypeInvoicing)Enum.Parse(typeof(TypeInvoicing), type, true)).GetDisplayName();

            try
            {
                var invoice = (dynamic)null;
                string logo = string.Empty;

                if (typeInvoicing == TypeInvoicing.ISSUED.GetDisplayName())
                {
                    invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);
                    logo = invoice.branchOffice != null ? invoice.branchOffice.logo : string.Empty;
                }
                else
                {
                    invoice = _invoiceReceivedService.FirstOrDefault(x => x.id == id);
                    //logo = invoice.branchOffice != null ? invoice.branchOffice.logo : string.Empty;
                }

                // var OfficeId = invoice.GetType().GetProperty("BranchOfficeId").GetValue(invoice, null);
                InvoicesVM cfdipdf = GetGenerateFilePDF(typeInvoicing, invoice, logo);


                //MensajeFlashHandler.RegistrarMensaje("Descargando...", TiposMensaje.Success);
                string rfc = authUser.Account.RFC;
                return new Rotativa.ViewAsPdf("InvoiceDownloadPDF", cfdipdf) { FileName = invoice.uuid + ".pdf" };
                //return View("InvoiceDownloadPDF", cfdipdf); 
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                //return View("InvoicesIssued", model);
                if (typeInvoicing == TypeInvoicing.ISSUED.GetDisplayName())
                    return RedirectToAction("InvoicesIssued");
                else
                    return RedirectToAction("InvoicesReceived");
            }

        }

        [HttpGet, AllowAnonymous]
        public void GetDownloadXML(Int64 id, string type)
        {
            var authUser = Authenticator.AuthenticatedUser;

            string typeInvoicing = ((TypeInvoicing)Enum.Parse(typeof(TypeInvoicing), type, true)).GetDisplayName();

            try
            {
                var StorageInvoices = typeInvoicing == TypeInvoicing.ISSUED.GetDisplayName() ? ConfigurationManager.AppSettings["StorageInvoicesIssued"] : ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                var invoice = (dynamic)null;

                if (typeInvoicing == TypeInvoicing.ISSUED.GetDisplayName())
                {
                    invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);
                }
                else
                {
                    invoice = _invoiceReceivedService.FirstOrDefault(x => x.id == id);
                }

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");

                MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoices, authUser.Account.RFC + "/" + invoice.uuid + ".xml");

                Response.ContentType = "application/xml";
                Response.AddHeader("Content-Disposition", "attachment;filename=" + invoice.uuid + ".xml");
                Response.BinaryWrite(stream.ToArray());
                Response.End();
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                Response.End();
            }
        }

        private InvoicesVM GetGenerateFilePDF(string typeInvoicing, dynamic invoice, string logo)
        {
            var authUser = Authenticator.AuthenticatedUser;

            var StorageInvoices = typeInvoicing == TypeInvoicing.ISSUED.GetDisplayName() ? ConfigurationManager.AppSettings["StorageInvoicesIssued"] : ConfigurationManager.AppSettings["StorageInvoicesReceived"];
            if (invoice == null)
                throw new Exception("No se encontro la factura emitida");

            if (invoice.xml == null)
                throw new Exception("El registro no cuenta con el xml de la factura emitida");

            MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoices, authUser.Account.RFC + "/" + invoice.uuid + ".xml");
            stream.Position = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3"); ;

            //Accedemos a nodo "Comprobante"
            XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

            string varFolio = nodeComprobante.Attributes["Folio"] != null ? nodeComprobante.Attributes["Folio"].Value : string.Empty;
            string varSerie = nodeComprobante.Attributes["Serie"] != null ? nodeComprobante.Attributes["Serie"].Value : string.Empty;
            string varFecha = nodeComprobante.Attributes["Fecha"].Value;
            string varMoneda = nodeComprobante.Attributes["Moneda"] != null ? nodeComprobante.Attributes["Moneda"].Value : string.Empty;
            string varSubTotal = nodeComprobante.Attributes["SubTotal"].Value;
            string varTotal = nodeComprobante.Attributes["Total"].Value;
            string varTipoComprobante = nodeComprobante.Attributes["TipoDeComprobante"].Value;
            string varLugarExpedicion = nodeComprobante.Attributes["LugarExpedicion"].Value;
            string varNoCertificado = nodeComprobante.Attributes["NoCertificado"].Value;
            string varCertificado = nodeComprobante.Attributes["Certificado"].Value;
            string varSello = nodeComprobante.Attributes["Sello"].Value;
            string varFormaPago = nodeComprobante.Attributes["FormaPago"] != null ? nodeComprobante.Attributes["FormaPago"].Value : string.Empty;
            string varMetodoPago = nodeComprobante.Attributes["MetodoPago"] != null ? nodeComprobante.Attributes["MetodoPago"].Value : string.Empty;
            string varDescuento1 = nodeComprobante.Attributes["Descuento"] != null ? nodeComprobante.Attributes["Descuento"].Value : string.Empty;
            string varTipoCambio = nodeComprobante.Attributes["TipoCambio"] != null ? nodeComprobante.Attributes["TipoCambio"].Value : "1";

            MonedaUtils formatoTexto = new MonedaUtils();
            var fecha = varFecha != null || varFecha != "" ? Convert.ToDateTime(varFecha).ToString("yyyy-MM-dd HH:mm:ss") : varFecha;

            InvoicesVM cfdipdf = new InvoicesVM()
            {
                Folio = varFolio,
                Serie = varSerie,
                SubTotal = varSubTotal,
                Total = varTotal,
                TipoDeComprobante = _typeVoucherService.FirstOrDefault(x => x.code == varTipoComprobante).Description,// ((TipoComprobante)Enum.Parse(typeof(TipoComprobante), varTipoComprobante, true)).GetDescription(),
                Certificado = varCertificado,
                NoCertificado = varNoCertificado,
                Sello = varSello,
                FormaPago = varFormaPago,
                MetodoPago = varMetodoPago, // _paymentMethodService.FirstOrDefault(x => x.code == varMetodoPago).Description, //((MetodoPago)Enum.Parse(typeof(MetodoPago), varMetodoPago, true)).GetDescription(),
                LugarExpedicion = varLugarExpedicion,
                Fecha = fecha,
                Moneda = varMoneda,
                TipoCambio = varTipoCambio,
                TotalTexto = formatoTexto.Convertir(varTotal.ToString(), true),
                Descuento = varDescuento1,
                Logo = logo
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
                    varRegimenFiscalText = _taxRegimeService.FirstOrDefault(x => x.code == varRegimenFiscal).description; //((RegimenFiscal)Enum.Parse(typeof(RegimenFiscal), "RegimenFiscal" + varRegimenFiscal, true)).GetDescription();
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
                    varUsoCFDIText = _useCFDIService.FirstOrDefault(x => x.code == varUsoCFDI).description; //((UsoCFDI)Enum.Parse(typeof(UsoCFDI), varUsoCFDI, true)).GetDescription();
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
                foreach (XmlNode node in nodeConceptos.ChildNodes)
                {

                    string varNoIdentificacion = node.Attributes["NoIdentificacion"] != null ? node.Attributes["NoIdentificacion"].Value : string.Empty;
                    string varClaveProdServ = node.Attributes["ClaveProdServ"] != null ? node.Attributes["ClaveProdServ"].Value : string.Empty;
                    string varCantidad = node.Attributes["Cantidad"] != null ? node.Attributes["Cantidad"].Value : string.Empty;
                    string varClaveUnidad = node.Attributes["ClaveUnidad"] != null ? node.Attributes["ClaveUnidad"].Value : string.Empty;
                    string varDescripcion = node.Attributes["Descripcion"] != null ? node.Attributes["Descripcion"].Value : string.Empty;
                    string varDescuento = node.Attributes["Descuento"] != null ? node.Attributes["Descuento"].Value : string.Empty;
                    string varImporte = node.Attributes["Importe"] != null ? node.Attributes["Importe"].Value : string.Empty;
                    string varValorUnitario = node.Attributes["ValorUnitario"] != null ? node.Attributes["ValorUnitario"].Value : string.Empty;
                    string varUnidad = node.Attributes["Unidad"] != null ? node.Attributes["Unidad"].Value : string.Empty;

                    Concepto concepto = new Concepto()
                    {
                        ClaveProdServ = varClaveProdServ,
                        NoIdentificacion = varNoIdentificacion,
                        Cantidad = varCantidad,
                        ClaveUnidad = varClaveUnidad,
                        Descripcion = varDescripcion,
                        Descuento = varDescuento,
                        Importe = varImporte,
                        ValorUnitario = varValorUnitario,
                        Unidad = string.IsNullOrEmpty(varUnidad) ? _driveKeyService.FirstOrDefault(x => x.code == varClaveUnidad).name : varUnidad
                    };

                    cfdipdf.Conceptos.Add(concepto);

                    XmlNode nodeImpuestos = nodeConceptos.SelectSingleNode("cfdi:Impuestos", nsm);
                    if (nodeImpuestos != null)
                    {
                        XmlNode nodeTraslados = nodeImpuestos.SelectSingleNode("cfdi:Traslados", nsm);
                        if (nodeTraslados != null)
                        {
                            XmlNode nodeTraslado = nodeTraslados.SelectSingleNode("cfdi:Traslado", nsm);
                            if (nodeTraslado != null)
                            {
                                string varImporteT = nodeTraslado.Attributes["Importe"] != null ? nodeTraslado.Attributes["Importe"].Value : string.Empty;
                                string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
                                string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;
                                string varImpuesto = nodeTraslado.Attributes["Impuesto"] != null ? nodeTraslado.Attributes["Impuesto"].Value : string.Empty;
                                string varBase = nodeTraslado.Attributes["Base"] != null ? nodeTraslado.Attributes["Base"].Value : string.Empty;

                                //Agregar modelo

                            }
                        }

                        XmlNode nodeRetenciones = nodeImpuestos.SelectSingleNode("cfdi:Retenciones", nsm);
                        if (nodeRetenciones != null)
                        {
                            XmlNode nodeRetencion = nodeRetenciones.SelectSingleNode("cfdi:Retencion", nsm);
                            if (nodeRetencion != null)
                            {
                                string varImporteT = nodeRetencion.Attributes["Importe"] != null ? nodeRetencion.Attributes["Importe"].Value : string.Empty;
                                string varTasaOCuota = nodeRetencion.Attributes["TasaOCuota"] != null ? nodeRetencion.Attributes["TasaOCuota"].Value : string.Empty;
                                string varTipoFactor = nodeRetencion.Attributes["TipoFactor"] != null ? nodeRetencion.Attributes["TipoFactor"].Value : string.Empty;
                                string varImpuesto = nodeRetencion.Attributes["Impuesto"] != null ? nodeRetencion.Attributes["Impuesto"].Value : string.Empty;
                                string varBase = nodeRetencion.Attributes["Base"] != null ? nodeRetencion.Attributes["Base"].Value : string.Empty;

                                //Agregar modelo

                            }
                        }
                    }
                }
                //}
            }

            XmlNode nodeImpuestosTT = nodeComprobante.SelectSingleNode("cfdi:Impuestos", nsm);
            if (nodeImpuestosTT != null)
            {
                string varTotalImpuestosTrasladados = nodeImpuestosTT.Attributes["TotalImpuestosTrasladados"] != null ? nodeImpuestosTT.Attributes["TotalImpuestosTrasladados"].Value : string.Empty;
                string varTotalImpuestosRetenidos = nodeImpuestosTT.Attributes["TotalImpuestosRetenidos"] != null ? nodeImpuestosTT.Attributes["TotalImpuestosRetenidos"].Value : string.Empty;

                cfdipdf.Impuestos.TotalImpuestosTrasladados = varTotalImpuestosTrasladados;
                cfdipdf.Impuestos.TotalImpuestosRetenidos = varTotalImpuestosRetenidos;

                XmlNode nodeTraslados = nodeImpuestosTT.SelectSingleNode("cfdi:Traslados", nsm);
                if (nodeTraslados != null)
                {
                    foreach (XmlNode node in nodeTraslados.ChildNodes)
                    {
                        //XmlNode nodeTraslado = node.SelectSingleNode("cfdi:Traslado", nsm);
                        //if (nodeTraslado != null)
                        //{
                        decimal varImporteT = 0;
                        string varImpuestoM = node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : string.Empty;
                        if (node.Attributes["Importe"] != null)
                        {
                            varImporteT = Convert.ToDecimal(node.Attributes["Importe"].Value);
                        }
                        //string varImporteT = nodeTraslado.Attributes["Importe"] != null ? nodeTraslado.Attributes["Importe"].Value : string.Empty;
                        //string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
                        //string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;

                        //Agregar modelo
                        Traslado tras = new Traslado()
                        {
                            Importe = varImporteT,
                            Impuesto = varImpuestoM
                        };

                        cfdipdf.Impuestos.Traslados.Add(tras);
                        //}
                    }
                }

                XmlNode nodeRetenciones = nodeImpuestosTT.SelectSingleNode("cfdi:Retenciones", nsm);
                if (nodeRetenciones != null)
                {
                    foreach (XmlNode node in nodeRetenciones.ChildNodes)
                    {
                        //XmlNode nodeRetencion = node.SelectSingleNode("cfdi:Retencion", nsm);
                        //if (nodeRetencion != null)
                        //{
                        decimal varImporteR = 0;
                        string varImpuestoR = node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : string.Empty;
                        if (node.Attributes["Importe"] != null)
                        {
                            varImporteR = Convert.ToDecimal(node.Attributes["Importe"].Value);
                        }
                        //string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
                        //string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;

                        //Agregar modelo
                        Retenido ret = new Retenido()
                        {
                            Importe = varImporteR,
                            Impuesto = varImpuestoR
                        };

                        cfdipdf.Impuestos.Retenidos.Add(ret);
                        //}
                    }

                    if (cfdipdf.Impuestos.Retenidos.Count() > 0)
                    {
                        cfdipdf.Impuestos.ImpuestosRetenidosISR = cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "001").Sum(x => x.Importe) > 0 ? cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "001").Sum(x => x.Importe).ToString() : string.Empty;
                        cfdipdf.Impuestos.ImpuestosRetenidosIVA = cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "002").Sum(x => x.Importe) > 0 ? cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "002").Sum(x => x.Importe).ToString() : string.Empty;
                    }
                }
            }

            XmlNode nodeCfdiRelacionados = nodeComprobante.SelectSingleNode("cfdi:CfdiRelacionados", nsm);
            if (nodeCfdiRelacionados != null)
            {
                string varTipoRelacion = nodeCfdiRelacionados.Attributes["TipoRelacion"] != null ? nodeCfdiRelacionados.Attributes["TipoRelacion"].Value : string.Empty;

                cfdipdf.CfdiRelacionados = new Models.CfdiRelacionados();
                cfdipdf.CfdiRelacionados.CfdiRelacionado = new List<Models.CfdiRelacionado>();
                cfdipdf.CfdiRelacionados.TipoRelación = varTipoRelacion;

                foreach (XmlNode node in nodeCfdiRelacionados.ChildNodes)
                {
                    string varUUDI = node.Attributes["UUID"] != null ? node.Attributes["UUID"].Value : string.Empty;

                    //Agregar modelo
                    Models.CfdiRelacionado cfdi = new Models.CfdiRelacionado()
                    {
                        UUID = varUUDI
                    };

                    cfdipdf.CfdiRelacionados.CfdiRelacionado.Add(cfdi);
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

                //Pagos
                nsm.AddNamespace("pago10", "http://www.sat.gob.mx/Pagos");

                XmlNode nodePagos = nodoComplemento.SelectSingleNode("pago10:Pagos", nsm);
                if (nodePagos != null)
                {
                    cfdipdf.Complemento.Pagos = new List<Models.Pago>();

                    foreach (XmlNode node in nodePagos.ChildNodes)
                    {
                        string varMonto = node.Attributes["Monto"] != null ? node.Attributes["Monto"].Value : string.Empty;
                        string varMonedaP = node.Attributes["MonedaP"] != null ? node.Attributes["MonedaP"].Value : string.Empty;
                        string varFormaDePagoP = node.Attributes["FormaDePagoP"] != null ? node.Attributes["FormaDePagoP"].Value : string.Empty;
                        string varFechaPago = node.Attributes["FechaPago"] != null ? node.Attributes["FechaPago"].Value : string.Empty;
                        string varNumOperacion = node.Attributes["NumOperacion"] != null ? node.Attributes["NumOperacion"].Value : string.Empty;
                        string varTipoCambioP = node.Attributes["TipoCambioP"] != null ? node.Attributes["TipoCambioP"].Value : string.Empty;

                        Models.Pago pago = new Models.Pago()
                        {
                            FechaPago = varFechaPago,
                            FormaDePagoP = varFormaDePagoP,
                            MonedaP = varMonedaP,
                            Monto = varMonto,
                            NumOperacion = varNumOperacion,
                            TipoCambioP = varTipoCambioP
                        };

                        XmlNode nodeDocto = node.SelectSingleNode("pago10:DoctoRelacionado", nsm);
                        if (nodeDocto != null)
                        {
                            string varImpSaldoInsoluto = nodeDocto.Attributes["ImpSaldoInsoluto"] != null ? nodeDocto.Attributes["ImpSaldoInsoluto"].Value : string.Empty;
                            string varImpSAldoAnt = nodeDocto.Attributes["ImpSaldoAnt"] != null ? nodeDocto.Attributes["ImpSaldoAnt"].Value : string.Empty;
                            string varNumParcialidad = nodeDocto.Attributes["NumParcialidad"] != null ? nodeDocto.Attributes["NumParcialidad"].Value : string.Empty;
                            string varMetodoDePagoDR = nodeDocto.Attributes["MetodoDePagoDR"] != null ? nodeDocto.Attributes["MetodoDePagoDR"].Value : string.Empty;
                            string varMonedaDR = nodeDocto.Attributes["MonedaDR"] != null ? nodeDocto.Attributes["MonedaDR"].Value : string.Empty;
                            string varIdDocumento = nodeDocto.Attributes["IdDocumento"] != null ? nodeDocto.Attributes["IdDocumento"].Value : string.Empty;
                            string varFolioCFDI = nodeDocto.Attributes["Folio"] != null ? nodeDocto.Attributes["Folio"].Value : string.Empty;
                            string varSerieCFDI = nodeDocto.Attributes["Serie"] != null ? nodeDocto.Attributes["Serie"].Value : string.Empty;

                            /*
                         
                            {
                                IdDocumento = item.uuid,
                                MonedaDR = item.currency,
                                MetodoDePagoDR = item.method,
                                NumParcialidad = item.numberPartialities.ToString(),
                                ImpSaldoAnt = item.previousBalance.ToString(),
                                ImpSaldoInsoluto = item.outstanding.ToString(),
                                Serie = item.serie,
                                Folio = item.folio
                            }
                         */


                            Models.DoctoRelacionado docto = new Models.DoctoRelacionado()
                            {
                                IdDocumento = varIdDocumento,
                                MonedaDR = varMonedaDR,
                                MetodoDePagoDR = varMetodoDePagoDR,
                                NumParcialidad = varNumParcialidad,
                                ImpSaldoAnt = varImpSAldoAnt,
                                ImpSaldoInsoluto = varImpSaldoInsoluto,
                                Folio = varFolioCFDI,
                                Serie = varSerieCFDI
                                //ImpPagado =
                            };

                            pago.DoctoRelacionado = docto;

                            cfdipdf.Complemento.Pagos.Add(pago);
                        }
                    }
                }
            }
            return cfdipdf;
        }

        ////prueba de formato de pdf
        //public ActionResult GetGenPdfTest()
        //{
        //    try
        //    {

        //        // taking full path of a file 
        //        //string strPath = "C:// myfiles//ref//file1.txt";
        //        string strPath = "C://Users//YelmyPech//Downloads//8bd7be67-9d24-439e-b646-48d7b8db82fb.xml";

        //        //// initialize the value of filename 
        //        //string filename = null;

        //        //// using the method 
        //        //filename = Path.GetFileName(strPath);
        //        ////Console.WriteLine("Filename = " + filename);

        //        //Console.ReadLine();
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(strPath);

        //        //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
        //        XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
        //        nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");

        //        //Accedemos a nodo "Comprobante"
        //        XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

        //        //Obtener Folio, Serie, SubTotal y Total

        //        string varFolio = nodeComprobante.Attributes["Folio"] != null ? nodeComprobante.Attributes["Folio"].Value : string.Empty;
        //        string varSerie = nodeComprobante.Attributes["Serie"] != null ? nodeComprobante.Attributes["Serie"].Value : string.Empty;
        //        string varSubTotal = nodeComprobante.Attributes["SubTotal"].Value;
        //        string varTotal = nodeComprobante.Attributes["Total"].Value;
        //        string varTipoComprobante = nodeComprobante.Attributes["TipoDeComprobante"].Value;
        //        string varCertificado = nodeComprobante.Attributes["Certificado"].Value;
        //        string varNoCertificado = nodeComprobante.Attributes["NoCertificado"].Value;
        //        string varSello = nodeComprobante.Attributes["Sello"].Value;
        //        string varFormaPago = nodeComprobante.Attributes["FormaPago"].Value;
        //        string varMetodoPago = nodeComprobante.Attributes["MetodoPago"].Value;
        //        string varLugarExpedicion = nodeComprobante.Attributes["LugarExpedicion"].Value;
        //        string varFecha = nodeComprobante.Attributes["Fecha"].Value;
        //        string varMoneda = nodeComprobante.Attributes["Moneda"] != null ? nodeComprobante.Attributes["Moneda"].Value : string.Empty;
        //        string varDescuento1 = nodeComprobante.Attributes["Descuento"] != null ? nodeComprobante.Attributes["Descuento"].Value : string.Empty;
        //        string varTipoCambio = nodeComprobante.Attributes["TipoCambio"] != null ? nodeComprobante.Attributes["TipoCambio"].Value : "1";

        //        MonedaUtils formatoTexto = new MonedaUtils();
        //        var fecha = varFecha != null || varFecha != "" ? Convert.ToDateTime(varFecha).ToString("yyyy-MM-dd HH:mm:ss") : varFecha;

        //        InvoicesVM cfdipdf = new InvoicesVM()
        //        {
        //            Folio = varFolio,
        //            Serie = varSerie,
        //            SubTotal = varSubTotal,
        //            Total = varTotal,
        //            TipoDeComprobante = _typeVoucherService.FirstOrDefault(x => x.code == varTipoComprobante).Description,// ((TipoComprobante)Enum.Parse(typeof(TipoComprobante), varTipoComprobante, true)).GetDescription(),
        //            Certificado = varCertificado,
        //            NoCertificado = varNoCertificado,
        //            Sello = varSello,
        //            FormaPago = varFormaPago,
        //            MetodoPago = varMetodoPago, // _paymentMethodService.FirstOrDefault(x => x.code == varMetodoPago).Description, //((MetodoPago)Enum.Parse(typeof(MetodoPago), varMetodoPago, true)).GetDescription(),
        //            LugarExpedicion = varLugarExpedicion,
        //            Fecha = fecha,
        //            Moneda = varMoneda,
        //            TipoCambio = varTipoCambio,
        //            TotalTexto = formatoTexto.Convertir(varTotal.ToString(), true),
        //            Descuento = varDescuento1,
        //            //Logo = logo
        //        };

        //        XmlNode nodeEmisor = nodeComprobante.SelectSingleNode("cfdi:Emisor", nsm);
        //        if (nodeEmisor != null)
        //        {
        //            string varNombre = nodeEmisor.Attributes["Nombre"].Value;
        //            string varRfc = nodeEmisor.Attributes["Rfc"].Value;
        //            string varRegimenFiscal = nodeEmisor.Attributes["RegimenFiscal"].Value;
        //            string varRegimenFiscalText = string.Empty;

        //            try
        //            {
        //                varRegimenFiscalText = _taxRegimeService.FirstOrDefault(x => x.code == varRegimenFiscal).description; //((RegimenFiscal)Enum.Parse(typeof(RegimenFiscal), "RegimenFiscal" + varRegimenFiscal, true)).GetDescription();
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            Models.Emisor emisor = new Models.Emisor()
        //            {
        //                Nombre = varNombre,
        //                Rfc = varRfc,
        //                RegimenFiscal = varRegimenFiscal,
        //                RegimenFiscalTexto = varRegimenFiscalText != null ? varRegimenFiscalText : varRegimenFiscal
        //            };

        //            cfdipdf.Emisor = emisor;
        //        }

        //        XmlNode nodeReceptor = nodeComprobante.SelectSingleNode("cfdi:Receptor", nsm);
        //        if (nodeReceptor != null)
        //        {
        //            string varNombre = nodeReceptor.Attributes["Nombre"].Value;
        //            string varRfc = nodeReceptor.Attributes["Rfc"].Value;
        //            string varUsoCFDI = nodeReceptor.Attributes["UsoCFDI"].Value;
        //            string varUsoCFDIText = string.Empty;
        //            try
        //            {
        //                varUsoCFDIText = _useCFDIService.FirstOrDefault(x => x.code == varUsoCFDI).description; //((UsoCFDI)Enum.Parse(typeof(UsoCFDI), varUsoCFDI, true)).GetDescription();
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            Models.Receptor receptor = new Models.Receptor()
        //            {
        //                Nombre = varNombre,
        //                Rfc = varRfc,
        //                UsoCFDI = varUsoCFDI,
        //                UsoCFDITexto = varUsoCFDIText != null ? varUsoCFDIText : varUsoCFDI
        //            };

        //            cfdipdf.Receptor = receptor;
        //        }

        //        GeneradorQR generador = new GeneradorQR();
        //        string selloQR = cfdipdf.Sello.Substring((cfdipdf.Sello.Length - 8), 8);
        //        string urlQR = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id=" + cfdipdf.Folio + "&re=" + cfdipdf.Emisor.Rfc + "&rr=" + cfdipdf.Receptor.Rfc + "&tt=" + cfdipdf.Total + "&fe=" + selloQR;
        //        var byteImage = generador.CrearCodigo(urlQR);
        //        cfdipdf.QR = "data:image/png;base64," + Convert.ToBase64String(byteImage);

        //        XmlNode nodeConceptos = nodeComprobante.SelectSingleNode("cfdi:Conceptos", nsm);
        //        if (nodeConceptos != null)
        //        {
        //            foreach (XmlNode node in nodeConceptos.ChildNodes)
        //            {

        //                string varNoIdentificacion = node.Attributes["NoIdentificacion"] != null ? node.Attributes["NoIdentificacion"].Value : string.Empty;
        //                string varClaveProdServ = node.Attributes["ClaveProdServ"] != null ? node.Attributes["ClaveProdServ"].Value : string.Empty;
        //                string varCantidad = node.Attributes["Cantidad"] != null ? node.Attributes["Cantidad"].Value : string.Empty;
        //                string varClaveUnidad = node.Attributes["ClaveUnidad"] != null ? node.Attributes["ClaveUnidad"].Value : string.Empty;
        //                string varDescripcion = node.Attributes["Descripcion"] != null ? node.Attributes["Descripcion"].Value : string.Empty;
        //                string varDescuento = node.Attributes["Descuento"] != null ? node.Attributes["Descuento"].Value : string.Empty;
        //                string varImporte = node.Attributes["Importe"] != null ? node.Attributes["Importe"].Value : string.Empty;
        //                string varValorUnitario = node.Attributes["ValorUnitario"] != null ? node.Attributes["ValorUnitario"].Value : string.Empty;
        //                string varUnidad = node.Attributes["Unidad"] != null ? node.Attributes["Unidad"].Value : string.Empty;

        //                Concepto concepto = new Concepto()
        //                {
        //                    ClaveProdServ = varClaveProdServ,
        //                    NoIdentificacion = varNoIdentificacion,
        //                    Cantidad = varCantidad,
        //                    ClaveUnidad = varClaveUnidad,
        //                    Descripcion = varDescripcion,
        //                    Descuento = varDescuento,
        //                    Importe = varImporte,
        //                    ValorUnitario = varValorUnitario,
        //                    Unidad = string.IsNullOrEmpty(varUnidad) ? _driveKeyService.FirstOrDefault(x => x.code == varClaveUnidad).name : varUnidad
        //                };

        //                cfdipdf.Conceptos.Add(concepto);

        //                XmlNode nodeImpuestos = nodeConceptos.SelectSingleNode("cfdi:Impuestos", nsm);
        //                if (nodeImpuestos != null)
        //                {
        //                    XmlNode nodeTraslados = nodeImpuestos.SelectSingleNode("cfdi:Traslados", nsm);
        //                    if (nodeTraslados != null)
        //                    {
        //                        XmlNode nodeTraslado = nodeTraslados.SelectSingleNode("cfdi:Traslado", nsm);
        //                        if (nodeTraslado != null)
        //                        {
        //                            string varImporteT = nodeTraslado.Attributes["Importe"] != null ? nodeTraslado.Attributes["Importe"].Value : string.Empty;
        //                            string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
        //                            string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;
        //                            string varImpuesto = nodeTraslado.Attributes["Impuesto"] != null ? nodeTraslado.Attributes["Impuesto"].Value : string.Empty;
        //                            string varBase = nodeTraslado.Attributes["Base"] != null ? nodeTraslado.Attributes["Base"].Value : string.Empty;

        //                            //Agregar modelo

        //                        }
        //                    }

        //                    XmlNode nodeRetenciones = nodeImpuestos.SelectSingleNode("cfdi:Retenciones", nsm);
        //                    if (nodeRetenciones != null)
        //                    {
        //                        XmlNode nodeRetencion = nodeRetenciones.SelectSingleNode("cfdi:Retencion", nsm);
        //                        if (nodeRetencion != null)
        //                        {
        //                            string varImporteT = nodeRetencion.Attributes["Importe"] != null ? nodeRetencion.Attributes["Importe"].Value : string.Empty;
        //                            string varTasaOCuota = nodeRetencion.Attributes["TasaOCuota"] != null ? nodeRetencion.Attributes["TasaOCuota"].Value : string.Empty;
        //                            string varTipoFactor = nodeRetencion.Attributes["TipoFactor"] != null ? nodeRetencion.Attributes["TipoFactor"].Value : string.Empty;
        //                            string varImpuesto = nodeRetencion.Attributes["Impuesto"] != null ? nodeRetencion.Attributes["Impuesto"].Value : string.Empty;
        //                            string varBase = nodeRetencion.Attributes["Base"] != null ? nodeRetencion.Attributes["Base"].Value : string.Empty;

        //                            //Agregar modelo

        //                        }
        //                    }
        //                }
        //            }
        //            //}

        //        }

        //        XmlNode nodeImpuestosTT = nodeComprobante.SelectSingleNode("cfdi:Impuestos", nsm);
        //        if (nodeImpuestosTT != null)
        //        {
        //            string varTotalImpuestosTrasladados = nodeImpuestosTT.Attributes["TotalImpuestosTrasladados"] != null ? nodeImpuestosTT.Attributes["TotalImpuestosTrasladados"].Value : string.Empty;
        //            string varTotalImpuestosRetenidos = nodeImpuestosTT.Attributes["TotalImpuestosRetenidos"] != null ? nodeImpuestosTT.Attributes["TotalImpuestosRetenidos"].Value : string.Empty;

        //            cfdipdf.Impuestos.TotalImpuestosTrasladados = varTotalImpuestosTrasladados;
        //            cfdipdf.Impuestos.TotalImpuestosRetenidos = varTotalImpuestosRetenidos;

        //            XmlNode nodeTraslados = nodeImpuestosTT.SelectSingleNode("cfdi:Traslados", nsm);
        //            if (nodeTraslados != null)
        //            {
        //                foreach (XmlNode node in nodeTraslados.ChildNodes)
        //                {
        //                    //XmlNode nodeTraslado = node.SelectSingleNode("cfdi:Traslado", nsm);
        //                    //if (nodeTraslado != null)
        //                    //{
        //                    decimal varImporteT = 0;
        //                    string varImpuestoM = node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : string.Empty;
        //                    if (node.Attributes["Importe"] != null)
        //                    {
        //                        varImporteT = Convert.ToDecimal(node.Attributes["Importe"].Value);
        //                    }
        //                    //string varImporteT = nodeTraslado.Attributes["Importe"] != null ? nodeTraslado.Attributes["Importe"].Value : string.Empty;
        //                    //string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
        //                    //string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;

        //                    //Agregar modelo
        //                    Traslado tras = new Traslado()
        //                    {
        //                        Importe = varImporteT,
        //                        Impuesto = varImpuestoM
        //                    };

        //                    cfdipdf.Impuestos.Traslados.Add(tras);
        //                    //}
        //                }
        //            }

        //            XmlNode nodeRetenciones = nodeImpuestosTT.SelectSingleNode("cfdi:Retenciones", nsm);
        //            if (nodeRetenciones != null)
        //            {
        //                foreach (XmlNode node in nodeRetenciones.ChildNodes)
        //                {
        //                    //XmlNode nodeRetencion = node.SelectSingleNode("cfdi:Retencion", nsm);
        //                    //if (nodeRetencion != null)
        //                    //{
        //                    decimal varImporteR = 0;
        //                    string varImpuestoR = node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : string.Empty;
        //                    if (node.Attributes["Importe"] != null)
        //                    {
        //                        varImporteR = Convert.ToDecimal(node.Attributes["Importe"].Value);
        //                    }
        //                    //string varTasaOCuota = nodeTraslado.Attributes["TasaOCuota"] != null ? nodeTraslado.Attributes["TasaOCuota"].Value : string.Empty;
        //                    //string varTipoFactor = nodeTraslado.Attributes["TipoFactor"] != null ? nodeTraslado.Attributes["TipoFactor"].Value : string.Empty;

        //                    //Agregar modelo
        //                    Retenido ret = new Retenido()
        //                    {
        //                        Importe = varImporteR,
        //                        Impuesto = varImpuestoR
        //                    };

        //                    cfdipdf.Impuestos.Retenidos.Add(ret);
        //                    //}
        //                }

        //                if (cfdipdf.Impuestos.Retenidos.Count() > 0)
        //                {
        //                    cfdipdf.Impuestos.ImpuestosRetenidosISR = cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "001").Sum(x => x.Importe) > 0 ? cfdipdf.Impuestos.Retenidos.Where(x => x.Impuesto == "001").Sum(x => x.Importe).ToString() : string.Empty;
        //                    cfdipdf.Impuestos.ImpuestosRetenidosIVA = cfdipdf.Impuestos.Traslados.Where(x => x.Impuesto == "002").Sum(x => x.Importe) > 0 ? cfdipdf.Impuestos.Traslados.Where(x => x.Impuesto == "002").Sum(x => x.Importe).ToString() : string.Empty;
        //                }
        //            }
        //        }

        //        XmlNode nodoComplemento = nodeComprobante.SelectSingleNode("cfdi:Complemento", nsm);
        //        if (nodoComplemento != null)
        //        {
        //            nsm.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

        //            XmlNode nodoTimbrado = nodoComplemento.SelectSingleNode("tfd:TimbreFiscalDigital", nsm);
        //            if (nodoTimbrado != null)
        //            {
        //                string varUUID = nodoTimbrado.Attributes["UUID"].Value;
        //                string varFechaTimbrado = nodoTimbrado.Attributes["FechaTimbrado"].Value;
        //                string varSelloCFD = nodoTimbrado.Attributes["SelloCFD"].Value;
        //                string varNoCertificadoSAT = nodoTimbrado.Attributes["NoCertificadoSAT"].Value;
        //                string varSelloSAT = nodoTimbrado.Attributes["SelloSAT"].Value;
        //                string varRfcProvCertif = nodoTimbrado.Attributes["RfcProvCertif"].Value;
        //                string varVersion = nodoTimbrado.Attributes["Version"].Value;

        //                var fechaTimbrado = varFechaTimbrado != null || varFechaTimbrado != "" ? Convert.ToDateTime(varFechaTimbrado).ToString("yyyy-MM-dd HH:mm:ss") : varFechaTimbrado;

        //                TimbreFiscalDigital timbre = new TimbreFiscalDigital()
        //                {
        //                    UUID = varUUID,
        //                    FechaTimbrado = fechaTimbrado,
        //                    NoCertificadoSAT = varNoCertificadoSAT,
        //                    RfcProvCertif = varRfcProvCertif,
        //                    SelloCFD = varSelloCFD,
        //                    SelloSAT = varSelloSAT,
        //                    Version = varVersion
        //                };

        //                cfdipdf.Complemento.TimbreFiscalDigital = timbre;
        //            }
        //        }

        //        return View("InvoiceDownloadPDF", cfdipdf);
        //    }
        //    catch (Exception ex)
        //    {
        //        string error = ex.Message.ToString();
        //        return View("Invoice");
        //    }
        //}

        ////Propuesta de reenvio de correo electronico
        //public ActionResult ForwardInvoice(string uuid)
        //{
        //    try
        //    {
        //        var authUser = Authenticator.AuthenticatedUser;
        //        var invoice = _invoiceIssuedService.FirstOrDefault(x => x.uuid.ToString() == uuid);
        //        var StorageInvoicesIssued = ConfigurationManager.AppSettings["StorageInvoicesIssued"];

        //        //MemoryStream streamPdf = AzureBlobService.DownloadFile(StorageInvoicesIssued, authUser.Account.RFC + "/" + invoice.uuid + ".xml");
        //        string url = string.Empty;
        //        //if (streamPdf == null)
        //        //{
        //            var byteArrayPdf = GetInvoicingPDF(invoice.id);
        //            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArrayPdf);
        //            var uploadPDF = AzureBlobService.UploadPublicFile(stream, invoice.uuid + ".pdf", StorageInvoicesIssued, authUser.Account.RFC);
        //            url = uploadPDF.Item1;
        //        //}

        //        SendInvoice("barbara@kiik.mx", authUser.Account.RFC, invoice.customer.businessName, "", invoice.xml, url);

        //        MensajeFlashHandler.RegistrarMensaje("Enviado...", TiposMensaje.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        string error = ex.Message.ToString();
        //        MensajeFlashHandler.RegistrarMensaje("Se realizo el reenvio del correo de factura", TiposMensaje.Error);
        //    }
        //    return RedirectToAction("Invoice/InvoicesIssued");
        //}

        #endregion
    }
}