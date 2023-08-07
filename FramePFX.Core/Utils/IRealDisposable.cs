using System;

namespace FramePFX.Core.Utils
{
    public interface IRealDisposable : IDisposable
    {
        bool IsDisposed { get; }

        void Dispose(bool isDisposing);
    }
}