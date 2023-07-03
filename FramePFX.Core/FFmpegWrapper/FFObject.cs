using System;

namespace FramePFX.Core.FFmpegWrapper {

    public abstract class FFObject : IDisposable {
        private volatile bool isDisposed;

        public bool IsDisposed => this.isDisposed;

        public void Dispose() {
            this.Free();
            this.isDisposed = true;
            GC.SuppressFinalize(this);
        }

        ~FFObject() => this.Free();

        protected abstract void Free();
    }
}