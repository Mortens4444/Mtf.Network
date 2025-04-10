using Mtf.Network.Interfaces;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Mtf.Network.Services
{
    public class ScreenRecorderImageSource : IImageSource
    {
        private readonly Rectangle rectangle;

        public ScreenRecorderImageSource(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        public Task<byte[]> CaptureAsync(CancellationToken token)
        {
            return Task.Run(() => ImageUtils.GetScreenAreaInByteArray(rectangle), token);
        }
    }
}
