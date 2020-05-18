using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class RegisterController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;

        public RegisterController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAccount(RegisterViewModel model)
        {
            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword)
                && !String.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
                }
            }

            if (ModelState.IsValid)
            {
                var UserVal = _userService.FindBy(x => x.Email == model.Email).FirstOrDefault();

                if (UserVal == null)
                {
                    DateTime todayDate = DateUtil.GetDateTimeNow();

                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));

                    var availableRoles = _roleService.GetAll();
                    var role = availableRoles.Where(x => x.Code == "EMPLOYEE").FirstOrDefault();

                    var user = new User
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        FirstName = model.FistName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Password = SecurityUtil.EncryptPassword(model.Password),
                        PasswordExpiration = passwordExpiration,
                        Username = model.Email,
                        CreatedAt = todayDate,
                        UpdatedAt = todayDate,
                        Status = true,
                        Role = new Role { Id = role.Id }
                    };

                    foreach (var permission in role.Permissions)
                    {
                        user.Permissions.Add(permission);
                    }
                    _userService.Create(user);
                    //ViewBag.Message = "Usuario registrado";
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.Error = "Ya existe el usuario con el Email registrado.";
                    return View("Index", model);
                }                
            }
            else
            {                
                return View("Index", model);
            }           
        }
                
    }
}