using System;

namespace FramePFX.Avalonia.Interactivity.Contexts;

public abstract class MultiChangeToken : IDisposable {
    public readonly IControlContextData Context;
    private bool disposed;

    public MultiChangeToken(IControlContextData context) {
        this.Context = context;
    }

    /// <summary>
    /// Disposes this token
    /// </summary>
    public void Dispose() {
        if (this.disposed)
            throw new ObjectDisposedException("Already disposed");
        
        this.disposed = true;
        this.OnDisposed();
    }

    protected abstract void OnDisposed();
}