using System.Collections.Generic;

namespace Pixelizer.Services.Strategies
{
    public class ByOrderStrategy : IPixelOrderStrategy
    {
        public List<(double, double)> GetPixelOrder(List<(double, double)> unorderedList)
        {
            return unorderedList;
        }
    }
}