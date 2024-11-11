using System;

namespace Mtf.Network.EventArg
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
