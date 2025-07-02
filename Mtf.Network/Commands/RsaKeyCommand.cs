using Mtf.Cryptography.Converters;
using Mtf.Cryptography.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Mtf.Network.Commands
{
    public class RsaKeyCommand
    {
        public void Execute(string message, Socket client, ICommunicator communicator)
        {
            try
            {
                var base64 = message.Substring("RSA key:".Length).Trim();
                var keyBytes = Convert.FromBase64String(base64);
                var rsaParams = RsaParametersConverter.ToRSAParameters(keyBytes);
                server.ClientPublicKeys[client] = rsaParams;

                communicator.SendAsymmetricCiphersPublicKeys();

                Console.WriteLine($"RSA kulcs fogadva a klienstől: {client.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("RSA kulcs feldolgozási hiba: " + ex.Message);
            }
        }
    }
}
