using System;
using System.Threading.Tasks;

namespace FramePFX.Utils {
    /// <summary>
    /// A class that stores a mutable disposable value that can be disposed and re-generated,
    /// and manages its disposal when used by different thread.
    /// </summary>
    /// <typeparam name="T">The type of value</typeparam>
    public class DisposableRef<T> where T : IDisposable {
        private int usageCount;
        private int disposeState; // 0 == valid, 1 == dispose queued, 2 == disposed
        private event EventHandler UsageEmpty;

        public T Value { get; }

        /// <summary>
        /// The constructor for a disposable reference
        /// </summary>
        /// <param name="value">The value that gets stored</param>
        /// <param name="isInitiallyDisposed">True to mark the value as disposed to implement lazily loading of the value</param>
        public DisposableRef(T value, bool isInitiallyDisposed = false) {
            this.Value = value;
            if (isInitiallyDisposed)
                this.disposeState = 2;
        }

        /// <summary>
        /// Tries to begin using this reference. If the value is disposed, then this method returns false. If the caller
        /// has ownership over the resource, then it it should be initialised after this call and then <see cref="ResetAndBeginUsage"/>
        /// should be called
        /// <para>
        /// This method MUST be called while a lock to this object's instance is acquired
        /// </para>
        /// </summary>
        /// <returns>True if not disposed, otherwise false</returns>
        public bool TryBeginUsage() {
            if (this.disposeState == 2) {
                return false;
            }

            this.disposeState = 0;
            this.usageCount++;
            return true;
        }

        /// <summary>
        /// Forces rendering to begin, assuming <see cref="TryBeginUsage"/> previously returned false and the value is now initialised
        /// <para>
        /// This method MUST be called while a lock to this object's instance is acquired
        /// </para>
        /// </summary>
        public void ResetAndBeginUsage() {
            this.disposeState = 0;
            this.usageCount++;
        }

        /// <summary>
        /// Tries to begin using this resource as an owner. If the value is disposed, then the resetter action is called
        /// and usage begins
        /// <para>
        /// This method automatically acquires the lock on this instance
        /// </para>
        /// </summary>
        /// <param name="owner">Passed to the resetter</param>
        /// <param name="resetter">The resetter to reset the value (to un-dispose it)</param>
        /// <typeparam name="TOwner">The owner type</typeparam>
        public void BeginUsage<TOwner>(TOwner owner, Action<TOwner, T> resetter) {
            lock (this) {
                if (this.disposeState == 2) {
                    this.disposeState = 0;
                    resetter(owner, this.Value);
                }

                this.usageCount++;
            }
        }

        /// <summary>
        /// Completes a usage phase of this reference.
        /// <para>
        /// This method automatically acquires the lock on this instance
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Excessive calls to CompleteUsage, or the lock was not acquired causing this object to become corrupted
        /// </exception>
        public void CompleteUsage() {
            lock (this) {
                if (this.usageCount < 1)
                    throw new InvalidOperationException("Expected a usage beforehand. Possible bug, excessive calls to CompleteUsage?");
                if (--this.usageCount == 0) {
                    if (this.disposeState == 1)
                        this.DisposeInternal();
                    this.UsageEmpty?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Marks the value to be disposed if in use, or disposes of the resource right now if not in use.
        /// <para>
        /// This method automatically acquires the lock on this instance
        /// </para>
        /// </summary>
        public void Dispose() {
            lock (this) {
                if (this.usageCount > 0) {
                    this.disposeState = 1;
                }
                else {
                    this.DisposeInternal();
                }
            }
        }

        private void DisposeInternal() {
            this.disposeState = 2;
            this.Value.Dispose();
        }

        public Task WaitForNoUsages() {
            if (this.usageCount < 1)
                return Task.CompletedTask;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            handler = (sender, args) => {
                tcs.SetResult(true);
                this.UsageEmpty -= handler;
            };

            this.UsageEmpty += handler;
            return tcs.Task;
        }
    }
}