using System;
using System.Threading.Tasks;
using FramePFX.Components;
using FramePFX.ServiceManaging;

namespace FramePFX.App {
    /// <summary>
    /// Represents the application. This class is mainly useful for accessing services and invoking read/write operations
    /// <para>
    /// The idea is that one or more read actions can happen on any number of threads but only as long as there are
    /// no write actions
    /// <para>
    /// Read actions cannot occur while a write action is in progress, and instead, the thread that is attempting
    /// to invoke a read operation will block until that write operation is completed. Read operations should be
    /// brief (due to the reasons below) and should check <see cref="IsWriteActionPending"/> every now and then,
    /// typically before a moderately expensive operation, just to check they won't be blocking the UI.
    /// </para>
    /// <para>
    /// Write actions cannot occur while read actions are in progress, and instead, the write thread (aka main thread)
    /// will block until that write operation is completed. This is generally a bad idea, because only the write thread
    /// can modify the underlying application UI states, meaning read operations will block the entire UI until they are
    /// all completed
    /// <para>
    /// Another thing as well is, write actions should be used when you want to modify the application data state (ADS),
    /// such as adding or removing clips, changing a track's opacity or display name, etc. These operations should not happen
    /// outside of a write action, because read operations could be reading as the application modifies those values, which is
    /// why the read/write actions should be used instead of <see cref="IDispatcher"/> usage
    /// </para>
    /// </para>
    /// <para>
    /// However, UI-specific modifications don't necessarily need to be wrapped in a write action, such as changing the background
    /// of a button, updating the text of a button based on a data object (read action not needed as modifications can only happen
    /// on the main thread, therefore, nothing is going to change during button text update), rendering a control, etc.
    /// </para>
    /// </para>
    /// </summary>
    public interface IApplication : IServiceProviderEx {
        /// <summary>
        /// Whether or not this application is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Returns true if we are on the write thread (aka the main application thread)
        /// </summary>
        bool IsOnMainThread { get; }

        /// <summary>
        /// Returns true if a write action is currently trying to acquire the write lock
        /// </summary>
        bool IsWriteActionPending { get; }

        /// <summary>
        /// Returns true is write access is granted, this is, when we are on the write thread and the
        /// write lock is acquired. Writing while the lock is not acquired is not allowed, and may corrupt
        /// the state of the application in unpredictable ways
        /// </summary>
        bool IsWriteAccessAllowed { get; }

        /// <summary>
        /// Returns true if read access is granted, this is, when we are either on the write thread, or
        /// the read lock is held in the current thread
        /// </summary>
        bool IsReadAccessAllowed { get; }

        /// <summary>
        /// Returns true if the application dispatcher is currently suspended. Dispatcher suspension usually occurs
        /// during the render phase, meaning, you cannot use the synchronous invoke methods of the dispatcher while on
        /// the main thread, because they require pushing a new dispatch frame which is not allowed during suspension.
        /// However, async invoke methods are allowed as they don't require pushing a dispatcher frame
        /// </summary>
        bool IsDispatcherSuspended { get; }

        /// <summary>
        /// Gets the current version of the application
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Invokes the given action as a read action. This can be called from any thread. The function may be immediately
        /// invoked if on the main thread or the read lock is already acquired on the current thread.
        /// When acquiring the lock, if there's write operations in progress then this method will block until they are completed
        /// </summary>
        /// <param name="action">The action to invoke while the read lock is acquired</param>
        void RunReadAction(Action action);

        /// <summary>
        /// Same as <see cref="RunReadAction"/> except allows a return value
        /// </summary>
        /// <param name="func">The function to invoke while the read lock is acquired</param>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The return value from the function</returns>
        T RunReadAction<T>(Func<T> func);

        /// <summary>
        /// Runs the given action as a write action. This can only be called on the main thread (AMT). If there are
        /// read operations in progress, this method blocks until they're all completed. Read operations have a chance
        /// of stopping early as the <see cref="BeforeWriteActionStarted"/> event is fired before attempting
        /// to acquire the lock. Read operations can listen to that event and stop reading, allowing the write operation to start
        /// otherwise
        /// </summary>
        /// <param name="action">The action to invoke while the write lock is acquired</param>
        void RunWriteAction(Action action);

        /// <summary>
        /// Same as <see cref="RunWriteAction"/>, except allows a return value
        /// </summary>
        /// <param name="func">The function to invoke while the write lock is acquired</param>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The return value from the function</returns>
        T RunWriteAction<T>(Func<T> func);

