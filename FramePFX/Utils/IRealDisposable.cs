using System;

namespace FramePFX.Utils {
    public interface IRealDisposable : IDisposable {
        bool IsDisposed { get; }

        void Dispose(bool isDisposing);
    }
}