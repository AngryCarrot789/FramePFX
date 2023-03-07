using System;
using System.Threading.Tasks;

namespace FrameControl.Core.Services {
    public interface IDispatcher {
        void Invoke(Action action);
        T Invoke<T>(Func<T> function);
        Task InvokeAsync(Action action);
        Task<T> InvokeAsync<T>(Func<T> function);
    }
}