using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;

namespace Pixelizer.Services.Strategies
{
    public class KdTreeOrderStrategy : IPixelOrderStrategy 
    {
        
        public List<(double, double)> GetPixelOrder(List<(double, double)> unorderedList)
        {
            if (unorderedList.Count == 0)
                return unorderedList;
            var tree = new KdTree<double, double>(2, new DoubleMath());
            foreach (var pixel in unorderedList.Skip(1))
            {
                tree.Add(new[] { pixel.Item1, pixel.Item2 }, 1);
            }


            var current = new[]{unorderedList[0].Item1, unorderedList[0].Item2};
            var results = new List<(double, double)>();
            while (true)
            {
                var nearestNeighbours = tree.GetNearestNeighbours(current, 1);
                if (nearestNeighbours == null || nearestNeighbours.Length == 0)
                    return results;

                var nearest = nearestNeighbours[0];
                results.Add((nearest.Point[0], nearest.Point[1]));
                
                current = nearest.Point;
                tree.RemoveAt(current);
            }
        }
    }
}