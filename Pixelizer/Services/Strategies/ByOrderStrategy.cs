using System.Collections;
using System.Collections.Generic;

namespace Pixelizer.Services.Strategies
{
    public class ByOrderStrategy : IPixelOrderStrategy
    {
        private readonly List<(double, double)> _pixels;
        private int _currentIndex;

        public ByOrderStrategy(List<(double, double)> pixels)
        {
            _pixels = pixels;
        }
        
        public bool MoveNext()
        {
            if (_currentIndex + 1 == _pixels.Count)
            {
                return false;
            }

            _currentIndex++;
            return true;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public (double, double) Current => _pixels[_currentIndex];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}