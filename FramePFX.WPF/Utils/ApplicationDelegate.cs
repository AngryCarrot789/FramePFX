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
    public class ApplicationDelegate : IDispatcher {
        private readonly Dispatcher dispatcher;

        public bool IsOnOwnerThread => this.dispatcher.CheckAccess();

        public ApplicationDelegate(AppWPF app) {
            this.dispatcher = app.Dispatcher ?? throw new Exception("Application dispatcher detached");
        }

        public void Invoke(Action action, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action();
            }
            else {
                this.dispatcher.Invoke(action, ApplicationModel.ConvertPriority(priority));
            }
        }

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

        public Task InvokeAsync(Action action, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action();
                return Task.CompletedTask;
            }

            return this.dispatcher.InvokeAsync(action, ApplicationModel.ConvertPriority(priority), CancellationToken.None).Task;
        }

        public Task InvokeAsync<T>(Action<T> action, T parameter, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action(parameter);
                return Task.CompletedTask;
            }

            return this.dispatcher.BeginInvoke(ApplicationModel.ConvertPriority(priority), action, parameter).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                return Task.FromResult(function());
            return this.dispatcher.InvokeAsync(function, ApplicationModel.ConvertPriority(priority), CancellationToken.None).Task;
        }
    }
}