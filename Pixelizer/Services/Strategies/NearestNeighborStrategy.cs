using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pixelizer.Services.Strategies
{
    public class NearestNeighborStrategy : IPixelOrderStrategy
    {
        private int _currentIndex;
        private readonly List<(double, double)> _pixels;
        private readonly List<(double, double)> _remaining;
        private readonly (double, double)[] _ordered;

        public NearestNeighborStrategy(List<(double, double)> pixels)
        {
            _pixels = pixels;
            _ordered = new (double, double)[_pixels.Count];
            _remaining = new List<(double, double)>();

            var first = pixels.First();
            _ordered[0] = first;
            _remaining.AddRange(_pixels);
            _remaining.Remove(first);
        }
        
        
        public bool MoveNext()
        {
            if (_currentIndex + 1 == _pixels.Count)
                return false;

            var next = GetNearest(_ordered[_currentIndex], _remaining);
            if (next == null)
            {
                return false;
            }
            _remaining.Remove(next.Value);
            _currentIndex++;
            _ordered[_currentIndex] = next.Value;

            return true;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public (double, double) Current => _ordered[_currentIndex];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
        
        
        private (double, double)? GetNearest((double, double) current, List<(double, double)> pixels)
        {
            if (pixels.Count == 0)
                return null;
            var currentMin = double.MaxValue;
            var minIndex = -1;
            for (int i = 0; i < pixels.Count; i++)
            {
                var distance = GetDistance(current, pixels[i]);
                if (distance < currentMin)
                {
                    minIndex = i;
                    currentMin = distance;
                }
            }

            return pixels[minIndex];
        }

        private double GetDistance((double, double) current, (double, double) other)
        {
            var deltaX = Math.Abs(current.Item1 - other.Item1);
            var deltaY = Math.Abs(current.Item2 - other.Item2);

            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            return distance;
        }

    }
}