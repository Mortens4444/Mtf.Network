using System.Drawing;

namespace Mtf.Network.Interfaces
{
    public interface IScreenInfoProvider
    {
        string Id { get; }

        Rectangle GetBounds();

        Size GetPrimaryScreenSize();
    }
}
