using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.App;
using FramePFX.App.Exceptions;
using FramePFX.Components;
using FramePFX.Logger;
using FramePFX.ServiceManaging;
using FramePFX.TaskSystem;
using FramePFX.WPF.Utils;
using Time = FramePFX.Utils.Time;

namespace FramePFX.WPF.App {
    /// <summary>
    /// The <see cref="IApplication"/> implementation for FramePFX
    /// </summary>
    public class ApplicationModel : IApplication {
        private static readonly long TicksPerQuaterSecond = Time.TICK_PER_SECOND / 4;
        private static readonly FieldInfo DisableProcessingCountField;
        private readonly Dispatcher dispatcher;
        private readonly AppWPF appInstance;
        private readonly ServiceManager services;
        private readonly ReaderWriterLockSlim appLock;
        private readonly Thread mainThread; // main app/dispatcher thread
        private volatile int isWriteActionPending;
        private int writeStackCount;

        public bool IsRunning => Application.Current != null;

        public bool IsWriteActionPending => this.isWriteActionPending != 0;
        public bool IsOnMainThread => Thread.CurrentThread == this.mainThread;

        public bool IsWriteAccessAllowed => this.IsOnMainThread && this.appLock.IsWriteLockHeld;
        public bool IsReadAccessAllowed => this.IsOnMainThread || this.appLock.IsReadLockHeld;

        public bool IsDispatcherSuspended {
            get => (int) DisableProcessingCountField.GetValue(this.dispatcher) > 0;
        }

        // I doubt this editor will ever be used by anyone except me, since it can barely do anything.
        // Therefore, major changes won't be applied here LOL. I will still do revisions though because why not
        public Version Version { get; } = new Version(1, 0, 0, 800);

        private readonly Action<Action> runWriteAction;
        private readonly Action<OperationToken> disposeToken;

        public ApplicationModel(AppWPF app) {
            this.appInstance = app ?? throw new ArgumentNullException(nameof(app));
            this.dispatcher = app.Dispatcher ?? throw new Exception("Application dispatcher detached");
            this.mainThread = this.dispatcher.Thread;
            this.appLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this.services = new ServiceManager();
            this.runWriteAction = this.RunWriteAction;
            this.disposeToken = OnDisposeToken;
            this.RegisterService<IDispatcher>(new ApplicationDelegate(app));
        }

        static ApplicationModel() {
            DisableProcessingCountField = typeof(Dispatcher).GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Invokes the given action as a read action. This can be called from any thread. The action may be invoked if there
        /// are no write operations in progress. If there are write operations, then this method blocks until they're done
        /// </summary>
        /// <param name="action">The action to invoke while the read lock is acquired</param>
        public void RunReadAction(Action action) {
            if (this.IsReadAccessAllowed) {
                action();
            }
            else {
                this.BeginReadOperation();
                try {
                    action();
                }
                finally {
                    this.FinishReadOperation();
                }
            }
        }

        public T RunReadAction<T>(Func<T> func) {
            if (this.IsReadAccessAllowed) {
                return func();
            }
            else {
                this.BeginReadOperation();
                try {
                    return func();
                }
                finally {
                    this.FinishReadOperation();
                }
            }
        }

        /// <summary>
        /// Invokes the given action as a write action. This must be invoked on the write thread (aka main application thread),
        /// otherwise
        /// </summary>
        /// <param name="action"></param>
        public void RunWriteAction(Action action) {
            this.BeginWriteOperation();
            try {
                action();
            }
            finally {
                this.FinishWriteOperation();
            }
        }

        public T RunWriteAction<T>(Func<T> func) {
            this.BeginWriteOperation();
            try {
                return func();
            }
            finally {
                this.FinishWriteOperation();
            }
        }

        public void InvokeOnMainThread(Action action, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.IsOnMainThread) {
                this.RunWriteAction(action);
            }
            else {
                this.dispatcher.Invoke(ConvertPriority(priority), this.runWriteAction, action);
            }
        }

        public T InvokeOnMainThread<T>(Func<T> func, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.IsOnMainThread)
                return this.RunWriteAction(func);
            return this.dispatcher.Invoke(() => this.RunWriteAction(func), ConvertPriority(priority));
        }

        public Task InvokeOnMainThreadAsync(Action action, DispatchPriority priority) {
            return this.dispatcher.BeginInvoke(ConvertPriority(priority), this.runWriteAction, action).Task;
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func, DispatchPriority priority) {
            return this.dispatcher.InvokeAsync(() => this.RunWriteAction(func), ConvertPriority(priority), CancellationToken.None).Task;
        }

