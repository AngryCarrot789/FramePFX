using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Services {
    public interface IDispatcher {
        /// <summary>
        /// Schedules a task to be invoked later. The action will not be called during this
        /// method invocation, but will be called on the dispatcher thread
        /// </summary>
        /// <param name="action"></param>
        void InvokeLater(Action action);

        void Invoke(Action action);

        T Invoke<T>(Func<T> function);

        Task InvokeAsync(Action action);

        Task<T> InvokeAsync<T>(Func<T> function);
    }
}