using Mtf.Network.EventArg;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Mtf.Network.Services
{
    public sealed class Ping : IDisposable
    {
        private const string Empty = "";
        private int cleanup = 0;
        private readonly System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        private byte[] dataToSend = Encoding.ASCII.GetBytes("ping");

        public delegate void PingReplyArrivedEventHandler(object sender, PingReplyArrivedEventArgs e);

        public bool ShowMessages { get; set; }
        public event PingReplyArrivedEventHandler PingReplyArrived;

        public Ping(string ipAddress, int timeout = 1000, string data = Empty, PingReplyArrivedEventHandler PingReplyArrivedHandler = null)
        {
            if (PingReplyArrivedHandler != null)
            {
                PingReplyArrived += PingReplyArrivedHandler;
            }

            Initialize(ipAddress, timeout, data);
        }

        public Ping(byte[] ipAddress, int timeout = 1000, string data = Empty, PingReplyArrivedEventHandler PingReplyArrivedHandler = null)
        {
            if (PingReplyArrivedHandler != null)
            {
                PingReplyArrived += PingReplyArrivedHandler;
            }

            var ip = new StringBuilder();
            for (var i = 0; i < ipAddress.Length; i++)
            {
                ip.Append(ipAddress[i]);
                if (i < ipAddress.Length - 1)
                {
                    ip.Append('.');
                }
            }
            Initialize(ip.ToString(), timeout, data);
        }

        ~Ping()
        {
            Dispose(false);
        }

        private void Initialize(string ipAddress, int timeout, string data)
        {
            try
            {
                if (!String.IsNullOrEmpty(data))
                {
                    dataToSend = Encoding.ASCII.GetBytes(data);
                }

                ping.PingCompleted += PingResult;
                ping.SendAsync(ipAddress, timeout, dataToSend, null);
            }
            catch
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref cleanup, 1) != 0)
            {
                return;
            }

            if (disposing)
            {
                if (ping != null)
                {
                    ((IDisposable)ping).Dispose();
                }
            }
        }

        private void OnPingResultArrived(PingReplyArrivedEventArgs e)
        {
            PingReplyArrived?.Invoke(this, e);
        }

        private void PingResult(object sender, PingCompletedEventArgs e)
        {
            OnPingResultArrived(new PingReplyArrivedEventArgs(e.Reply, ShowMessages));
            ((IDisposable)ping).Dispose();
        }

        public static string GetIPStatusDescription(IPStatus status)
        {
            switch (status)
            {
                case IPStatus.Success:
                    return "The ping request found the host.";
                case IPStatus.DestinationNetworkUnreachable:
                    return "The ICMP echo request failed because the network that contains the destination computer is not reachable.";
                case IPStatus.DestinationHostUnreachable:
                    return "The ICMP echo request failed because the destination computer is not reachable.";
                case IPStatus.DestinationProtocolUnreachable:
                    return "The ICMP echo request failed because the destination computer that is specified in an ICMP echo message is not reachable,\nbecause it does not support the packet's protocol, or the ICMP echo request failed because contact with the destination computer is administratively prohibited.";
                case IPStatus.DestinationPortUnreachable:
                    return "The ICMP echo request failed because the port on the destination computer is not available.";
                case IPStatus.NoResources:
                    return "The ICMP echo request failed because of insufficient network resources.";
                case IPStatus.BadOption:
                    return "The ICMP echo request failed because it contains an invalid option.";
                case IPStatus.HardwareError:
                    return "The ICMP echo request failed because of a hardware error.";
                case IPStatus.PacketTooBig:
                    return "The ICMP echo request failed because the packet containing the request is larger than the maximum transmission unit (MTU) of\na node (router or gateway) located between the source and destination.\nThe MTU defines the maximum size of a transmittable packet.";
                case IPStatus.TimedOut:
                    return "The ICMP echo Reply was not received within the allotted time. The default time allowed for replies is 5 seconds.\nYou can change this value using the Send or SendAsync methods that take a timeout parameter.";
                case IPStatus.BadRoute:
                    return "The ICMP echo request failed because there is no valid route between the source and destination computers.";
                case IPStatus.TtlExpired:
                    return "The ICMP echo request failed because its Time to Live (TTL) value reached zero, causing the forwarding node (router or gateway) to discard the packet.";
                case IPStatus.TtlReassemblyTimeExceeded:
                    return "The ICMP echo request failed because the packet was divided into fragments for transmission and all of the fragments were not\nreceived within the time allotted for reassembly. RFC 2460 (available at www.ietf.org)\nspecifies 60 seconds as the time limit within which all packet fragments must be received.";
                case IPStatus.ParameterProblem:
                    return "The ICMP echo request failed because a node (router or gateway) encountered problems while processing the packet header.\nThis is the status if, for example, the header contains invalid field data or an unrecognized option.";
                case IPStatus.SourceQuench:
                    return "The ICMP echo request failed because the packet was discarded. This occurs when the source computer's output queue has insufficient storage space,\nor when packets arrive at the destination too quickly to be processed.";
                case IPStatus.BadDestination:
                    return "The ICMP echo request failed because the destination IP address cannot receive ICMP echo requests\nor should never appear in the destination address field of any IP datagram.\nFor example, calling Send and specifying IP address \"000.0.0.0\" returns this status.";
                case IPStatus.DestinationUnreachable:
                    return "The ICMP echo request failed because the destination computer that is specified in an ICMP echo message is not reachable;\nthe exact cause of problem is unknown.";
                case IPStatus.TimeExceeded:
                    return "The ICMP echo request failed because its Time to Live (TTL) value reached zero, causing the forwarding node\n(router or gateway) to discard the packet.";
                case IPStatus.BadHeader:
                    return "The ICMP echo request failed because the header is invalid.";
                case IPStatus.UnrecognizedNextHeader:
                    return "The ICMP echo request failed because the Next Header field does not contain a recognized value.\nThe Next Header field indicates the extension header type (if present) or the protocol above the IP layer, for example, TCP or UDP.";
                case IPStatus.IcmpError:
                    return "The ICMP echo request failed because of an ICMP protocol error.";
                case IPStatus.DestinationScopeMismatch:
                    return "The ICMP echo request failed because the source address and destination address\nthat are specified in an ICMP echo message are not in the same scope.\nThis is typically caused by a router forwarding a packet using an interface\nthat is outside the scope of the source address. Address scopes (link-local, site-local, and global scope) determine where on the network an address is valid.";
                case IPStatus.Unknown:
                    return "The ICMP echo request failed for an unknown reason.";
                default:
                    return "The ping request could not find the target host.";
            }
        }
    }
}
