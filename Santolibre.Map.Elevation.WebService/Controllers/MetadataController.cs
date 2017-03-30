using Santolibre.Map.Elevation.Lib.Models;
using Santolibre.Map.Elevation.Lib.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.Controllers
{
    [RoutePrefix("api/v1")]
    public class MetadataController : ApiController
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [Route("metadata")]
        [HttpGet]
        public List<SrtmRectangle> Metadata()
        {
            var srtmRectangles = new List<SrtmRectangle>();
            srtmRectangles.AddRange(_metadataService.GetSRTM1Rectangles());
            srtmRectangles.AddRange(_metadataService.GetSRTM3Rectangles());
            return srtmRectangles;
        }
    }
}