        private static byte CreateTokenFlags(bool isWrite, bool isFastReadAllowed) {
            byte flags = (byte) (isWrite ? 0 : 1);
            if (isFastReadAllowed)
                flags |= 0b100;
            return flags;
        }

        private static void ExtractTokenFlags(byte flags, out int type, out bool isFastReadAllowed) {
            type = flags & 0b11;
            isFastReadAllowed = (flags & 0b100) != 0;
        }

        public OperationToken CreateWriteToken() {
            this.ValidateIsMainThread("Write tokens can only be created on the main thread");
            this.BeginWriteOperation();
            return new OperationToken(this, CreateTokenFlags(true, false), this.disposeToken);
        }

        public OperationToken CreateReadToken() {
            bool isFastReadAllowed = this.IsReadAccessAllowed;
            if (!isFastReadAllowed) {
                this.BeginReadOperation();
            }

            return new OperationToken(this, CreateTokenFlags(false, isFastReadAllowed), this.disposeToken);
        }

        public object GetService(Type type) {
            return this.services.GetService(type);
        }

        public bool HasService(Type serviceType) {
            return this.services.HasService(serviceType);
        }

        public bool TryGetService(Type serviceType, out object service) {
            return this.services.TryGetService(serviceType, out service);
        }

        public void RegisterService<T>(T service) {
            this.services.Register(service);
        }

        public void RegisterService(Type type, object service) {
            this.services.Register(type, service);
        }

        private void AcquireReadLockBlocking() {
            bool isLocked;
            do {
                isLocked = this.appLock.TryEnterReadLock(10);
            } while (!isLocked);
        }

        private void AcquireWriteLockBlocking() {
            bool isLocked;
            do {
                isLocked = this.appLock.TryEnterWriteLock(10);
            } while (!isLocked);
        }

        private void BeginReadOperation() => this.AcquireReadLockBlocking();

        private void FinishReadOperation() => this.appLock.ExitReadLock();

        private void BeginWriteOperation() {
            this.ValidateIsMainThread("Write actions can only happen on the main thread");
            int isWriteApending = Interlocked.Exchange(ref this.isWriteActionPending, 1);
            try {
                TaskManager.Instance.OnApplicationWriteActionStarting();
                if (!this.appLock.IsWriteLockHeld) {
                    long startTime = Time.GetSystemTicks();
                    this.AcquireWriteLockBlocking();
                    long acquireDuration = Time.GetSystemTicks() - startTime;
                    if (acquireDuration > TicksPerQuaterSecond) {
                        AppLogger.WriteLine("Application stalled for " + ((int) Math.Round(acquireDuration / Time.TICK_PER_MILLIS_D)).ToString() + " millis while acquiring write lock");
                    }
                }
            }
            finally {
                this.isWriteActionPending = isWriteApending;
            }

            this.writeStackCount++;
        }

        private void FinishWriteOperation() {
            if (--this.writeStackCount < 1) {
                this.appLock.ExitWriteLock();
            }
        }

        public void ValidateIsMainThread(string exceptionMessage) {
            if (!this.IsOnMainThread) {
                throw new WrongThreadException(exceptionMessage);
            }
        }

        public void ValidateHasWriteAccess(string exceptionMessage) {
            if (!this.IsWriteAccessAllowed) {
                throw new WrongThreadException(exceptionMessage);
            }
        }

        public void ValidateHasReadAccess(string exceptionMessage) {
            if (!this.IsReadAccessAllowed) {
                throw new WrongThreadException(exceptionMessage);
            }
        }

        public void ValidateDispatcherNotSuspended() {
            if (this.IsDispatcherSuspended)
                throw new InvalidOperationException("Dispatcher is suspended. Cannot push a new dispatcher frame");
        }

        public static void OnDisposeToken(OperationToken token) {
            ExtractTokenFlags(token.Flags, out int type, out bool hasPreExistingReadAccess);
            if (type == 0) {
                ((ApplicationModel) token.app).FinishWriteOperation();
            }
            else if (type == 1 && !hasPreExistingReadAccess) {
                ((ApplicationModel) token.app).FinishReadOperation();
            }
        }

        public static DispatcherPriority ConvertPriority(DispatchPriority priority) {
            switch (priority) {
                case DispatchPriority.AppIdle: return DispatcherPriority.ApplicationIdle;
                case DispatchPriority.Background: return DispatcherPriority.Background;
                case DispatchPriority.AfterRender: return DispatcherPriority.Render;
                case DispatchPriority.Normal: return DispatcherPriority.Normal;
                case DispatchPriority.Send: return DispatcherPriority.Send;
                default: throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
            }
        }
    }
}