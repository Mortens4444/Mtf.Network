namespace Mtf.Network.Enums
{
    public enum SnmpTypes
    {
        /// <summary>
        /// IP Address (Octet string (size(4))) (0).
        /// </summary>
        IPAddress = 0,

        /// <summary>
        /// Counter (Integer) (1).
        /// </summary>
        Counter = 1,

        /// <summary>
        /// Gauge (Integer) (2).
        /// </summary>
        Gauge = 2,

        /// <summary>
        /// TimeTicks (Integer) (3).
        /// </summary>
        TimeTicks = 3,

        /// <summary>
        /// Octet string (4).
        /// </summary>
        OctetString = 4,

        /// <summary>
        /// NULL (5).</summary>
        Null = 5,

        /// <summary>
        /// Object identifier (OID) (6).
        /// </summary>
        ObjectIdentifier = 6,

        /// <summary>
        /// SNMP sequence start (48).
        /// </summary>
        SNMPSequenceStart = 48,

        /// <summary>
        /// Integer (66).
        /// </summary>
        Integer66 = 66,

        /// <summary>
        /// Integer (67).
        /// </summary>
        Integer67 = 67
    }
}
