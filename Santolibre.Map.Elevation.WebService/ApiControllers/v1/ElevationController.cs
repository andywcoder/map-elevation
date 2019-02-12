using AutoMapper;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Lib.Services;
using Santolibre.Map.Elevation.WebService.ApiControllers.v1.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.ApiControllers.v1
{
    [RoutePrefix("api/v1")]
    public class ElevationController : ApiController
    {
        private readonly IElevationService _elevationService;
        private readonly IMapper _mapper;
        private readonly IConfigurationService _configurationService;

        public ElevationController(IElevationService elevationService, IMapper mapper, IConfigurationService configurationService)
        {
            _elevationService = elevationService;
            _mapper = mapper;
            _configurationService = configurationService;
        }

        [Route("elevation")]
        [HttpGet]
        public HttpResponseMessage Elevation([FromUri]ElevationQuery elevationQuery)
        {
            var points = GooglePolyline.Decode(elevationQuery.EncodedPoints).ToList();
            if (!points.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Error = "Couldn't decode points" });
            }

            var elevationModelType = _elevationService.GetElevations(points, elevationQuery.SmoothingMode, int.Parse(_configurationService.GetValue("ElevationQueryMaxNodes")));
            if (elevationModelType.HasValue)
            {
                return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<ElevationResponse>(points));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { Error = "No elevation data for this area" });
            }
        }
    }
}
