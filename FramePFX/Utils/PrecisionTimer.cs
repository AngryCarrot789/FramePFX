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
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Utils
{
    public class PrecisionTimer : IDisposable
    {
        private const long MILLIS_PER_THREAD_SPLICE = 16; // 16.4

        private static readonly long THREAD_SPLICE_IN_TICKS = (long) (16.4d * Time.TICK_PER_MILLIS);

        // 1.71ms to 2.3ms is the max yield interval i found
        // private static readonly long YIELD_MILLIS_IN_TICKS = (long) (1.71d * Time.TICK_PER_MILLIS);
        private static readonly long YIELD_MILLIS_IN_TICKS = Time.TICK_PER_MILLIS / 10;

        public Action TickCallback { get; set; }

        public long Interval
        {
            get => this.intervalMillis;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be bigger than 0");
                }

                this.intervalMillis = value;
                this.intervalTicks = value * Time.TICK_PER_MILLIS;
            }
        }

        private Task task;
        private Timer osTimer;
        private volatile bool isRunning;
        private long nextTickTime;
        private long intervalMillis;
        private long intervalTicks;

        public PrecisionTimer()
        {
        }

        public void Start(bool usePrecisionMode)
        {
            this.isRunning = true;
            this.SetupTask(usePrecisionMode);
        }

        public async Task StopAsync()
        {
            this.isRunning = false;
            if (this.task != null)
            {
                try
                {
                    if (!this.task.IsCanceled && !this.task.IsCompleted)
                    {
                        await this.task;
                    }
                }
                catch
                {
                    /* ignored */
                }

                this.task = null;
            }

            if (this.osTimer != null)
            {
                try
                {
                    this.osTimer.Dispose();
                }
                catch
                {
                    /* ignored */
                }

                this.osTimer = null;
            }
        }

        public async Task RestartAsync(bool usePrecisionMode)
        {
            await this.StopAsync();
            this.Start(usePrecisionMode);
        }

        private void SetupTask(bool usePrecisionMode)
        {
            this.nextTickTime = Time.GetSystemTicks();
            this.isRunning = true;
            if (usePrecisionMode)
            {
                this.task = Task.Factory.StartNew(this.TaskMain, TaskCreationOptions.LongRunning);
            }
            else
            {
                this.osTimer = new Timer((s) =>
                {
                    if (this.isRunning)
                    {
                        this.OnTimerTick();
                    }
                }, null, 0, 1);
            }
        }

        private void TaskMain()
        {
            do
            {
                long target = this.nextTickTime;
                while ((target - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS)
                    Thread.Sleep(1);
                while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS)
                    Thread.Yield();

                // CPU intensive wait
                long time = Time.GetSystemTicks();
                while (time < target)
                {
                    Thread.SpinWait(8);
                    time = Time.GetSystemTicks();
                }

                this.nextTickTime = Time.GetSystemTicks() + this.intervalTicks;
                if (!this.isRunning)
                    return;
                this.TickCallback?.Invoke();
            } while (true);
        }

        private void OnTimerTick()
        {
            long currentTime = Time.GetSystemTicks();
            while (currentTime >= this.nextTickTime)
            {
                this.nextTickTime += this.intervalTicks;
                long a = Time.GetSystemTicks();
                this.TickCallback?.Invoke();
                long b = Time.GetSystemTicks() - a;
                if (b >= this.nextTickTime)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                this.task?.Dispose();
            }
            catch
            {
                /* ignored */
            }

            this.osTimer?.Dispose();
            this.TickCallback = null;
        }
    }
}