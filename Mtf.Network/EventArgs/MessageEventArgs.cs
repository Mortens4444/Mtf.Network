namespace Mtf.Network.EventArgs
{
    public class MessageEventArgs : System.EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
