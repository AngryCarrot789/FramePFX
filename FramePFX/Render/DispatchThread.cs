using System;
using System.Threading;

namespace FramePFX.Render {
    /// <summary>
    /// Represents a thread that actions can be dispatched to
    /// </summary>
    public class DispatchThread : DispatchReceiver {
        private static int nextId;
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
            this.Thread = new Thread(this.ThreadMain) {
                Name = $"Dispatch Thread #{nextId++}"
            };
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
                // this.Thread.Join();
            }
        }

        public override bool IsOnOwnerThread() {
            return this.Thread == Thread.CurrentThread;
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
    }
}