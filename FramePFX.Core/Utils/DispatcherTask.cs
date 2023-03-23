using System;

namespace FramePFX.Core.Utils {
    public class DispatcherTask {
        private readonly Func<bool> canExecute;
        private readonly Action action;
        private volatile bool isCompleted;

        public bool IsCompleted {
            get => this.isCompleted;
            set => this.isCompleted = value;
        }

        public DispatcherTask(Func<bool> canExecute, Action action) {
            this.canExecute = canExecute;
            this.action = action;
        }

        public static void FireAndForget(Func<bool> canExecute, Action action) {
            new DispatcherTask(canExecute, action).AttemptExecuteOrRegisterTask();
        }

        public void RegisterTask() {
            if (this.isCompleted) {
                return;
            }

            IoC.Dispatcher.InvokeLater(this.AttemptExecuteOrRegisterTask);
        }

        public void AttemptExecuteOrRegisterTask() {
            if (this.isCompleted) {
                return;
            }

            if (this.canExecute()) {
                try {
                    this.action();
                }
                finally {
                    this.isCompleted = true;
                }
            }
            else {
                this.RegisterTask();
            }
        }
    }
}