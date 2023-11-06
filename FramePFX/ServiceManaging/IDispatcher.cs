using System;
using System.Threading.Tasks;
using FramePFX.App;

namespace FramePFX.ServiceManaging {
    /// <summary>
    /// An interface used to access an application dispatcher. This can be used to execute methods on the owner thread
    /// </summary>
    public interface IDispatcher {
        /// <summary>
        /// Whether or not the caller is on the application thread or not. When true, using any of the dispatcher functions is typically unnecessary
        /// </summary>
        bool IsOnOwnerThread { get; }

        /// <summary>
        /// Returns true if the dispatcher is currently suspended. Dispatcher suspension usually occurs
        /// during the render phase, meaning, you cannot use the synchronous invoke methods of the dispatcher while on
        /// the dispatcher thread, because they require pushing a new dispatch frame which is not allowed during suspension.
        /// However, async invoke methods are allowed as they don't require pushing a dispatcher frame
        /// </summary>
        bool IsSuspended { get; }

        /// <summary>
        /// Synchronously executes the given function on the UI thread, or dispatches its execution on the UI thread if we are not
        /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        void Invoke(Action action, DispatchPriority priority = DispatchPriority.Send);

        /// <summary>
        /// Synchronously executes the given function on the UI thread, or dispatches its execution on the UI thread if we are not
        /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
        /// <para>
        /// Unless already on the main thread with a priority of <see cref="DispatchPriority.Send"/>,
        /// <see cref="Invoke"/> should be preferred over this method when an additional parameter is needed
        /// due to the late-bound dynamic method invocation, which a lambda closure will likely outperform
        /// </para>
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="parameter">A parameter to pass to the action</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="T">Type of parameter</typeparam>
        void Invoke<T>(Action<T> action, T parameter, DispatchPriority priority = DispatchPriority.Send);

        /// <summary>
        /// The same as <see cref="Invoke"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>The return value of the parameter '<see cref="function"/>'</returns>
        T Invoke<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Send);

        /// <summary>
        /// Asynchronously executes the given function on the UI thread, or dispatches its execution on the UI thread
        /// if we are not currently on it. This is the best way to execute a function on the UI thread asynchronously
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task InvokeAsync(Action action, DispatchPriority priority = DispatchPriority.Normal);

        // Unless we are on the main thread and priority is Send, Invoke and InvokeAsync with the parameter provides
        // practically no additional performance benifits for valuetype objects, because the parameter has to
        // get boxed anyway, and not to mention the fact that WPF dispatcher operations create a instance of
        // DispatcherOperationTaskSource which also creates a TaskCompletionSource and DispatcherOperationTaskMapping
        // AND an instance of CulturePreservingExecutionContext gets created too...

        /// <summary>
        /// Asynchronously executes the given function on the UI thread. If we are already on the main thread and
        /// <see cref="priority"/> is <see cref="DispatchPriority.Send"/> then the action is invoked synchronously.
        /// <para>
        /// Unless already on the main thread with a priority of <see cref="DispatchPriority.Send"/>,
        /// <see cref="InvokeAsync"/> should be preferred over this method when an additional parameter is needed
        /// due to the late-bound dynamic method invocation, which a lambda closure will likely outperform
        /// </para>
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="parameter">A parameter to pass to the action</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task InvokeAsync<T>(Action<T> action, T parameter, DispatchPriority priority = DispatchPriority.Normal);

        /// <summary>
        /// The same as <see cref="InvokeAsync"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority = DispatchPriority.Normal);
    }
}