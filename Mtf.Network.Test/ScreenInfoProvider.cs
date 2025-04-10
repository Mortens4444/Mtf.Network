using Mtf.Network.Interfaces;

namespace Mtf.Network.Test
{
    internal class ScreenInfoProvider : IScreenInfoProvider
    {
        public string Id => Screen.PrimaryScreen.DeviceName;

        public Rectangle GetBounds()
        {
            return Screen.PrimaryScreen.Bounds;
        }

        public Size GetPrimaryScreenSize()
        {
            return Screen.PrimaryScreen.Bounds.Size;
        }
    }
}
