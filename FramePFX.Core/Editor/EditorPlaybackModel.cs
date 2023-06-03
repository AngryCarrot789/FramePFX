using System;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class EditorPlaybackModel {
        private volatile bool isPlaying;

        public PrecisionTimer FrameStepTimer { get; }

        public bool UsePrecisionTimingMode { get; set; }

        public bool IsPlaying {
            get => this.isPlaying;
            set => this.isPlaying = value;
        }

        public Action OnStepFrame {
            get => this.FrameStepTimer.TickCallback;
            set => this.FrameStepTimer.TickCallback = value;
        }

        public VideoEditorModel Editor { get; }

        public EditorPlaybackModel(VideoEditorModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.FrameStepTimer = new PrecisionTimer();
        }

        public void StartTimer() {
            this.FrameStepTimer.Start(this.UsePrecisionTimingMode);
        }

        public Task PauseTimerAsync() {
            return this.FrameStepTimer.StopAsync();
        }
    }
}