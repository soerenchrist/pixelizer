using System.Globalization;

namespace Pixelizer.Util
{
    public static class StringUtils
    {
        public static int ParseOrDefault(this string value, int defaultValue)
        {
            if (int.TryParse(value, out var numValue))
            {
                return numValue;
            }

            return defaultValue;
        }
        public static double ParseOrDefault(this string value, double defaultValue)
        {
            if (double.TryParse(value, out var numValue))
            {
                return numValue;
            }

            return defaultValue;
        }

        public static string ToFormatted(this double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }
    }
}