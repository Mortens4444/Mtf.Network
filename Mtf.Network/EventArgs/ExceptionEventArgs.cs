using System;

namespace Mtf.Network.EventArgs
{
    public class ExceptionEventArgs : System.EventArgs
    {
        public Exception Exception { get; }

        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
