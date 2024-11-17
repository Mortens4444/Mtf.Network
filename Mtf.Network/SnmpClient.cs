using Mtf.Network.Enums;
using Mtf.Network.EventArg;
using Mtf.Network.Models;
using System;
using System.Globalization;
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
                _ = GetLength(e.Data, ref index); // snmpDataLength

                _ = e.Data[index++]; // snmpVersionType
                _ = GetLength(e.Data, ref index); // snmpVersionLength
                _ = e.Data[index++]; // snmpVersion

                _ = e.Data[index++]; // communityNameType
                var communityLength = GetLength(e.Data, ref index);
                var communityBytes = new byte[communityLength];
                for (var i = 0; i < communityLength; i++)
                {
                    communityBytes[i] = e.Data[index++];
                }

                _ = (SnmpMethod)e.Data[index++]; // method
                _ = GetLength(e.Data, ref index); // responseDataLength

                _ = e.Data[index++]; // snmpResponseIdType
                var snmpResponseIdLength = e.Data[index++];
                _ = ReadBytes(e.Data, ref index, snmpResponseIdLength); // snmpResponseId

                _ = e.Data[index++]; // snmpErrorStatusType
                var snmpErrorStatusLength = e.Data[index++];
                var snmpErrorStatus = ReadBytes(e.Data, ref index, snmpErrorStatusLength);

                foreach (var errorByte in snmpErrorStatus)
                {
                    _ = response.AppendLine(((SnmpStatus)errorByte).ToString());
                }

                _ = e.Data[index++]; // snmpErrorIndexType
                var snmpErrorIndexLength = e.Data[index++];
                _ = ReadBytes(e.Data, ref index, snmpErrorIndexLength); // snmpErrorIndex

                index++; // Start of variable bindings sequence
                _ = GetLength(e.Data, ref index); // sizeOfVariableBinding
                index++; // Start of first variable bindings sequence
                _ = GetLength(e.Data, ref index); // size

                while (index < e.Data.Length)
                {
                    var snmpType = (SnmpType)e.Data[index++];
                    switch (snmpType)
                    {
                        case SnmpType.IPAddress:
                            ReadIpAddress(e.Data, ref index, response);
                            break;
                        case SnmpType.OctetString:
                            ReadOctetString(e.Data, ref index, response);
                            break;
                        case SnmpType.Null:
                            index += 2;
                            break;
                        case SnmpType.ObjectIdentifier:
                            ReadObjectIdentifier(e.Data, ref index, response);
                            break;
                        case SnmpType.Sequence:
                            break; // Continue the main loop for another sequence
                        default:
                            ReadDefaultType(e.Data, ref index, snmpType, response);
                            break;
                    }
                }

                var array = Encoding.GetBytes(response.ToString());
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
            _ = response.AppendLine("IP Address")
                .AppendLine($"Length: {ipLength}")
                .Append("Value: ");
            for (var i = 0; i < ipLength; i++)
            {
                _ = response.Append(data[index++]);
                if (i < ipLength - 1)
                {
                    _ = response.Append('.');
                }
            }
            _ = response.AppendLine();
        }

        private static void ReadOctetString(byte[] data, ref int index, StringBuilder response)
        {
            var strLength = GetLength(data, ref index);
            _ = response.AppendLine("Octet String")
                .AppendLine($"Length: {strLength}")
                .Append("Value: ");
            for (var i = 0; i < strLength; i++)
            {
                _ = response.Append((char)data[index++]);
            }
            _ = response.AppendLine();
        }

        private static void ReadObjectIdentifier(byte[] data, ref int index, StringBuilder response)
        {
            var oidLength = GetLength(data, ref index);
            _ = response.AppendLine("Object Identifier")
                .AppendLine($"Length: {oidLength}")
                .Append("Value: ");
            var k = 0;
            while (k < oidLength)
            {
                if (k > 0)
                {
                    _ = response.Append('.');
                }

                if (data[index] >= HalfByte)
                {
                    var n = data[index] - HalfByte;
                    _ = response.Append(data[++index] + (n * HalfByte));
                    k++;
                    index++;
                }
                else
                {
                    _ = response.Append(data[index++]);
                }
                k++;
            }
            _ = response.AppendLine();
        }

        private static void ReadDefaultType(byte[] data, ref int index, SnmpType type, StringBuilder response)
        {
            var length = GetLength(data, ref index);
            _ = response.AppendLine($"Unknown type ({type})")
                .AppendLine($"Length: {length}")
                .Append("Value: ");

            for (var i = 0; i < length; i++)
            {
                _ = response.Append(data[index] != 0 ? data[index].ToString(CultureInfo.InvariantCulture) : "<NUL>")
                    .Append($" ({data[index++]}) ");
            }
            _ = response.AppendLine();
        }

        public SnmpMethod SnmpMethod { get; set; } = SnmpMethod.Get;

        public string SnmpCommunity { get; set; }

        /// <summary>
        /// Sends data, if socket is not connected, try to send to all clients.
        /// </summary>
        /// <param name="bytes">The byte array to send.</param>
        /// <returns>True, if all bytes has been sent</returns>
        public new bool Send(byte[] bytes)
        {
            return Send(Socket, bytes);
        }

        public new bool Send(Socket socket, byte[] bytes)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var sentBytes = 0;
            var str = Encoding.GetString(bytes, 0, bytes.Length);
            var packet = new SnmpPacket(SnmpCommunity, str, SnmpMethod);
            if (socket.Connected)
            {
                sentBytes = socket.Send(packet.RawPacket, packet.RawPacket.Length, SocketFlags.None);
            }
            return sentBytes == bytes.Length;
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