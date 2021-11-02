using System;

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
    }
}