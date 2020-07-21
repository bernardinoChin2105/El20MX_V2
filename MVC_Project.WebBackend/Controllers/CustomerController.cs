using ExcelDataReader;
using MVC_Project.BackendWeb.Attributes;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
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

namespace MVC_Project.WebBackend.Controllers
{
    public class CustomerController : Controller
    {
        private IAccountService _accountService;
        private ICustomerService _customerService;
        private IStateService _stateService;

        public CustomerController(IAccountService accountService, ICustomerService customerService, IStateService stateService)
        {
            _accountService = accountService;
            _customerService = customerService;
            _stateService = stateService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCustomers(JQueryDataTableParams param, string filtros)
        {
            try
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

                var listResponse = _customerService.CustomerList(pagination, filters);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                int totalDisplay = 0;
                int total = 0;
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
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
                //new { success = false, Mensaje = new { title = "Error", message = ex.Message }
                return new JsonResult
                {
                    Data =
                    new
                    {
                        success = true,
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<CustomerList>()
                    },
                    //},
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
            //MensajeFlashHandler.RegistrarMensaje("¡Registro de Cliente realizado!", TiposMensaje.Success);
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


                string[] indexsP = model.indexPhone.Split(',');
                string[] indexsE = model.indexEmail.Split(',');

                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Emails[i].EmailOrPhone.Trim() != "" && !indexsE.Contains(i.ToString()))
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
                        if (model.Phones[i].EmailOrPhone.Trim() != "" && !indexsP.Contains(i.ToString()))
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
                model.DeliveryAddress = customer.deliveryAddress;
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

                var emails = customer.customerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString())
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

                var phones = customer.customerContacts.Where(x => x.typeContact == TypeContact.PHONE.ToString())
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

                #region Actualizar registros de las listas de emails y teléfonos 

                string[] indexsP = model.indexPhone.Split(',');
                string[] indexsE = model.indexEmail.Split(',');

                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Emails[i].EmailOrPhone.Trim() != "" && !indexsE.Contains(i.ToString()))
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
                        if (model.Phones[0].EmailOrPhone.Trim() != "" && !indexsP.Contains(i.ToString()))
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
                var list = model.dataContacts.Split(',');
                //string[] dataIds = model.dataContacts.Split(',');
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var itemToRemove = customerData.customerContacts.SingleOrDefault(r => r.id == Convert.ToInt64(item));
                        if (itemToRemove != null)
                            customerData.customerContacts.Remove(itemToRemove);
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

                    campo.Cells["A1"].Value = "Id";
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
                    campo.Cells["O1"].Value = "Domicilio Comercial";
                    campo.Cells["P1"].Value = "Email";
                    campo.Cells["Q1"].Value = "Teléfono";
                    campo.Cells["R1"].Value = "Fecha Creación";
                    campo.Cells["S1"].Value = "uuid Cuenta";

