using System.Collections.Generic;
using System.Drawing;
using System.IO;
using gs;
using Pixelizer.Models;
using Pixelizer.Services.Strategies;
using Sutro.PathWorks.Plugins.API;

namespace Pixelizer.Services
{
    public class GcodeEncoder
    {
        public string ToGcode(Bitmap image, GcodeConfig config)
        {
            Color c;

            var dots = new List<(double, double)>();
            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                c = image.GetPixel(x, y);
                if (c.R == 0 && c.G == 0 && c.B == 0)
                {
                    var xCoord = x * config.PenWidth;
                    var yCoord = y * config.PenWidth;
                    dots.Add((xCoord, yCoord));
                }
            }

            var strategy = new NearestNeighborStrategy(dots);

            var file = new GCodeFile();
            var accumulator = new GCodeFileAccumulator(file);
            var builder = new GCodeBuilder(accumulator);
            // perform homing
            builder.AddExplicitLine("G28");
            // set to absolute positioning
            builder.AddExplicitLine("G90");
            foreach (var dot in new PixelsEnumerable(strategy))
            {
                // Move to next dot position
                builder.AddExplicitLine($"G1 X{dot.Item1} Y{dot.Item2} F{config.FeedRate}");
                // Draw a dot
                builder.AddExplicitLine($"G1 Z{config.ZAxisDown}");
                // pull pen up
                builder.AddExplicitLine($"G1 Z{config.ZAxisUp}");
            }

            var writer = new StandardGCodeWriter();
            using var fileWriter = File.OpenWrite(@"C:\Users\chris\Downloads\result.gcode");
            using var streamWriter = new StreamWriter(fileWriter);
            writer.WriteFile(file, streamWriter);

            return "";
        }
    }
}