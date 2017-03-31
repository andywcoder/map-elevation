using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Lib.Services;
using Santolibre.Map.Elevation.WebService.Controllers.v1.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.Controllers.v1
{
    [RoutePrefix("api/v1")]
    public class ElevationController : ApiController
    {
        private readonly IElevationService _elevationService;

        public ElevationController(IElevationService elevationService)
        {
            _elevationService = elevationService;
        }

        [Route("elevation")]
        [HttpGet]
        public HttpResponseMessage Elevation(string encodedPoints = null)
        {
            if (encodedPoints != null)
            {
                var points = GooglePoints.Decode(encodedPoints).ToList();
                if (!points.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                var elevationModelType = _elevationService.GetElevations(points, Lib.Models.SmoothingMode.None, 10000);

                if (elevationModelType.HasValue)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new ElevationResponse() { RangeHeight = points.Select(x => new float[] { (float)Math.Round(x.Distance * 1000), (float)Math.Round(x.Elevation, 1) }).ToList() });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
