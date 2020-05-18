﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Utils.Enums
{
    public sealed class FilterStatusEnum : Enumeration
    {
        public static readonly FilterStatusEnum INACTIVE = new FilterStatusEnum(0, "Inactivo");
        public static readonly FilterStatusEnum ACTIVE = new FilterStatusEnum(1, "Activo");
        public static readonly FilterStatusEnum ALL = new FilterStatusEnum(MVC_Project.Utils.Constants.SEARCH_ALL, "Todos");

        private FilterStatusEnum(int id, string name) : base(id, name)
        {
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            List<SelectListItem> filterStatusList = new List<SelectListItem>();
            foreach (var status in GetAll<FilterStatusEnum>())
            {
                filterStatusList.Add(new SelectListItem
                {
                    Text = status.Name,
                    Value = status.Id.ToString()
                });
            }
            return filterStatusList;
        }
    }
}