        /// <summary>
        /// Invokes the given action on the main thread, synchronously, with write access. Invokes immediately
        /// if <see cref="priority"/> is <see cref="DispatchPriority.Send"/> and already on the main thread.
        /// <para>
        /// Ensure that when on the main thread but not using <see cref="DispatchPriority.Send"/>,
        /// <see cref="IsDispatcherSuspended"/> is false before calling this on the main thread, otherwise
        /// an exception is thrown for the reasons specified in the property's docs
        /// </para>
        /// </summary>
        /// <param name="action">The action to invoke on the main thread with write access</param>
        /// <param name="priority">
        /// The execution priority, which allows finer control over when the action is invoked. Default
        /// is send, meaning the action can be invoked immediately if on the main thread (bypasses dispatch queue)
        /// </param>
        void InvokeOnMainThread(Action action, DispatchPriority priority = DispatchPriority.Send);

        /// <summary>
        /// Same as <see cref="InvokeOnMainThread"/>, except allows a return value
        /// </summary>
        /// <param name="func">The function to invoke on the main thread with write access</param>
        /// <param name="priority">
        /// The execution priority, which allows finer control over when the action is invoked. Default
        /// is send, meaning the action can be invoked immediately if on the main thread (bypasses dispatch queue)
        /// </param>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The return value from the function</returns>
        T InvokeOnMainThread<T>(Func<T> func, DispatchPriority priority = DispatchPriority.Send);

        /// <summary>
        /// Invokes the given action to be invoked on the main thread, asynchronously, with write access. This
        /// is effectively the same as <see cref="InvokeOnMainThread"/> except non-blocking. Using a priority
        /// of <see cref="DispatchPriority.Send"/> will not bypass the dispatch queue when on the AMT,
        /// unlike the non-async version of this method
        /// </summary>
        /// <param name="action">The action to invoke on the main thread with write access</param>
        /// <param name="priority">
        /// The execution priority, which allows finer control over when the action is invoked. Default is normal
        /// </param>
        /// <returns></returns>
        Task InvokeOnMainThreadAsync(Action action, DispatchPriority priority = DispatchPriority.Normal);

        /// <summary>
        /// Same as <see cref="InvokeOnMainThreadAsync"/>, except allows a return value
        /// </summary>
        /// <param name="func">The function to invoke on the main thread with write access</param>
        /// <param name="priority">
        /// The execution priority, which allows finer control over when the action is invoked. Default is normal
        /// </param>
        /// <typeparam name="T">The return type</typeparam>
        /// <returns>The return value from the function</returns>
        Task<T> InvokeOnMainThreadAsync<T>(Func<T> func, DispatchPriority priority = DispatchPriority.Normal);

        /// <summary>
        /// Creates a token that can be used to enter a write-safe context, blocking until all read operations
        /// are completed and then blocks all read operations until the struct is disposed.
        /// <para>
        /// The returned struct should only really be created and disposed in the same stack frame (e.g. in a
        /// using statement). Not doing this may result in undefined behaviour
        /// </para>
        /// </summary>
        OperationToken CreateWriteToken();

        /// <summary>
        /// Creates a token that can be used to enter a read-safe context, blocking until all write
        /// operations are completed and then blocks all write operations until the struct is disposed.
        /// <para>
        /// The returned struct should only really be created and disposed in the same stack frame (e.g. in a
        /// using statement). Not doing this may result in undefined behaviour
        /// </para>
        /// </summary>
        OperationToken CreateReadToken();

        /// <summary>
        /// Convenience function to throw an exception if not on the main thread
        /// </summary>
        /// <param name="exceptionMessage">The exception message</param>
        void ValidateIsMainThread(string exceptionMessage = "Not on the main/write thread");

        /// <summary>
        /// Convenience function to throw an exception if there is currently no write access (not on main thread or lock not acquired)
        /// </summary>
        /// <param name="exceptionMessage">The exception message</param>
        void ValidateHasWriteAccess(string exceptionMessage = "Write access not granted");

        /// <summary>
        /// Convenience function to throw an exception if there is currently no read access (lock not acquired)
        /// </summary>
        /// <param name="exceptionMessage">The exception message</param>
        void ValidateHasReadAccess(string exceptionMessage = "Read access not granted");
    }
}