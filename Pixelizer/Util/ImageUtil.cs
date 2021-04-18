using System.Drawing;

namespace Pixelizer.Util
{
    public static class ImageUtil
    {
        public static void ToBlackAndWhite(this Bitmap bmp, int threshold)
        {
            Color c;

            for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                c = bmp.GetPixel(x, y);
                
                int average = ((c.R + c.B + c.G) / 3);
                bmp.SetPixel(x, y, average < threshold ? Color.Black : Color.White);
            }
        }
    }
}