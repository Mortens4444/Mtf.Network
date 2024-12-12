using System;
using System.Net.Sockets;

namespace Mtf.Network
{
    public class Pop3Client : Client
    {
        public Pop3Client(string serverHost, ushort listenerPort = 110)
            : base(serverHost, listenerPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }

        public void User(string user) => Send($"USER {user}\r\n");

        public void Pass(string password) => Send($"PASS {password}\r\n");

        public void List(string message = null) => Send(String.IsNullOrEmpty(message) ? "LIST" : $"LIST {message}\r\n");

        public void Retrieve(string message) => Send($"RETR {message}\r\n");

        public void Delete(string message) => Send($"DELE {message}\r\n");

        public void Apop(string name, string digest) => Send($"APOP {name} {digest}\r\n");

        public void Uidl(string message = null) => Send(String.IsNullOrEmpty(message) ? "UIDL" : $"UIDL {message}\r\n");

        public void Top(string message, ushort n) => Send($"TOP {message} {n}\r\n");

        public void GetStatus() => Send("STAT\r\n");

        public void Reset() => Send("RSET\r\n");

        public void NoOperation() => Send("NOOP\r\n");

        public void Quit() => Send("QUIT\r\n");
    }
}
