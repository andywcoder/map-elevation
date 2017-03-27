using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface IElevationService
    {
        DigitalElevationModelType? GetElevations(List<Node> nodes, SmoothingMode smoothingMode, int maxNodes);
    }
}
