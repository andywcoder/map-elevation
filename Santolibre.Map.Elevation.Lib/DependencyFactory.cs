using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Santolibre.Map.Elevation.Lib
{
    public class DependencyFactory
    {
        public static UnityContainer Container { get; private set; }

        static DependencyFactory()
        {
            var container = new UnityContainer();
            var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            if (section != null)
            {
                section.Configure(container);
            }
            Container = container;
        }

        public static T Resolve<T>()
        {
            var instance = default(T);

            if (Container.IsRegistered(typeof (T)))
            {
                instance = Container.Resolve<T>();
            }

            return instance;
        }

        public static object Resolve(Type type)
        {
            var instance = Container.Resolve(type);
            return instance;
        }
    }
}
