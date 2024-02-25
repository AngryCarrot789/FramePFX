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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FramePFX {
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

        public void Invoke(Action action, DispatcherPriority priority) {
            if (priority == DispatcherPriority.Send && this.dispatcher.CheckAccess()) {
                action();
            }
            else {
                this.dispatcher.Invoke(action, priority);
            }
        }

        // Unless we are on the main thread and priority is Send, Invoke with the parameter provides
        // practically no additional performance benifits for valuetype objects, because the parameter
        // has to get boxed anyway, and not to mention the fact that WPF dispatcher operations create
        // an instance of DispatcherOperationTaskSource which also creates a TaskCompletionSource and
        // DispatcherOperationTaskMapping AND an instance of CulturePreservingExecutionContext gets created too...

        public void Invoke<T>(Action<T> action, T parameter, DispatcherPriority priority) {
            if (priority == DispatcherPriority.Send && this.dispatcher.CheckAccess()) {
                action(parameter);
            }
            else {
                this.dispatcher.Invoke(priority, action, parameter);
            }
        }

        public T Invoke<T>(Func<T> function, DispatcherPriority priority) {
            if (priority == DispatcherPriority.Send && this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function, priority);
        }

        public Task InvokeAsync(Action action, DispatcherPriority priority, CancellationToken token = default) {
            return this.dispatcher.InvokeAsync(action, priority, token).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function, DispatcherPriority priority, CancellationToken token = default) {
            return this.dispatcher.InvokeAsync(function, priority, token).Task;
        }
    }
}