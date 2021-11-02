using System;
using System.Drawing;
using System.Threading;
using Pixelizer.Models;
using Pixelizer.Services.Image.Abstractions;

namespace Pixelizer.Services.Image
{
    /// <summary>
    /// Check each pixel -> bigger than threshold = black, smaller than threshold = white
    /// </summary>
    public class ThresholdingPixelizerService : IPixelizerService
    {
        private readonly int _threshold;

        public ThresholdingPixelizerService(int threshold)
        {
            _threshold = threshold;
        }
        
        public void ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token)
        {
            Color c;

            for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                if (token.IsCancellationRequested)
                    return;
                c = bmp.GetPixel(x, y);
                int average;
                if (c.A == 0)
                {
                    average = colorMode switch
                    {
                        ColorMode.Black => 255,
                        _ => 0,
                    };
                }
                else
                {
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
                    bmp.SetPixel(x, y, average < _threshold ? selectionColor : Color.White);
                }
                else
                {
                    bmp.SetPixel(x, y , average > _threshold ? selectionColor : Color.White);
                }

            }
        }
    }
}