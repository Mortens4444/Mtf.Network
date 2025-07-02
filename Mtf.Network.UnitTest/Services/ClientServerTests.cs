using Mtf.Cryptography.AsymmetricCiphers;
using Mtf.Cryptography.Interfaces;
using Mtf.Cryptography.SymmetricCiphers;
using Mtf.Extensions;
using Mtf.Network.EventArg;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mtf.Network.UnitTest.Services
{
    [TestFixture]
    public class ClientServerTests
    {
        [Test]
        public void ClientServerSendReceiveTestWithoutEncryption()
        {
            var messageId = 0;
            var serverReceivedFirst = new TaskCompletionSource<bool>();
            var serverReceivedSecond = new TaskCompletionSource<bool>();
            var client1Received = new TaskCompletionSource<bool>();
            var client2Received = new TaskCompletionSource<bool>();

            var server = new Server(ipAddress: IPAddress.Parse("192.168.0.58"));
            Client client1 = null;
            Client client2 = null;

            server.DataArrived += (object sender, DataArrivedEventArgs e) =>
            {
                lock (server)
                {
                    if (messageId == 0)
                    {
                        Assert.That(Enumerable.SequenceEqual(e.Data, new byte[] { 65, 66, 67 }), Is.True);
                        server.SendMessageToClient((((Server)sender).ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client1.ListenerPortOfClient).Key), "A");
                        serverReceivedFirst.SetResult(true);
                        messageId++;
                    }
                    else if (messageId == 1)
                    {
                        Assert.That(Enumerable.SequenceEqual(e.Data, new byte[] { 67, 66, 65 }), Is.True);
                        server.SendMessageToClient(((Server)sender).ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client2.ListenerPortOfClient).Key, "B");
                        serverReceivedSecond.SetResult(true);
                        messageId++;
                    }
                }
            };
            server.Start();

            client1 = new Client(server);
            client2 = new Client(server);

            client1.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 65 }));
                client1Received.SetResult(true);
            };
            client1.Connect();

            client2.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 66 }));
                client2Received.SetResult(true);
            };
            client2.Connect();

            SendData(client1, "ABC");
            SendData(client2, "CBA");

            Task.WaitAll(
                serverReceivedFirst.Task,
                serverReceivedSecond.Task,
                client1Received.Task,
                client2Received.Task
            );

            server.Dispose();
            client1.Dispose();
            client2.Dispose();

            Assert.That(messageId, Is.EqualTo(2));
        }

        [Test]
        public void ClientServerSendReceiveTestWithEncryption()
        {
            var messageId = 0;
            var serverReceivedFirst = new TaskCompletionSource<bool>();
            var serverReceivedSecond = new TaskCompletionSource<bool>();
            var client1Received = new TaskCompletionSource<bool>();
            var client2Received = new TaskCompletionSource<bool>();

            var serverCiphers = new ICipher[] { new CaesarCipher(1), new RsaCipher("serverFullKey.xml") };
            var server = new Server(ipAddress: IPAddress.Parse("192.168.0.58"), ciphers: serverCiphers);
            Client client1 = null;
            Client client2 = null;

            server.DataArrived += (object sender, DataArrivedEventArgs e) =>
            {
                lock (server)
                {
                    if (messageId == 0)
                    {
                        Assert.That(Enumerable.SequenceEqual(e.Data, new byte[] { 65, 66, 67 }), Is.True);
                        server.SendMessageToClient((((Server)sender).ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client1.ListenerPortOfClient).Key), "A");
                        serverReceivedFirst.SetResult(true);
                        messageId++;
                    }
                    else if (messageId == 1)
                    {
                        Assert.That(Enumerable.SequenceEqual(e.Data, new byte[] { 67, 66, 65 }), Is.True);
                        server.SendMessageToClient(((Server)sender).ConnectedClients.First(c => c.Key.RemoteEndPoint.GetPort() == client2.ListenerPortOfClient).Key, "B");
                        serverReceivedSecond.SetResult(true);
                        messageId++;
                    }
                }    
            };
            server.Start();

            var client1Ciphers = new ICipher[] { new CaesarCipher(1)/*, new RsaCipher("client1FullKey.xml")*/ };
            client1 = new Client(server, ciphers: client1Ciphers);
            var client2Ciphers = new ICipher[] { new CaesarCipher(1)/*, new RsaCipher("client2FullKey.xml")*/ };
            client2 = new Client(server, ciphers: client2Ciphers);

            client1.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 65 }));
                client1Received.SetResult(true);
            };
            client1.Connect();

            client2.DataArrived += (s, e) =>
            {
                Assert.That(e.Data, Is.EqualTo(new byte[] { 66 }));
                client2Received.SetResult(true);
            };
            client2.Connect();

            SendData(client1, "ABC");
            SendData(client2, "CBA");

            Task.WaitAll(
                serverReceivedFirst.Task,
                serverReceivedSecond.Task,
                client1Received.Task,
                client2Received.Task
            );

            server.Dispose();
            client1.Dispose();
            client2.Dispose();

            Assert.That(messageId, Is.EqualTo(2));
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
