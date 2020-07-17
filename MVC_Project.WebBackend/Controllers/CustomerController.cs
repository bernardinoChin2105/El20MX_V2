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

                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Emails[0].EmailOrPhone.Trim() != "")
                        {
                            CustomerContact email = new CustomerContact()
                            {
                                emailOrPhone = model.Emails[0].EmailOrPhone,
                                typeContact = model.Emails[0].TypeContact,
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
                        if (model.Phones[0].EmailOrPhone.Trim() != "")
                        {
                            CustomerContact phone = new CustomerContact()
                            {
                                emailOrPhone = model.Phones[0].EmailOrPhone,
                                typeContact = model.Phones[0].TypeContact,
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
                model.taxRegime = customer.taxRegime;
                model.Street = customer.street;
                model.OutdoorNumber = customer.outdoorNumber;
                model.InteriorNumber = customer.interiorNumber;
                model.ZipCode = customer.zipCode;
                model.Colony = customer.colony;
                model.Municipality = customer.municipality;
                model.State = customer.state;
                model.DeliveryAddress = customer.deliveryAddress;

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
                if (model.Emails.Count() > 0)
                {
                    for (int i = 0; i < model.Emails.Count(); i++)
                    {
                        if (model.Emails[i].EmailOrPhone.Trim() != "")
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
                        if (model.Phones[0].EmailOrPhone.Trim() != "")
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
                    ExcelWorksheet campo = pck.Workbook.Worksheets.Add("CUADRO PAGOS DIARIOS");

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

                        campo.Cells["G" + rowIndexString].Value = listResponse[i].taxRegime;
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


                //return Json(new
                //{
                //    draw = Filtros.draw,
                //    recordsFiltered = GridData.TOTAL_ELEMENTOS,
                //    recordsTotal = GridData.TOTAL_ELEMENTOS,
                //    data = GridData.Datos,
                //}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //return Json(new
                //{
                //    error = ex.GetBaseException().Message
                //});
                throw;
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