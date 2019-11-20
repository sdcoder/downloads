[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(LightStreamWeb.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(LightStreamWeb.App_Start.NinjectWebCommon), "Stop")]

namespace LightStreamWeb.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using LightStreamWeb.App_State;
    using LightStreamWeb.Filters;
    using Ninject.Web.Mvc.FilterBindingSyntax;
    using Ninject.Web.Common.WebHost;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static Bootstrapper Bootstrapper
        {
            get
            {
                return bootstrapper;
            }
        }

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ICurrentUser>().To<CurrentUser>();
            kernel.Bind<ICurrentHttpRequest>().To<CurrentHttpRequest>();
            kernel.Bind<Models.Middleware.IAppSettings>()
                .ToMethod(method => Models.Middleware.AppSettings.Load())
                .InSingletonScope();
            kernel.Bind<IVersionInfo>().To<VersionInfo>().InSingletonScope();
            kernel.Bind<IEndpoints>().To<Endpoints>().InSingletonScope();
            kernel.BindFilter<RequireApplicationIdAttribute>(System.Web.Mvc.FilterScope.Action, 0)
                .WhenActionMethodHas<RequireApplicationIdAttribute>()
                .WithConstructorArgument("user", new CurrentUser());
        }        
    }
}
