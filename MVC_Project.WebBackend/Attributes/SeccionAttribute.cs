using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utils;

namespace MVC_Project.BackendWeb.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Method)] 
    public class SeccionAttribute : Attribute
    {
        //public Constantes.SeccionesSistema Seccion { get; set; }

    }
}