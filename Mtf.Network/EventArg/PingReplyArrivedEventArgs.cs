using System;
using System.Net.NetworkInformation;

namespace Mtf.Network.EventArg
{
    public class PingReplyArrivedEventArgs : System.EventArgs
    {
        public PingReplyArrivedEventArgs(PingReply pingReply)
            : this(pingReply, false)
        {
        }

        public PingReplyArrivedEventArgs(PingReply pingReply, bool showStatusMessages)
        {
            PingReply = pingReply;
            if ((pingReply != null) && (pingReply.Address != null))
            {
                Sender = pingReply.Address.ToString();
                Success = pingReply.Status == IPStatus.Success;
                StatusMessage = Services.Ping.GetIPStatusDescription(pingReply.Status);
            }
            else
            {
                Sender = String.Empty;
                Success = false;
                StatusMessage = Services.Ping.GetIPStatusDescription(IPStatus.Unknown);
            }

            ShowStatusMessages = showStatusMessages;
        }

        public string Sender { get; private set; }

        public PingReply PingReply { get; private set; }

        public bool Success { get; private set; }

        public bool ShowStatusMessages { get; private set; }

        public string StatusMessage { get; private set; }

        public override string ToString()
        {
            return StatusMessage;
        }
    }
}
