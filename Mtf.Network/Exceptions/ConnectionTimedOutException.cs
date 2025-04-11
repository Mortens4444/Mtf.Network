using System;

namespace Mtf.Network.Exceptions
{
    public class ConnectionTimedOutException : Exception
    {
        public ConnectionTimedOutException(string serverAddress, uint port, string localEndPoint) :
            base($"Connection timed out from {localEndPoint} to: {serverAddress}:{port}\r\nCheck the connection timeout, ensure the service is running on the remote machine, verify that you are using the correct username and password, and confirm the firewall settings are correct.")
        { }
    }
}
