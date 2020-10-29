using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.SAT
{
    public static class SATwsEnums
    {
        public static string GetDescriptionSAT(this Enum elemento)
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

    public enum SATwsEnumsAccept
    {
        [Description("text/xml")]
        textxml,
        [Description("application/pdf")]
        applicationpdf,
        [Description("application/json")]
        applicationjson
    }

}
