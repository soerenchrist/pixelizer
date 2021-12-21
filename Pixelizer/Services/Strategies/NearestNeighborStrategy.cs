using Pixelizer.Util;

namespace Pixelizer.Services.Strategies;

public class NearestNeighborStrategy : IPixelOrderStrategy
{
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

    private (double, double)? GetNearest((double, double) current, List<(double, double)> pixels)
    {
        if (pixels.Count == 0)
            return null;
        var currentMin = double.MaxValue;
        var minIndex = -1;
        for (var i = 0; i < pixels.Count; i++)
        {
            var distance = current.DistanceTo(pixels[i]);
            if (distance < currentMin)
            {
                minIndex = i;
                currentMin = distance;
            }
        }

        return pixels[minIndex];
    }
}