using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Models;
using System;
using System.Net.Sockets;
using System.Text;

namespace Mtf.Network
{
    /// <summary>
    /// NOT TESTED - RFC 1988, RFC 1993, RFC 2002
    /// </summary>
    public class SnmpClient : Client
    {
        public const int HalfByte = 128;

        public const string SystemInformation = "1.3.6.1.2.1.1.1.0";
        public const string Oids = "1.3.6.1.2.1.1.2.0";
        public const string Uptime = "1.3.6.1.2.1.1.3.0";
        public const string SysAdmin = "1.3.6.1.2.1.1.4.0";
        public const string RemoteHost = "1.3.6.1.2.1.1.5.0";
        public const string ServerRoom = "1.3.6.1.2.1.1.6.0";
        public const string TimeTicks = "1.3.6.1.2.1.1.8.0";

        public const string ColdStart = "1.3.6.1.6.3.1.1.5.1";

        public const string A = "1.3.6.1.2.1.1.9.1.2.1";
        public const string B = "1.3.6.1.2.1.1.9.1.2.2";
        public const string C = "1.3.6.1.2.1.1.9.1.2.3";

        public SnmpClient(string serverHost, string community, ushort listenerPort)
            : base(serverHost, listenerPort, AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            SnmpCommunity = community;

            DataArrived += DataArrivedEventHandler;
        }

        private void DataArrivedEventHandler(object sender, DataArrivedEventArgs e)
        {
            var response = new StringBuilder();
            var index = 1;

            while (index < e.Data.Length)
            {
                var snmpDataLength = GetLength(e.Data, ref index);

                var snmpVersionType = e.Data[index++];
                var snmpVersionLength = GetLength(e.Data, ref index);
                var snmpVersion = e.Data[index++];

                var communityNameType = e.Data[index++];
                var communityLength = GetLength(e.Data, ref index);
                var communityBytes = new byte[communityLength];
                for (var i = 0; i < communityLength; i++)
                {
                    communityBytes[i] = e.Data[index++];
                }

                var method = (SnmpMethod)e.Data[index++];
                var responseDataLength = GetLength(e.Data, ref index);

                var snmpResponseIdType = e.Data[index++];
                var snmpResponseIdLength = e.Data[index++];
                var snmpResponseId = ReadBytes(e.Data, ref index, snmpResponseIdLength);

                var snmpErrorStatusType = e.Data[index++];
                var snmpErrorStatusLength = e.Data[index++];
                var snmpErrorStatus = ReadBytes(e.Data, ref index, snmpErrorStatusLength);

                foreach (var errorByte in snmpErrorStatus)
                {
                    response.AppendLine(((SnmpStatus)errorByte).ToString());
                }

                var snmpErrorIndexType = e.Data[index++];
                var snmpErrorIndexLength = e.Data[index++];
                var snmpErrorIndex = ReadBytes(e.Data, ref index, snmpErrorIndexLength);

                index++; // Start of variable bindings sequence
                var sizeOfVariableBinding = GetLength(e.Data, ref index);
                index++; // Start of first variable bindings sequence
                var size = GetLength(e.Data, ref index);

                while (index < e.Data.Length)
                {
                    var snmpType = (SnmpTypes)e.Data[index++];
                    switch (snmpType)
                    {
                        case SnmpTypes.IPAddress:
                            ReadIpAddress(e.Data, ref index, response);
                            break;
                        case SnmpTypes.OctetString:
                            ReadOctetString(e.Data, ref index, response);
                            break;
                        case SnmpTypes.Null:
                            index += 2;
                            break;
                        case SnmpTypes.ObjectIdentifier:
                            ReadObjectIdentifier(e.Data, ref index, response);
                            break;
                        case SnmpTypes.SNMPSequenceStart:
                            break; // Continue the main loop for another sequence
                        default:
                            ReadDefaultType(e.Data, ref index, snmpType, response);
                            break;
                    }
                }

                var array = Encoding.ASCII.GetBytes(response.ToString());
                OnDataArrived(Socket, array);
            }
        }

        private static byte[] ReadBytes(byte[] data, ref int index, int length)
        {
            var result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            index += length;
            return result;
        }

