using System.Collections.Generic;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Utils.Enums
{
    public class FilterAccountStatusEnum : Enumeration
    {
        public static readonly FilterAccountStatusEnum ALL = new FilterAccountStatusEnum(MVC_Project.Utils.Constants.SEARCH_ALL, "Todos");
        public static readonly FilterAccountStatusEnum ACTIVE = new FilterAccountStatusEnum(0, "Activo");
        public static readonly FilterAccountStatusEnum INACTIVE = new FilterAccountStatusEnum(1, "Inactivo");

        private FilterAccountStatusEnum(int id, string name) : base(id, name)
        {
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            List<SelectListItem> filterStatusList = new List<SelectListItem>();
            foreach (var status in GetAll<FilterAccountStatusEnum>())
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