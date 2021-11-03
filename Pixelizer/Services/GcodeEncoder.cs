using System.Collections.Generic;
using Pixelizer.Models;
using Pixelizer.Services.Strategies;

namespace Pixelizer.Services
{
    public class GcodeEncoder
    {
        private readonly IPixelOrderStrategy _pixelOrderStrategy;

        public GcodeEncoder(IPixelOrderStrategy pixelOrderStrategy)
        {
            _pixelOrderStrategy = pixelOrderStrategy;
        }
        
        public string ConvertImageToGcode(List<(double, double)> pixels, int width, int height, GcodeConfig config)
        {
            var builder = new GcodeBuilder();
            // Perform homing
            if (config.AutoHome)
            {
                builder.Home();
            }

            builder.SetZPosition(config.ZAxisUp);
            if (config.DrawFrame)
            {
                double totalWidth = width * config.PenWidth;
                double totalHeight = height * config.PenWidth;
                
                builder.MoveTo(config.OffsetX, config.OffsetY, config.FeedRate);
                builder.SetZPosition(config.ZAxisDown);
                builder.MoveTo(totalWidth + config.OffsetX, config.OffsetY, config.FeedRate);
                builder.MoveTo(totalWidth + config.OffsetX, totalHeight + config.OffsetY, config.FeedRate);
                builder.MoveTo(config.OffsetX, totalHeight + config.OffsetY, config.FeedRate);
                builder.MoveTo(config.OffsetX, config.OffsetY, config.FeedRate);
            }
            builder.SetZPosition(config.ZAxisUp);

            var orderedPixels = _pixelOrderStrategy.GetPixelOrder(pixels);
            foreach (var dot in orderedPixels)
            {
                // Move to next dot position
                builder.MoveTo(dot.Item1, dot.Item2, config.FeedRate);
                // Draw a dot
                builder.SetZPosition(config.ZAxisDown);
                // pull pen up
                builder.SetZPosition(config.ZAxisUp);
            }

            return builder.Build();
        }

        
    }
}