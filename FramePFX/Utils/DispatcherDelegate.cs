using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using FramePFX.Core.Services;

namespace FramePFX.Utils
{
    public class DispatcherDelegate : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        public bool IsOnOwnerThread => this.dispatcher.CheckAccess();

        public DispatcherDelegate(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher), "Dispatcher cannot be null");
        }

        public void Invoke(Action action)
        {
            if (this.dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                this.dispatcher.Invoke(action);
            }
        }

        public void InvokeLater(Action action, bool wayLater = false)
        {
            this.dispatcher.Invoke(action, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public T Invoke<T>(Func<T> function)
        {
            if (this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function);
        }

        public T InvokeLater<T>(Func<T> function, bool wayLater = false)
        {
            return this.dispatcher.Invoke(function, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal);
        }

        public Task InvokeAsync(Action action)
        {
            return DispatcherUtils.InvokeAsync(this.dispatcher, action);
        }

        public Task InvokeLaterAsync(Action action, bool wayLater = false)
        {
            return this.dispatcher.InvokeAsync(action, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> function)
        {
            return DispatcherUtils.InvokeAsync(this.dispatcher, function);
        }

        public Task<T> InvokeLaterAsync<T>(Func<T> function, bool wayLater = false)
        {
            return this.dispatcher.InvokeAsync(function, wayLater ? DispatcherPriority.Background : DispatcherPriority.Normal).Task;
        }
    }
}