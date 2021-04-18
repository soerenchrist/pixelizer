using System.Drawing;
using System.IO;

namespace Pixelizer.Models
{
    public class ImageInfo
    {
        public string Path { get; }
        public int Width { get; }
        public int Height { get; }

        public ImageInfo(string path, int width, int height)
        {
            Path = path;
            Width = width;
            Height = height;
        }

        public static ImageInfo FromPath(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();
            var bitmap = new Bitmap(File.OpenRead(path));
            return new ImageInfo(path, bitmap.Width, bitmap.Height);
        }
    }
}