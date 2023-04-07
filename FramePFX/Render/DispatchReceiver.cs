using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Render {
    public abstract class DispatchReceiver {
        protected readonly CASLock actionLock;
        protected readonly CASLock taskLock;
        protected readonly List<Action> actions;
        protected readonly List<Task> tasks;

        protected DispatchReceiver() {
            this.actionLock = new CASLock();
            this.taskLock = new CASLock();
            this.actions = new List<Action>();
            this.tasks = new List<Task>();
        }

        /// <summary>
        /// Checks if the current thread owns this dispatch receiver
        /// </summary>
        public abstract bool IsOnOwnerThread();

        public void Invoke(Action action, bool invokeLater = false) {
            if (!invokeLater && this.IsOnOwnerThread()) {
                action();
            }
            else {
                this.actionLock.Lock(true);
                this.actions.Add(action);
                this.actionLock.Unlock();
                this.OnActionEnqueued(action, invokeLater);
            }
        }

        public Task InvokeAsync(Action action, bool invokeLater = false) {
            if (!invokeLater && this.IsOnOwnerThread()) {
                action();
                return Task.CompletedTask;
            }
            else {
                Task task = new Task(action);
                this.taskLock.Lock(true);
                this.tasks.Add(task);
                this.taskLock.Unlock();
                this.OnAsyncActionEnqueued(action, invokeLater);
                return task;
            }
        }

        protected virtual void OnActionEnqueued(Action action, bool invokeLater) {

        }

        protected virtual void OnAsyncActionEnqueued(Action action, bool invokeLater) {

        }

        protected virtual void ProcessPendingActions() {
            if (this.actionLock.Lock(false)) {
                foreach (Action action in this.actions)
                    action();
                this.actions.Clear();
                this.actionLock.Unlock();
            }

            if (this.taskLock.Lock(false)) {
                foreach (Task action in this.tasks)
                    action.RunSynchronously();
                this.tasks.Clear();
                this.taskLock.Unlock();
            }
        }
    }
}