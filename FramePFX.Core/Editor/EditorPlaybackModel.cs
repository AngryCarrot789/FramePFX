using System;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class EditorPlaybackModel {
        private volatile bool isPlaying;

        public PrecisionTimer PlaybackTimer { get; }

        public bool UsePrecisionTimingMode { get; set; }

        public bool IsPlaying {
            get => this.isPlaying;
            set => this.isPlaying = value;
        }

        public Action OnStepFrame {
            get => this.PlaybackTimer.TickCallback;
            set => this.PlaybackTimer.TickCallback = value;
        }

        public VideoEditorModel Editor { get; }

        public EditorPlaybackModel(VideoEditorModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.PlaybackTimer = new PrecisionTimer();
        }
    }
}