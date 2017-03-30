using System;

namespace Santolibre.Map.Elevation.Lib
{
    public static class FloatExtensions
    {
        public static float ToRad(this float value)
        {
            return value * (float)Math.PI / 180;
        }

        public static double ToRad(this double value)
        {
            return value * (float)Math.PI / 180;
        }

        public static float ToDeg(this float value)
        {
            return value * 180 / (float)Math.PI;
        }

        public static double ToDeg(this double value)
        {
            return value * 180 / (float)Math.PI;
        }
    }
}
