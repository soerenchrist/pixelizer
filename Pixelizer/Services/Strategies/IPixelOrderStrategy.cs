using System.Collections.Generic;

namespace Pixelizer.Services.Strategies
{
    public interface IPixelOrderStrategy : IEnumerator<(double, double)>
    {
    }
}