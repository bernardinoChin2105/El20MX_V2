using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
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
                if (Type == "-1") Type = "";

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
                    Value = x.id.ToString()
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
            ViewBag.Date = DateTime.Now.ToString("dd-MM-yyyy");
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
                if (model.AccountId != null)
                {
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
                }

                _promotionService.Save(promotion, promotionsXaccounts, discounts);
                LogUtil.AddEntry(
                       "Promocion creada: " + promotion.name,
                       ENivelLog.Info,
                       authUser.Id,
                       authUser.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       JsonConvert.SerializeObject(promotion)
                    );
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
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
                    Value = x.id.ToString()
                }).ToList();

                model.TypeList = new SelectList(list);
                model.AccountMultipleList = new MultiSelectList(accountViewModel.accountListItems, "Value", "Text", model.AccountId);

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
                    discount = Math.Round(promotion.discount, 2),
                    discountRate = Math.Round(promotion.discountRate, 2),
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
                    Value = x.id.ToString()
                }).ToList();

                model.TypeList = new SelectList(list);
                model.AccountMultipleList = new MultiSelectList(accountViewModel.accountListItems, "Value", "Text", model.AccountId);

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
                promotion.hasPeriod = model.hasPeriod;
                promotion.hasValidity = model.hasValidity;
                promotion.modifiedAt = todayDate;

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


                List<PromotionAccount> prmAccountsAdd = new List<PromotionAccount>();
                List<PromotionAccount> prmAccountsUpd = new List<PromotionAccount>();
                List<PromotionAccount> prmAccountsDel = new List<PromotionAccount>();
                List<Discount> discountsAdd = new List<Discount>();
                List<Discount> discountsUpd = new List<Discount>();
                List<Discount> discountsDel = new List<Discount>();

                var discountsData = _discountService.GetDiscounts(promotion.uuid.ToString());
                var promotionAccountData = _promotionAccountService.GetPromotionAccount(promotion.uuid.ToString());

                List<Int64> idsPro = promotionAccountData.Select(c => c.account.id).Distinct().ToList();
                List<Int64> idsDisc = discountsData.Select(c => c.account.id).Distinct().ToList();


                if (model.AccountId.Count() > 0)
                {
                    if (promotionAccountData != null)
                    {
                        //Agregar
                        List<Int64> NoExistP = model.AccountId.Except(idsPro).ToList();

                        if (NoExistP.Count > 0)
                        {
                            foreach (var item in NoExistP)
                            {
                                PromotionAccount promotionAccount = new PromotionAccount()
                                {
                                    account = new Account { id = item },
                                    promotion = promotion
                                };
                                prmAccountsAdd.Add(promotionAccount);
                            }
                        }

                        //Eliminar
                        List<Int64> NoExistPE = idsPro.Except(model.AccountId).ToList();
                        prmAccountsDel = promotionAccountData.Where(x => NoExistPE.Contains(x.account.id)).ToList();
                    }

                    if (discountsData != null)
                    {
                        //Agregar
                        List<Int64> NoExistD = model.AccountId.Except(idsDisc).ToList();

                        if (NoExistD.Count > 0)
                        {
                            foreach (var item in NoExistD)
                            {
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

                                discountsAdd.Add(discount);
                            }
                        }

                        //Actualizar
                        List<Int64> ExistD = model.AccountId.Intersect(idsDisc).ToList();

                        foreach (var item in ExistD)
                        {
                            var data = discountsData.FirstOrDefault(x => x.account.id == item);
                            if (data != null)
                            {
                                data.name = model.name;
                                data.type = model.TypeId;
                                data.discount = model.discount;
                                data.discountRate = model.discountRate;
                                data.hasPeriod = model.hasPeriod;
                                data.hasValidity = model.hasValidity;
                                data.modifiedAt = todayDate;
                                data.periodInitial = model.initialPeriod;
                                data.periodFinal = model.finalPeriod;
                                data.validityFinalAt = model.finalDate;

                                discountsUpd.Add(data);
                            }
                        }

                        //Eliminar
                        List<Int64> NoExistDE = idsDisc.Except(model.AccountId).ToList();
                        discountsDel = discountsData.Where(x => NoExistDE.Contains(x.account.id)).ToList();
                    }
                }
                else
                {
                    if (promotionAccountData == null)
                    {
                        prmAccountsDel = promotionAccountData;
                    }
                    if (discountsData == null)
                    {
                        discountsDel = discountsData;
                    }
                }

                _promotionService.Update(promotion, prmAccountsAdd, prmAccountsDel, discountsAdd, discountsUpd, discountsDel);
                LogUtil.AddEntry(
                       "Promocion actualizada: " + promotion.name,
                       ENivelLog.Info,
                       userAuth.Id,
                       userAuth.Email,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       JsonConvert.SerializeObject(promotion)
                    );
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
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
                    Value = x.id.ToString()
                }).ToList();

                model.TypeList = new SelectList(list);
                model.AccountMultipleList = new MultiSelectList(accountViewModel.accountListItems, "Value", "Text", model.AccountId);

                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Delete(string uuid, FormCollection collection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            bool success = true;
            string error = String.Empty;
            try
            {
                var promotion = _promotionService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (promotion != null)
                {
                    if (promotion.status == SystemStatus.ACTIVE.ToString())
                    {
                        promotion.status = SystemStatus.INACTIVE.ToString();
                    }
                    else
                    {
                        promotion.status = SystemStatus.ACTIVE.ToString();
                    }

                }
                _promotionService.Update(promotion);

                LogUtil.AddEntry(
                   "Actualización del status: " + promotion.name,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   "Estatus: " + promotion.status
                );

                //return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message.ToString();
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
                //return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = success,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }
    }
}