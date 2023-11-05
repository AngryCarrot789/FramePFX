using System;
using System.Threading;

namespace FramePFX.Utils {
    public class DispatcherTask {
        private readonly Func<bool> canExecute;
        private readonly Action action;
        private volatile bool isCompleted;
        private volatile int isRegistered;
        private volatile int isScheduled;
        private readonly Action onDispatcherAction;

        public bool IsCompleted {
            get => this.isCompleted;
            set => this.isCompleted = value;
        }

        public DispatcherTask(Func<bool> canExecute, Action action) {
            this.canExecute = canExecute;
            this.action = action;
            this.onDispatcherAction = this.OnDispatcherAction;
        }

        public static void FireAndForget(Func<bool> canExecute, Action action) {
            new DispatcherTask(canExecute, action).AttemptExecuteOrRegisterTask();
        }

        public void RegisterTask() {
            if (this.isCompleted || Interlocked.CompareExchange(ref this.isRegistered, 1, 0) != 0) {
                return;
            }

            this.RegisterInternal();
        }

        public void AttemptExecuteOrRegisterTask() {
            if (this.isCompleted) {
                this.isScheduled = 0;
                return;
            }

            if (this.canExecute()) {
                try {
                    this.action();
                }
                finally {
                    this.isCompleted = true;
                    this.isScheduled = 0;
                }
            }
            else if (Interlocked.CompareExchange(ref this.isScheduled, 1, 0) == 0) {
                this.RegisterInternal();
            }
        }

        private void RegisterInternal() => IoC.Application.InvokeOnMainThreadAsync(this.onDispatcherAction);

        private void OnDispatcherAction() {
            this.isScheduled = 0;
            this.AttemptExecuteOrRegisterTask();
        }
    }
}