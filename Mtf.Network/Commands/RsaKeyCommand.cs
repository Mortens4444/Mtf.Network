using Mtf.Cryptography.AsymmetricCiphers;
using Mtf.Cryptography.Converters;
using Mtf.Network.Interfaces;
using System;
using System.Net.Sockets;

namespace Mtf.Network.Commands
{
    public class RsaKeyCommand : ICommand
    {
        public bool CanHandle(string message) => message?.StartsWith(RsaCipher.RsaKeyHeader, StringComparison.InvariantCulture) ?? false;

        public void Execute(string message, Socket client, IServer server)
        {
            var base64 = message.Substring(RsaCipher.RsaKeyHeader.Length).Trim();
            var keyBytes = Convert.FromBase64String(base64);
            var rsaParams = RsaParametersConverter.ToRSAParameters(keyBytes);
            server.ClientPublicKeys[client] = rsaParams;

            server.SendAsymmetricCiphersPublicKeys();
        }
    }
}
