using AutoMapper;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using NLog;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Lib.Services;

namespace Santolibre.Map.Elevation.WebService
{
    public class Configuration
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void SetupServices()
        {
            Logger.Trace("Setting up services");

            DependencyFactory.Container.AddNewExtension<Interception>();

            DependencyFactory.Container.RegisterType<IConfigurationService, ConfigurationService>(
                new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
            DependencyFactory.Container.RegisterType<ICacheService, CacheService>(
                new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
            DependencyFactory.Container.RegisterType<IMetadataService, MetadataService>(
                new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
            DependencyFactory.Container.RegisterType<IElevationService, ElevationService>(
                new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<LoggingInterceptionBehavior>());
            DependencyFactory.Container.RegisterInstance<IMapper>(AutoMapper.CreateMapper());
        }
    }
}
