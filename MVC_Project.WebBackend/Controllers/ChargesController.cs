using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.Recurly;
using MVC_Project.Integrations.SAT;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using MVC_Project.WebBackend.Utils.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class ChargesController : Controller
    {
        static IAccountService _accountService;
        static ICredentialService _credentialService;
        static IRecurlySubscriptionService _recurlySubscriptionService;

        public ChargesController(IAccountService accountService, ICredentialService credentialService, IRecurlySubscriptionService recurlySubscriptionService)
        {
            _accountService = accountService;
            _credentialService = credentialService;
            _recurlySubscriptionService = recurlySubscriptionService;
        }

        // GET: Charges
        public ActionResult Index()
        {
            ChargeClientFilterViewModel model = new ChargeClientFilterViewModel
            {
                Status = "",
            };

            var statusSelectList = new List<SelectListItem>();
            statusSelectList.Add(new SelectListItem
            {
                Text = "Todos",
                Value = "",
                Selected = true
            });
            statusSelectList.AddRange(PopulateStatus());
            model.Statuses = statusSelectList;
            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetClients(JQueryDataTableParams param, string filtros)
        {
            try
            {
                int totalDisplay = 0;
                int total = 0;
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);

                var listResponse = _accountService.FilterBy(filtersValues, param.iDisplayStart, param.iDisplayLength);

                var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
                var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

                var plans = RecurlyService.GetPlans(siteId, provider);

                //var activeSubscriptions = _recurlySubscriptionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString());

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                total = listResponse.Item2;
                totalDisplay = listResponse.Item2;

                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResponse.Item1.Select(x => new ChargeListViewModel
                    {
                        uuid = x.uuid.ToString(),
                        businessName = x.name,
                        rfc = x.rfc,
                        status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), x.status)).GetDisplayName(),
                        billingStart = x.inicioFacturacion?.ToString("MM/yyyy"),//activeSubscriptions.Any(rs => rs.account.id == x.id) ? activeSubscriptions.First(rs => rs.account.id == x.id).createdAt.ToString("MM/yyyy") : x.inicioFacturacion?.ToString("MM/yyyy"),
                        plan = plans.data.FirstOrDefault(rp => rp.code == x.planFijo)?.name,
                        accountOwner = x.memberships.Where(member => member.role.code == SystemRoles.ACCOUNT_OWNER.ToString() && member.status == SystemStatus.ACTIVE.ToString() && member.role.status == SystemStatus.ACTIVE.ToString()).Select(member => $"{member.user.profile.firstName} {member.user.profile.lastName}").FirstOrDefault()
                    })
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

        [Authorize]
        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                Account account = _accountService.FindBy(x => x.uuid == Guid.Parse(uuid)).First();
                if (account == null)
                    throw new Exception("Verifica la información e intenta de nuevo.");

                var activeSubscriptions = _recurlySubscriptionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString() && x.account.id == account.id);
                var subscription = activeSubscriptions.FirstOrDefault();

                ChargeClientEditViewModel model = new ChargeClientEditViewModel();
                model.Uuid = account.uuid.ToString();
                model.Name = account.name;
                model.RFC = account.rfc;
                model.BillingStart = account.inicioFacturacion;
                model.PlanList = PopulatePlans(account.planSchema);
                model.Plan = account.planFijo;
                model.Status = account.status;
                model.StatusList = PopulateStatus(account.status);
                model.AccountOwner = account.memberships.Where(member => member.role.code == SystemRoles.ACCOUNT_OWNER.ToString() && member.status == SystemStatus.ACTIVE.ToString() && member.role.status == SystemStatus.ACTIVE.ToString()).Select(member => $"{member.user.profile.firstName} {member.user.profile.lastName}").FirstOrDefault();

                ViewData["canEditStart"] = subscription == null;

                return View(model);
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
                return RedirectToAction("Index");
            }
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Edit(ChargeClientEditViewModel model)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            Account account = null;
            try
            {
                account = _accountService.FindBy(x => x.uuid == Guid.Parse(model.Uuid)).First();
                var activeSubscriptions = _recurlySubscriptionService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString() && x.account.id == account.id);
                var subscription = activeSubscriptions.FirstOrDefault();

                ViewData["canEditStart"] = subscription == null;

                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                if (subscription == null && model.BillingStart.HasValue)
                {
                    account.inicioFacturacion = model.BillingStart;
                }

                account.status = ((SystemStatus)Enum.Parse(typeof(SystemStatus), model.Status)).ToString();

                account.planFijo = model.Plan;

                _accountService.Update(account);

                LogUtil.AddEntry(
                   "Cuenta editada: " + account.name,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   JsonConvert.SerializeObject(account)
                );

                MensajeFlashHandler.RegistrarMensaje("Configuración realizada correctamente.", TiposMensaje.Success);
                return RedirectToAction("Index");
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
                model.Name = account.name;
                model.RFC = account.rfc;
                model.BillingStart = account.inicioFacturacion;
                model.PlanList = PopulatePlans(model.Plan);
                model.StatusList = PopulateStatus(model.Status);
                model.AccountOwner = account.memberships.Where(member => member.role.code == SystemRoles.ACCOUNT_OWNER.ToString() && member.status == SystemStatus.ACTIVE.ToString() && member.role.status == SystemStatus.ACTIVE.ToString()).Select(member => $"{member.user.profile.firstName} {member.user.profile.lastName}").FirstOrDefault();
                return View(model);
            }
        }

        private List<SelectListItem> PopulatePlans(string planSchema)
        {
            var provider = ConfigurationManager.AppSettings["RecurlyProvider"];
            var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];

            var plans = RecurlyService.GetPlans(siteId, provider);
            List<PlanDataModel> plansList = new List<PlanDataModel>();

            if (!string.IsNullOrEmpty(planSchema) && planSchema.StartsWith(SystemPlan.OLD_SCHEMA.ToString()))
            {
                plansList = plans.data.Where(x => x.code.Contains(SystemPlan.OLD_SCHEMA.GetDisplayName())).ToList();
            }
            else
            {
                plansList = plans.data.Where(x => !x.code.Contains(SystemPlan.OLD_SCHEMA.GetDisplayName())).ToList();
            }

            var rolesList = plansList.Select(x =>
                new SelectListItem
                {
                    Value = x.code,
                    Text = x.name
                }
            );

            return rolesList.ToList();
        }

        private List<SelectListItem> PopulateStatus(string status = null)
        {
            var statusSelectList = new List<SelectListItem>();
            statusSelectList.Add(new SelectListItem
            {
                Text = SystemStatus.ACTIVE.GetDisplayName(),
                Value = SystemStatus.ACTIVE.ToString(),
            });
            statusSelectList.Add(new SelectListItem
            {
                Text = SystemStatus.SUSPENDED.GetDisplayName(),
                Value = SystemStatus.SUSPENDED.ToString(),
            });
            statusSelectList.Add(new SelectListItem
            {
                Text = SystemStatus.CANCELLED.GetDisplayName(),
                Value = SystemStatus.CANCELLED.ToString(),
            });
            if (!string.IsNullOrEmpty(status))
            {
                for (int i = 0; i < statusSelectList.Count; i++)
                {
                    var listItem = statusSelectList[i];
                    listItem.Selected = listItem.Value == status;
                }
            }
            return statusSelectList;
        }

        private bool CanceledAccount(Account account)
        {
            var RecurlyProvider = ConfigurationManager.AppSettings["RecurlyProvider"];
            var siteId = ConfigurationManager.AppSettings["Recurly.SiteId"];
            var SATProvider = ConfigurationManager.AppSettings["SATProvider"];
            var BankProvider = ConfigurationManager.AppSettings["BankProvider"];
            var userAuth = Authenticator.AuthenticatedUser;
            //bool cancel = false;

            var accountCredentials = _credentialService.FindBy(x => x.account.id == account.id && x.status == SystemStatus.ACTIVE.ToString());

            foreach (var credential in accountCredentials)
            {
                try
                {
                    string provider = string.Empty;
                    bool delete = false;
                    string statusProvider = "";

                    if (credential.provider == SystemProviders.SATWS.ToString())
                    {
                        //Evento para desactivar la cuenta en satws
                        //la opción que se tiene es delete credential
                        provider = SystemProviders.SATWS.ToString();
                        try
                        {
                            //Si es eliminada, no regresa nada
                            SATService.DeleteCredential(credential.idCredentialProvider, provider);
                            LogUtil.AddEntry(
                                 "Credencial eliminada de SATws",
                                ENivelLog.Info,
                                userAuth.Id,
                                userAuth.Email,
                                EOperacionLog.ACCESS,
                                string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                                ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                                JsonConvert.SerializeObject(account)
                             );
                            statusProvider = SystemStatus.INACTIVE.ToString();
                            delete = true;
                        }
                        catch (Exception ex)
                        {
                            //Si marca error entonces no se elimino la cuenta en satws
                            LogUtil.AddEntry(
                               "Error al eliminar la cuenta de SATws: " + ex.Message.ToString(),
                               ENivelLog.Error,
                               userAuth.Id,
                               userAuth.Email,
                               EOperacionLog.ACCESS,
                               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                            );
                        }
                    }
                    else if (credential.provider == SystemProviders.SYNCFY.GetDisplayName())
                    {
                        //Evento para desactivar la cuenta en syncfy
                        //La opcion que se tiene es delete credential       
                        provider = SystemProviders.SYNCFY.GetDisplayName();
                        var response = PaybookService.DeleteUser(credential.idCredentialProvider, "Delete", true);

                        LogUtil.AddEntry(
                               "Eliminación de la cuenta de Paybook.",
                               ENivelLog.Info,
                               userAuth.Id,
                               userAuth.Email,
                               EOperacionLog.ACCESS,
                               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                               JsonConvert.SerializeObject(account)
                            );

                        if (response)
                        {
                            delete = true;
                            statusProvider = SystemStatus.INACTIVE.ToString();
                        }
                    }
                    else if (credential.provider == SystemProviders.RECURLY.ToString())
                    {
                        //Evento para desactivar la cuenta en recurly
                        //También sería el evento de delete account
                        provider = SystemProviders.RECURLY.ToString();
                        var response = RecurlyService.DeleteAccount(credential.idCredentialProvider, siteId, provider);

                        LogUtil.AddEntry(
                               "Respuesta de Recurly al eliminar la cuenta.",
                               ENivelLog.Info,
                               userAuth.Id,
                               userAuth.Email,
                               EOperacionLog.ACCESS,
                               string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                               JsonConvert.SerializeObject(account)
                            );

                        if (response != null)
                        {
                            statusProvider = response.State;
                            delete = true;
                        }
                    }

                    //Inactivar cuentas desde nuestras tablas 
                    if (delete)
                    {
                        if (!string.IsNullOrEmpty(statusProvider))
                            credential.statusProvider = statusProvider;

                        credential.status = SystemStatus.INACTIVE.ToString();
                        credential.modifiedAt = DateUtil.GetDateTimeNow();
                        _credentialService.Update(credential);
                    }
                    else
                        throw new Exception("No se pudo realizar la desactivación de la credencial de " + provider + ", credentialId: " + credential.id + ", accountId: " + credential.account.id);

                    //guardar logs

                    LogUtil.AddEntry(
                       "Método para cancelación de credenciales",
                         ENivelLog.Info,
                         userAuth.Id,
                         userAuth.Email,
                         EOperacionLog.ACCESS,
                         string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                         ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                         JsonConvert.SerializeObject(account)
                      );
                }
                catch (Exception ex)
                {
                    //Guardar en el log, el motivo de la excepción
                    LogUtil.AddEntry(
                        "Detalle del error al cancelar la credencial de la cuenta: " + ex.Message.ToString(),
                         ENivelLog.Info,
                         userAuth.Id,
                         userAuth.Email,
                         EOperacionLog.ACCESS,
                         string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                         ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                         JsonConvert.SerializeObject(account)
                      );
                }
            }

            var credentialConfirm = _credentialService.FindBy(x => x.account.id == account.id && x.status == "ACTIVE");

            if (credentialConfirm.Count() == 0)
            {
                //Cambiar el status de la cuenta
                var accountD = _accountService.FirstOrDefault(x => x.id == account.id);
                if (accountD != null)
                {
                    account.modifiedAt = DateUtil.GetDateTimeNow();
                    account.status = SystemStatus.CANCELLED.ToString();
                    _accountService.Update(account);

                    LogUtil.AddEntry(
                       "Se cambio el status de la cuenta de Suspendido a Cancelado.",
                         ENivelLog.Info,
                         userAuth.Id,
                         userAuth.Email,
                         EOperacionLog.ACCESS,
                         string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                         ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                         JsonConvert.SerializeObject(account)
                      );
                    
                }
            }

            return account.status == SystemStatus.CANCELLED.ToString()? true: false;
        }
    }
}