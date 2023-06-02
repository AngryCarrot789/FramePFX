using System;
using System.Threading;

namespace FramePFX.Core.Utils {
    public class ThreadTimer {
        private const long MILLIS_PER_THREAD_SPLICE = 16;
        private static readonly long THREAD_SPLICE_IN_TICKS = MILLIS_PER_THREAD_SPLICE * Time.TICK_PER_MILLIS;
        private static readonly long FIVE_MILLIS_IN_TICKS = 5L * Time.TICK_PER_MILLIS;
        private static readonly long HALF_MILLI_IN_TICKS = Time.TICK_PER_MILLIS / 2;

        private static int nextId;
        private TimeSpan interval;
        private Action tickAction;
        private Action startedAction;
        private Action stoppedAction;
        private ThreadPriority priority;
        private volatile bool isRunning;
        private string threadName;
        private volatile bool isMarkedForSlowSleep;

        public bool IsRunning => this.isRunning;

        public bool IsMarkedForSlowSleep {
            get => this.isMarkedForSlowSleep;
            set => this.isMarkedForSlowSleep = value;
        }

        public TimeSpan Interval {
            get => this.interval;
            set {
                this.ValidateNotRunning();
                this.interval = value;
            }
        }

        public string ThreadName {
            get => this.threadName;
            set {
                this.ValidateNotRunning();
                this.threadName = value;
            }
        }

        public ThreadPriority Priority {
            get => this.priority;
            set {
                this.ValidateNotRunning();
                this.priority = value;
            }
        }

        public Action TickAction {
            get => this.tickAction;
            set {
                this.ValidateNotRunning();
                this.tickAction = value;
            }
        }

        public Action StartedAction {
            get => this.startedAction;
            set {
                this.ValidateNotRunning();
                this.startedAction = value;
            }
        }

        public Action StoppedAction {
            get => this.stoppedAction;
            set {
                this.ValidateNotRunning();
                this.stoppedAction = value;
            }
        }

        public Thread Thread { get; private set; }

        public ThreadTimer() {

        }

        public ThreadTimer(TimeSpan interval) {
            this.interval = interval;
        }

        public ThreadTimer(TimeSpan interval, Action tickAction) {
            this.interval = interval;
            this.tickAction = tickAction;
        }

        private void MainThread() {
            Action action = this.tickAction ?? throw new Exception("No tick action was set");
            this.startedAction?.Invoke();
            long interval_ticks = this.interval.Ticks;
            while (this.isRunning) {
                if (this.isMarkedForSlowSleep) {
                    // try to save lots of CPU time, at the cost of bad timing (+- 16ms)
                    Thread.Sleep(15);
                }

                // Get the target time that the action should be executed
                long targetTime = Time.GetSystemTicks() + interval_ticks;

                // While the waiting time is larger than the thread splice time
                // (target - time) == duration to wait
                while ((targetTime - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS) {
                    Thread.Sleep(1); // sleep for roughly 15-16ms, on windows at least
                }

                // targetTime will likely be larger than GetSystemTicks(), e.g the interval is 20ms
                // and we delayed for about 16ms, so extraWaitTime is about 4ms
                while ((targetTime - Time.GetSystemTicks()) > FIVE_MILLIS_IN_TICKS) {
                    // Thread.Sleep(1);

                    // Yield may result in more precise timing
                    Thread.Yield();
                }

                // CPU intensive wait
                while (Time.GetSystemTicks() < targetTime) {
                    Thread.SpinWait(32);
                    // SpinWait may result in more precise timing
                    // Thread.Yield();
                }

                if (this.isRunning) {
                    action();
                }
            }

            this.stoppedAction?.Invoke();
            this.Thread = null;
        }

        public void Start(bool joinIfRunning = true) {
            if (this.isRunning) {
                throw new InvalidOperationException("Timer is already running");
            }

            if (this.tickAction == null) {
                throw new InvalidOperationException("Tick action is not present. Cannot start thread without tick-action");
            }

            // isRunning is already false, so it should
            if (this.Thread != null) {
                if (joinIfRunning)
                    this.Thread.Join();
                this.Thread = null;
            }

            this.isRunning = true;
            this.Thread = new Thread(this.MainThread) {
                Priority = this.Priority,
                Name = this.ThreadName ?? $"Timer Thread #{nextId++}"
            };

            this.Thread.Start();
        }

        public void Stop(bool join = true) {
            if (!this.isRunning) {
                throw new InvalidOperationException("Thread is not running");
            }

            // mark thread to stop
            this.isRunning = false;

            // wait for thread to stop
            if (join) {
                this.Thread.Join();
                this.Thread = null;
            }
        }

        private void ValidateNotRunning() {
            if (this.isRunning) {
                throw new InvalidOperationException("Thread is running");
            }
        }
    }
}