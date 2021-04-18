namespace Pixelizer.Models
{
    public class GcodeConfig
    {
        public int ZAxisUp { get; set; }
        public int ZAxisDown { get; set; }
        public double PenWidth { get; set; }
        public int FeedRate { get; set; } = 300;
    }
}