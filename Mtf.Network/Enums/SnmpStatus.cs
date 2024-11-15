namespace Mtf.Network.Enums
{
    public enum SnmpStatus
    {
        Success = 0,
        MessageError = 1,
        OIDNotExists = 2,
        SetActionNotAvailable = 3,
        GetActionNotAvailable = 4,
        BufferSizeError = 5,
        ValueNotWithinSyntaxError = 6,
        CallbackExecutionError = 7
    }
}
