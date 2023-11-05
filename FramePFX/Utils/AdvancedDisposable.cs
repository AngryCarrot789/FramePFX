using System;
using System.Collections.Generic;
using System.Threading;

namespace FramePFX.Utils {
    public abstract class AdvancedDisposable : IDisposableEx {
        private readonly object locker;
        private List<IDisposableEx> children;
        private int state;

        public bool IsDisposing => this.state == 1;

        public bool IsDisposed => this.state == 2;

        protected AdvancedDisposable(bool canDisposeInDestructor = true) {
            if (!canDisposeInDestructor) {
                GC.SuppressFinalize(this);
            }

            this.locker = new object();
        }

        ~AdvancedDisposable() => this.Dispose(false);

        public void AddDisposableChild(IDisposableEx disposable) {
            lock (this.locker) {
                (this.children ?? (this.children = new List<IDisposableEx>())).Add(disposable);
            }
        }

        public void Dispose() => this.Dispose(true);

        public bool Dispose(bool isDisposing) {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) != 0) {
                return false;
            }

            try {
                ErrorList exceptions = new ErrorList();
                lock (this.locker) {
                    List<IDisposableEx> list = this.children;
                    if (list != null) {
                        for (int i = 0; i < list.Count; i++) {
                            IDisposableEx disposable = list[i];
                            list.RemoveAt(i);
                            try {
                                disposable.Dispose(isDisposing);
                            }
                            catch (Exception e) {
                                exceptions.Add(new DisposalException("Exception disposing child object", this, disposable, e));
                            }
                        }
                    }

                    try {
                        this.DisposeCore(isDisposing);
                    }
                    catch (Exception e) {
                        exceptions.Add(new DisposalException("Exception disposing ourself", this, null, e));
                    }
                }

                if (exceptions.TryGetException(out Exception exception)) {
                    throw exception;
                }
            }
            finally {
                this.state = 2;
            }

            if (isDisposing)
                GC.SuppressFinalize(this);

            return true;
        }

        /// <summary>
        /// The core method for disposing of this current object
        /// </summary>
        /// <param name="isDisposing">
        /// True if the object was disposed successfully, otherwise false if we are
        /// already disposed or currently being disposed on another thread
        /// </param>
        protected virtual void DisposeCore(bool isDisposing) {

        }
    }
}