using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class PlanController : Controller
    {
        private IPlanService _planService;
        private IPlanChargeService _planChargeService;
        private IPlanAssignmentsService _planAssignmentsService;
        private IPlanChargeConfigService _planChargeConfigService;
        private IPlanAssignmentConfigService _planAssignmentConfigService;

        public PlanController(IPlanService planeService, IPlanChargeService planChangeService, IPlanAssignmentsService planAssignmentsService,
            IPlanChargeConfigService planChargeConfigService, IPlanAssignmentConfigService planAssignmentConfigService)
        {
            _planService = planeService;
            _planChargeService = planChangeService;
            _planAssignmentsService = planAssignmentsService;
            _planChargeConfigService = planChargeConfigService;
            _planAssignmentConfigService = planAssignmentConfigService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetPlans(JQueryDataTableParams param, string filtros)//, bool first
        {
            var userAuth = Authenticator.AuthenticatedUser;
            int totalDisplay = 0;
            int total = 0;
            bool success = true;
            string message = string.Empty;
            var listResponse = new List<PlansViewModel>();

            try
            {
                //if (!first)
                //{
                NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                string Name = filtersValues.Get("Name").Trim();

                var pagination = new BasePagination();
                pagination.PageSize = param.iDisplayLength;
                pagination.PageNum = param.iDisplayLength == 1 ? (param.iDisplayStart + 1) : (int)(Math.Floor((decimal)((param.iDisplayStart + 1) / param.iDisplayLength)) + 1);
                //if (Name != "") filters.businessName = businessName;

                listResponse = _planService.GetPlans(pagination, Name);

                //Corroborar los campos iTotalRecords y iTotalDisplayRecords
                if (listResponse.Count() > 0)
                {
                    totalDisplay = listResponse[0].Total;
                    total = listResponse.Count();
                }
                //}

                LogUtil.AddEntry(
                   "Lista de clientes totalDisplay: " + totalDisplay + ", total: " + total,
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message.ToString();
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
            }

            return Json(new
            {
                success = success,
                message = message,
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResponse
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                PlanViewModel model = new PlanViewModel();
                model.LabelConcepts = _planChargeService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanLabelsViewModel { Id = x.id, Label = x.name }).ToList();
                model.LabelAssignment = _planAssignmentsService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanAssignmentLabels
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        fieldType = x.fielType,
                                        operation = x.operation,
                                        dataType = (x.dataType == "System.Int32" ? "number" : "text"),
                                        providerData = x.providerData
                                    }).ToList();
                //Faltan las características

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Create(FormCollection formCollection)
        {
            var authUser = Authenticator.AuthenticatedUser;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                string Name = formCollection["name"];
                if (_planService.FindBy(x => x.name == Name).Any())
                    throw new Exception("Ya existe un Plan con el nombre proporcionado");

                List<string> isCurrent = formCollection["isCurrent"].Split(',').ToList();
                List<string> ConceptsId = formCollection["ConceptId[]"].Split(',').ToList();
                List<string> AssigId = formCollection["AssigId[]"].Split(',').ToList();

                DateTime todayDate = DateUtil.GetDateTimeNow();
                Plan plan = new Plan()
                {
                    uuid = Guid.NewGuid(),
                    name = Name,
                    isCurrent = Convert.ToBoolean(isCurrent.First()),
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.ACTIVE.ToString()
                };

                List<PlanChargeConfiguration> charges = new List<PlanChargeConfiguration>();
                List<PlanAssignmentConfiguration> assignments = new List<PlanAssignmentConfiguration>();

                if (ConceptsId.Count() > 0)
                {
                    foreach (var id in ConceptsId)
                    {
                        decimal chargeV = Convert.ToDecimal(formCollection["Concept" + id]);
                        PlanChargeConfiguration chargeC = new PlanChargeConfiguration()
                        {
                            uuid = Guid.NewGuid(),
                            plan = plan,
                            planCharge = new PlanCharge { id = Convert.ToInt64(id) },
                            charge = chargeV,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };
                        charges.Add(chargeC);
                    }
                }

                if (AssigId.Count() > 0)
                {
                    foreach (var id in AssigId)
                    {
                        //Assig@(item.Id)Value2
                        string assigV1 = formCollection["Assig" + id + "Value1"];
                        PlanAssignmentConfiguration assignment = new PlanAssignmentConfiguration()
                        {
                            uuid = Guid.NewGuid(),
                            plan = plan,
                            planAssignment = new PlanAssignment { id = Convert.ToInt64(id) },
                            value1 = assigV1,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };
                        if (formCollection.AllKeys.Contains("Assig" + id + "Value2"))
                            assignment.value2 = formCollection["Assig" + id + "Value2"];

                        assignments.Add(assignment);
                    }
                }

                var planR = _planService.SavePlan(plan, charges, assignments);

                LogUtil.AddEntry(
                    "Crea nuevo plan: " + JsonConvert.SerializeObject(planR),
                    ENivelLog.Info,
                    authUser.Id,
                    authUser.Email,
                    EOperacionLog.ACCESS,
                    string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
                    ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                    string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
                );
                MensajeFlashHandler.RegistrarMensaje("Registro exitoso", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                PlanViewModel modelC = new PlanViewModel();
                modelC.LabelConcepts = _planChargeService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanLabelsViewModel { Id = x.id, Label = x.name }).ToList();
                modelC.LabelAssignment = _planAssignmentsService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanAssignmentLabels
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        fieldType = x.fielType,
                                        operation = x.operation,
                                        dataType = (x.dataType == "System.Int32" ? "number" : "text"),
                                        providerData = x.providerData
                                    }).ToList();
                //Faltan las características

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

                return View(modelC);
            }
        }

        public ActionResult Edit(string uuid)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            try
            {
                var plan = _planService.FirstOrDefault(x => x.uuid.ToString() == uuid);
                if (plan == null)
                    throw new Exception("El Plan no se encontró en la base de datos");

                var planChargeConfig = _planChargeConfigService.FindBy(x => x.plan.id == plan.id);
                var planAssigConfig = _planAssignmentConfigService.FindBy(x => x.plan.id == plan.id);

                PlanViewModel model = new PlanViewModel();
                model.Id = plan.id;
                model.Name = plan.name;
                model.isCurrent = plan.isCurrent;
                model.LabelConcepts = _planChargeService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanLabelsViewModel
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        ChargeId = planChargeConfig.Count() > 0 ? planChargeConfig.FirstOrDefault(y => y.planCharge.id == x.id).id : 0,
                                        Value = planChargeConfig.Count() > 0 ? planChargeConfig.FirstOrDefault(y => y.planCharge.id == x.id).charge : 0
                                    }).ToList();
                model.LabelAssignment = _planAssignmentsService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanAssignmentLabels
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        fieldType = x.fielType,
                                        operation = x.operation,
                                        dataType = (x.dataType == "System.Int32" ? "number" : "text"),
                                        providerData = x.providerData,
                                        AssignmentId = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).id : 0,
                                        Value1 = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).value1 : "",
                                        Value2 = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).value2 : "",
                                    }).ToList();
                //Faltan las características

                return View(model);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Index");
            }
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public ActionResult Edit(FormCollection formCollection)
        {
            var userAuth = Authenticator.AuthenticatedUser;
            Int64 Id = Convert.ToInt64(formCollection["Id"]);
            var dataPlan = _planService.FirstOrDefault(x => x.id == Id);

            try
            {

                string Name = formCollection["name"];
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                DateTime todayDate = DateUtil.GetDateTimeNow();
                List<string> isCurrent = formCollection["isCurrent"].Split(',').ToList();
                List<string> ChargeId = formCollection["ChargeId[]"].Split(',').ToList();
                List<string> AssigId = formCollection["AssigId[]"].Split(',').ToList();

                //ponerle validación de nombre?

                dataPlan.name = Name;
                dataPlan.isCurrent = Convert.ToBoolean(isCurrent.First());
                dataPlan.modifiedAt = todayDate;

                #region Actualizar registros de las listas de emails y teléfonos 
                List<PlanChargeConfiguration> charges = new List<PlanChargeConfiguration>();
                List<PlanAssignmentConfiguration> assignments = new List<PlanAssignmentConfiguration>();
                List<string> ChargeConfigId = formCollection["ChargeConfigId[]"].Split(',').ToList();
                List<string> AssigConfigId = formCollection["AssigConfigId[]"].Split(',').ToList();

                if (ChargeId.Count() > 0)
                {
                    foreach (var id in ChargeId)
                    {
                        var chargeMod = _planChargeConfigService.FirstOrDefault(x => x.planCharge.id.ToString() == id);
                        decimal chargeV = Convert.ToDecimal(formCollection["Charge" + id]);

                        if (chargeMod != null && ChargeConfigId.Contains(chargeMod.id.ToString()))
                        {
                            //editado
                            chargeMod.charge = chargeV;
                            chargeMod.modifiedAt = todayDate;

                            charges.Add(chargeMod);
                        }
                        else
                        {
                            //Nuevo
                            PlanChargeConfiguration chargeC = new PlanChargeConfiguration()
                            {
                                uuid = Guid.NewGuid(),
                                plan = dataPlan,
                                planCharge = new PlanCharge { id = Convert.ToInt64(id) },
                                charge = chargeV,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };
                            charges.Add(chargeC);
                        }
                    }
                }

                //Faltan las categorías

                if (AssigId.Count() > 0)
                {
                    foreach (var id in AssigId)
                    {
                        var assignMod = _planAssignmentConfigService.FirstOrDefault(x => x.planAssignment.id.ToString() == id);
                        string assigV1 = formCollection["Assig" + id + "Value1"];

                        if (assignMod != null && AssigConfigId.Contains(assignMod.id.ToString()))
                        {
                            //editado
                            assignMod.value1 = assigV1;
                            assignMod.modifiedAt = todayDate;
                            if (formCollection.AllKeys.Contains("Assig" + id + "Value2"))
                                assignMod.value2 = formCollection["Assig" + id + "Value2"];

                            assignments.Add(assignMod);
                        }
                        else
                        {
                            //Nuevo                            
                            PlanAssignmentConfiguration assignment = new PlanAssignmentConfiguration()
                            {
                                uuid = Guid.NewGuid(),
                                plan = dataPlan,
                                planAssignment = new PlanAssignment { id = Convert.ToInt64(id) },
                                value1 = assigV1,
                                createdAt = todayDate,
                                modifiedAt = todayDate,
                                status = SystemStatus.ACTIVE.ToString()
                            };
                            if (formCollection.AllKeys.Contains("Assig" + id + "Value2"))
                                assignment.value2 = formCollection["Assig" + id + "Value2"];

                            assignments.Add(assignment);
                        }
                    }
                }

                #endregion

                _planService.SavePlan(dataPlan, charges, assignments);

                LogUtil.AddEntry(
                   "Editar cliente: " + JsonConvert.SerializeObject(dataPlan),
                   ENivelLog.Info,
                   userAuth.Id,
                   userAuth.Email,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", userAuth.Email, DateUtil.GetDateTimeNow())
                );

                MensajeFlashHandler.RegistrarMensaje("Actualización exitosa", TiposMensaje.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);

                var planChargeConfig = _planChargeConfigService.FindBy(x => x.plan.id == dataPlan.id);
                var planAssigConfig = _planAssignmentConfigService.FindBy(x => x.plan.id == dataPlan.id);

                PlanViewModel model = new PlanViewModel();
                model.Id = dataPlan.id;
                model.Id = dataPlan.id;
                model.Name = dataPlan.name;
                model.isCurrent = dataPlan.isCurrent;
                model.LabelConcepts = _planChargeService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanLabelsViewModel
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        ChargeId = planChargeConfig.Count() > 0 ? planChargeConfig.FirstOrDefault(y => y.planCharge.id == x.id).id : 0,
                                        Value = planChargeConfig.Count() > 0 ? planChargeConfig.FirstOrDefault(y => y.planCharge.id == x.id).charge : 0
                                    }).ToList();
                model.LabelAssignment = _planAssignmentsService.FindBy(x => x.status == SystemStatus.ACTIVE.ToString())
                                    .Select(x => new PlanAssignmentLabels
                                    {
                                        Id = x.id,
                                        Label = x.name,
                                        fieldType = x.fielType,
                                        operation = x.operation,
                                        dataType = (x.dataType == "System.Int32" ? "number" : "text"),
                                        providerData = x.providerData,
                                        AssignmentId = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).id : 0,
                                        Value1 = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).value1 : "",
                                        Value2 = planAssigConfig.Count() > 0 ? planAssigConfig.FirstOrDefault(y => y.planAssignment.id == x.id).value2 : "",
                                    }).ToList();
                //Faltan las características

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
    }
}