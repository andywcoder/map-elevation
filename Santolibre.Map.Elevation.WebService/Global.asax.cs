using Santolibre.Map.Elevation.Lib;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Configuration.SetupServices();
        }
    }
}
