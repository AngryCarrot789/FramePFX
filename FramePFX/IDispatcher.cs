//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FramePFX {
    /// <summary>
    /// Provides a way of queueing work on a thread, synchronously (blocking waiting for completion) or asynchronously (task representing the completion)
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
        void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Send);

        /// <summary>
        /// Synchronously executes the given function on the UI thread, or dispatches its execution on the UI thread if we are not
        /// currently on it. This effectively blocks the current thread until the <see cref="Action"/> returns
        /// <para>
        /// Unless already on the main thread with a priority of <see cref="DispatcherPriority.Send"/>,
        /// <see cref="Invoke"/> should be preferred over this method when an additional parameter is needed
        /// due to the late-bound dynamic method invocation, which a lambda closure will likely outperform
        /// </para>
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="parameter">A parameter to pass to the action</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="T">Type of parameter</typeparam>
        void Invoke<T>(Action<T> action, T parameter, DispatcherPriority priority = DispatcherPriority.Send);

        /// <summary>
        /// The same as <see cref="Invoke"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>The return value of the parameter '<see cref="function"/>'</returns>
        T Invoke<T>(Func<T> function, DispatcherPriority priority = DispatcherPriority.Send);

        /// <summary>
        /// Asynchronously executes the given function on the UI thread, or dispatches its execution on the UI thread
        /// if we are not currently on it. This is the best way to execute a function on the UI thread asynchronously
        /// </summary>
        /// <param name="action">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <param name="token"></param>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken token = default);

        /// <summary>
        /// The same as <see cref="InvokeAsync"/> but allows a return value
        /// </summary>
        /// <param name="function">The function to execute on the UI thread</param>
        /// <param name="priority">The priority of the dispatch</param>
        /// <param name="token"></param>
        /// <typeparam name="TResult">The return value for the function</typeparam>
        /// <returns>A task that can be awaited, which is completed once the function returns on the UI thread</returns>
        Task<T> InvokeAsync<T>(Func<T> function, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken token = default);
    }
}