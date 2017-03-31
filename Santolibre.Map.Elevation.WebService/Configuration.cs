using AutoMapper;
using Microsoft.Practices.Unity;
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

            DependencyFactory.Container.RegisterType<IConfigurationService, ConfigurationService>();
            DependencyFactory.Container.RegisterType<ICacheService, CacheService>();
            DependencyFactory.Container.RegisterType<IMetadataService, MetadataService>();
            DependencyFactory.Container.RegisterType<IElevationService, ElevationService>();
            DependencyFactory.Container.RegisterInstance<IMapper>(AutoMapper.CreateMapper());
        }
    }
}
