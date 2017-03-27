using System;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface ICacheService
    {
        object GetValue(string key);
        bool Add(string key, object value, DateTimeOffset absExpiration);
        void Delete(string key);
    }
}
