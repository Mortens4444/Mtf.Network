using System;

namespace Mtf.Network.Services
{
    public static class MagicPacketForger
    {
        public const int MacAddressLengthInBytes = 6;
        public const int MacAddressRepetitionsInMagicPacket = 16;

        public static byte[] Forge(byte[] macByteArray)
        {
            if (macByteArray == null)
            {
                throw new ArgumentNullException(nameof(macByteArray));
            }

            var result = new byte[MacAddressLengthInBytes + MacAddressRepetitionsInMagicPacket * MacAddressLengthInBytes];
            for (var i = 0; i < MacAddressLengthInBytes; i++)
            {
                result[i] = 255;
            }
            for (var i = 0; i < MacAddressRepetitionsInMagicPacket; i++)
            {
                for (var j = 0; j < MacAddressLengthInBytes; j++)
                {
                    result[i * MacAddressLengthInBytes + j + MacAddressLengthInBytes] = macByteArray[j];
                }
            }
            return result;
        }
    }
}
