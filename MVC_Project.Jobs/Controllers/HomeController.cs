using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Jobs.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MVC_Project.Jobs.Controllers
{
    public class HomeController : Controller
    {
        readonly IProcessService _processService;

        public HomeController()
        {
            _processService = new ProcessService(new Repository<Process>(new UnitOfWork()));
        }

        public ActionResult Index()
        {
            IList<ProcessExecution> executions = _processService.GetAllExecutions();
            ProcessViewModel model = new ProcessViewModel();
            foreach(var execution in executions)
            {
                string SuccessStr = "PENDING...";
                if (!execution.Status && execution.Success) SuccessStr = "OK";
                if (!execution.Status && !execution.Success) SuccessStr = "ERROR";

                model.executions.Add(new ProcessExecutionModel() {
                    ExecutionId = execution.Id,
                    ProcessName = execution.Process.Code,
                    StartDate = execution.StartAt.Value.ToString(),
                    EndDate = execution.EndAt.HasValue ? execution.EndAt.Value.ToString() : "N/A",
                    Status = execution.Status ? "RUNNING" : "FINISHED",
                    Success = SuccessStr
                });
            }
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}