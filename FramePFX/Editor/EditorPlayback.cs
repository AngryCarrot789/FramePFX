using System;
using System.Threading;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor {
    public class EditorPlayback {
        private volatile bool isPlaying;
        private volatile int suspensionCount;

        public PrecisionTimer PlaybackTimer { get; }

        public bool UsePrecisionTimingMode { get; set; }

        public bool IsPlaybackRenderAllowed => this.suspensionCount < 1;

        /// <summary>
        /// A UI specific thing. Whether or not zooming into the view port will zoom towards the cursor
        /// </summary>
        public bool ZoomToCursor { get; set; }

        public bool IsPlaying {
            get => this.isPlaying;
            set => this.isPlaying = value;
        }

        public Action OnStepFrame {
            get => this.PlaybackTimer.TickCallback;
            set => this.PlaybackTimer.TickCallback = value;
        }

        public VideoEditor Editor { get; }

        public EditorPlayback(VideoEditor editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.PlaybackTimer = new PrecisionTimer();
        }

        public RenderSuspension SuspendRendering() {
            Interlocked.Increment(ref this.suspensionCount);
            return new RenderSuspension();
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetBool(nameof(this.UsePrecisionTimingMode), this.UsePrecisionTimingMode);
            data.SetBool(nameof(this.ZoomToCursor), this.ZoomToCursor);
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.UsePrecisionTimingMode = data.GetBool(nameof(this.UsePrecisionTimingMode));
            this.ZoomToCursor = data.GetBool(nameof(this.ZoomToCursor));
        }

        public readonly struct RenderSuspension : IDisposable {
            private readonly EditorPlayback playback;

            private RenderSuspension(EditorPlayback playback) {
                this.playback = playback;
            }

            public void Dispose() {
                Interlocked.Decrement(ref this.playback.suspensionCount);
            }
        }
    }
}