using MVC_Project.Domain.Services;
using MVC_Project.Integrations.SAT;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class DiagnosticController : Controller
    {
        private IAccountService _accountService;
        private IUserService _userService;

        public DiagnosticController(IAccountService accountService, IUserService userService)
        {
            _accountService = accountService;
            _userService = userService;
        }

        // GET: Diagnostic
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Date = new
            {
                MinDate = DateTime.Now.AddDays(-10).ToString("dd-MM-yyyy"),
                MaxDate = DateTime.Now.ToString("dd-MM-yyyy")
            };
            return View();
        }
        [AllowAnonymous]
        public ActionResult DiagnosticDetail()
        {
            return View();
        }

        public JsonResult GenerateDx0()
        {
            try
            {
                //Obtener la credencial por la cuenta logeada
                //Obtener 
                //var responseSat = SATws.CallServiceSATws("credentials", dataSat, "Get");

                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            
        }

        [HttpGet, AllowAnonymous]
        public JsonResult ObtenerDiagnostic(JQueryDataTableParams param, string filtros)
        {
            try
            {
                var userAuth = Authenticator.AuthenticatedUser;
                IList<RoleData> UsuariosResponse = new List<RoleData>();
                int totalDisplay = 0;
                if (userAuth.Account != null)
                {
                    //var roles = _roleService.FilterBy(filtros, userAuth.Account.Id, param.iDisplayStart, param.iDisplayLength);
                    //totalDisplay = roles.Item2;
                    //foreach (var rol in roles.Item1)
                    //{
                    //    RoleData userData = new RoleData();
                    //    userData.Name = rol.name;
                    //    userData.Description = rol.description;
                    //    userData.CreatedAt = rol.createdAt;
                    //    userData.UpdatedAt = rol.modifiedAt;
                    //    userData.Status = rol.status == SystemStatus.ACTIVE.ToString();
                    //    userData.Uuid = rol.uuid.ToString();
                    //    UsuariosResponse.Add(userData);
                    //}
                }
                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = UsuariosResponse.Count(),
                    iTotalDisplayRecords = totalDisplay,
                    aaData = UsuariosResponse
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