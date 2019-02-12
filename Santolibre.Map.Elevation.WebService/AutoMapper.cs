using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Elevation.WebService
{
    public static class AutoMapper
    {
        public static IMapper CreateMapper()
        {
            IMapper mapper = null;

            var config = new MapperConfiguration(x =>
            {
                x.CreateMap<Lib.Models.SrtmRectangle, ApiControllers.v1.Models.SrtmRectangle>();
                x.CreateMap<List<Lib.Models.Node>, ApiControllers.v1.Models.ElevationResponse>()
                    .ForMember(dest => dest.RangeHeight, opt => opt.MapFrom(src => src.Select(y => new float[] { (float)Math.Round(y.Distance * 1000), (float)Math.Round(y.Elevation, 1) }).ToList()));
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
            return mapper;
        }
    }
}
