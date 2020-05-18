using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Desktop.Helpers;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Domain.Services;
using System;
using System.Windows.Forms;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace MVC_Project.Desktop
{
    static class Program
    {
        

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UnityHelper.Init();
            Application.Run(new LoginForm());
            //Application.Run(container.Resolve<LoginForm>());
            //Application.Run(container);
        }
       
    }
}
