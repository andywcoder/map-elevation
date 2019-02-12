using Santolibre.Map.Elevation.Lib.Models;
using System.ComponentModel.DataAnnotations;

namespace Santolibre.Map.Elevation.WebService.ApiControllers.v1.Models
{
    public class ElevationQuery
    {
        [Required]
        public string EncodedPoints { get; set; }

        public SmoothingMode SmoothingMode { get; set; }
    }
}
