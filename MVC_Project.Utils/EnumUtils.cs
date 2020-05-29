using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Utils
{
    public static class EnumUtils
    {
        public static string GetDisplayName(this Enum elemento)
        {
            Type type = elemento.GetType();
            MemberInfo[] member = type.GetMember(elemento.ToString());
            DisplayAttribute displayName = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            if (displayName != null)
            {
                return displayName.Name;
            }
            return null;
        }
        public static string GetDescription(this Enum elemento)
        {
            Type type = elemento.GetType();
            MemberInfo[] member = type.GetMember(elemento.ToString());
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)member[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }
            return null;
        }
    }
}
