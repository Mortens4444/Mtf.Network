using System.Drawing;

namespace Mtf.Network.Interfaces
{
    public interface IScreenInfoProvider
    {
        Rectangle GetBounds();

        Size GetPrimaryScreenSize();
    }
}