        private static void ReadIpAddress(byte[] data, ref int index, StringBuilder response)
        {
            var ipLength = GetLength(data, ref index);
            response.AppendLine("IP Address")
                    .Append("Length: ").AppendLine(ipLength.ToString())
                    .Append("Value: ");
            for (var i = 0; i < ipLength; i++)
            {
                response.Append(data[index++]);
                if (i < ipLength - 1) response.Append('.');
            }
            response.AppendLine();
        }

        private static void ReadOctetString(byte[] data, ref int index, StringBuilder response)
        {
            var strLength = GetLength(data, ref index);
            response.AppendLine("Octet String")
                    .Append("Length: ").AppendLine(strLength.ToString())
                    .Append("Value: ");
            for (var i = 0; i < strLength; i++)
            {
                response.Append((char)data[index++]);
            }
            response.AppendLine();
        }

        private static void ReadObjectIdentifier(byte[] data, ref int index, StringBuilder response)
        {
            var oidLength = GetLength(data, ref index);
            response.AppendLine("Object Identifier")
                    .Append("Length: ").AppendLine(oidLength.ToString())
                    .Append("Value: ");
            var k = 0;
            while (k < oidLength)
            {
                if (k > 0) response.Append('.');
                if (data[index] >= HalfByte)
                {
                    var n = data[index] - HalfByte;
                    response.Append(data[++index] + n * HalfByte);
                    k++;
                    index++;
                }
                else
                {
                    response.Append(data[index++]);
                }
                k++;
            }
            response.AppendLine();
        }

        private static void ReadDefaultType(byte[] data, ref int index, SnmpTypes type, StringBuilder response)
        {
            response.AppendLine($"Unknown type ({type})")
                .Append("Length: ");

            var length = GetLength(data, ref index);
            response.AppendLine(length.ToString())
                    .Append("Value: ");
            for (var i = 0; i < length; i++)
            {
                response.Append(data[index] != 0 ? data[index].ToString() : "<NUL>");
                response.Append($" ({data[index++]}) ");
            }
            response.AppendLine();
        }

        public SnmpMethod SnmpMethod { get; set; } = SnmpMethod.Get;

        public string SnmpCommunity { get; set; }

        /// <summary>
        /// Sends data, if socket is not connected, try to send to all clients.
        /// </summary>
        /// <param name="bytes">The byte array to send.</param>
        /// <returns>True, if all bytes has been sent</returns>
        public bool Send(byte[] bytes)
        {
            var str = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            var packet = new SnmpPacket(SnmpCommunity, str, SnmpMethod);
            var sent_bytes = Socket.Send(packet.RawPacket, packet.RawPacket.Length, SocketFlags.None);
            var success = sent_bytes == packet.RawPacket.Length;
            return success;
        }

        public bool Send(Socket s, byte[] bytes)
        {
            var sent_bytes = 0;
            var str = Encoding.GetString(bytes, 0, bytes.Length);
            var packet = new SnmpPacket(SnmpCommunity, str, SnmpMethod);
            if (s.Connected)
            {
                sent_bytes = s.Send(packet.RawPacket, packet.RawPacket.Length, SocketFlags.None);
            }
            return sent_bytes == bytes.Length;
        }

        public static TimeSpan ToTimeSpan(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        private static int GetLength(byte[] array, ref int index)
        {
            var result = 0;
            if (array[index] > 0x80)
            {
                var n = (byte)(array[index] - 0x80);
                for (var i = 0; i < n; i++)
                {
                    result = (result * 256) + array[++index];
                }
                index++;
            }
            else
            {
                result = array[index++];
            }
            return result;
        }

        public void Send(string oid, SnmpMethod method)
        {
            SnmpMethod = method;
            Send(oid);
        }

        public void GetSystemInformation()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(SystemInformation);
        }

        public void GetOIDs()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(Oids);
        }

        public void GetUptime()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(Uptime);
        }

        public void GetColdStart()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(ColdStart);
        }

        public void GetSysAdmin()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(SysAdmin);
        }

        public void GetRemoteHost()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(RemoteHost);
        }

        public void GetServerRoom()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(ServerRoom);
        }

        public void GetTimeTicks()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(TimeTicks);
        }

        public void GetA()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(A);
        }

        public void GetB()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(B);
        }

        public void GetC()
        {
            SnmpMethod = SnmpMethod.Get;
            Send(C);
        }
    }
}