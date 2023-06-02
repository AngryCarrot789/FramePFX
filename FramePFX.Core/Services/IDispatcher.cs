using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Services {
    public interface IDispatcher {
        bool IsOnOwnerThread { get; }

        void Invoke(Action action);
        void InvokeLater(Action action, bool wayLater = false);

        T Invoke<T>(Func<T> function);
        T InvokeLater<T>(Func<T> function, bool wayLater = false);

        Task InvokeAsync(Action action);
        Task InvokeLaterAsync(Action action, bool wayLater = false);

        Task<T> InvokeAsync<T>(Func<T> function);
        Task<T> InvokeLaterAsync<T>(Func<T> function, bool wayLater = false);
    }
}