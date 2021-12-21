using System.Drawing;
using Pixelizer.Models;
using Pixelizer.Services.Image.Abstractions;
using Pixelizer.Util;

namespace Pixelizer.Services.Image;

/// <summary>
///     Check each pixel -> bigger than threshold = black, smaller than threshold = white
/// </summary>
public class ThresholdingPixelizerService : IPixelizerService
{
    private readonly int _threshold;

    public ThresholdingPixelizerService(int threshold)
    {
        _threshold = threshold;
    }

    public Bitmap? ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token)
    {
        Color c;
        var newBitmap = new Bitmap(bmp.Width, bmp.Height);
        for (var y = 0; y < bmp.Height; y++)
        for (var x = 0; x < bmp.Width; x++)
        {
            if (token.IsCancellationRequested)
                return null;
            c = bmp.GetPixel(x, y);
            var average = c.GetAverageColor(colorMode);

            var selectionColor = colorMode switch
            {
                ColorMode.Black => Color.Black,
                ColorMode.Blue => Color.Blue,
                ColorMode.Red => Color.Red,
                ColorMode.Green => Color.Green,
                _ => throw new ArgumentOutOfRangeException(nameof(colorMode), colorMode, null)
            };

            if (colorMode == ColorMode.Black)
                newBitmap.SetPixel(x, y, average < _threshold ? selectionColor : Color.White);
            else
                newBitmap.SetPixel(x, y, average > _threshold ? selectionColor : Color.White);
        }

        return newBitmap;
    }
}