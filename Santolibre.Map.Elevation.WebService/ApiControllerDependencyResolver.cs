using Santolibre.Map.Elevation.Lib;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Santolibre.Map.Elevation.WebService
{
    public class ApiControllerDependencyResolver : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            if (serviceType.IsSubclassOf(typeof(ApiController)))
            {
                try
                {
                    return (ApiController)DependencyFactory.Resolve(serviceType);
                }
                catch (Exception)
                {
                    throw new Exception("Could not resolve type '" + serviceType + "'");
                }
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
