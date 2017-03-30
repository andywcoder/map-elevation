using System;

namespace Santolibre.Map.Elevation.Lib.Models
{
    public class Node
    {
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Elevation { get; set; }
        public float Distance { get; set; }

        public float GetDistanceToNode(Node node)
        {
            var R = 6371;
            var dLat = (node.Latitude - Latitude).ToRad();
            var dLon = (node.Longitude - Longitude).ToRad();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Latitude.ToRad()) * Math.Cos(node.Latitude.ToRad()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return (float)d;
        }
    }
}
