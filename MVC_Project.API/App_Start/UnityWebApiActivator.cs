using MVC_Project.API.Components;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using Unity.AspNet.WebApi;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(MVC_Project.API.UnityWebApiActivator), nameof(MVC_Project.API.UnityWebApiActivator.Start))]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(MVC_Project.API.UnityWebApiActivator), nameof(MVC_Project.API.UnityWebApiActivator.Shutdown))]

namespace MVC_Project.API
{
    /// <summary>
    /// Provides the bootstrapping for integrating Unity with WebApi when it is hosted in ASP.NET.
    /// </summary>
    public static class UnityWebApiActivator
    {
        /// <summary>
        /// Integrates Unity when the application starts.
        /// </summary>
        public static void Start() 
        {
            // Use UnityHierarchicalDependencyResolver if you want to use
            // a new child container for each IHttpController resolution.
            //var resolver = new UnityHierarchicalDependencyResolver(UnityConfig.Container);
            var resolver = new UnityDependencyResolver(UnityConfig.Container);
            var config = GlobalConfiguration.Configuration;
            config.DependencyResolver = resolver;
            var providers = config.Services.GetFilterProviders().ToList();

            var defaultprovider = providers.Single(i => i is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);

            config.Services.Add(typeof(IFilterProvider), new UnityFilterProvider(UnityConfig.Container));
        }

        /// <summary>
        /// Disposes the Unity container when the application is shut down.
        /// </summary>
        public static void Shutdown()
        {
            UnityConfig.Container.Dispose();
        }
    }
}