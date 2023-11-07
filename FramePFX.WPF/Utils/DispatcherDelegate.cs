using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.App;
using FramePFX.ServiceManaging;
using FramePFX.WPF.App;

namespace FramePFX.WPF.Utils {
    public class DispatcherDelegate : IDispatcher {
        private static readonly FieldInfo DisableProcessingCountField;
        private readonly Dispatcher dispatcher;

        public bool IsOnOwnerThread => this.dispatcher.CheckAccess();

        public bool IsSuspended {
            get => (int) DisableProcessingCountField.GetValue(this.dispatcher) > 0;
        }

        public DispatcherDelegate(Dispatcher dispatcher) {
            this.dispatcher = dispatcher ?? throw new Exception("Application dispatcher detached");
        }

        static DispatcherDelegate() {
            DisableProcessingCountField = typeof(Dispatcher).GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
        }

        public void Invoke(Action action, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action();
            }
            else {
                this.dispatcher.Invoke(action, ApplicationModel.ConvertPriority(priority));
            }
        }

        // Unless we are on the main thread and priority is Send, Invoke with the parameter provides
        // practically no additional performance benifits for valuetype objects, because the parameter
        // has to get boxed anyway, and not to mention the fact that WPF dispatcher operations create
        // an instance of DispatcherOperationTaskSource which also creates a TaskCompletionSource and
        // DispatcherOperationTaskMapping AND an instance of CulturePreservingExecutionContext gets created too...

        public void Invoke<T>(Action<T> action, T parameter, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action(parameter);
            }
            else {
                this.dispatcher.Invoke(ApplicationModel.ConvertPriority(priority), action, parameter);
            }
        }

        public T Invoke<T>(Func<T> function, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function, ApplicationModel.ConvertPriority(priority));
        }

        public Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action();
                return Task.CompletedTask;
            }

            return this.dispatcher.InvokeAsync(action, ApplicationModel.ConvertPriority(priority), token).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                return Task.FromResult(function());
            return this.dispatcher.InvokeAsync(function, ApplicationModel.ConvertPriority(priority), token).Task;
        }
    }
}