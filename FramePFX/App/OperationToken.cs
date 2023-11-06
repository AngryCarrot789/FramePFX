using System;

namespace FramePFX.App {
    /// <summary>
    /// A struct which is used to create a write-safe
    /// </summary>
    public readonly struct OperationToken : IDisposable {
        public readonly IApplication_OLD app;
        public readonly byte Flags;
        private readonly Action<OperationToken> dispose;

        public OperationToken(IApplication_OLD app, byte flags, Action<OperationToken> dispose) {
            this.app = app;
            this.Flags = flags;
            this.dispose = dispose;
        }

        public void Dispose() => this.dispose(this);
    }
}