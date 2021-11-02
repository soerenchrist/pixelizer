using System.Collections.Generic;

namespace Pixelizer.Services.Strategies
{
    public interface IPixelOrderStrategy
    {
        List<(double, double)> GetPixelOrder(List<(double, double)> unorderedList);
    }
}