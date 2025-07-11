using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mtf.Network.Interfaces
{
    public interface ISocketSender
    {
        bool Send(Socket socket, byte[] data, bool appendNewLine = false);

        Task<bool> SendAsync(Socket socket, byte[] data, bool appendNewLine = false);
    }
}
