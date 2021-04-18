using System;
using System.Drawing;
using System.Security.Cryptography;
using Pixelizer.Models;

namespace Pixelizer.Util
{
    public static class ImageUtil
    {
        public static void ToPixelImage(this Bitmap bmp, ColorMode colorMode, int threshold)
        {
            Color c;

            for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                c = bmp.GetPixel(x, y);
                int average;
                switch (colorMode)
                {
                    case ColorMode.Black:
                        average = ((c.R + c.B + c.G) / 3);
                        break;
                    case ColorMode.Red:
                        average = c.R > c.B && c.R > c.G ? c.R : 0;
                        break;
                    case ColorMode.Green:
                        average = c.G > c.B && c.G > c.R ? c.G : 0;
                        break;
                    case ColorMode.Blue:
                        average = c.B > c.R && c.B > c.G ? c.B : 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, null);
                }

                Color selectionColor = colorMode switch
                {
                    ColorMode.Black => Color.Black,
                    ColorMode.Blue => Color.Blue,
                    ColorMode.Red => Color.Red,
                    ColorMode.Green => Color.Green,
                    _ => throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, null)
                };

                if (colorMode == ColorMode.Black)
                {
                    bmp.SetPixel(x, y, average < threshold ? selectionColor : Color.White);
                }
                else
                {
                    bmp.SetPixel(x, y , average > threshold ? selectionColor : Color.White);
                }

            }
        }
    }
}