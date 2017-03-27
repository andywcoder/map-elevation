using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface IMetadataService
    {
        List<SrtmRectangle> GetSRTM1Rectangles();
        List<SrtmRectangle> GetSRTM3Rectangles();
    }
}
