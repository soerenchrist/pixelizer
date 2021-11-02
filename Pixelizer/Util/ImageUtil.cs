using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public static Avalonia.Media.Imaging.Bitmap ToAvaloniaBitmap(this Bitmap bitmap)
        {
            var tempFile = Path.GetTempFileName();
            bitmap.Save(tempFile);

            var result = new Avalonia.Media.Imaging.Bitmap(tempFile);
            File.Delete(tempFile);
            return result;
        }

        public static Bitmap ToSystemBitmap(this Avalonia.Media.Imaging.Bitmap bitmap)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream);
            return new Bitmap(memoryStream);
        }

        public static double GetAverageColor(this Color color, ColorMode colorMode)
        {
            if (color.A == 0)
            {
                return colorMode switch
                {
                    ColorMode.Black => 255,
                    _ => 0,
                };
            }
            return colorMode switch
            {
                ColorMode.Black => ((color.R + color.B + color.G) / 3),
                ColorMode.Red => color.R > color.B && color.R > color.G ? color.R : 0,
                ColorMode.Green => color.G > color.B && color.G > color.R ? color.G : 0,
                ColorMode.Blue => color.B > color.R && color.B > color.G ? color.B : 0,
                _ => throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, null)
            };
        }
    }
}