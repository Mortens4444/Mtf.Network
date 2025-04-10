using System;

namespace Mtf.Network.Services
{
    public static class KeyboardSimulator
    {
        public static void SendChar(string str)
        {
            foreach (var c in str)
            {
                byte vk = (byte)WinAPI.VkKeyScan(c);
                WinAPI.keybd_event(vk, 0, 0, UIntPtr.Zero);
                WinAPI.keybd_event(vk, 0, WinAPI.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }
    }
}
