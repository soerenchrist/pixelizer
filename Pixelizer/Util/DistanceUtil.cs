using System;
using System.Collections.Generic;

namespace Pixelizer.Util
{
    public static class DistanceUtil
    {
        public static double DistanceTo(this (double, double) pixel, (double, double) other)
        {
            var (x, y) = pixel;
            var (otherX, otherY) = other;
            return Math.Sqrt(
                Math.Pow(x - otherX, 2) + Math.Pow(y - otherY, 2)
            );
        }
        
        
        public static double CalculateDistance(this List<(double, double)> pixels)
        {
            var distance = 0.0;
            for (int i = 1; i < pixels.Count; i++)
            {
                var prev = pixels[i-1];
                var current = pixels[i];
                distance += prev.DistanceTo(current);
            }

            return distance;
        }
    }
}