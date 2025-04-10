using System.Diagnostics;

namespace Mtf.Network.Interfaces
{
    public interface IProcessResultParser
    {
        void ErrorDataReceived(object sender, DataReceivedEventArgs e);

        void OutputDataReceived(object sender, DataReceivedEventArgs e);
    }
}
