using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixelizer.Services.Strategies
{
    public class NearestNeighborStrategy : IPixelOrderStrategy
    {
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

        public List<(double, double)> GetPixelOrder(List<(double, double)> unorderedList)
        {
            if (unorderedList.Count == 0)
                return unorderedList;
            
            var current = unorderedList.First();
            var remaining = unorderedList.Skip(1).ToList();
            var ordered = new List<(double, double)>();
            ordered.Add(current);

            while (remaining.Count > 0)
            {
                var nearest = GetNearest(current, remaining);
                if (nearest != null)
                {
                    ordered.Add(nearest.Value);
                    remaining.Remove(nearest.Value);
                }
            }

            return ordered;
        }
    }
}