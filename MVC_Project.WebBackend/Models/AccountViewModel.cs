﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public Int64 accountId { get; set; }
        public string credentialStatus { get; set; }
        public string imagen { get; set; }
        public string accountStatus { get; set; }
        public string credentialId { get; set; }
        public string ciec { get; set; }
    }

    public class AccountSelectViewModel
    {
        public List<AccountListViewModel> accountListViewModels;
        public int count;

        public List<SelectListItem> accountListItems;
        public int accountListItem;
    }

    public class AvatarAccountViewMovel
    {
        public string uuid { get; set; }
        public string fileName { get; set; }
        public HttpPostedFileBase image { get; set; }
    }
}