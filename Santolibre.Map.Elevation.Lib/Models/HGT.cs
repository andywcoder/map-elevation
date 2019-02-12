using System;
using System.IO;

namespace Santolibre.Map.Elevation.Lib.Models
{
    public class HGT
    {
        public const int HGT3601 = 25934402;
        public const int HGT1201 = 2884802;
        private readonly byte[] _hgtData;

        public HGT(byte[] hgtData)
        {
            _hgtData = hgtData;
        }

        public static string GetFilename(double lat, double lon)
        {
            char latDir;
            char lonDir;
            int latAdj;
            int lonAdj;

            if (lat < 0)
            {
                latDir = 'S';
                latAdj = 1;
            }
            else
            {
                latDir = 'N';
                latAdj = 0;
            }
            if (lon < 0)
            {
                lonDir = 'W';
                lonAdj = 1;
            }
            else
            {
                lonDir = 'E';
                lonAdj = 0;
            }

            var latString = latDir + ((int)Math.Floor(lat + latAdj)).ToString("00");
            var lonString = lonDir + ((int)Math.Floor(lon + lonAdj)).ToString("000");
            return latString + lonString + ".hgt";
        }

        public static HGT Create(Stream stream)
        {
            var hgtData = new byte[stream.Length];
            stream.Read(hgtData, 0, Convert.ToInt32(stream.Length));
            stream.Close();

            if (hgtData.Length != HGT1201 && hgtData.Length != HGT3601)
            {
                throw new Exception("HGT file has no valid size");
            }

            return new HGT(hgtData);
        }

        public int GetElevation(double latitude, double longitude)
        {
            int latAdj = latitude < 0 ? 1 : 0;
            int lonAdj = longitude < 0 ? 1 : 0;

            switch (_hgtData.Length)
            {
                case HGT1201:
                    return GetElevation(latitude, longitude, latAdj, lonAdj, 1200, 2402);
                default:
                    return GetElevation(latitude, longitude, latAdj, lonAdj, 3600, 7202);
            }
        }

        private int GetElevation(double latitude, double longitude, int latAdj, int lonAdj, int width, int stride)
        {
            double y = latitude;
            double x = longitude;
            var offset = ((int)((x - (int)x + lonAdj) * width) * 2 + (width - (int)((y - (int)y + latAdj) * width)) * stride);
            var h1 = _hgtData[offset + 1] + _hgtData[offset + 0] * 256;
            var h2 = _hgtData[offset + 3] + _hgtData[offset + 2] * 256;
            var h3 = _hgtData[offset - stride + 1] + _hgtData[offset - stride + 0] * 256;
            var h4 = _hgtData[offset - stride + 3] + _hgtData[offset - stride + 2] * 256;

            var m = Math.Max(h1, Math.Max(h2, Math.Max(h3, h4)));
            if (h1 == -32768)
                h1 = m;
            if (h2 == -32768)
                h2 = m;
            if (h3 == -32768)
                h3 = m;
            if (h4 == -32768)
                h4 = m;

            var fx = longitude - (int)(longitude);
            var fy = latitude - (int)(latitude);

            var elevation = (int)Math.Round((h1 * (1 - fx) + h2 * fx) * (1 - fy) + (h3 * (1 - fx) + h4 * fx) * fy);

            return elevation < -1000 ? 0 : elevation;
        }
    }
}
