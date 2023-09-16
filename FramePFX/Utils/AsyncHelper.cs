using System;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Services;

namespace FramePFX.Utils {
    public static class AsyncHelper {
        public static void ExecuteTask(Task task, bool forceLoop = false) {
            if (!task.IsCompleted && task.Status < TaskStatus.Running)
                task.Start();
            IApplication application = IoC.Application;
            if (forceLoop || !application.IsOnOwnerThread) {
                while (!task.IsCompleted) {
                    Thread.Sleep(1);
                }
            }
            else {
                while (!task.IsCompleted) {
                    IoC.Application.Invoke(() => Thread.Sleep(1), ExecutionPriority.Background);
                }
            }

            Exception e;
            if (task.IsFaulted && (e = task.Exception) != null) {
                throw e.InnerException ?? e;
            }
        }
    }
}