using Santolibre.Map.Elevation.Lib.Models;
using Santolibre.Map.Elevation.Lib.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.Controllers
{
    [RoutePrefix("api/v1/metadata")]
    public class MetadataController : ApiController
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [Route("{srtmType}")]
        [HttpGet]
        public List<SrtmRectangle> Metadata(string srtmType)
        {
            List<SrtmRectangle> srtmRectangles = new List<SrtmRectangle>();
            if (srtmType == "srtm1")
                srtmRectangles = _metadataService.GetSRTM1Rectangles();
            else if (srtmType == "srtm3")
                srtmRectangles = _metadataService.GetSRTM3Rectangles();
            return srtmRectangles;
        }
    }
}
