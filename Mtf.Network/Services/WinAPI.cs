using Mtf.Network.Models;
using System;
using System.Runtime.InteropServices;

namespace Mtf.Network.Services
{
    public static class WinAPI
    {
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        public const int MOUSEEVENTF_MIDDLEUP = 0x40;
        public const int MOUSEEVENTF_WHEEL = 0x800;

        public const int KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        public static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("User32.dll")]
        public static extern bool GetCursorInfo(ref CURSORINFO pci);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

#if NET462
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, long dwExtraInfo);
        
        //public static void MouseEvent(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo)
        public static void MouseEvent(int dwFlags, int dx, int dy, int cButtons, long dwExtraInfo)
        {
            mouse_event(dwFlags, dx, dy, cButtons, dwExtraInfo);
        }

#else
        [DllImport("User32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void MouseEvent(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
#endif
        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);
    }
}
