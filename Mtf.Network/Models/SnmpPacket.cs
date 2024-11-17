using Mtf.Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mtf.Network.Models
{
    public class SnmpPacket
    {
        private const byte SnmpSequenceStart = 0x30;

        public byte[] RawPacket { get; private set; }

        private readonly string community;
        private readonly string oidString;
        private readonly SnmpMethod method;
        private readonly byte snmpVersion;
        private readonly uint packetId;
        private readonly byte errorStatus;
        private readonly byte errorIndex;

        // General constructor
        public SnmpPacket(string community, string oidString, SnmpMethod method, byte snmpVersion = 1, uint packetId = 1, byte errorStatus = 0, byte errorIndex = 0)
        {
            this.community = community ?? throw new ArgumentNullException(nameof(community));
            this.oidString = oidString ?? throw new ArgumentNullException(nameof(oidString));
            this.method = method;
            this.snmpVersion = snmpVersion;
            this.packetId = packetId;
            this.errorStatus = errorStatus;
            this.errorIndex = errorIndex;

            CreatePacket();
        }

        private void CreatePacket()
        {
            var oid = OidToByteArray(oidString); // Convert OID to byte array
            var communityBytes = Encoding.ASCII.GetBytes(community);

            // Calculate packet size
            var packetLength = 28 + communityBytes.Length + oid.Length;
            var packet = new byte[packetLength];
            var index = 0;

            // SNMP Sequence Start
            packet[index++] = SnmpSequenceStart;
            packet[index++] = (byte)(packetLength - 2);

            // SNMP Version
            packet[index++] = (byte)SnmpType.Integer32;
            packet[index++] = 0x01;
            packet[index++] = snmpVersion;

            // Community
            packet[index++] = (byte)SnmpType.Integer32;
            packet[index++] = (byte)communityBytes.Length;
            Array.Copy(communityBytes, 0, packet, index, communityBytes.Length);
            index += communityBytes.Length;

            // Method
            packet[index++] = (byte)method;
            packet[index++] = (byte)(19 + oid.Length);

            // Request ID
            AddInteger(packet, ref index, packetId);

            // Error Status
            packet[index++] = (byte)SnmpType.Integer32;
            packet[index++] = 0x01;
            packet[index++] = errorStatus;

            // Error Index
            packet[index++] = (byte)SnmpType.Integer32;
            packet[index++] = 0x01;
            packet[index++] = errorIndex;

            // Variable Bindings Sequence
            packet[index++] = SnmpSequenceStart;
            packet[index++] = (byte)(5 + oid.Length);

            // First Variable Binding
            packet[index++] = SnmpSequenceStart;
            packet[index++] = (byte)(3 + oid.Length);

            // OID
            packet[index++] = (byte)SnmpType.ObjectIdentifier;
            packet[index++] = (byte)(oid.Length - 1);
            Array.Copy(oid, 0, packet, index, oid.Length);
            index += oid.Length - 2;

            // Value: Null
            packet[index++] = (byte)SnmpType.Null;
            packet[index++] = 0x00;

            RawPacket = packet;
        }

        private static void AddInteger(byte[] packet, ref int index, uint value)
        {
            packet[index++] = (byte)SnmpType.Integer32;
            packet[index++] = 0x04;
            packet[index++] = (byte)(value >> 24);
            packet[index++] = (byte)(value >> 16);
            packet[index++] = (byte)(value >> 8);
            packet[index++] = (byte)value;
        }

        public static byte[] OidToByteArray(string oidString)
        {
            if (String.IsNullOrWhiteSpace(oidString))
            {
                throw new ArgumentException("OID string cannot be null or empty.", nameof(oidString));
            }

            var oidParts = oidString.Split('.').Select(Int16.Parse).ToArray();
            if (oidParts.Length < 2)
            {
                throw new FormatException("OID string must contain at least two components.");
            }

            var oid = new List<byte>
            {
                // First two components are encoded as 40 * first + second
                (byte)((40 * oidParts[0]) + oidParts[1])
            };

            // Encode the rest of the OID
            for (var i = 2; i < oidParts.Length; i++)
            {
                var value = oidParts[i];

                if (value < 0)
                {
                    throw new FormatException("OID components must be non-negative.");
                }

                // Encode the value using base 128 (7-bit encoding)
                var encoded = new Stack<byte>();
                encoded.Push((byte)(value & 0x7F)); // First byte (LSB) without continuation bit

                while ((value >>= 7) > 0)
                {
                    encoded.Push((byte)((value & 0x7F) | 0x80)); // Set continuation bit
                }

                oid.AddRange(encoded);
            }

            return oid.ToArray();
        }
    }
}
