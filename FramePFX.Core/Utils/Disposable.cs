using System;
using System.Threading;

namespace FramePFX.Core.Utils
{
    public abstract class Disposable : IRealDisposable
    {
        private volatile int isDisposed;

        public bool IsDisposed
        {
            get => this.isDisposed != 0;
        }

        protected Disposable(bool canDisposeInDestructor = true)
        {
            if (!canDisposeInDestructor)
            {
                GC.SuppressFinalize(this);
            }
        }

        ~Disposable()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            if (Interlocked.CompareExchange(ref this.isDisposed, 1, 0) != 0)
                return;
            GC.SuppressFinalize(this);
            this.OnDispose(isDisposing);
        }

        protected abstract void OnDispose(bool isDisposing);
    }
}