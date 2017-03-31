using AutoMapper;

namespace Santolibre.Map.Elevation.WebService
{
    public static class AutoMapper
    {
        public static IMapper CreateMapper()
        {
            IMapper mapper = null;

            var config = new MapperConfiguration(x =>
            {
                x.CreateMap<Elevation.Lib.Models.SrtmRectangle, Elevation.WebService.Controllers.v1.Models.SrtmRectangle>();
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
            return mapper;
        }
    }
}
