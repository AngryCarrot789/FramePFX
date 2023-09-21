using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.ServiceManaging;

namespace FramePFX.WPF.Utils {
    public class ApplicationDelegate : IApplication {
        private readonly Dispatcher dispatcher;
        private readonly App app;

        public bool IsOnOwnerThread => this.dispatcher.CheckAccess();

        public bool IsRunning => Application.Current != null;

        public ApplicationDelegate(App app) {
            this.app = app ?? throw new ArgumentNullException(nameof(app));
            this.dispatcher = app.Dispatcher ?? throw new Exception("Application dispatcher detached");
        }

        public void Invoke(Action action, ExecutionPriority priority) {
            if (priority == ExecutionPriority.Send && this.dispatcher.CheckAccess()) {
                action();
            }
            else {
                this.dispatcher.Invoke(action, ConvertPriority(priority));
            }
        }

        public void InvokeLater(Action action, bool wayLater = false) {
            this.dispatcher.Invoke(action, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public T Invoke<T>(Func<T> function, ExecutionPriority priority) {
            if (priority == ExecutionPriority.Send && this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function, ConvertPriority(priority));
        }

        public T InvokeLater<T>(Func<T> function, bool wayLater = false) {
            return this.dispatcher.Invoke(function, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public Task InvokeAsync(Action action, ExecutionPriority priority) {
            if (priority == ExecutionPriority.Send && this.dispatcher.CheckAccess()) {
                action();
                return Task.CompletedTask;
            }

            return this.dispatcher.InvokeAsync(action, ConvertPriority(priority), CancellationToken.None).Task;
        }

        public Task InvokeLaterAsync(Action action, bool wayLater = false) {
            return this.dispatcher.InvokeAsync(action, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function, ExecutionPriority priority) {
            if (priority == ExecutionPriority.Send && this.dispatcher.CheckAccess())
                return Task.FromResult(function());
            return this.dispatcher.InvokeAsync(function, ConvertPriority(priority), CancellationToken.None).Task;
        }

        public Task<T> InvokeLaterAsync<T>(Func<T> function, bool wayLater = false) {
            return this.dispatcher.InvokeAsync(function, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }

        public static DispatcherPriority ConvertPriority(ExecutionPriority priority) {
            switch (priority) {
                case ExecutionPriority.Send:
                    return DispatcherPriority.Send;
                case ExecutionPriority.Normal:
                    return DispatcherPriority.Normal;
                case ExecutionPriority.Background:
                    return DispatcherPriority.Background;
                case ExecutionPriority.AppIdle:
                    return DispatcherPriority.ApplicationIdle;
                default: throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
            }
        }
    }
}