using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Utils {
    public static class DispatcherUtils {
        public static void ThrowOrLog(string msg) {
            #if DEBUG
            throw new Exception(msg);
            #else
            FramePFX.Core.AppLogger.WriteLine(msg);
            #endif
        }

        public static Task WaitUntilRenderPhase(Dispatcher dispatcher) {
            return dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render).Task;
        }

        public static Task WaitUntilBackgroundActivity(Dispatcher dispatcher) {
            return dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background).Task;
        }

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

        public static void Invoke(Action action) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                Invoke(dispatcher, action);
            }

            throw new Exception("Application main thread is unavailable");
        }

        public static TResult Invoke<TResult>(Func<TResult> function) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                return Invoke(dispatcher, function);
            }

            throw new Exception("Application main thread is unavailable");
        }

        /// <summary>
        /// Creates a new task which, when awaited, will invoke the given function on the given dispatcher. If the current thread 
        /// owns the dispatcher, action is invoked and <see cref="Task.CompletedTask"/> is returned
        /// <para>
        /// This basically converts a function into a task, and may not actually invoke the function until needed
        /// </para>
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to invoke on</param>
        /// <param name="action">The action to invoke</param>
        /// <returns>A task that can be awaited which will execute the given function on the dispatcher thread</returns>
        public static Task InvokeAsync(Dispatcher dispatcher, Action action) {
            if (dispatcher.CheckAccess()) {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action).Task;
        }

        /// <summary>
        /// Creates a new task which, when awaited, will invoke the given function on the given dispatcher. If the current thread 
        /// owns the dispatcher, the function is invoked and <see cref="Task.FromResult{TResult}(TResult)"/> is returned
        /// <para>
        /// This basically converts a function into a task, and may not actually invoke the function until needed
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="dispatcher">The target dispatcher to invoke on</param>
        /// <param name="function">The function to invoke</param>
        /// <returns>A task that can be awaited which will execute the given function on the dispatcher thread</returns>
        public static Task<TResult> InvokeAsync<TResult>(Dispatcher dispatcher, Func<TResult> function) {
            if (dispatcher.CheckAccess())
                return Task.FromResult(function());
            return dispatcher.InvokeAsync(function).Task;
        }

        /// <summary>
        /// Synchronously invokes the given function on the dispatcher thread and waits for it to complete (halting the current thread 
        /// until the dispatcher completes the function). If the current thread owns the dispatcher, then the function is invoked in this method
        /// <para>
        /// This basically invokes the function on the dispatcher's thread and returns the return value
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="dispatcher">The target dispatcher to invoke on</param>
        /// <param name="function">The function to invoke</param>
        /// <returns>The result value from the function</returns>
        public static TResult Invoke<TResult>(Dispatcher dispatcher, Func<TResult> function) {
            if (dispatcher.CheckAccess())
                return function();
            return dispatcher.Invoke(function);
        }

        /// <summary>
        /// Synchronously invokes the given function on the dispatcher thread and waits for it to complete (halting the current thread 
        /// until the dispatcher completes the function). If the current thread owns the dispatcher, then the function is invoked in this method
        /// <para>
        /// This basically invokes the function on the dispatcher's thread
        /// </para>
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to invoke on</param>
        /// <param name="action">The function to invoke</param>
        public static void Invoke(Dispatcher dispatcher, Action action) {
            if (dispatcher.CheckAccess()) {
                action();
            }
            else {
                dispatcher.Invoke(action);
            }
        }
    }
}