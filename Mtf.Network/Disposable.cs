using System;
using System.Threading;

namespace Mtf.Network
{
    public abstract class Disposable : IDisposable
    {
        private int disposed;

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            if (disposing)
            {
                CancellationTokenSource?.Dispose();
                CancellationTokenSource = null;
                DisposeManagedResources();
            }

            DisposeUnmanagedResources();
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected virtual void DisposeManagedResources()
        {
        }
    }
}
