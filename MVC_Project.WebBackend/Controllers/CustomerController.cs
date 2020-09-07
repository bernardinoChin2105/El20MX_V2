using ExcelDataReader;
using LogHubSDK.Models;
using MVC_Project.BackendWeb.Attributes;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace MVC_Project.WebBackend.Controllers
{
    public class CustomerController : Controller
    {
        private IAccountService _accountService;
        private ICustomerService _customerService;
        private ICustomerContactService _customerContactService;
        private IStateService _stateService;
        private ICurrencyService _currencyService;
        private IPaymentFormService _paymentFormService;
        private IPaymentMethodService _paymentMethodService;
        private IInvoiceIssuedService _invoiceIssuedService;

        public CustomerController(IAccountService accountService, ICustomerService customerService, IStateService stateService,
            ICustomerContactService customerContactService, ICurrencyService currencyService, IPaymentFormService paymentFormService,
            IPaymentMethodService paymentMethodService, IInvoiceIssuedService invoiceIssuedService)
        {
            _accountService = accountService;
            _customerService = customerService;
            _customerContactService = customerContactService;
            _stateService = stateService;
            _currencyService = currencyService;
            _paymentFormService = paymentFormService;
            _paymentMethodService = paymentMethodService;
            _invoiceIssuedService = invoiceIssuedService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCustomers(JQueryDataTableParams param, string filtros, bool first)
        {
            try
            {
                int totalDisplay = 0;
                int total = 0;
                var listResponse = new List<CustomerList>();
                if (!first)
                {
                    var userAuth = Authenticator.AuthenticatedUser;
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    string rfc = filtersValues.Get("RFC").Trim();
                    string businessName = filtersValues.Get("BusinessName").Trim();
                    string email = filtersValues.Get("Email").Trim();

                    var pagination = new BasePagination();
                    var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                    pagination.PageSize = param.iDisplayLength;
                    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                    if (rfc != "") filters.rfc = rfc;
                    if (businessName != "") filters.businessName = businessName;
                    if (email != "") filters.email = email;

                    listResponse = _customerService.CustomerList(pagination, filters);

                    //Corroborar los campos iTotalRecords y iTotalDisplayRecords

                    if (listResponse.Count() > 0)
                    {
                        totalDisplay = listResponse[0].Total;
                        total = listResponse.Count();
                    }
                }

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResponse
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

        [AllowAnonymous]
        public ActionResult Create()
        {
            try
            {
                var createCustomer = new CustomerViewModel();
                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                createCustomer.ListRegimen = new SelectList(regimenList);
                createCustomer.ListState = new SelectList(stateList);
                return View(createCustomer);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
            // return View();
        }

        // POST: Customer/Create
        //[HttpPost]
        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(CustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var authUser = Authenticator.AuthenticatedUser;

                if (_customerService.FindBy(x => x.rfc == model.RFC && x.account.id == authUser.Account.Id).Any())
                    throw new Exception("Ya existe un Cliente con el RFC proporcionado");

                //Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();
                DateTime todayDate = DateUtil.GetDateTimeNow();

                Customer customer = new Customer()
                {

                    uuid = Guid.NewGuid(),
                    account = new Account { id = authUser.Account.Id },
                    firstName = model.FistName,
                    lastName = model.LastName,
                    rfc = model.RFC,
                    curp = model.CURP,
                    businessName = model.BusinessName,
                    taxRegime = model.taxRegime,
                    street = model.Street,
                    interiorNumber = model.InteriorNumber,
                    outdoorNumber = model.OutdoorNumber,
                    zipCode = model.ZipCode,
                    deliveryAddress = model.DeliveryAddress,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString(),
                };

                if (model.Colony.Value > 0)
                    customer.colony = model.Colony.Value;
                if (model.Municipality.Value > 0)
                    customer.municipality = model.Municipality.Value;
                if (model.State.Value > 0)
                    customer.state = model.State.Value;
                if (model.Country.Value > 0)
                    customer.country = model.Country.Value;

                List<string> indexsP = new List<string>();
                List<string> indexsE = new List<string>();

                if (model.indexPhone != null)
                    indexsP = model.indexPhone.Split(',').ToList();

                if (model.indexEmail != null)
                    indexsE = model.indexEmail.Split(',').ToList();

                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Phones[i].EmailOrPhone != null && model.Emails[i].EmailOrPhone.Trim() != "" && !indexsE.Contains(i.ToString()))
                        {
                            CustomerContact email = new CustomerContact()
                            {
                                emailOrPhone = model.Emails[i].EmailOrPhone,
                                typeContact = model.Emails[i].TypeContact,
                                customer = customer,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            customer.customerContacts.Add(email);
                        }
                    }
                }

                if (model.Phones.Count() > 0)
                {
                    for (int i = 0; i < model.Phones.Count(); i++)
                    {
                        if (model.Phones[i].EmailOrPhone != null && model.Phones[i].EmailOrPhone.Trim() != "" && !indexsP.Contains(i.ToString()))
                        {
                            CustomerContact phone = new CustomerContact()
                            {
                                emailOrPhone = model.Phones[i].EmailOrPhone,
                                typeContact = model.Phones[i].TypeContact,
                                customer = customer,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            customer.customerContacts.Add(phone);
                        }
                    }
                }

                _customerService.Create(customer);
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                model.ListRegimen = new SelectList(regimenList);
                model.ListState = new SelectList(stateList);
                return View(model);
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetLocations(string zipCode)
        {
            var authUser = Authenticator.AuthenticatedUser;
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

        // GET: Customer/Edit/5
        public ActionResult Edit(string uuid)
        {
            try
            {
                CustomerViewModel model = new CustomerViewModel();
                var userAuth = Authenticator.AuthenticatedUser;

                var customer = _customerService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
                if (customer == null)
                    throw new Exception("El registro de Cliente no se encontró en la base de datos");

                model.Id = customer.id;
                model.FistName = customer.firstName;
                model.LastName = customer.lastName;
                model.RFC = customer.rfc;
                model.CURP = customer.curp;
                model.BusinessName = customer.businessName;
                model.Street = customer.street;
                model.OutdoorNumber = customer.outdoorNumber;
                model.InteriorNumber = customer.interiorNumber;
                model.ZipCode = customer.zipCode;
                model.Colony = customer.colony;
                model.Municipality = customer.municipality;
                model.State = customer.state;
                model.Country = customer.country;
                model.DeliveryAddress = customer.deliveryAddress;
                if (customer.taxRegime != null)
                    model.taxRegime = ((TypeTaxRegimen)Enum.Parse(typeof(TypeTaxRegimen), customer.taxRegime)).ToString();

                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                model.ListRegimen = new SelectList(regimenList);
                model.ListState = new SelectList(stateList);

                var emails = customer.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new CustomerContactsViewModel
                            {
                                Id = x.id,
                                TypeContact = x.typeContact,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList();

                if (emails.Count() > 0)
                {
                    model.Emails = emails;
                }

                var phones = customer.customerContacts.Where(x => x.typeContact == TypeContact.PHONE.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new CustomerContactsViewModel
                            {
                                Id = x.id,
                                TypeContact = x.typeContact,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList();
                if (phones.Count() > 0)
                {
                    model.Phones = phones;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        // POST: Customer/Edit/5
        [HttpPost]
        public ActionResult Edit(CustomerViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            CustomerViewModel customer = new CustomerViewModel();
            var customerData = _customerService.FirstOrDefault(x => x.id == model.Id && x.account.id == userAuth.Account.Id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var authUser = Authenticator.AuthenticatedUser;
                DateTime todayDate = DateUtil.GetDateTimeNow();

                customerData.firstName = model.FistName;
                customerData.lastName = model.LastName;
                customerData.rfc = model.RFC;
                customerData.curp = model.CURP;
                customerData.businessName = model.BusinessName;
                customerData.taxRegime = model.taxRegime;
                customerData.street = model.Street;
                customerData.interiorNumber = model.InteriorNumber;
                customerData.outdoorNumber = model.OutdoorNumber;
                customerData.zipCode = model.ZipCode;
                customerData.deliveryAddress = model.DeliveryAddress;
                customerData.modifiedAt = todayDate;
                customerData.status = SystemStatus.ACTIVE.ToString();

                if (model.Colony.Value > 0)
                    customerData.colony = model.Colony.Value;
                if (model.Municipality.Value > 0)
                    customerData.municipality = model.Municipality.Value;
                if (model.State.Value > 0)
                    customerData.state = model.State.Value;
                if (model.Country.Value > 0)
                    customerData.country = model.Country.Value;

                #region Actualizar registros de las listas de emails y teléfonos 
                List<string> indexsP = new List<string>();
                List<string> indexsE = new List<string>();

                if (model.indexPhone != null)
                    indexsP = model.indexPhone.Split(',').ToList();

                if (model.indexEmail != null)
                    indexsE = model.indexEmail.Split(',').ToList();

                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Emails[i].EmailOrPhone != null && model.Emails[i].EmailOrPhone.Trim() != "" && !indexsE.Contains(i.ToString()))
                        {
                            if (model.Emails[i].Id > 0)
                            {
                                //actualizar lista
                                var contact = customerData.customerContacts.Where(x => x.id == model.Emails[i].Id).FirstOrDefault();
                                contact.emailOrPhone = model.Emails[i].EmailOrPhone;
                                contact.modifiedAt = todayDate;
                            }
                            else
                            {
                                CustomerContact email = new CustomerContact()
                                {
                                    emailOrPhone = model.Emails[i].EmailOrPhone,
                                    typeContact = model.Emails[i].TypeContact,
                                    customer = customerData,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };

                                customerData.customerContacts.Add(email);
                            }
                        }
                    }
                }

                if (model.Phones.Count() > 0)
                {
                    for (int i = 0; i < model.Phones.Count(); i++)
                    {
                        if (model.Phones[0].EmailOrPhone != null && model.Phones[0].EmailOrPhone.Trim() != "" && !indexsP.Contains(i.ToString()))
                        {
                            if (model.Phones[i].Id > 0)
                            {
                                //actualizar lista
                                var contact = customerData.customerContacts.Where(x => x.id == model.Phones[i].Id).FirstOrDefault();
                                contact.emailOrPhone = model.Phones[i].EmailOrPhone;
                                contact.modifiedAt = todayDate;
                            }
                            else
                            {
                                CustomerContact phone = new CustomerContact()
                                {
                                    emailOrPhone = model.Phones[i].EmailOrPhone,
                                    typeContact = model.Phones[i].TypeContact,
                                    customer = customerData,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };

                                customerData.customerContacts.Add(phone);
                            }
                        }
                    }
                }

                //Eliminar registros de la lista
                List<string> list = new List<string>();

                if (model.dataContacts != null)
                    list = model.dataContacts.Split(',').ToList();

                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var itemToRemove = customerData.customerContacts.SingleOrDefault(r => r.id == Convert.ToInt64(item));
                        if (itemToRemove != null)
                        {
                            itemToRemove.status = SystemStatus.INACTIVE.ToString();
                        }
                    }
                }
                #endregion

                _customerService.Update(customerData);
                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                var stateList = _stateService.GetAll().Select(x => new SelectListItem() { Text = x.nameState, Value = x.id.ToString() }).ToList();
                stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    });

                model.ListRegimen = new SelectList(regimenList);
                model.ListState = new SelectList(stateList);
                return View(model);
            }
        }

        [HttpGet, AllowAnonymous, FileDownload]
        public FileResult ExportTemplate()
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;               

                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet campo = pck.Workbook.Worksheets.Add("LISTA DE CLIENTES");

                    campo.Cells["A1:F1"].Style.Font.Bold = true;

                    //campo.Cells["A1"].Value = "No.";
                    campo.Cells["A1"].Value = "Nombre(s)";
                    campo.Cells["B1"].Value = "Apellido(s)";
                    campo.Cells["C1"].Value = "RFC";
                    //campo.Cells["E1"].Value = "CURP";
                    campo.Cells["D1"].Value = "Nombre/Razón Social";
                    campo.Cells["E1"].Value = "Tipo Régimen Fiscal";
                    //campo.Cells["H1"].Value = "Calle y Cruzamientos";
                    //campo.Cells["I1"].Value = "Número Exterior";
                    //campo.Cells["J1"].Value = "Número Interior";
                    campo.Cells["F1"].Value = "C.P.";
                    //campo.Cells["L1"].Value = "Colonia";
                    //campo.Cells["M1"].Value = "Alcaldía/Municipio";
                    //campo.Cells["N1"].Value = "Estado";
                    //campo.Cells["O1"].Value = "País";
                    //campo.Cells["P1"].Value = "Domicilio Comercial";
                    //campo.Cells["Q1"].Value = "Email";
                    //campo.Cells["R1"].Value = "Teléfono";
                    //campo.Cells["S1"].Value = "Fecha Creación";
                    //campo.Cells["T1"].Value = "RFC Cuenta";

                    campo.Cells[campo.Dimension.Address].AutoFitColumns();
                    byte[] bin = pck.GetAsByteArray();
                    return File(bin, "application/vnd.ms-excel", "ListaClientesPlantilla.xlsx");
                }
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                throw;
            }
        }

        [HttpPost, AllowAnonymous, FileDownload]
        public FileResult ExportListCustomer(string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string rfc = filtersValues.Get("RFC").Trim();
                string businessName = filtersValues.Get("BusinessName").Trim();
                string email = filtersValues.Get("Email").Trim();

                var filters = new CustomerFilter() { uuid = userAuth.Account.Uuid.ToString() };
                if (rfc != "") filters.rfc = rfc;
                if (businessName != "") filters.businessName = businessName;
                if (email != "") filters.email = email;

                var listResponse = _customerService.ExportListCustomer(filters);

                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet campo = pck.Workbook.Worksheets.Add("LISTA DE CLIENTES");

                    campo.Cells["A1:Z1"].Style.Font.Bold = true;

                    campo.Cells["A1"].Value = "No.";
                    campo.Cells["B1"].Value = "Nombre(s)";
                    campo.Cells["C1"].Value = "Apellido(s)";
                    campo.Cells["D1"].Value = "RFC";
                    campo.Cells["E1"].Value = "CURP";
                    campo.Cells["F1"].Value = "Nombre/Razón Social";
                    campo.Cells["G1"].Value = "Tipo Régimen Fiscal";
                    campo.Cells["H1"].Value = "Calle y Cruzamientos";
                    campo.Cells["I1"].Value = "Número Exterior";
                    campo.Cells["J1"].Value = "Número Interior";
                    campo.Cells["K1"].Value = "C.P.";
                    campo.Cells["L1"].Value = "Colonia";
                    campo.Cells["M1"].Value = "Alcaldía/Municipio";
                    campo.Cells["N1"].Value = "Estado";
                    campo.Cells["O1"].Value = "País";
                    campo.Cells["P1"].Value = "Domicilio Comercial";
                    campo.Cells["Q1"].Value = "Email";
                    campo.Cells["R1"].Value = "Teléfono";
                    campo.Cells["S1"].Value = "Fecha Creación";
                    campo.Cells["T1"].Value = "RFC Cuenta";

                    int rowIndex = 2;
                    for (int i = 0; i < listResponse.Count(); i++)
                    {
                        string enumFiscal = string.Empty;
                        string rowIndexString = rowIndex.ToString();
                        campo.Cells["A" + rowIndexString].Value = i + 1;
                        campo.Cells["A" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        campo.Cells["B" + rowIndexString].Value = listResponse[i].first_name;
                        campo.Cells["B" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["C" + rowIndexString].Value = listResponse[i].last_name;
                        campo.Cells["C" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["D" + rowIndexString].Value = listResponse[i].rfc;
                        campo.Cells["D" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["E" + rowIndexString].Value = listResponse[i].curp;
                        campo.Cells["E" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["F" + rowIndexString].Value = listResponse[i].businessName;
                        campo.Cells["F" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        if (listResponse[i].taxRegime != null)
                        {
                            enumFiscal = ((TypeTaxRegimen)Enum.Parse(typeof(TypeTaxRegimen), listResponse[i].taxRegime)).GetDisplayName();
                        }

                        campo.Cells["G" + rowIndexString].Value = enumFiscal;
                        campo.Cells["G" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["H" + rowIndexString].Value = listResponse[i].street;
                        campo.Cells["H" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["I" + rowIndexString].Value = listResponse[i].outdoorNumber;
                        campo.Cells["I" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["J" + rowIndexString].Value = listResponse[i].interiorNumber;
                        campo.Cells["J" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["K" + rowIndexString].Value = listResponse[i].zipCode;
                        campo.Cells["K" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["L" + rowIndexString].Value = listResponse[i].nameSettlementType + " " + listResponse[i].nameSettlement;
                        campo.Cells["L" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["M" + rowIndexString].Value = listResponse[i].nameMunicipality;
                        campo.Cells["M" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["N" + rowIndexString].Value = listResponse[i].nameState;
                        campo.Cells["N" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["O" + rowIndexString].Value = listResponse[i].nameCountry;
                        campo.Cells["O" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["P" + rowIndexString].Value = listResponse[i].deliveryAddress;
                        campo.Cells["P" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["Q" + rowIndexString].Value = listResponse[i].email;
                        campo.Cells["Q" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["R" + rowIndexString].Value = listResponse[i].phone;
                        campo.Cells["R" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["S" + rowIndexString].Value = listResponse[i].createdAt.ToShortDateString();
                        campo.Cells["S" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["T" + rowIndexString].Value = listResponse[i].rfcAccount;
                        campo.Cells["T" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        rowIndex++;
                    }

                    campo.Cells[campo.Dimension.Address].AutoFitColumns();
                    byte[] bin = pck.GetAsByteArray();
                    return File(bin, "application/vnd.ms-excel", "ListaClientes_(" + DateTime.Now.ToString("G") + ").xlsx");
                }
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                throw;
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult ImportExcelCustomer(HttpPostedFileBase Excel)
        {

            List<object> Errores = new List<object>();
            List<ExportListCustomer> datosErroneos = new List<ExportListCustomer>();
            List<ExportListCustomer> datos = new List<ExportListCustomer>();
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();

                if (Excel != null && Excel.ContentLength > 0)
                {
                    Stream stream = Excel.InputStream;
                    IExcelDataReader reader = null;

                    if (Excel.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (Excel.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        throw new Exception("Favor de seleccionar un formato de Excel permitido.");
                    }

                    //reader.IsFirstRowAsColumnNames = true;
                    DataSet result = reader.AsDataSet();
                    var tabla = result.Tables[0];

                    if (tabla.Rows.Count > 0)
                    {
                        for (int i = 0; i < tabla.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                try
                                {
                                    if (!tabla.Rows[i].IsNull(0)
                                        || !tabla.Rows[i].IsNull(1)
                                        || !tabla.Rows[i].IsNull(2)
                                        || !tabla.Rows[i].IsNull(3)
                                        || !tabla.Rows[i].IsNull(4)
                                        || !tabla.Rows[i].IsNull(5))
                                    {
                                        //Validar
                                        string taxRegime = string.Empty;
                                        if (!tabla.Rows[i].IsNull(4) && tabla.Rows[i].ItemArray[4].ToString() != "")
                                        {
                                            var taxRegimeEnum = EnumUtils.GetValueFromDescription<TypeTaxRegimen>(tabla.Rows[i].ItemArray[4].ToString());//funciona cuando obtenemos la descripción
                                            taxRegime = taxRegimeEnum.ToString();
                                        }

                                        //bool deliveryAddress = false;
                                        //if (!tabla.Rows[i].IsNull(15) && tabla.Rows[i].ItemArray[15].ToString() != "")
                                        //{
                                        //    deliveryAddress = Convert.ToBoolean(tabla.Rows[i].ItemArray[15].ToString());
                                        //}

                                        ExportListCustomer customers = new ExportListCustomer
                                        {
                                            first_name = tabla.Rows[i].ItemArray[0].ToString(),
                                            last_name = tabla.Rows[i].ItemArray[1].ToString(),
                                            rfc = tabla.Rows[i].ItemArray[2].ToString(),
                                            //curp = tabla.Rows[i].ItemArray[4].ToString(),
                                            businessName = tabla.Rows[i].ItemArray[3].ToString(),
                                            taxRegime = taxRegime,
                                            //street = tabla.Rows[i].ItemArray[7].ToString(),
                                            //interiorNumber = tabla.Rows[i].ItemArray[8].ToString(),
                                            //outdoorNumber = tabla.Rows[i].ItemArray[9].ToString(),
                                            zipCode = tabla.Rows[i].ItemArray[5].ToString(),
                                            //nameSettlement = tabla.Rows[i].ItemArray[11].ToString(),
                                            //nameMunicipality = tabla.Rows[i].ItemArray[12].ToString(),
                                            //nameState = tabla.Rows[i].ItemArray[13].ToString(),
                                            //nameCountry = tabla.Rows[i].ItemArray[14].ToString(),
                                            //deliveryAddress = deliveryAddress,
                                            //email = tabla.Rows[i].ItemArray[16].ToString(),
                                            //phone = tabla.Rows[i].ItemArray[17].ToString(),
                                            createdAt = todayDate,
                                            modifiedAt = todayDate,
                                            status = SystemStatus.ACTIVE.ToString(),
                                            uuid = Guid.NewGuid(),
                                            accountId = authUser.Account.Id,
                                        };

                                        datos.Add(customers);
                                    }
                                }
                                catch (Exception Error)
                                {
                                    Errores.Add(new { Error = Error.Message.ToString(), Elemento = tabla.Rows[i].ItemArray });
                                }
                            }
                            else
                            {
                                object[] encabezado = tabla.Rows[0].ItemArray;

                                try
                                {
                                    //if (encabezado[0].ToString() != "No.") throw new Exception("Título de columna inválida");
                                    if (encabezado[0].ToString() != "Nombre(s)") throw new Exception("Título de columna inválida");
                                    if (encabezado[1].ToString() != "Apellido(s)") throw new Exception("Título de columna inválida");
                                    if (encabezado[2].ToString() != "RFC") throw new Exception("Título de columna inválida");
                                    //if (encabezado[4].ToString() != "CURP") throw new Exception("Título de columna inválida");
                                    if (encabezado[3].ToString() != "Nombre/Razón Social") throw new Exception("Título de columna inválida");
                                    if (encabezado[4].ToString() != "Tipo Régimen Fiscal") throw new Exception("Título de columna inválida");
                                    //if (encabezado[7].ToString() != "Calle y Cruzamientos") throw new Exception("Título de columna inválida");
                                    //if (encabezado[8].ToString() != "Número Exterior") throw new Exception("Título de columna inválida");
                                    //if (encabezado[9].ToString() != "Número Interior") throw new Exception("Título de columna inválida");
                                    if (encabezado[5].ToString() != "C.P.") throw new Exception("Título de columna inválida");
                                    //if (encabezado[11].ToString() != "Colonia") throw new Exception("Título de columna inválida");
                                    //if (encabezado[12].ToString() != "Alcaldía/Municipio") throw new Exception("Título de columna inválida");
                                    //if (encabezado[13].ToString() != "Estado") throw new Exception("Título de columna inválida");
                                    //if (encabezado[14].ToString() != "País") throw new Exception("Título de columna inválida");
                                    //if (encabezado[15].ToString() != "Domicilio Comercial") throw new Exception("Título de columna inválida");
                                    //if (encabezado[16].ToString() != "Email") throw new Exception("Título de columna inválida");
                                    //if (encabezado[17].ToString() != "Teléfono") throw new Exception("Título de columna inválida");
                                    //if (encabezado[18].ToString() != "Fecha Creación") throw new Exception("Título de columna inválida");
                                    //if (encabezado[19].ToString() != "RFC Cuenta") throw new Exception("Título de columna inválida");
                                }
                                catch (Exception Error)
                                {
                                    Errores.Add(new
                                    {
                                        Error = Error,
                                        elemento = tabla.Rows[0].ItemArray
                                    });
                                }
                            }
                        }
                    }

                    if (Errores.Count > 0)
                    {
                        throw new Exception("¡Verifica el archivo, no se cuentan con los datos obligatorios!");
                        //return Json(new { Success = false, Mensaje =, Tipo = 0 }, JsonRequestBehavior.AllowGet);
                    }

                    /*
                     * - Validar si el usuario existe
                     * - sino existe no guarda el archivo 
                     **/

                    // Validar usuarios duplicados
                    List<string> rfcs = datos.Select(c => c.rfc).ToList();
                    List<Duplicates> records = (from item in rfcs
                                                group item by item into g
                                                select new Duplicates
                                                {
                                                    RFCS = g.Key,
                                                    Repetitions = g.Count()
                                                }).ToList();

                    List<Duplicates> RDuplicates = (from d in records where d.Repetitions > 1 select d).ToList();
                    if (RDuplicates.Count > 0)
                    {
                        List<string> RFCDuplicates = (from d in RDuplicates select d.RFCS).ToList();
                        IEnumerable<ExportListCustomer> SinRegistros = from d in datos where RFCDuplicates.Contains(d.rfc) select d;
                        SinRegistros = SinRegistros.Select(c => { c.Observaciones = "RFC con registro duplicados en el archivo."; return c; }).ToList();
                        datosErroneos = datosErroneos.Union(SinRegistros).ToList();
                    }

                    //Validar rfc
                    var Existen = _customerService.ValidateRFC(rfcs, authUser.Account.Id);

                    List<string> NoExisten = rfcs.Except(Existen).ToList();

                    if (Existen.Count > 0)
                    {
                        IEnumerable<ExportListCustomer> SiExisteRegistros = from d in datos where Existen.Contains(d.rfc) select d;
                        SiExisteRegistros = SiExisteRegistros.Select(c => { c.Observaciones = "Existe registrado el RFC de el cliente."; return c; }).ToList();
                        datosErroneos = datosErroneos.Union(SiExisteRegistros).ToList();
                        //return Json(new { Success = false, Mensaje = "¡Verifica el archivo, hay usuarios que no existen!", Tipo = 3, SinGuardar = SinRegistros }, JsonRequestBehavior.AllowGet);
                    }

                    if (datosErroneos.Count > 0)
                    {
                        return Json(new { Success = false, Mensaje = "¡Verifica el archivo, hay clientes ya existentes o estan duplicados!", Tipo = 1, SinGuardar = datosErroneos }, JsonRequestBehavior.AllowGet);
                    }

                    reader.Close();

                    //Guardado de información
                    List<Customer> clientes = new List<Customer>();

                    clientes = datos.Select(x => new Customer
                    {
                        uuid = Guid.NewGuid(),
                        account = new Account { id = authUser.Account.Id },
                        firstName = x.first_name,
                        lastName = x.last_name,
                        rfc = x.rfc,
                        //curp = x.curp,
                        businessName = x.businessName,
                        //street = x.street,
                        //interiorNumber = x.interiorNumber,
                        //outdoorNumber = x.outdoorNumber,
                        zipCode = x.zipCode,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString(),
                        //deliveryAddress = x.deliveryAddress,
                        //customerContacts = new List<CustomerContact>
                        //{
                        //    x.email != ""? new CustomerContact
                        //    {
                        //        emailOrPhone = x.email,
                        //        typeContact = TypeContact.EMAIL.ToString(),
                        //        createdAt = todayDate,
                        //        modifiedAt = todayDate,
                        //        status = SystemStatus.ACTIVE.ToString()
                        //    }: null,
                        //    x.phone != ""? new CustomerContact
                        //    {
                        //        emailOrPhone = x.phone,
                        //        typeContact = TypeContact.PHONE.ToString(),
                        //        createdAt = todayDate,
                        //        modifiedAt = todayDate,
                        //        status = SystemStatus.ACTIVE.ToString()
                        //    }: null
                        //}
                    }).ToList();

                    //clientes = clientes.Select(x =>
                    //{
                    //    x.customerContacts = x.customerContacts.Where(b => b != null)
                    //    .Select(b => { b.customer = x; return b; }).ToList();
                    //    return x;
                    //}).ToList();

                    if (clientes.Count() > 0)
                    {
                        _customerService.Create(clientes);
                        //LogHub de bitacora
                        return Json(new
                        {
                            Success = true,
                            Mensaje = "¡" + clientes.Count() + " Registros guardados exitosamente!",
                            //Tipo = 2,
                            //SinGuardar = clientesNoRegistrados,
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente!", Tipo = 0 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente! Archivo no válido", Tipo = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Error)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + Error.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );
                return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente! " + Error.Message.ToString(), Tipo = 0, Error = Error.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public ActionResult InvoicesIssued()
        {
            ViewBag.Date = new
            {
                MinDate = DateTime.Now.AddDays(-10).ToString("dd-MM-yyyy"),
                MaxDate = DateTime.Now.ToString("dd-MM-yyyy")
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
                if (!first)
                {
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                    string FilterEnd = filtersValues.Get("FilterEndDate").Trim();
                    string Folio = filtersValues.Get("Folio").Trim();
                    string rfc = filtersValues.Get("RFC").Trim();
                    string PaymentMethod = filtersValues.Get("PaymentMethod").Trim();
                    string PaymentForm = filtersValues.Get("PaymentForm").Trim();
                    string Currency = filtersValues.Get("Currency").Trim();

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

                    listResponse = _customerService.CustomerCDFIList(pagination, filters);

                    list = listResponse.Select(x => new InvoicesIssuedListVM
                    {
                        id = x.id,
                        folio = x.folio,
                        serie = x.serie,
                        paymentMethod = x.paymentMethod,
                        paymentForm = x.paymentForm,
                        currency = x.currency,
                        amount = x.amount.ToString("C2"),
                        iva = x.iva.ToString("C2"),
                        totalAmount = x.totalAmount.ToString("C2"),
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

        [HttpGet, AllowAnonymous]
        public ActionResult GetDownloadPDF(Int64 id)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                var invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");

                if (invoice.xml == null)
                    throw new Exception("El registro no cuenta con el xml de la factura emitida");

                //Factura
                XmlDocument doc = new XmlDocument();
                InvoicesVM cfdi = new InvoicesVM();
                doc.LoadXml(invoice.xml);//Leer el XML
                string pdf = string.Empty;

                //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
                XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
                nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");

                //Accedemos a nodo "Comprobante"
                XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

                //Obtener Folio, Serie, SubTotal y Total
                 cfdi.Folio = nodeComprobante.Attributes["Folio"].Value;
                 cfdi.Serie = nodeComprobante.Attributes["Serie"].Value;
                 cfdi.SubTotal = nodeComprobante.Attributes["SubTotal"].Value;
                 cfdi.Total = nodeComprobante.Attributes["Total"].Value;
                //pdf = String.Format("Serie: {0}, Folio: {1}, SubTotal: {2}, Total: {3}", varSerie, varFolio, varSubTotal, varTotal);
                //pdf += "<br />";

                

                //Obtener impuestos
                XmlNode nodeImpuestos = nodeComprobante.SelectSingleNode("cfdi:Impuestos", nsm);
                if (nodeImpuestos != null)
                {
                    //Obtenemos TotalImpuestosRetenidos y TotalImpuestosTrasladados
                    string varTotalImpuestosRetenidos = nodeImpuestos.Attributes["TotalImpuestosRetenidos"] != null ? nodeImpuestos.Attributes["TotalImpuestosRetenidos"].Value : "";
                    string varTotalImpuestosTrasladados = nodeImpuestos.Attributes["TotalImpuestosTrasladados"] != null ? nodeImpuestos.Attributes["TotalImpuestosTrasladados"].Value : "";
                    //Obtener impuestos retenidos
                    pdf += "Retenciones: <br />";
                    XmlNode nodeImpuestosRetenciones = nodeImpuestos.SelectSingleNode("cfdi:Retenciones", nsm);
                    if (nodeImpuestosRetenciones != null)
                    {
                        foreach (XmlNode node in nodeImpuestosRetenciones.SelectNodes("cfdi:Retencion", nsm))
                        {
                            pdf += String.Format("Impuesto: {0}, Importe: {1} <br />",
                                                            node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : "",
                                                            node.Attributes["Importe"] != null ? node.Attributes["Importe"].Value : "");
                        }
                    }

                    //Obtener impuestos trasladados
                    pdf += "Traslados: <br />";
                    XmlNode nodeImpuestosTraslados = nodeImpuestos.SelectSingleNode("cfdi:Traslados", nsm);
                    foreach (XmlNode node in nodeImpuestosTraslados.SelectNodes("cfdi:Traslado", nsm))
                    {
                        pdf += String.Format("Impuesto: {0}, Importe: {1} <br />",
                                                        node.Attributes["Impuesto"] != null ? node.Attributes["Impuesto"].Value : "",
                                                        node.Attributes["Importe"] != null ? node.Attributes["Importe"].Value : "");
                    }

                }

                InvoicesIssuedListVM pdfModel = new InvoicesIssuedListVM();
                pdfModel.xml = pdf;

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
                return new Rotativa.ViewAsPdf("InvoiceDownloadPDF", pdfModel) { FileName = invoice.folio + invoice.serie + "_" + invoice.invoicedAt + ".pdf" };
            }
            catch (Exception ex)
            {
                //LogUtil.AddEntry(
                //       "Error al descargar el diagnostico: " + ex.Message.ToString(),
                //       ENivelLog.Error,
                //       authUser.Id,
                //       authUser.Email,
                //       EOperacionLog.ACCESS,
                //       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                //       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                //       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                //    );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View("GetInvoices");
            }

        }

        [HttpGet, AllowAnonymous]
        public void GetDownloadXML(Int64 id)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                var invoice = _invoiceIssuedService.FirstOrDefault(x => x.id == id);

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");

                LogUtil.AddEntry(
                       "Descarga del xml de la cuenta " + invoice.account.id,
                       ENivelLog.Info,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );

                Response.ContentType = "application/xml";
                Response.AddHeader("Content-Disposition", "attachment;filename=Customers.xml");
                Response.Write(invoice.xml);
                Response.End();
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                       "Error al descargar el xml: " + ex.Message.ToString(),
                       ENivelLog.Error,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                    );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
        }

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
    }
}