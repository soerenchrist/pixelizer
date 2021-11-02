using System.Drawing;
using System.Threading;
using Pixelizer.Models;

namespace Pixelizer.Services.Image.Abstractions
{
    public interface IPixelizerService
    {
        Bitmap? ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, CancellationToken token);
    }
}