using System.Configuration;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
