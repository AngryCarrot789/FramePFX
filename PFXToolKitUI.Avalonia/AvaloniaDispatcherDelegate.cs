using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PFXToolKitUI.Avalonia;

/// <summary>
/// A delegate around the avalonia dispatcher so that core projects can access it, since features RateLimitedDispatchAction require it
/// </summary>
public class AvaloniaDispatcherDelegate : IDispatcher {
    private static readonly Action EmptyAction = () => {
    };

    private readonly Dispatcher dispatcher;

    public AvaloniaDispatcherDelegate(Dispatcher dispatcher) {
        this.dispatcher = dispatcher;
    }

    public bool CheckAccess() {
        return this.dispatcher.CheckAccess();
    }

    public void VerifyAccess() {
        this.dispatcher.VerifyAccess();
    }

    public void Invoke(Action action, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
            action();
        }
        else {
            this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
        }
    }

    public T Invoke<T>(Func<T> function, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
            return function();
        return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
    }

    public Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
        return this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token).GetTask();
    }

    public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
        return this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token).GetTask();
    }

    public void Post(Action action, DispatchPriority priority = DispatchPriority.Default) {
        this.dispatcher.Post(action, ToAvaloniaPriority(priority));
    }

    public Task Process(DispatchPriority priority) {
        return this.InvokeAsync(EmptyAction, priority);
    }

    public void InvokeShutdown() {
        this.dispatcher.InvokeShutdown();
    }

    private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
        return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
    }
}