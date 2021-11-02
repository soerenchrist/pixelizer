using System;
using System.Drawing;
using System.Threading;
using Pixelizer.Models;
using Pixelizer.Services.Image.Abstractions;
using Pixelizer.Util;

namespace Pixelizer.Services.Image
{
    /// <summary>
    /// Check a field of brushSize pixels around the current pixel. The average of the field is the threshold
    /// Bigger than threshold = black, smaller than threshold = white
    /// </summary>
    public class AdaptiveThresholdingPixelizerService : IPixelizerService
    {
        private readonly int _brushSize;
        private readonly int _offset;

        public AdaptiveThresholdingPixelizerService(int brushSize, int offset)
        {
            _brushSize = brushSize;
            _offset = offset;
        }
        
        public Bitmap? ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token)
        {
            var imgArray = new bool[bmp.Height, bmp.Width];
            for (var y = 0; y < bmp.Height; y++)
            for (var x = 0; x < bmp.Width; x++)
            {
                if (token.IsCancellationRequested)
                    return null;
                var average = GetFrameAverage(bmp, x, y, colorMode, token);
                var pixelAverage = GetPixelAverage(bmp, x, y, colorMode);

                imgArray[y, x] = pixelAverage < average - _offset;
            }

            var newBitmap = new Bitmap(bmp.Width, bmp.Height);

            for (var y = 0; y < bmp.Height; y++)
            for (var x = 0; x < bmp.Width; x++)
            {
                if (token.IsCancellationRequested)
                    return null;
                var isBlack = imgArray[y, x];
                
                var selectionColor = colorMode switch
                {
                    ColorMode.Black => Color.Black,
                    ColorMode.Blue => Color.Blue,
                    ColorMode.Red => Color.Red,
                    ColorMode.Green => Color.Green,
                    _ => throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, null)
                };
                newBitmap.SetPixel(x, y, isBlack ? selectionColor : Color.White);
            }

            return newBitmap;
        }

        private double GetPixelAverage(Bitmap bmp, int x, int y, ColorMode colorMode)
        {
            var color = bmp.GetPixel(x, y);
            return color.GetAverageColor(colorMode);
        }

        private double GetFrameAverage(Bitmap bmp, int x, int y, ColorMode colorMode, CancellationToken token)
        {
            var offset = _brushSize / 2;
            var leftX = Math.Max(x - offset, 0);
            var topY = Math.Max(y - offset, 0);
            var rightX = Math.Min(x + offset, bmp.Width - 1);
            var bottomY = Math.Min(y + offset, bmp.Height - 1);

            var average = 0.0;

            int pixelCount = 0;
            for(var currentY = topY; currentY <= bottomY; currentY++)
            for (var currentX = leftX; currentX <= rightX; currentX++)
            {
                if (token.IsCancellationRequested)
                    return 0;
                var color = bmp.GetPixel(currentX, currentY);
                var currentAverage = color.GetAverageColor(colorMode);
                
                average += currentAverage;
                pixelCount++;
            }

            return average / pixelCount;
        }
    }
}