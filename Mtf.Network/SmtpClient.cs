using System;
using System.Net.Sockets;
using System.Text;

namespace Mtf.Network
{
    /// <summary>
    /// RFC 3207 - 
    /// </summary>
    public class SmtpClient : Client
    {
        public int TimeOut { get; set; }

        public SmtpClient(string serverHost, ushort listenerPort = 25)
            : base(serverHost, listenerPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }

        public void Helo(string host) => Send($"HELO {host}\r\n");

        public void Ehlo(string host) => Send($"EHLO {host}\r\n");

        public void Data() => Send("DATA\r\n");

        public void Quit() => Send("QUIT\r\n");

        public void Reset() => Send("RSET\r\n");

        public void StartTls() => Send("STARTTLS\r\n");

        public void NoOperation() => Send("NOOP\r\n");

        public void Turn() => Send("TURN\r\n");

        public void Expanse(string listName) => Send($"EXPN {listName}\r\n");

        public void MailFrom(string senderEmailAddress) => Send($"MAIL FROM: <{senderEmailAddress}>\r\n");

        public void CheckMessageSize(ulong messageSize) => Send($"SIZE={messageSize}\r\n");

        public void Verify(string emailAddress) => Send($"VRFY {emailAddress}\r\n");

        public void SendFrom(string senderEmailAddress) => Send($"SEND FROM: <{senderEmailAddress}>\r\n");

        public void SendOrMailFrom(string senderEmailAddress) => Send($"SOML FROM: <{senderEmailAddress}>\r\n");

        public void SamlFrom(string senderEmailAddress) => Send($"SAML FROM: <{senderEmailAddress}>\r\n");

        public void Authenticate(SmtpAuthenticationMechanism authenticationMechanism, string user, string password)
        {
            switch (authenticationMechanism)
            {
                case SmtpAuthenticationMechanism.Plain:
                    var plainAuthString = $"{user}\0{user}\0{password}";
                    var plainAuthBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(plainAuthString));
                    Send($"AUTH PLAIN {plainAuthBase64}\r\n");
                    break;

                case SmtpAuthenticationMechanism.Login:
                    Send("AUTH LOGIN\r\n");
                    var usernameBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(user));
                    Send($"{usernameBase64}\r\n");
                    var passwordBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
                    Send($"{passwordBase64}\r\n");
                    break;

                case SmtpAuthenticationMechanism.CramMd5:
                    Send("AUTH CRAM-MD5\r\n");
                    // You would handle the challenge-response steps here
                    break;

                case SmtpAuthenticationMechanism.DigestMd5:
                    Send("AUTH DIGEST-MD5\r\n");
                    // Handle challenge-response steps here
                    break;

                case SmtpAuthenticationMechanism.XoAuth2:
                    Send($"AUTH XOAUTH2 {password}\r\n"); // Assuming password holds OAuth token
                    break;

                default:
                    throw new ArgumentException("Unsupported authentication mechanism.");
            }
        }

        protected override void DisposeManagedResources()
        {
            Quit();
        }
    }
}