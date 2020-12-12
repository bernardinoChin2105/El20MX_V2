using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.Jobs.Models
{
    public class ProcessViewModel
    {
        public ProcessViewModel()
        {
            executions = new List<ProcessExecutionModel>();
        }

        public List<ProcessExecutionModel> executions { set; get; }

    }

    public class ProcessExecutionModel
    {
        public Int64 ExecutionId { set; get; }
        public string ProcessName { set; get; }
        public string Status { set; get; }

        public string Success { set; get; }

        public string StartDate { set; get; }

        public string EndDate { set; get; }

    }
}