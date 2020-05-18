using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace MVC_Project.Desktop.Helpers
{
    public class UnityHelper
    {
        static IUnityContainer container;

        public static void Init()
        {
            RegisterTypes();
        }
        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        private static void RegisterTypes()
        {
            container = new UnityContainer();
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerThreadLifetimeManager());
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
            container.RegisterType(typeof(IService<>), typeof(ServiceBase<>));
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<IRoleService, RoleService>();
        }

        
    }
}
