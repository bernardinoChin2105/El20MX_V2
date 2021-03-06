﻿using ExcelDataReader;
using LogHubSDK.Models;
using MVC_Project.BackendWeb.Attributes;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Storage;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace MVC_Project.WebBackend.Controllers
{
    public class ProviderController : Controller
    {
        private IAccountService _accountService;
        private IProviderService _providerService;
        private IStateService _stateService;
        private ICurrencyService _currencyService;
        private IPaymentFormService _paymentFormService;
        private IPaymentMethodService _paymentMethodService;
        private IInvoiceReceivedService _invoiceReceivedService;
        private ICountryService _countryService;

        public ProviderController(IAccountService accountService, IProviderService providerService, IStateService stateService,
            ICurrencyService currencyService, IPaymentFormService paymentFormService, IPaymentMethodService paymentMethodService,
            IInvoiceReceivedService invoiceReceivedService, ICountryService countryService)
        {
            _accountService = accountService;
            _providerService = providerService;
            _stateService = stateService;
            _currencyService = currencyService;
            _paymentFormService = paymentFormService;
            _paymentMethodService = paymentMethodService;
            _invoiceReceivedService = invoiceReceivedService;
            _countryService = countryService;
        }

        // GET: Provider
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var createProvider = new ProviderViewModel();

                SetCombos(string.Empty, ref createProvider);

                return View(createProvider);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                LogUtil.AddEntry(
                  "Se encontro un error: " + ex.Message.ToString(),
                  ENivelLog.Error,
                  userAuth.Id,
                  userAuth.Email,
                  EOperacionLog.ACCESS,
                  string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                  ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                  string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
               );
                return RedirectToAction("Index");
            }
            // return View();
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(ProviderViewModel model)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El registro de proveedor no es válido");


                if (_providerService.FindBy(x => x.rfc == model.RFC && x.account.id == authUser.Account.Id).Any())
                    throw new Exception("Ya existe un Proveedor con el RFC proporcionado");

                //Account account = _accountService.FindBy(x => x.id == authUser.Account.Id).FirstOrDefault();
                DateTime todayDate = DateUtil.GetDateTimeNow();

                Provider provider = new Provider()
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

                if (model.taxRegime == TypeTaxRegimen.PERSONA_FISICA.ToString())
                    provider.businessName = model.BusinessName;

                if (model.Colony.Value > 0)
                    provider.colony = model.Colony.Value;
                if (model.Municipality.Value > 0)
                    provider.municipality = model.Municipality.Value;
                if (model.State.Value > 0)
                    provider.state = model.State.Value;
                if (model.Country.Value > 0)
                    provider.country = model.Country.Value;

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
                            ProviderContact email = new ProviderContact()
                            {
                                emailOrPhone = model.Emails[i].EmailOrPhone,
                                typeContact = model.Emails[i].TypeContact,
                                provider = provider,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            provider.providerContacts.Add(email);
                        }
                    }
                }

                if (model.Phones.Count() > 0)
                {
                    for (int i = 0; i < model.Phones.Count(); i++)
                    {
                        if (model.Phones[i].EmailOrPhone != null && model.Phones[i].EmailOrPhone.Trim() != "" && !indexsP.Contains(i.ToString()))
                        {
                            ProviderContact phone = new ProviderContact()
                            {
                                emailOrPhone = model.Phones[i].EmailOrPhone,
                                typeContact = model.Phones[i].TypeContact,
                                provider = provider,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };

                            provider.providerContacts.Add(phone);
                        }
                    }
                }

                _providerService.Create(provider);
                LogUtil.AddEntry(
                    "Nuevo proveedor creado: " + provider.businessName,
                    ENivelLog.Info,
                    authUser.Id,
                    authUser.Email,
                    EOperacionLog.ACCESS,
                    string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                    ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                    JsonConvert.SerializeObject(provider)
                );
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);

                SetCombos(model.ZipCode, ref model);

                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   authUser.Id,
                   authUser.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );

                return View(model);
            }
        }

        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                ProviderViewModel model = new ProviderViewModel();


                var provider = _providerService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid) && x.account.id == userAuth.Account.Id);
                if (provider == null)
                    throw new Exception("El registro de Proveedor no se encontró en la base de datos");

                model.Id = provider.id;
                model.FistName = provider.firstName;
                model.LastName = provider.lastName;
                model.RFC = provider.rfc;
                model.CURP = provider.curp;
                model.BusinessName = provider.businessName;
                model.Street = provider.street;
                model.OutdoorNumber = provider.outdoorNumber;
                model.InteriorNumber = provider.interiorNumber;
                model.ZipCode = provider.zipCode;
                model.Colony = provider.colony;
                model.Municipality = provider.municipality;
                model.State = provider.state;
                model.Country = provider.country;
                model.DeliveryAddress = provider.deliveryAddress;
                model.taxRegime = provider.taxRegime;
                
                var emails = provider.providerContacts.Where(x => x.typeContact == TypeContact.EMAIL.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new ProviderContactsViewModel
                            {
                                Id = x.id,
                                TypeContact = x.typeContact,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList();

                if (emails.Count() > 0)
                {
                    model.Emails = emails;
                }

                var phones = provider.providerContacts.Where(x => x.typeContact == TypeContact.PHONE.ToString() && x.status == SystemStatus.ACTIVE.ToString())
                            .Select(x => new ProviderContactsViewModel
                            {
                                Id = x.id,
                                TypeContact = x.typeContact,
                                EmailOrPhone = x.emailOrPhone
                            }).ToList();
                if (phones.Count() > 0)
                {
                    model.Phones = phones;
                }

                SetCombos(provider.zipCode, ref model);

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(ProviderViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            ProviderViewModel provider = new ProviderViewModel();
            var providerData = _providerService.FirstOrDefault(x => x.id == model.Id && x.account.id == userAuth.Account.Id);
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El registro de proveedor no es válido");

                var authUser = Authenticator.AuthenticatedUser;
                DateTime todayDate = DateUtil.GetDateTimeNow();

                providerData.firstName = model.FistName;
                providerData.lastName = model.LastName;
                providerData.rfc = model.RFC;
                providerData.curp = model.CURP;
                providerData.businessName = model.BusinessName;
                providerData.taxRegime = model.taxRegime;
                providerData.street = model.Street;
                providerData.interiorNumber = model.InteriorNumber;
                providerData.outdoorNumber = model.OutdoorNumber;
                providerData.zipCode = model.ZipCode;
                providerData.deliveryAddress = model.DeliveryAddress;
                providerData.modifiedAt = todayDate;
                providerData.status = SystemStatus.ACTIVE.ToString();

                if (model.taxRegime == TypeTaxRegimen.PERSONA_FISICA.ToString())
                    providerData.businessName = model.BusinessName;

                if (model.Colony.Value > 0)
                    providerData.colony = model.Colony.Value;
                if (model.Municipality.Value > 0)
                    providerData.municipality = model.Municipality.Value;
                if (model.State.Value > 0)
                    providerData.state = model.State.Value;
                if (model.Country.Value > 0)
                    providerData.country = model.Country.Value;

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
                                var contact = providerData.providerContacts.Where(x => x.id == model.Emails[i].Id).FirstOrDefault();
                                contact.emailOrPhone = model.Emails[i].EmailOrPhone;
                                contact.modifiedAt = todayDate;
                            }
                            else
                            {
                                ProviderContact email = new ProviderContact()
                                {
                                    emailOrPhone = model.Emails[i].EmailOrPhone,
                                    typeContact = model.Emails[i].TypeContact,
                                    provider = providerData,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };

                                providerData.providerContacts.Add(email);
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
                                var contact = providerData.providerContacts.Where(x => x.id == model.Phones[i].Id).FirstOrDefault();
                                contact.emailOrPhone = model.Phones[i].EmailOrPhone;
                                contact.modifiedAt = todayDate;
                            }
                            else
                            {
                                ProviderContact phone = new ProviderContact()
                                {
                                    emailOrPhone = model.Phones[i].EmailOrPhone,
                                    typeContact = model.Phones[i].TypeContact,
                                    provider = providerData,
                                    createdAt = todayDate,
                                    modifiedAt = todayDate,
                                    status = SystemStatus.ACTIVE.ToString()
                                };

                                providerData.providerContacts.Add(phone);
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
                        var itemToRemove = providerData.providerContacts.SingleOrDefault(r => r.id == Convert.ToInt64(item));
                        if (itemToRemove != null)
                        {
                            itemToRemove.status = SystemStatus.INACTIVE.ToString();
                        }
                    }
                }
                #endregion

                _providerService.Update(providerData);

                LogUtil.AddEntry(
                   "Preveedor editado: " + providerData.businessName,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(providerData)
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                
                SetCombos(model.ZipCode, ref model);

                LogUtil.AddEntry(
                  "Se encontro un error: " + ex.Message.ToString(),
                  ENivelLog.Error,
                  userAuth.Id,
                  userAuth.Email,
                  EOperacionLog.ACCESS,
                  string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                  ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                  string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
               );
                return View(model);
            }
        }

        private void SetCombos(string zipCode, ref ProviderViewModel model)
        {
            model.ListRegimen = Enum.GetValues(typeof(TypeTaxRegimen)).Cast<TypeTaxRegimen>()
                       .Select(e => new SelectListItem
                       {
                           Value = e.ToString(),
                           Text = EnumUtils.GetDisplayName(e)
                       }).ToList();

            
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
        public JsonResult GetProviders(JQueryDataTableParams param, string filtros, bool first)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                int totalDisplay = 0;
                int total = 0;
                var listResponse = new List<ListProviders>();
                if (!first)
                {
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    string rfc = filtersValues.Get("RFC").Trim();
                    string businessName = filtersValues.Get("BusinessName").Trim();
                    string email = filtersValues.Get("Email").Trim();

                    var pagination = new BasePagination();
                    var filters = new FilterProvider() { uuid = userAuth.Account.Uuid.ToString() };
                    pagination.PageSize = param.iDisplayLength;
                    pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                    if (rfc != "") filters.rfc = rfc;
                    if (businessName != "") filters.businessName = businessName;
                    if (email != "") filters.email = email;

                    listResponse = _providerService.ListProvider(pagination, filters);

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
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                return new JsonResult
                {
                    Data =
                    new
                    {
                        success = false,
                        message = ex.Message,
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = new List<CustomerList>()
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
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

        [HttpGet, AllowAnonymous, FileDownload]
        public FileResult ExportTemplate()
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;

                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet campo = pck.Workbook.Worksheets.Add("LISTA DE PROVEEDORES");

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
                    return File(bin, "application/vnd.ms-excel", "ListaProveedoresPlantilla.xlsx");
                }
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                throw;
            }
        }

        [HttpPost, AllowAnonymous, FileDownload]
        public FileResult ExportListProvider(string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string rfc = filtersValues.Get("RFC").Trim();
                string businessName = filtersValues.Get("BusinessName").Trim();
                string email = filtersValues.Get("Email").Trim();

                var filters = new FilterProvider() { uuid = userAuth.Account.Uuid.ToString() };
                if (rfc != "") filters.rfc = rfc;
                if (businessName != "") filters.businessName = businessName;
                if (email != "") filters.email = email;

                var listResponse = _providerService.ExportListProvider(filters);

                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet campo = pck.Workbook.Worksheets.Add("LISTA DE PROVEEDORES");

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
                    return File(bin, "application/vnd.ms-excel", "ListaProveedores_(" + DateUtil.GetDateTimeNow().ToString("G") + ").xlsx");
                }
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                throw;
            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult ImportExcelProvider(HttpPostedFileBase Excel)
        {
            List<object> Errores = new List<object>();
            List<ExportListProviders> datosErroneos = new List<ExportListProviders>();
            List<ExportListProviders> datos = new List<ExportListProviders>();
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
                        throw new Exception("Favor de seleccionar un formato de Excel permitido (\".xlsx\", \".xls\").");
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

                                        ExportListProviders providers = new ExportListProviders
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
                                            //uuidAccount = authUser.Account.Uuid
                                        };

                                        datos.Add(providers);
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
                        IEnumerable<ExportListProviders> SinRegistros = from d in datos where RFCDuplicates.Contains(d.rfc) select d;
                        SinRegistros = SinRegistros.Select(c => { c.Observaciones = "RFC con registro duplicados en el archivo."; return c; }).ToList();
                        datosErroneos = datosErroneos.Union(SinRegistros).ToList();
                    }

                    //Validar rfc
                    var Existen = _providerService.ValidateRFC(rfcs, authUser.Account.Id);

                    List<string> NoExisten = rfcs.Except(Existen).ToList();

                    if (Existen.Count > 0)
                    {
                        IEnumerable<ExportListProviders> SiExisteRegistros = from d in datos where Existen.Contains(d.rfc) select d;
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
                    List<Provider> proveedores = new List<Provider>();

                    proveedores = datos.Select(x => new Provider
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
                        //providerContacts = new List<ProviderContact>
                        //{
                        //    x.email != ""? new ProviderContact
                        //    {            
                        //        //provider = pro,
                        //        emailOrPhone = x.email,
                        //        typeContact = TypeContact.EMAIL.ToString(),
                        //        createdAt = todayDate,
                        //        modifiedAt = todayDate,
                        //        status = SystemStatus.ACTIVE.ToString()
                        //    }: null,
                        //    x.phone != ""? new ProviderContact
                        //    {
                        //        emailOrPhone = x.phone,
                        //        typeContact = TypeContact.PHONE.ToString(),
                        //        createdAt = todayDate,
                        //        modifiedAt = todayDate,
                        //        status = SystemStatus.ACTIVE.ToString()
                        //    }: null
                        //}
                    }).ToList();

                    //proveedores = proveedores.Select(x =>
                    //{
                    //    x.providerContacts = x.providerContacts.Where(b => b != null)
                    //    .Select(b => { b.provider = x; return b; }).ToList();
                    //    return x;
                    //}).ToList();

                    if (proveedores.Count() > 0)
                    {
                        _providerService.Create(proveedores);
                        //LogHub de bitacora
                        LogUtil.AddEntry(
                          "Carga de archivo excel de proveedores ¡" + proveedores.Count() + " Registros guardados exitosamente!",
                          ENivelLog.Info,
                          authUser.Id,
                          authUser.Email,
                          EOperacionLog.ACCESS,
                          string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                          ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                          string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                       );

                        return Json(new
                        {
                            Success = true,
                            Mensaje = "¡" + proveedores.Count() + " Registros guardados exitosamente!",
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

        /*Funciones para las facturas*/
        [AllowAnonymous]
        public ActionResult InvoicesReceived()
        {
            ViewBag.Date = new
            {
                MinDate = DateUtil.GetDateTimeNow().AddDays(-10).ToString("dd-MM-yyyy"),
                MaxDate = DateUtil.GetDateTimeNow().ToString("dd-MM-yyyy")
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
            var listResponse = new List<InvoicesReceivedList>();
            var list = new List<InvoicesReceivedListVM>();

            try
            {
                if (!first)
                {
                    NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                    string FilterStart = filtersValues.Get("FilterInitialDate").Trim();
                    string FilterEnd = filtersValues.Get("FilterEndDate").Trim();
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
                var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                var invoice = _invoiceReceivedService.FirstOrDefault(x => x.id == id);

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");
                
                MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoicesReceived, authUser.Account.RFC + "/" + invoice.uuid + ".xml");
                stream.Position = 0;

                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
                XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
                nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                
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
                //string varDescuento1 = nodeComprobante.Attributes["Descuento"].Value;
                string varDescuento1 = nodeComprobante.Attributes["Descuento"] != null ? nodeComprobante.Attributes["Descuento"].Value : string.Empty;

                MonedaUtils formatoTexto = new MonedaUtils();
                var fecha = varFecha != null || varFecha != "" ? Convert.ToDateTime(varFecha).ToString("yyyy-MM-dd HH:mm:ss") : varFecha;

                InvoicesVM cfdipdf = new InvoicesVM()
                {
                    Folio = varFolio,
                    Serie = varSerie,
                    SubTotal = varSubTotal,
                    Total = varTotal,
                    TipoDeComprobante = ((TipoComprobante)Enum.Parse(typeof(TipoComprobante), varTipoComprobante, true)).GetDescription()  ,                    
                    Certificado = varCertificado,
                    NoCertificado = varNoCertificado,
                    Sello = varSello,
                    FormaPago = varFormaPago,
                    MetodoPago = ((MetodoPago)Enum.Parse(typeof(MetodoPago), varMetodoPago, true)).GetDescription(),
                    LugarExpedicion = varLugarExpedicion,
                    Fecha = fecha,
                    //Moneda = varMoneda,
                    Moneda = formatoTexto.Convertir(varTotal.ToString(),true),
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
                    

                    Emisor emisor = new Emisor()
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

                    Receptor receptor = new Receptor()
                    {
                        Nombre = varNombre,
                        Rfc = varRfc,
                        UsoCFDI = varUsoCFDI,
                        UsoCFDITexto = varUsoCFDIText != null? varUsoCFDIText: varUsoCFDI
                    };

                    cfdipdf.Receptor = receptor;
                }

                GeneradorQR generador = new GeneradorQR();
                string selloQR = cfdipdf.Sello.Substring((cfdipdf.Sello.Length - 8), 8);
                string urlQR = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id=" + cfdipdf.Folio + "&re=" + cfdipdf.Emisor.Rfc + "&rr=" + cfdipdf.Receptor.Rfc + "&tt=" + cfdipdf.Total + "&fe="+ selloQR;
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
                        //string varDescuento = nodeConcepto.Attributes["Descuento"].Value;
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
                        //cfdipdf.Conceptos.Concepto = concepto;
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
                
                string rfc = authUser.Account.RFC;
                return new Rotativa.ViewAsPdf("InvoiceDownloadPDF", cfdipdf) { FileName = invoice.uuid + ".pdf" };
            }
            catch (Exception ex)
            {     
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);                
                return RedirectToAction("InvoicesReceived");
            }

        }

        [HttpGet, AllowAnonymous]
        public void GetDownloadXML(Int64 id)
        {
            var authUser = Authenticator.AuthenticatedUser;

            try
            {
                var StorageInvoicesReceived = ConfigurationManager.AppSettings["StorageInvoicesReceived"];
                var invoice = _invoiceReceivedService.FirstOrDefault(x => x.id == id);

                if (invoice == null)
                    throw new Exception("No se encontro la factura emitida");
                
                MemoryStream stream = AzureBlobService.DownloadFile(StorageInvoicesReceived, authUser.Account.RFC + "/" + invoice.uuid + ".xml");

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

        [HttpGet, AllowAnonymous]
        public JsonResult GetAutoComplite()
        {
            List<string> list = new List<string>();
            try
            {
                var authUser = Authenticator.AuthenticatedUser;
                var listRFC = _providerService.ListProviderAutoComplete(authUser.Account.Id);
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