using Santolibre.Map.Elevation.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public class ElevationService : IElevationService
    {
        private readonly ICacheService _cacheService;
        private readonly IConfigurationService _configurationService;

        public ElevationService(ICacheService cacheService, IConfigurationService configurationService)
        {
            _cacheService = cacheService;
            _configurationService = configurationService;
        }

        private void GetHGTValue(Node node, Dictionary<string, object> cache)
        {
            int latAdj;
            int lonAdj;
            var filename = GetHGTFilename(node.Latitude, node.Longitude, out latAdj, out lonAdj);
            var hgtData = (byte[])cache[filename];

            switch (hgtData.Length)
            {
                case HGT.HGT1201:
                    GetHGTValue(node, hgtData, latAdj, lonAdj, 1200, 2402);
                    break;
                case HGT.HGT3601:
                    GetHGTValue(node, hgtData, latAdj, lonAdj, 3600, 7202);
                    break;
            }
        }

        private void GetHGTValue(Node node, byte[] hgtData, int latAdj, int lonAdj, int width, int stride)
        {
            double y = node.Latitude;
            double x = node.Longitude;
            var offset = ((int)((x - (int)x + lonAdj) * width) * 2 + (width - (int)((y - (int)y + latAdj) * width)) * stride);
            var h1 = hgtData[offset + 1] + hgtData[offset + 0] * 256;
            var h2 = hgtData[offset + 3] + hgtData[offset + 2] * 256;
            var h3 = hgtData[offset - stride + 1] + hgtData[offset - stride + 0] * 256;
            var h4 = hgtData[offset - stride + 3] + hgtData[offset - stride + 2] * 256;

            var m = Math.Max(h1, Math.Max(h2, Math.Max(h3, h4)));
            if (h1 == -32768)
                h1 = m;
            if (h2 == -32768)
                h2 = m;
            if (h3 == -32768)
                h3 = m;
            if (h4 == -32768)
                h4 = m;

            var fx = node.Longitude - (int)(node.Longitude);
            var fy = node.Latitude - (int)(node.Latitude);

            var elevation = (int)Math.Round((h1 * (1 - fx) + h2 * fx) * (1 - fy) + (h3 * (1 - fx) + h4 * fx) * fy);
            
			node.Elevation = elevation < -1000 ? 0 : elevation;
        }

        private void GetGeoTiffValue(Node node, Dictionary<string, object> cache)
        {
            double minLon = (int)node.Longitude - (int)node.Longitude % 5;
            double maxLat = (int)node.Latitude - (int)node.Latitude % 5 + 5;
            if (node.Latitude < 0)
                maxLat -= 5;
            var filename = GetGeoTiffFilename(node.Latitude, node.Longitude);
            var pixels = (Int16[])cache[filename];

            var x = (int)Math.Round((node.Longitude - (5.0 / 6000) - minLon) / (5.0 / 6000));
            var y = (int)Math.Round((maxLat - node.Latitude) / (5.0 / 6000));

            node.Elevation = node.Elevation < -1000 ? 0 : pixels[y * 6000 + x];
        }

        private string GetHGTFilename(double lat, double lon, out int latAdj, out int lonAdj)
        {
            char latDir;
            char lonDir;

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

        private string GetGeoTiffFilename(double lat, double lon)
        {
            var latIndex = (int)((60 - lat) / 5) + 1;
            var lonIndex = (int)(lon / 5) + 37;
            if (lon < 0)
                lonIndex--;
            return "srtm_" + lonIndex.ToString("00") + "_" + latIndex.ToString("00") + ".tif";
        }

        private void WindowSmooth(List<Node> nodes, float[] smoothingFilter)
        {
            for (var i = smoothingFilter.Length / 2; i < nodes.Count - smoothingFilter.Length / 2; i++)
            {
                float elevationSum = 0;
                for (var j = -smoothingFilter.Length / 2; j <= smoothingFilter.Length / 2; j++)
                {
                    elevationSum += smoothingFilter[j + smoothingFilter.Length / 2] * nodes[i - j].Elevation;
                }
                nodes[i].Elevation = elevationSum / smoothingFilter.Sum();
            }
        }

        private void FeedbackSmooth(List<Node> nodes, float feedbackWeight, float currentWeight)
        {
            var filteredValue = nodes[0].Elevation;
            for (var i = 0; i < nodes.Count; i++)
            {
                filteredValue = (filteredValue * feedbackWeight + nodes[i].Elevation * currentWeight) / (feedbackWeight + currentWeight);
                nodes[i].Elevation = filteredValue;
            }
            filteredValue = nodes[nodes.Count - 1].Elevation;
            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                filteredValue = (filteredValue * feedbackWeight + nodes[i].Elevation * currentWeight) / (feedbackWeight + currentWeight);
                nodes[i].Elevation = filteredValue;
            }
        }

        private DigitalElevationModelType? GetElevations(List<Node> nodes)
        {
            var dataPath = _configurationService.GetValue("DemDataFolder");

            Dictionary<string, object> cache;
            if (_cacheService.GetValue("SRTM") != null)
            {
                cache = (Dictionary<string, object>)_cacheService.GetValue("SRTM");
                if (cache.Count > 10)
                {
                    var removeKeys = cache.Keys.ToList().Take(cache.Count - 10).ToList();
                    removeKeys.ForEach(x => cache.Remove(x));
                }
            }
            else
            {
                cache = new Dictionary<string, object>();
                _cacheService.Add("SRTM", cache, DateTimeOffset.UtcNow + new TimeSpan(1, 0, 0));
            }
            var areFilesAvailable = true;

            // Check HGT files
            foreach (var node in nodes)
            {
                int latAdj;
                int lonAdj;
                var filename = GetHGTFilename(node.Latitude, node.Longitude, out latAdj, out lonAdj);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream hgtStream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var bytes = new byte[hgtStream.Length];
                            hgtStream.Read(bytes, 0, Convert.ToInt32(hgtStream.Length));
                            hgtStream.Close();
                            cache.Add(filename, bytes);
                        }
                    }
                }
                else
                {
                    areFilesAvailable = false;
                }
            }
            if (areFilesAvailable)
            {
                foreach (var node in nodes)
                {
                    GetHGTValue(node, cache);
                }
                return DigitalElevationModelType.SRTM1;
            }

            // Check geotiff files
            areFilesAvailable = true;
            foreach (var node in nodes)
            {
                var filename = GetGeoTiffFilename(node.Latitude, node.Longitude);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream tiffStream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var tiffDecoder = new TiffBitmapDecoder(tiffStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                            if (tiffDecoder.Frames.Count == 1)
                            {
                                var tiffFrameCopy = tiffDecoder.Frames[0];
                                var pixels = new Int16[tiffFrameCopy.PixelWidth * tiffFrameCopy.PixelHeight];
                                tiffFrameCopy.CopyPixels(pixels, tiffFrameCopy.PixelWidth * 2, 0);
                                cache.Add(filename, pixels);
                            }
                        }
                    }
                }
                else
                {
                    areFilesAvailable = false;
                }
            }
            if (areFilesAvailable)
            {
                foreach (var node in nodes)
                {
                    GetGeoTiffValue(node, cache);
                }
                return DigitalElevationModelType.SRTM3;
            }

            return null;
        }

        public DigitalElevationModelType? GetElevations(List<Node> nodes, SmoothingMode smoothingMode, int maxNodes)
        {
            nodes = nodes.Take(maxNodes).ToList();

            var demType = GetElevations(nodes);
            if (demType.HasValue)
            {
                if (demType.Value == DigitalElevationModelType.SRTM1)
                {
                    switch (smoothingMode)
                    {
                        case SmoothingMode.WindowSmooth:
                            WindowSmooth(nodes, new float[] { 0.1f, 1f, 0.1f });
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            FeedbackSmooth(nodes, 1, 3);
                            break;
                    }
                }
                else
                {
                    switch (smoothingMode)
                    {
                        case SmoothingMode.WindowSmooth:
                            WindowSmooth(nodes, new float[] { 1, 2, 3, 2, 1 });
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            FeedbackSmooth(nodes, 3, 1);
                            break;
                    }
                }

                var totalDistance = 0f;
                for (var i = 1; i < nodes.Count; i++)
                {
                    var distance = nodes[i - 1].GetDistanceToNode(nodes[i]);
                    totalDistance += distance;
                    nodes[i].Distance = totalDistance;
                }

                return demType.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
