using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class AccountViewModel
    {
    }

    public class AccountListViewModel
    {
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string rfc { get; set; }
        public string role { get; set; }
    }

    public class AccountSelectViewModel
    {
        public List<AccountListViewModel> accountListViewModels;
        public int count;
    }
}