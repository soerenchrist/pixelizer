using System.Drawing;
using gs;
using Sutro.PathWorks.Plugins.API;

namespace Pixelizer.Services
{
    public class GcodeEncoder
    {
        public string ToGcode(Bitmap image)
        {
            Color c;

            var file = new GCodeFile();
            var accumulator = new GCodeFileAccumulator(file);
            var builder = new GCodeBuilder(accumulator);
            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                
                
            }

            return string.Empty;
        }
    }
}