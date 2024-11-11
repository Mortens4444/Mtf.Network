using System;

namespace Mtf.Network.Exceptions
{
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException(string serverAddress, uint port) :
            base($"Connection failed to: {serverAddress}:{port}\r\nCheck the connection timeout, ensure the service is running on the remote machine, verify that you are using the correct username and password, and confirm the firewall settings are correct.")
        { }
    }
}
