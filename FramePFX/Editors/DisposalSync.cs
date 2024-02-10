using System;

namespace FramePFX.Editors {
    /// <summary>
    /// A class that stores a mutable disposable value that can be disposed and re-generated,
    /// and manages its disposal when used by different thread.
    /// </summary>
    /// <typeparam name="T">The type of value</typeparam>
    public class DisposalSync<T> where T : IDisposable {
        private volatile bool isRendering;
        private volatile int disposeState;

        public object DisposeLock { get; }

        public T Value { get; }

        public DisposalSync(T value) {
            this.DisposeLock = new object();
            this.Value = value;
        }

        /// <summary>
        /// Tries to begin rendering. If the value is disposed, it should be initialised after this
        /// call and then <see cref="OnResetAndRenderBegin"/> should be called. This method MUST be
        /// called while <see cref="DisposeLock"/> is acquired
        /// </summary>
        /// <returns>True if not disposed, otherwise false</returns>
        public bool OnRenderBegin() {
            if (this.disposeState == 2) {
                return false;
            }

            this.disposeState = 0;
            this.isRendering = true;
            return true;
        }

        /// <summary>
        /// Forces rendering to begin, assuming <see cref="OnRenderBegin"/> previously returned false and the value is now initialised
        /// </summary>
        public void OnResetAndRenderBegin() {
            this.disposeState = 0;
            this.isRendering = true;
        }

        public void OnRenderFinished() {
            if (!this.isRendering)
                throw new InvalidOperationException("Expected to be rendering");
            lock (this.DisposeLock) {
                if (this.disposeState == 1) {
                    this.DisposeResource();
                }
                else {
                    this.isRendering = false;
                }
            }
        }

        /// <summary>
        /// Marks the value to be disposed if in use, or disposes of the resource right now if not in use
        /// </summary>
        public void Dispose() {
            lock (this.DisposeLock) {
                if (this.isRendering) {
                    this.disposeState = 1;
                }
                else {
                    this.DisposeResource();
                }
            }
        }

        private void DisposeResource() {
            this.disposeState = 2;
            this.Value.Dispose();
        }
    }
}