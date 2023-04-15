using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Utils {
    public static class DispatcherUtils {
        public static Task InvokeAsync(Action action) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                return InvokeAsync(dispatcher, action);
            }

            return Task.CompletedTask;
        }

        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> function) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                return InvokeAsync(dispatcher, function);
            }

            return Task.FromResult<TResult>(default);
        }

        public static Task InvokeAsync(Dispatcher dispatcher, Action action) {
            if (dispatcher.CheckAccess()) {
                action();
                return Task.CompletedTask;
            }
            else {
                return dispatcher.InvokeAsync(action).Task;
            }
        }

        public static Task<TResult> InvokeAsync<TResult>(Dispatcher dispatcher, Func<TResult> function) {
            if (dispatcher.CheckAccess()) {
                return Task.FromResult(function());
            }

            return dispatcher.InvokeAsync(function).Task;
        }

        public static async Task<TResult> InvokeAsync<TResult>(Dispatcher dispatcher, Func<Task<TResult>> function) {
            if (dispatcher.CheckAccess()) {
                return await function();
            }

            Task<TResult> task = await dispatcher.InvokeAsync(function);
            return await task;
        }
    }
}