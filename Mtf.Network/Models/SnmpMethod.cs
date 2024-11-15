namespace Mtf.Network.Models
{
    public enum SnmpMethod : int
    {
        /// <summary>SNMP_PDU_GET.</summary>
        /// <remarks>SNMP_PDU_GET</remarks>
        Get = 160,

        /// <summary>SNMP_PDU_GETNEXT.</summary>
        /// <remarks>SNMP_PDU_GETNEXT</remarks>
        GetNext = 161,

        /// <summary>SNMP_PDU_RESPONSE.</summary>
        /// <remarks>SNMP_PDU_RESPONSE</remarks>
        Response = 162,

        /// <summary>SNMP_PDU_SET.</summary>
        /// <remarks>SNMP_PDU_SET</remarks>
        Set = 163,

        /// <summary>SNMP_PDU_V1TRAP.</summary>
        /// <remarks>SNMP_PDU_V1TRAP</remarks>
        V1Trap = 164,

        /// <summary>SNMP_PDU_GETBULK.</summary>
        /// <remarks>SNMP_PDU_GETBULK</remarks>
        GetBulk = 165,

        /// <summary>SNMP_PDU_INFORM.</summary>
        /// <remarks>SNMP_PDU_INFORM</remarks>
        Inform = 166,

        /// <summary>SNMP_PDU_TRAP.</summary>
        /// <remarks>SNMP_PDU_TRAP</remarks>
        Trap = 167
    }
}
