using System.Drawing;
using Pixelizer.Models;

namespace Pixelizer.Services.Image.Abstractions;

public interface IPixelizerService
{
    Bitmap? ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token);
}