using System.Threading.Tasks;
using System.Threading;

namespace Mtf.Network.Interfaces
{
    public interface IImageSource
    {
        Task<byte[]> CaptureAsync(CancellationToken token);
    }
}
