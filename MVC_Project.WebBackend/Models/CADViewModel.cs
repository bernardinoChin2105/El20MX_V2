using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class CADAccountsViewModel
    {
        public Int64 cad { get; set; }
        public List<SelectListItem> cads { get; set; }
    }
    
}