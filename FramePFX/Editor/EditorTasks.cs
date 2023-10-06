using System;
using System.Threading;

namespace FramePFX.Editor {
    public static class EditorTasks {
        private static volatile int SuspensionCount;

        public static bool IsRenderSuspended { get; private set; }

        /// <summary>
        /// Suspends rendering by the main thread, incrementing an internal suspension
        /// counter to allow stacked instances of the returned struct
        /// </summary>
        /// <returns></returns>
        public static RenderSuspension SuspendRendering() {
            Interlocked.Increment(ref SuspensionCount);
            IsRenderSuspended = true;
            return new RenderSuspension();
        }

        public struct RenderSuspension : IDisposable {
            public void Dispose() {
                if (Interlocked.Decrement(ref SuspensionCount) < 1) {
                    IsRenderSuspended = false;
                }
            }
        }
    }
}