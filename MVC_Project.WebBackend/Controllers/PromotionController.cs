﻿using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class PromotionController : Controller
    {
        private IPromotionService _promotionService;
        private IAccountService _accountService;
        private IPromotionAccountService _promotionAccountService;
        private IDiscountService _discountService;

        public PromotionController(IPromotionService promotionService, IAccountService accountService, IDiscountService discountService,
            IPromotionAccountService promotionAccountService)
        {
            _promotionService = promotionService;
            _promotionAccountService = promotionAccountService;
            _accountService = accountService;
            _discountService = discountService;
        }

        public ActionResult Index()
        {
            PromotionFilterViewModel model = new PromotionFilterViewModel();

            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos", Value = "-1" });

            var types = Enum.GetValues(typeof(TypePromotions)).Cast<TypePromotions>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = EnumUtils.GetDisplayName(e)
                }).ToList();
            list.AddRange(types);

            model.typeList = new SelectList(list);

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPromotions(JQueryDataTableParams param, string filtros)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            var listResponse = new List<PromotionsList>();
            string error = string.Empty;
            bool success = true;

            try
            {
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string Name = filtersValues.Get("Name");
                string Type = filtersValues.Get("Type");

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);

                listResponse = _promotionService.GetPromotionList(pagination, Name, Type);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();

                    listResponse.Select(c =>
                    {
                        c.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), c.status)).GetDisplayName();
                        c.type = ((TypePromotions)Enum.Parse(typeof(TypePromotions), c.type)).GetDisplayName();
                        c.discount = Math.Round(c.discount, 2);
                        c.discountRate = Math.Round(c.discountRate, 2);
                        return c;
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                success = false;
            }

            return Json(new
            {
                success = success,
                error = error,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Date = DateTime.Now.ToString("dd-MM-yyyy");

            PromotionViewModel model = new PromotionViewModel();
            try
            {
                var list = Enum.GetValues(typeof(TypePromotions)).Cast<TypePromotions>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();

                var accounts = _accountService.GetAll();
                var accountViewModel = new AccountSelectViewModel { accountListItems = new List<SelectListItem>() };
                accountViewModel.accountListItems = accounts.Select(x => new SelectListItem
                {
                    Text = x.name + " ( " + x.rfc + " )",
                    Value = x.uuid.ToString()
                }).ToList();

                model.TypeList = new SelectList(list);
                model.AccountMultipleList = new MultiSelectList(accountViewModel.accountListItems, "Value", "Text");

            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }
            return View(model);
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(PromotionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var authUser = Authenticator.AuthenticatedUser;

                if (_promotionService.FindBy(x => x.name == model.name).Any())
                    throw new Exception("Ya existe una con el Nombre proporcionado");

                DateTime todayDate = DateUtil.GetDateTimeNow();

                Promotion promotion = new Promotion()
                {
                    uuid = Guid.NewGuid(),
                    name = model.name,
                    type = model.TypeId,
                    discount = model.discount,
                    discountRate = model.discountRate,
                    hasPeriod = model.hasPeriod,
                    hasValidity = model.hasValidity,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString()
                };

                if (model.hasPeriod)
                {
                    promotion.periodInitial = model.initialPeriod;
                    promotion.periodFinal = model.finalPeriod;
                }

                if (model.hasValidity)
                {
                    promotion.validityInitialAt = model.finalDate;
                    promotion.validityFinalAt = model.finalDate;
                }

                List<PromotionAccount> promotionsXaccounts = new List<PromotionAccount>();
                List<Discount> discounts = new List<Discount>();
                foreach (var item in model.AccountId)
                {

                    PromotionAccount promotionAccount = new PromotionAccount()
                    {
                        account = new Account { id = item },
                        promotion = promotion
                    };

                    promotionsXaccounts.Add(promotionAccount);

                    Discount discount = new Discount()
                    {
                        uuid = Guid.NewGuid(),
                        name = model.name,
                        type = model.TypeId,
                        discount = model.discount,
                        discountRate = model.discountRate,
                        hasPeriod = model.hasPeriod,
                        hasValidity = model.hasValidity,
                        account = new Account { id = item },
                        promotion = promotion,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString()
                    };

                    if (model.hasPeriod)
                    {
                        discount.periodInitial = model.initialPeriod;
                        discount.periodFinal = model.finalPeriod;
                    }

                    if (model.hasValidity)
                    {
                        discount.validityInitialAt = model.finalDate;
                        discount.validityFinalAt = model.finalDate;
                    }

                    discounts.Add(discount);
                }

                _promotionService.Save(promotion, promotionsXaccounts, discounts);
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje("Error al guardar: " + ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }

        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var promotion = _promotionService.FirstOrDefault(x => x.uuid == Guid.Parse(uuid));
                if (promotion == null)
                    throw new Exception("El registro de promoción no se encontró en la base de datos");

                PromotionViewModel model = new PromotionViewModel()
                {
                    id = promotion.id,
                    uuid = promotion.uuid,
                    name = promotion.name,
                    discount = promotion.discount,
                    discountRate = promotion.discountRate,
                    TypeId = promotion.type,
                    hasPeriod = promotion.hasPeriod,
                    initialPeriod = promotion.periodInitial,
                    finalPeriod = promotion.periodFinal,
                    hasValidity = promotion.hasValidity,
                    finalDate = promotion.validityFinalAt
                };

                var discounts = _discountService.GetDiscounts(promotion.uuid.ToString());
                var promotionAccount = _promotionAccountService.GetPromotionAccount(promotion.uuid.ToString());

                if (promotionAccount != null)
                {
                    model.AccountId = promotionAccount.Select(x => x.account.id).ToArray();
                }

                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Text = "Seleccionar", Value = "-1" });

                var types = Enum.GetValues(typeof(TypePromotions)).Cast<TypePromotions>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    }).ToList();
                list.AddRange(types);

                var accounts = _accountService.GetAll();
                var accountViewModel = new AccountSelectViewModel { accountListItems = new List<SelectListItem>() };
                accountViewModel.accountListItems = accounts.Select(x => new SelectListItem
                {
                    Text = x.name + " ( " + x.rfc + " )",
                    Value = x.uuid.ToString()
                }).ToList();

                model.TypeList = new SelectList(list);
                model.AccountMultipleList = new MultiSelectList(list, "Value", "Text", model.AccountId);

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(PromotionViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                DateTime todayDate = DateUtil.GetDateTimeNow();
                Promotion promotion = _promotionService.FirstOrDefault(x => x.id == model.id);

                promotion.name = model.name;
                promotion.modifiedAt = todayDate;
                promotion.discount = model.discount;
                promotion.discountRate = model.discountRate;
                promotion.type = model.TypeId;           

                PromotionViewModel model2 = new PromotionViewModel()
                {
                  
                    hasPeriod = promotion.hasPeriod,
                    initialPeriod = promotion.periodInitial,
                    finalPeriod = promotion.periodFinal,
                    hasValidity = promotion.hasValidity,
                    finalDate = promotion.validityFinalAt
                };

                var discountsData = _discountService.GetDiscounts(promotion.uuid.ToString());
                var promotionAccountData = _promotionAccountService.GetPromotionAccount(promotion.uuid.ToString());

                if (promotionAccountData != null)
                {
                    model.AccountId = promotionAccountData.Select(x => x.account.id).ToArray();
                }

                if (model.hasPeriod)
                {
                    promotion.periodInitial = model.initialPeriod;
                    promotion.periodFinal = model.finalPeriod;
                }

                if (model.hasValidity)
                {
                    promotion.validityInitialAt = model.finalDate;
                    promotion.validityFinalAt = model.finalDate;
                }

                List<PromotionAccount> promotionsXaccounts = new List<PromotionAccount>();
                List<Discount> discounts = new List<Discount>();
                foreach (var item in model.AccountId)
                {

                    PromotionAccount promotionAccount = new PromotionAccount()
                    {
                        account = new Account { id = item },
                        promotion = promotion
                    };

                    promotionsXaccounts.Add(promotionAccount);

                    Discount discount = new Discount()
                    {
                        uuid = Guid.NewGuid(),
                        name = model.name,
                        type = model.TypeId,
                        discount = model.discount,
                        discountRate = model.discountRate,
                        hasPeriod = model.hasPeriod,
                        hasValidity = model.hasValidity,
                        account = new Account { id = item },
                        promotion = promotion,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString()
                    };

                    if (model.hasPeriod)
                    {
                        discount.periodInitial = model.initialPeriod;
                        discount.periodFinal = model.finalPeriod;
                    }

                    if (model.hasValidity)
                    {
                        discount.validityInitialAt = model.finalDate;
                        discount.validityFinalAt = model.finalDate;
                    }

                    discounts.Add(discount);
                }

                _promotionService.Update(promotion, promotionsXaccounts, discounts);
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }
    }
}