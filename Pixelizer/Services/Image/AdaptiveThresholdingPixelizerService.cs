using System;
using System.Drawing;
using System.Threading;
using Pixelizer.Models;
using Pixelizer.Services.Image.Abstractions;

namespace Pixelizer.Services.Image
{
    public class AdaptiveThresholdingPixelizerService : IPixelizerService
    {
        private readonly int _brushSize;
        private readonly int _offset;

        public AdaptiveThresholdingPixelizerService(int brushSize, int offset)
        {
            _brushSize = brushSize;
            _offset = offset;
        }
        
        public void ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token)
        {
            var imgArray = new bool[bmp.Height, bmp.Width];
            for (var y = 0; y < bmp.Height; y++)
            for (var x = 0; x < bmp.Width; x++)
            {
                if (token.IsCancellationRequested)
                    return;
                var average = GetFrameAverage(bmp, x, y, colorMode, token);
                var pixelAverage = GetPixelAverage(bmp, x, y);

                imgArray[y, x] = pixelAverage < average - _offset;
            }

            for (var y = 0; y < bmp.Height; y++)
            for (var x = 0; x < bmp.Width; x++)
            {
                if (token.IsCancellationRequested)
                    return;
                var isBlack = imgArray[y, x];
                bmp.SetPixel(x, y, isBlack ? Color.Black : Color.White);
            }
        }

        private double GetPixelAverage(Bitmap bmp, int x, int y)
        {
            var color = bmp.GetPixel(x, y);
            int currentAverage;
            if (color.A == 0)
                currentAverage = 255;
            else
            {
                currentAverage = (color.R + color.B + color.G) / 3;
            }

            return currentAverage;
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
                int currentAverage;
                if (color.A == 0)
                    currentAverage = 255;
                else
                {
                    currentAverage = (color.R + color.B + color.G) / 3;
                }

                average += currentAverage;
                pixelCount++;
            }

            return average / pixelCount;
        }
    }
}