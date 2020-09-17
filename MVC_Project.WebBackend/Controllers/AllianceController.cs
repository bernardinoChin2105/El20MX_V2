using LogHubSDK.Models;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Services;
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
    public class AllianceController : Controller
    {
        private IAllyService _allyService;

        public AllianceController(IAllyService allyService)
        {
            _allyService = allyService;
        }

        // GET: Alliance
        public ActionResult Index()
        {
            return View();
        }
        
    }
}