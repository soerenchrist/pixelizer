using System.Drawing;
using Pixelizer.Models;

namespace Pixelizer.Services.Image.Abstractions
{
    public interface IPixelizerService
    {
        void ConvertToPixelImage(Bitmap bmp, ColorMode colorMode, int threshold);
    }
}