using Santolibre.Map.Elevation.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                var filename = HGT.GetFilename(node.Latitude, node.Longitude);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream stream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var hgt = HGT.Create(stream);
                            cache.Add(filename, hgt);
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
                    var hgt = (HGT)cache[HGT.GetFilename(node.Latitude, node.Longitude)];
                    node.Elevation = hgt.GetElevation(node.Latitude, node.Longitude);
                }
                return DigitalElevationModelType.SRTM1;
            }

            // Check geotiff files
            areFilesAvailable = true;
            foreach (var node in nodes)
            {
                var filename = GeoTiff.GetFilename(node.Latitude, node.Longitude);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream stream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var hgt = GeoTiff.Create(stream);
                            cache.Add(filename, hgt);
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
                    var geoTiff = (GeoTiff)cache[GeoTiff.GetFilename(node.Latitude, node.Longitude)];
                    node.Elevation = geoTiff.GetElevation(node.Latitude, node.Longitude);
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
