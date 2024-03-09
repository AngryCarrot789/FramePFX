//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Threading.Tasks;

namespace FramePFX.Utils
{
    /// <summary>
    /// A class used for executing a tasks when an input signal is received, and ensuring the task is not
    /// executed too quickly (time since last execution will exceed <see cref="MinimumInterval"/>).
    /// </summary>
    public class RateLimitedExecutor
    {
        private volatile bool isCondition;
        private volatile bool isTaskRunning;
        private volatile bool isExecutingTask;
        private volatile bool isCriticalCondition;
        private DateTime lastExecutionTime = DateTime.MinValue;
        private readonly object locker = new object();

        /// <summary>
        /// A function that returns a task that is awaited when required. This is executed on a
        /// task scheduler thread, so it must be prepared to use the application dispatcher (if necessary)
        /// </summary>
        public Func<Task> Execute { get; set; }

        /// <summary>
        /// The smallest amount of time that must pass before the <see cref="Execute"/> task may be executed again
        /// </summary>
        public TimeSpan MinimumInterval { get; set; }

        public RateLimitedExecutor() : this(null) { }

        public RateLimitedExecutor(Func<Task> execute) : this(execute, TimeSpan.FromMilliseconds(250)) { }

        public RateLimitedExecutor(Func<Task> execute, TimeSpan minimumInterval)
        {
            this.Execute = execute;
            this.MinimumInterval = minimumInterval;

            Progress<string> cum = new Progress<string>((s) =>
            {
            });

            // cum.Report("sexxx");
        }

        /// <summary>
        /// Triggers this executor, possibly starting a new <see cref="Task"/>, or notifying the existing internal task that there's new input
        /// </summary>
        public void OnInput()
        {
            lock (this.locker)
            {
                if (this.isExecutingTask)
                {
                    this.isCriticalCondition = true;
                }
                else
                {
                    this.isCondition = true;
                }

                if (!this.isTaskRunning)
                {
                    this.isTaskRunning = true;
                    Task.Run(this.TaskMain);
                }
            }
        }

        private async Task TaskMain()
        {
            do
            {
                // Ensure maximum interval
                TimeSpan timeSinceLastExecute = DateTime.Now - this.lastExecutionTime;
                TimeSpan minInterval = this.MinimumInterval;
                if (timeSinceLastExecute < minInterval)
                    await Task.Delay(minInterval - timeSinceLastExecute);

                lock (this.locker)
                {
                    if (this.isCondition)
                    {
                        this.isCondition = false;
                    }
                    else
                    {
                        this.isTaskRunning = false;
                        return;
                    }
                }

                this.isExecutingTask = true;
                try
                {
                    Func<Task> func = this.Execute;
                    Task task = func?.Invoke();
                    if (task != null && !task.IsCompleted)
                    {
                        await task;
                    }
                }
                finally
                {
                    // This sets condition to false, indicating that there is no more work required.
                    // However there is a window between when the task finishes and condition being set to false
                    // where another thread can set condition to true:
                    //     Task just completes, another thread sets condition from false to true,
                    //     but then that change is overwritten and condition is set to false here
                    //
                    // That might mean that whatever work the task does will lose out on the absolute latest
                    // update (that occurred a few microseconds~ after the task completed)
                    // So hopefully, the usage of isExecutingTask and isCriticalCondition will help against that
                    lock (this.locker)
                    {
                        if (this.isCriticalCondition)
                        {
                            this.isCondition = true;
                            this.isCriticalCondition = false;
                        }
                        else
                        {
                            this.isCondition = false;
                        }
                    }

                    this.isExecutingTask = false;
                }

                this.lastExecutionTime = DateTime.Now;
            } while (true);
        }
    }
}