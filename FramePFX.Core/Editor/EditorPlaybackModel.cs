using System;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class VideoEditorPlaybackModel {
        private volatile bool isPlaying;

        public PrecisionTimer Timer { get; }

        public bool UsePrecisionTimingMode { get; set; }

        public bool IsPlaying {
            get => this.isPlaying;
            set => this.isPlaying = value;
        }

        public Action PlaybackCallback {
            get => this.Timer.TickCallback;
            set => this.Timer.TickCallback = value;
        }

        public VideoEditorModel Editor { get; }

        public VideoEditorPlaybackModel(VideoEditorModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.Timer = new PrecisionTimer();
        }

        public Task PauseTimerAsync() {
            return this.Timer.StopAsync();
        }
    }
}