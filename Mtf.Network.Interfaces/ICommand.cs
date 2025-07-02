using System.Net.Sockets;

namespace Mtf.Network.Interfaces
{
    public interface ICommand
    {
        bool CanHandle(string message);

        void Execute(string message, Socket client, IServer server);
    }
}
