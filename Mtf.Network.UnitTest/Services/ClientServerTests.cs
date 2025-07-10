using Mtf.Cryptography.AsymmetricCiphers;
using Mtf.Cryptography.Interfaces;
using Mtf.Cryptography.KeyGenerators;
using Mtf.Cryptography.SymmetricCiphers;
using Mtf.Extensions;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mtf.Network.UnitTest.Services
{
    [TestFixture]
    public class ClientServerTests
    {
        private const bool IncludePrivateParameters = true;
        private const bool UseOaepPadding = true;

        [TestCase(null, null, null, null, TestName = "NoEncryption")]
        [TestCase(typeof(CaesarCipher), new object[] { 1 }, typeof(CaesarCipher), new object[] { 1 }, TestName = "CaesarEncryption")]
        [TestCase(typeof(RsaCipher), new object[] { "serverFullKey.xml", IncludePrivateParameters, UseOaepPadding }, typeof(RsaCipher), new object[] { "serverFullKey.xml", IncludePrivateParameters, UseOaepPadding }, TestName = "SameRsaKey")]
        [TestCase(typeof(RsaCipher), new object[] { "serverFullKey.xml", IncludePrivateParameters, UseOaepPadding }, typeof(RsaCipher), new object[] { "client1FullKey.xml", IncludePrivateParameters, UseOaepPadding }, TestName = "MixedEncryption")]
        public void ClientServerSendReceiveTest(Type serverCipherType, object[] serverArgs, Type clientCipherType, object[] clientArgs)
        {
            foreach (var privateKey in new[] { "serverFullKey.xml", "client1FullKey.xml" })
            {
                if (!File.Exists(privateKey))
                {
                    var publicKey = "public_" + privateKey;
                    RsaKeyGenerator.GenerateKeyFiles(privateKey, publicKey);
                }
            }

            var messageId = 0;
            var serverReceivedFirst = new TaskCompletionSource<bool>();
            var serverReceivedSecond = new TaskCompletionSource<bool>();
            var client1Received = new TaskCompletionSource<bool>();
            var client2Received = new TaskCompletionSource<bool>();

            var serverCiphers = CreateCiphers(serverCipherType, serverArgs);
            var server = new Server(ipAddress: IPAddress.Parse("192.168.0.58"), ciphers: serverCiphers);
            Client client1 = null;
            Client client2 = null;

            server.ErrorOccurred += (s, e) =>
            {
                client1Received.SetResult(true);
                client2Received.SetResult(true);
                serverReceivedSecond.SetResult(true);
                serverReceivedFirst.SetResult(true);
                Assert.Fail($"Server error: {e.Exception.Message}");
            };
            server.DataArrived += (sender, e) =>
            {
                lock (server)
                {
                    if (messageId == 0)
                    {
                        Assert.That(e.Data, Is.EqualTo(new byte[] { 65, 66, 67 }));
                        server.SendMessageToClient(server.ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client1.ListenerPortOfClient).Key, "A");
                        serverReceivedFirst.SetResult(true);
                        messageId++;
                    }
                    else if (messageId == 1)
                    {
                        Assert.That(e.Data, Is.EqualTo(new byte[] { 67, 66, 65 }));
                        server.SendMessageToClient(server.ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client2.ListenerPortOfClient).Key, "B");
                        serverReceivedSecond.SetResult(true);
                        messageId++;
                    }
                }
            };
            server.Start();

            client1 = new Client(server, ciphers: CreateCiphers(clientCipherType, clientArgs));
            client2 = new Client(server, ciphers: CreateCiphers(clientCipherType, clientArgs));

            client1.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 65 }));
                client1Received.SetResult(true);
            };
            client2.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 66 }));
                client2Received.SetResult(true);
            };

            client1.Connect();
            client2.Connect();

            SendData(client1, "ABC");
            Task.WaitAll(serverReceivedFirst.Task);
            SendData(client2, "CBA");
            Task.WaitAll(serverReceivedSecond.Task, client1Received.Task, client2Received.Task);

            server.Dispose();
            client1.Dispose();
            client2.Dispose();

            Assert.That(messageId, Is.EqualTo(2));
        }

        private static ICipher[] CreateCiphers(Type type, object[] args)
        {
            if (type == null)
            {
                return Array.Empty<ICipher>();
            }

            if (args == null)
            {
                return new[] { (ICipher)Activator.CreateInstance(type) };
            }

            return new[] { (ICipher)Activator.CreateInstance(type, args) };
        }

        private static void SendData(Communicator communicator, string data)
        {
            if (!communicator.Send(data))
            {
                throw new InvalidOperationException($"Failed to send data from {communicator}");
            }
        }
    }
}
