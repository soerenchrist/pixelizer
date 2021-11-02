using System;
using System.Collections.Generic;
using System.Drawing;
using Pixelizer.Models;

namespace Pixelizer.Util
{
    public static class ImageUtil
    {
        public static List<(double, double)> GetPixels(this Bitmap image, GcodeConfig config)
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