using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Services
{
    /// <summary>
    /// An interface used to execute actions on a specific thread
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Whether or not the caller is on the owner thread or not. When true, using any of the dispatcher functions is typically unnecessary
        /// </summary>
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