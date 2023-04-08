using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using FramePFX.Core.Services;

namespace FramePFX {
    public class DispatcherDelegate : IDispatcher {
        private readonly Dispatcher dispatcher;

        public DispatcherDelegate(Dispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void InvokeLater(Action action) {
            this.dispatcher.Invoke(action, DispatcherPriority.Normal);
        }

        public void Invoke(Action action) {
            this.dispatcher.Invoke(action);
        }

        public T Invoke<T>(Func<T> function) {
            return this.dispatcher.Invoke(function);
        }

        public async Task InvokeAsync(Action action) {
            await this.dispatcher.InvokeAsync(action);
        }

        public async Task<T> InvokeAsync<T>(Func<T> function) {
            return await this.dispatcher.InvokeAsync(function);
        }
    }
}