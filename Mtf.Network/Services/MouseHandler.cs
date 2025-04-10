using Mtf.Network.Enums;
using Mtf.Network.Models;
using System;
using System.Threading.Tasks;

namespace Mtf.Network.Services
{
    public static class MouseHandler
    {
        public static async Task ProcessMessageAsync(string clickControlMessage, int clickType)
        {
            var clickButton = clickControlMessage.Split(' ')[1];
            WinAPI.GetCursorPos(out POINT location);

            if (clickType == 3)
            {
                var clickFlags = ClickFlagsProvider.Get(clickButton, 2);
                for (int i = 0; i < 3; i++) // Why 3?
                {
                    WinAPI.MouseEvent(clickFlags, location.X, location.Y, 0, 0);
                    await Task.Delay(50);
                }
            }
            else
            {
                var clickFlags = ClickFlagsProvider.Get(clickButton, clickType);
                WinAPI.MouseEvent(clickFlags, location.X, location.Y, 0, 0);
            }
        }

        public static async Task ProcessMessageAsync(string mouseControlMessage)
        {
            var mouseEvents = mouseControlMessage.Split(new string[] { VncCommand.Mouse }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < mouseEvents.Length; i++)
            {
                var clickType = ClickButtonProvider.Get(mouseEvents[i]);
                if (clickType != -1)
                {
                    await MouseHandler.ProcessMessageAsync(mouseEvents[i], clickType);
                    await Task.Delay(50);
                }
                else
                {
                    var mouseMoveArgs = mouseEvents[i].Split(' ');

                    try
                    {
                        var x = Convert.ToInt32(mouseMoveArgs[1]);
                        var y = Convert.ToInt32(mouseMoveArgs[2]);

                        _ = WinAPI.SetCursorPos(x, y);
                    }
                    catch //(Exception ex)
                    {
                        // FIXME: mouseMoveArgs = 289getscre
                        //ErrorBox.Show("Mouse control error", $"Unable to parse message: {mouseControlMessage}. Exception: {ex}");
                    }
                }
            }
        }
    }
}
