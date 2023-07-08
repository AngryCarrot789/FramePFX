using System;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// A class primarily used for invoking an async action from a non-async context, while providing helpers to
    /// handle the case when a task is still running when attempting to
    /// </summary>
    public class AsyncLock {
        private readonly Func<Task> taskFunc;
        private volatile Task task;

        public AsyncLock(Func<Task> taskFunc = null) {
            this.taskFunc = taskFunc;
        }

        public bool CanExecute() {
            return this.task == null;
        }

        public bool CanExecute(int spins) {
            Thread.SpinWait(spins);
            return this.CanExecute();
        }

        public void Execute() {
            this.Execute(this.taskFunc);
        }

        public async void Execute(Func<Task> func) {
            if (this.task != null) {
                throw new Exception($"Already running. {nameof(this.CanExecute)} should be checked");
            }

            Task t = func();
            if (t == null) {
                throw new Exception("Func returned a null task");
            }

            this.task = t;
            try {
                await t;
            }
            finally {
                this.task = null;
            }
        }

        public Task ExecuteAsync() {
            return this.ExecuteAsync(this.taskFunc);
        }

        public async Task ExecuteAsync(Func<Task> func) {
            Task t = this.task;
            if (t != null) {
                try {
                    await t;
                }
                catch (TaskCanceledException) { }
            }

            if ((t = func()) == null) {
                throw new Exception("Func returned a null task");
            }

            this.task = t;
            try {
                await t;
            }
            finally {
                this.task = null;
            }
        }
    }
}