using AutoMapper;
using Santolibre.Map.Elevation.Lib.Services;
using Santolibre.Map.Elevation.WebService.Controllers.v1.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace Santolibre.Map.Elevation.WebService.Controllers.v1
{
    [RoutePrefix("api/v1")]
    public class MetadataController : ApiController
    {
        private readonly IMetadataService _metadataService;
        private readonly IMapper _mapper;

        public MetadataController(IMetadataService metadataService, IMapper mapper)
        {
            _metadataService = metadataService;
            _mapper = mapper;
        }

        [Route("metadata")]
        [HttpGet]
        public List<SrtmRectangle> Metadata()
        {
            var srtmRectangles = new List<SrtmRectangle>();
            srtmRectangles.AddRange(_mapper.Map<List<SrtmRectangle>>(_metadataService.GetSRTM1Rectangles()));
            srtmRectangles.AddRange(_mapper.Map<List<SrtmRectangle>>(_metadataService.GetSRTM3Rectangles()));
            return srtmRectangles;
        }
    }
}
