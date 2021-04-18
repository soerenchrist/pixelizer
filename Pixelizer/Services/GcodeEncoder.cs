using System.Collections.Generic;
using System.Drawing;
using Pixelizer.Models;
using Pixelizer.Services.Strategies;

namespace Pixelizer.Services
{
    public class GcodeEncoder
    {
        public string ConvertImageToGcode(Bitmap image, GcodeConfig config)
        {
            var pixels = GetPixels(image, config);

            var strategy = new NearestNeighborStrategy(pixels);

            var builder = new GcodeBuilder();
            // Perform homing
            builder.Home();
            foreach (var dot in new PixelsEnumerable(strategy))
            {
                // Move to next dot position
                builder.MoveTo(dot.Item1, dot.Item2, config.FeedRate);
                // Draw a dot
                builder.SetZPosition(config.ZAxisDown);
                // pull pen up
                builder.SetZPosition(config.ZAxisUp);
            }

            return builder.Build();
        }

        private static List<(double, double)> GetPixels(Bitmap image, GcodeConfig config)
        {
            Color c;

            var dots = new List<(double, double)>();
            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                c = image.GetPixel(x, y);
                if (c.R != 255 || c.G != 255 && c.B != 255)
                {
                    double xDouble = x;
                    double yDouble = y;
                    var xCoord = xDouble * config.PenWidth + config.OffsetX;
                    var yCoord = yDouble * config.PenWidth + config.OffsetY;
                    dots.Add((xCoord, yCoord));
                }
            }

            return dots;
        }
    }
}