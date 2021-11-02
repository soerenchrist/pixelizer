using System.Globalization;

namespace Pixelizer.Util
{
    public static class StringUtils
    {
        public static string ToFormatted(this double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }
    }
}