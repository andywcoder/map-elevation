namespace Santolibre.Map.Elevation.WebService.Controllers.v1.Models
{
    public class SrtmRectangle
    {
        public string FileFormat { get; set; }
        public string Resolution { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}