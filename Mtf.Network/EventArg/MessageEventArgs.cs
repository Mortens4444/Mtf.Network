using System;

namespace Mtf.Network.EventArg
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
