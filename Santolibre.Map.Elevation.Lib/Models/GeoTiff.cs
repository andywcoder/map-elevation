using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace Santolibre.Map.Elevation.Lib.Models
{
    public class GeoTiff
    {
        private readonly short[] _geoTiffData;

        public GeoTiff(short[] getTiffData)
        {
            _geoTiffData = getTiffData;
        }

        public static string GetFilename(double lat, double lon)
        {
            var latIndex = (int)((60 - lat) / 5) + 1;
            var lonIndex = (int)(lon / 5) + 37;
            if (lon < 0)
                lonIndex--;
            return "srtm_" + lonIndex.ToString("00") + "_" + latIndex.ToString("00") + ".tif";
        }

        public static GeoTiff Create(Stream stream)
        {
            using (var inputImage = Tiff.ClientOpen("geotiff", "r", stream, new TiffStream()))
            {
                var width = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var height = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                var bitsPerSample = inputImage.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                if (inputImage.IsBigEndian())
                {
                    throw new Exception("Tiff has to be little-endian format");
                }
                if (bitsPerSample != 16)
                {
                    throw new Exception("Tiff pixel format has to be 16 bit grayscale");
                }
                var bytes = new byte[width * height * bitsPerSample / 8];
                var offset = 0;
                for (int i = 0; i < inputImage.NumberOfStrips(); i++)
                {
                    offset += inputImage.ReadRawStrip(i, bytes, offset, (int)inputImage.RawStripSize(i));
                }
                var geoTiffData = new short[bytes.Length / 2];
                for (var i = 0; i < geoTiffData.Length; i++)
                {
                    geoTiffData[i] = (short)(ushort)(bytes[i * 2] + (ushort)(bytes[i * 2 + 1] << 8));
                }
                return new GeoTiff(geoTiffData);
            }
        }

        public int GetElevation(double latitude, double longitude)
        {
            double minLon = (int)longitude - (int)longitude % 5;
            double maxLat = (int)latitude - (int)latitude % 5 + 5;
            if (latitude < 0)
                maxLat -= 5;

            var x = (int)Math.Round((longitude - (5.0 / 6000) - minLon) / (5.0 / 6000));
            var y = (int)Math.Round((maxLat - latitude) / (5.0 / 6000));

            var elevation = _geoTiffData[y * 6000 + x];

            return elevation < -1000 ? 0 : elevation;
        }
    }
}
