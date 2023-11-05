using System;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.App;
using FramePFX.ServiceManaging;

namespace FramePFX.Utils {
    public static class AsyncUtils {
        public static void ExecuteWriteTask(Task task, bool forceLoop = false) {
            if (!task.IsCompleted && task.Status < TaskStatus.Running)
                task.Start();
            if (forceLoop || !IoC.Application.IsOnMainThread) {
                while (!task.IsCompleted) {
                    Thread.Sleep(1);
                }
            }
            else {
                while (!task.IsCompleted) {
                    IoC.Dispatcher.Invoke(() => Thread.Sleep(1), DispatchPriority.Background);
                }
            }

            AggregateException e;
            if (task.IsFaulted && (e = task.Exception) != null) {
                throw e;
            }
        }

        public static Task<T> ReturnConst<T>(this Task task, T value) {
            if (task.IsCompleted)
                return Task.FromResult(value);
            return task.ContinueWith(x => value);
        }
    }
}