using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Lib.Models;
using Santolibre.Map.Elevation.Lib.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.Controllers
{
    [RoutePrefix("api/v1/elevation")]
    public class ElevationController : ApiController
    {
        private readonly IElevationService _elevationService;

        public ElevationController(IElevationService elevationService)
        {
            _elevationService = elevationService;
        }

        [Route("{encodedPoints}")]
        [HttpGet]
        public List<Node> Elevation(string encodedPoints)
        {
            var nodes = GooglePoints.Decode(encodedPoints).ToList();
            var elevationModelType = _elevationService.GetElevations(nodes, SmoothingMode.None, 10000);
            return elevationModelType.HasValue ? nodes : null;
        }
    }
}
