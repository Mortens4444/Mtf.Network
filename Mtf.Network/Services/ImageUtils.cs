using Mtf.Network.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Mtf.Network.Services
{
    public static class ImageUtils
    {
        public static Image ByteArrayToImage(byte[] imageArray)
        {
            return imageArray == null ? null : Image.FromStream(new MemoryStream(imageArray));
        }

        public static byte[] GetScreenAreaInByteArray(Rectangle rectangle)
        {
            var bitmap = GetScreenAreaBitmap(rectangle, PixelFormat.Format16bppRgb565);
            return ImageToByteArray(bitmap);
        }

        public static Bitmap GetScreenAreaBitmap(Rectangle rectangle, PixelFormat pixelFormat)
        {
            using (var bmp = new Bitmap(rectangle.Width, rectangle.Height))
            {
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, bmp.Size);
                    var cursorInfo = new CURSORINFO();
                    cursorInfo.Initialize();
                    WinAPI.GetCursorInfo(ref cursorInfo);
                    if (cursorInfo.hCursor != IntPtr.Zero)
                    {
                        var cursorPosition = new Point(cursorInfo.ptScreenPos.X - rectangle.X, cursorInfo.ptScreenPos.Y - rectangle.Y);
                        var hdc = graphics.GetHdc();
                        WinAPI.DrawIcon(hdc, cursorPosition.X, cursorPosition.Y, cursorInfo.hCursor);
                        graphics.ReleaseHdc(hdc);
                    }
                }
                return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), pixelFormat);
            }
        }

        public static byte[] ImageToByteArray(Image image)
        {
            return ImageToByteArray(image, ImageFormat.Png);
        }

        public static byte[] ImageToByteArray(Image image, ImageFormat format)
        {
            if (image == null)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            image.Save(memoryStream, format);
            return memoryStream.ToArray();
        }
    }
}
