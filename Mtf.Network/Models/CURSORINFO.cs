using System;
using System.Runtime.InteropServices;

namespace Mtf.Network.Models
{
    public struct CURSORINFO
    {
        public uint cbSize;
        public uint flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;

        public void Initialize()
        {
            cbSize = (uint)Marshal.SizeOf(typeof(CURSORINFO));
        }
    }
}
