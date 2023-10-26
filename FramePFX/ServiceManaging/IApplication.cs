using System;
using System.Threading.Tasks;

namespace FramePFX.ServiceManaging {
    /// <summary>
    /// An interface used to do things with the main application
    /// </summary>
    public interface IApplication {
        /// <summary>
        /// Whether or not the caller is on the application thread or not. When true, using any of the dispatcher functions is typically unnecessary
        /// </summary>
        bool IsOnOwnerThread { get; }

        /// <summary>
        /// Whether or not this application is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Whether or not the dispatcher is currently suspended. When suspended,
        /// invoking any dispatcher methods will cause an exception to be thrown
        /// </summary>
        bool IsDispatcherSuspended { get; }

        /// <summary>
        /// Synchronously executes the given function on the UI thread, or dispatches its execution on the UI thread if we are not
        /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        void Invoke(Action action, ExecutionPriority priority = ExecutionPriority.Send);

        /// <summary>
        /// Synchronously executes the given function on the UI thread, or dispatches its execution on the UI thread if we are not
        /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="parameter">A parameter to pass to the action</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="T">Type of parameter</typeparam>
        void Invoke<T>(Action<T> action, T parameter, ExecutionPriority priority = ExecutionPriority.Send);

        /// <summary>
        /// The same as <see cref="Invoke"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>The return value of the parameter '<see cref="function"/>'</returns>
        T Invoke<T>(Func<T> function, ExecutionPriority priority = ExecutionPriority.Send);

        /// <summary>
        /// Asynchronously executes the given function on the UI thread, or dispatches its execution on the UI thread
        /// if we are not currently on it. This is the best way to execute a function on the UI thread asynchronously
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task InvokeAsync(Action action, ExecutionPriority priority = ExecutionPriority.Normal);

        // Unless we are on the main thread and priority is Send, Invoke and InvokeAsync with the parameter provides
        // practically no additional performance benifits for valuetype objects, because the parameter has to
        // get boxed anyway, and not to mention the fact that WPF dispatcher operations create a instance of
        // DispatcherOperationTaskSource which also creates a TaskCompletionSource and DispatcherOperationTaskMapping
        // AND an instance of CulturePreservingExecutionContext gets created too...

        /// <summary>
        /// Asynchronously executes the given function on the UI thread, or dispatches its execution on the UI thread
        /// if we are not currently on it. This is the best way to execute a function on the UI thread asynchronously
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="parameter">A parameter to pass to the action</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task InvokeAsync<T>(Action<T> action, T parameter, ExecutionPriority priority = ExecutionPriority.Normal);

        /// <summary>
        /// The same as <see cref="InvokeAsync"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task<T> InvokeAsync<T>(Func<T> function, ExecutionPriority priority = ExecutionPriority.Normal);
    }
}