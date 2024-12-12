using Mtf.Network.Models;
using System;

namespace Mtf.Network.EventArg
{
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public Device Device { get; }

        public DeviceDiscoveredEventArgs(Device device)
        {
            Device = device;
        }
    }
}
