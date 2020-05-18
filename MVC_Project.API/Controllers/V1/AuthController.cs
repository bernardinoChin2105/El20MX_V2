﻿using Microsoft.Web.Http;
using MVC_Project.API.Models.Api_Request;
using MVC_Project.API.Models.Api_Response;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MVC_Project.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/v{version:apiVersion}/auth")]
    public class AuthController : BaseApiController
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IAuthService _authService;

        public AuthController(IUserService userService, IRoleService roleService, IAuthService authService)
        {
            _userService = userService;
            _roleService = roleService;
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public HttpResponseMessage Login(LoginRequest request) 
        {
            try
            {
                List<MessageResponse> messages = new List<MessageResponse>();
                var authUser = _authService.Authenticate(request.Username, SecurityUtil.EncryptPassword(request.Password));
                if(authUser == null || !authUser.Status)
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El usuario no existe o contraseña inválida." });
                    return CreateErrorResponse(null, messages);
                }
                if(authUser.Role.Code != Constants.ROLE_APP_USER)
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El usuario no cuenta con acceso al app." });
                    return CreateErrorResponse(null, messages);
                }
                var user = _userService.FindBy(x => x.Uuid == authUser.Uuid).First();
                var expiration = DateTime.UtcNow.AddHours(Constants.HOURS_EXPIRATION_KEY);
                user.ApiKey = Guid.NewGuid().ToString();
                user.ExpiraApiKey = expiration;
                _userService.Update(user);
                var response = new AuthUserResponse
                {
                    ApiKey = user.ApiKey,
                    ApiKeyExpiration = expiration.ToString(Constants.DATE_FORMAT_CALENDAR),
                    UserData = new AuthUser
                    {
                        Uuid = user.Uuid,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    }
                };
                return CreateResponse(response);
            }
            catch(Exception e)
            {
                return CreateErrorResponse(e, null);
            }
        }

        [HttpPost]
        [Route("register")]
        public HttpResponseMessage Register(RegisterRequest request)
        {
            try
            {
                List<MessageResponse> messages = new List<MessageResponse>();
                var currentUsers = _userService.GetAll();
                if(currentUsers.Any(x => x.Email == request.Email))
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El correo electrónico proporcionado ya se encuentra registrado." });
                    return CreateErrorResponse(null, messages);
                }
                User user = new User();
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.Password = SecurityUtil.EncryptPassword(request.Password);
                user.Uuid = Guid.NewGuid().ToString();
                //TODO:ESTO ESTA MAL, SE DEBE TRAER POR CODIGO DE ROLE
                user.Role = new Role { Id = 3 }; //ROL DE APP 
                user.Permissions = _roleService.GetById(user.Role.Id).Permissions.ToList();
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                user.Status = true;
                _userService.Create(user);
                messages.Add(new MessageResponse { Type = MessageType.info.ToString("G"), Description = "Usuario creado correctamente." });
                return CreateResponse(messages);
            }
            catch (Exception e)
            {
                return CreateErrorResponse(e, null);
            }
        }

        [HttpGet]
        [Route("recover")]
        public HttpResponseMessage Recover([FromUri (Name = "email")] string email)
        {
            try
            {
                List<MessageResponse> messages = new List<MessageResponse>();

                var user = _userService.FindBy(e => e.Email == email).FirstOrDefault();
                if (user == null)
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El correo electrónico solicitado no se encuentra registrado." });
                    return CreateErrorResponse(null, messages);
                }
                if (user.Role.Code != "APP_USER")
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El usuario no cuenta con acceso al app." });
                    return CreateErrorResponse(null, messages);
                }
                string token = (user.Uuid + "@" + DateTime.Now.AddDays(1).ToString());
                token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                List<string> Email = new List<string>();
                Email.Add(user.Email);
                Dictionary<string, string> customParams = new Dictionary<string, string>();
                string urlAccion = ConfigurationManager.AppSettings["_UrlServerAccess"].ToString();
                string link = urlAccion + "Auth/AccedeToken?token=" + token;
                customParams.Add("param1", user.Email);
                customParams.Add("param2", link);
                string template = "aa61890e-5e39-43c4-92ff-fae95e03a711";
                NotificationUtil.SendNotification(Email, customParams, template);

                user.ExpiraToken = DateTime.Now.AddDays(1);
                user.Token = token;
                _userService.Update(user);
                
                messages.Add(new MessageResponse { Type = MessageType.info.ToString("G"), Description = "Solicitud enviada." });
                return CreateResponse(messages);
            }
            catch (Exception e)
            {
                return CreateErrorResponse(e, null);
            }
        }

        [HttpPost]
        [Route("reset")]
        public HttpResponseMessage resetPass(ResetPassRequest request)
        {
            try
            {
                List<MessageResponse> messages = new List<MessageResponse>();
                var decrypted = EncryptorText.DataDecrypt(request.Token.Replace("!!", "/").Replace("$", "+"));
                if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(decrypted))
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "Token de recuperación no encontrado." });
                    return CreateErrorResponse(null, messages);
                }
                string id = decrypted.Split('@').First();
                var user = _userService.FindBy(x => x.Uuid == id).First();
                if(user == null || DateTime.Now > user.ExpiraToken)
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El token ha expirado." });
                    return CreateErrorResponse(null, messages);
                }
                if (user.Role.Code != "APP_USER")
                {
                    messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = "El usuario no cuenta con acceso al app." });
                    return CreateErrorResponse(null, messages);
                }
                user.Password = request.Password;
                _userService.Update(user);
                messages.Add(new MessageResponse { Type = MessageType.info.ToString("G"), Description = "Contraseña actualizada." });
                return CreateResponse(messages);
            }
            catch(Exception e)
            {
                return CreateErrorResponse(e, null);
            }
        }
    }
}
