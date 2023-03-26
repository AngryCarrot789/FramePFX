using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Render {
    /// <summary>
    /// Represents a thread that actions can be dispatched to
    /// </summary>
    public class DispatchThread {
        private readonly CASLockV2 actionLock;
        private readonly CASLockV2 taskLock;
        private readonly List<Action> actions;
        private readonly List<Task> tasks;
        private volatile bool isThreadRunning;
        private volatile bool isFullyStopped;

        public Thread Thread { get; }

        /// <summary>
        /// Whether this thread is supposed to be running or not
        /// </summary>
        public bool IsRunning => this.isThreadRunning;

        /// <summary>
        /// Whether the actual thread has fully stopped or not
        /// </summary>
        public bool IsFullyStopped => this.isFullyStopped;

        public DispatchThread() {
            this.actionLock = new CASLockV2();
            this.taskLock = new CASLockV2();
            this.actions = new List<Action>();
            this.tasks = new List<Task>();
            this.Thread = new Thread(this.ThreadMain);
        }

        public void Start() {
            if (this.isFullyStopped) {
                throw new InvalidOperationException("Thread has already been stopped");
            }
            else if (this.isThreadRunning) {
                throw new InvalidOperationException("Thread is already running");
            }

            this.isThreadRunning = true;
            this.Thread.Start();
        }

        public void Stop(bool join) {
            this.isThreadRunning = false;
            if (join) {
                this.Thread.Join();
            }
        }

        public void Invoke(Action action, bool forceInvokeLater = false) {
            if (forceInvokeLater || Thread.CurrentThread != this.Thread) {
                // The tick thread may call Invoke() or InvokeAsync(), so
                // it's still a good idea to take into account the lock type
                this.actionLock.Lock(true);
                this.actions.Add(action);
                this.actionLock.Unlock();
            }
            else {
                action();
            }
        }

        public Task InvokeAsync(Action action, bool forceInvokeLater = false) {
            if (forceInvokeLater || Thread.CurrentThread != this.Thread) {
                Task task = new Task(action);
                this.actionLock.Lock(true);
                this.tasks.Add(task);
                this.actionLock.Unlock();
                return task;
            }
            else {
                action();
                return Task.CompletedTask;
            }
        }

        private void ThreadMain() {
            this.OnThreadStart();
            while (this.isThreadRunning) {
                this.ProcessPendingActions();
                if (this.isThreadRunning) {
                    this.OnThreadTick();
                }
                else {
                    break;
                }
            }

            this.isFullyStopped = true;
            this.OnThreadStop();
        }

        protected virtual void OnThreadStart() {

        }

        protected virtual void OnThreadTick() {

        }

        protected virtual void OnThreadStop() {

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