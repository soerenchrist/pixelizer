using System.Collections;
using System.Collections.Generic;
using Pixelizer.Services.Strategies;

namespace Pixelizer.Services
{
    public class PixelsEnumerable : IEnumerable<(double, double)>
    {
        private readonly IPixelOrderStrategy _pixelOrderStrategy;

        public PixelsEnumerable(IPixelOrderStrategy pixelOrderStrategy)
        {
            _pixelOrderStrategy = pixelOrderStrategy;
        }
        
        public IEnumerator<(double, double)> GetEnumerator()
        {
            return _pixelOrderStrategy;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}