                    int rowIndex = 2;
                    for (int i = 0; i < listResponse.Count(); i++)
                    {
                        string enumFiscal = string.Empty;
                        string rowIndexString = rowIndex.ToString();
                        campo.Cells["A" + rowIndexString].Value = listResponse[i].id;
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
                            //var pal2 = EnumUtils.GetValueFromDescription<TypeTaxRegimen>(enumFiscal);//funciona cuando obtenemos la descripción
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

                        campo.Cells["O" + rowIndexString].Value = listResponse[i].deliveryAddress;
                        campo.Cells["O" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["P" + rowIndexString].Value = listResponse[i].email;
                        campo.Cells["P" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["Q" + rowIndexString].Value = listResponse[i].phone;
                        campo.Cells["Q" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["R" + rowIndexString].Value = listResponse[i].createdAt.ToShortDateString();
                        campo.Cells["R" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        campo.Cells["S" + rowIndexString].Value = listResponse[i].uuidAccount;
                        campo.Cells["S" + rowIndexString].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
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
            try
            {
                var authUser = Authenticator.AuthenticatedUser;
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
                                    if (!tabla.Rows[i].IsNull(1)
                                        || !tabla.Rows[i].IsNull(2)
                                        || !tabla.Rows[i].IsNull(3)
                                        || !tabla.Rows[i].IsNull(5)
                                        || !tabla.Rows[i].IsNull(10))
                                    {
                                        //Validar
                                        string taxRegime = string.Empty;
                                        if (!tabla.Rows[i].IsNull(6))
                                        {
                                            var taxRegimeEnum = EnumUtils.GetValueFromDescription<TypeTaxRegimen>(tabla.Rows[i].ItemArray[6].ToString());//funciona cuando obtenemos la descripción
                                            taxRegime = taxRegimeEnum.ToString();
                                        }

                                        //deliveryAddress = tabla.Rows[i].ItemArray[14].ToString(),
                                        ExportListCustomer customers = new ExportListCustomer
                                        {
                                            first_name = tabla.Rows[i].ItemArray[1].ToString(),
                                            last_name = tabla.Rows[i].ItemArray[2].ToString(),
                                            rfc = tabla.Rows[i].ItemArray[3].ToString(),
                                            curp = tabla.Rows[i].ItemArray[4].ToString(),
                                            businessName = tabla.Rows[i].ItemArray[5].ToString(),
                                            taxRegime = taxRegime,
                                            street = tabla.Rows[i].ItemArray[7].ToString(),
                                            interiorNumber = tabla.Rows[i].ItemArray[8].ToString(),
                                            outdoorNumber = tabla.Rows[i].ItemArray[9].ToString(),
                                            zipCode = tabla.Rows[i].ItemArray[10].ToString(),
                                            nameSettlement = tabla.Rows[i].ItemArray[11].ToString(),
                                            nameMunicipality = tabla.Rows[i].ItemArray[12].ToString(),
                                            nameState = tabla.Rows[i].ItemArray[13].ToString(),
                                            //deliveryAddress = tabla.Rows[i].ItemArray[14].ToString(),
                                            email = tabla.Rows[i].ItemArray[16].ToString(),
                                            phone = tabla.Rows[i].ItemArray[17].ToString(),
                                            createdAt = todayDate,
                                            modifiedAt = todayDate,
                                            status = SystemStatus.ACTIVE.ToString(),
                                            uuid = Guid.NewGuid(),
                                            accountId = authUser.Account.Id,
                                            uuidAccount = authUser.Account.Uuid
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
                                    if (encabezado[0].ToString() != "Id") throw new Exception("Título de columna inválida");
                                    if (encabezado[1].ToString() != "Nombre(s)") throw new Exception("Título de columna inválida");
                                    if (encabezado[2].ToString() != "Apellido(s)") throw new Exception("Título de columna inválida");
                                    if (encabezado[3].ToString() != "RFC") throw new Exception("Título de columna inválida");
                                    if (encabezado[4].ToString() != "CURP") throw new Exception("Título de columna inválida");
                                    if (encabezado[5].ToString() != "Nombre/Razón Social") throw new Exception("Título de columna inválida");
                                    if (encabezado[6].ToString() != "Tipo Régimen Fiscal") throw new Exception("Título de columna inválida");
                                    if (encabezado[7].ToString() != "Calle y Cruzamientos") throw new Exception("Título de columna inválida");
                                    if (encabezado[8].ToString() != "Número Exterior") throw new Exception("Título de columna inválida");
                                    if (encabezado[9].ToString() != "Número Interior") throw new Exception("Título de columna inválida");
                                    if (encabezado[10].ToString() != "C.P.") throw new Exception("Título de columna inválida");
                                    if (encabezado[11].ToString() != "Colonia") throw new Exception("Título de columna inválida");
                                    if (encabezado[12].ToString() != "Alcaldía/Municipio") throw new Exception("Título de columna inválida");
                                    if (encabezado[13].ToString() != "Estado") throw new Exception("Título de columna inválida");
                                    if (encabezado[14].ToString() != "Domicilio Comercial") throw new Exception("Título de columna inválida");
                                    if (encabezado[15].ToString() != "Email") throw new Exception("Título de columna inválida");
                                    if (encabezado[16].ToString() != "Teléfono") throw new Exception("Título de columna inválida");
                                    if (encabezado[17].ToString() != "Fecha Creación") throw new Exception("Título de columna inválida");
                                    if (encabezado[18].ToString() != "uuid Cuenta") throw new Exception("Título de columna inválida");
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
                    List<string> clientesNoRegistrados = new List<string>();
                    foreach (var item in datos)
                    {
                        try
                        {
                            Customer nuevo = new Customer()
                            {
                                uuid = Guid.NewGuid(),
                                account = new Account { id = authUser.Account.Id },
                                firstName = item.first_name,
                                lastName = item.last_name,
                                rfc = item.rfc,
                                curp = item.curp,
                                businessName = item.businessName,
                                street = item.street,
                                interiorNumber = item.interiorNumber,
                                outdoorNumber = item.outdoorNumber,
                                zipCode = item.zipCode,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                                //string taxRegime { get; set;}
                                //Int64 colony { get; set; }
                                //Int64 municipality { get; set; }
                                //Int64 state { get; set; }
                                //bool deliveryAddress { get; set; }
                            };

                            CustomerContact email = new CustomerContact()
                            {
                                emailOrPhone = item.email,
                                typeContact = TypeContact.EMAIL.ToString(),
                                customer = nuevo,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            nuevo.customerContacts.Add(email);

                            CustomerContact phone = new CustomerContact()
                            {
                                emailOrPhone = item.phone,
                                typeContact = TypeContact.PHONE.ToString(),
                                customer = nuevo,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };
                            nuevo.customerContacts.Add(phone);

                            _customerService.Create(nuevo);
                            clientes.Add(nuevo);
                        }
                        catch (Exception ex)
                        {
                            clientesNoRegistrados.Add(item.rfc);
                        }
                    }

                    if (clientes.Count() > 0)
                    {
                        //LogHub de bitacora
                        return Json(new
                        {
                            Success = true,
                            Mensaje = "¡" + clientes.Count() + " Registros guardados exitosamente!",
                            //Tipo = 2,
                            SinGuardar = clientesNoRegistrados,
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente!", Tipo = 0 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente! Archivo no válido", Tipo = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Error)
            {
                return Json(new { Success = false, Mensaje = "¡Intentelo nuevamente!", Tipo = 0, Error = Error.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }


        //// GET: Role/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Role/Delete/5
        //[HttpPost]
        //public ActionResult Delete(string uuid, FormCollection collection)
        //{
        //    try
        //    {
        //        var rol = _roleService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
        //        if (rol != null)
        //        {
        //            if (rol.status == SystemStatus.ACTIVE.ToString())
        //            {
        //                rol.status = SystemStatus.INACTIVE.ToString();
        //            }
        //            else
        //            {
        //                rol.status = SystemStatus.ACTIVE.ToString();
        //            }

        //        }
        //        _roleService.Update(rol);
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //}
    }
}