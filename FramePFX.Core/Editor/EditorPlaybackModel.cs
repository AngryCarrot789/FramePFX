using System;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class EditorPlaybackModel : IRBESerialisable {
        private volatile bool isPlaying;

        public PrecisionTimer PlaybackTimer { get; }

        public bool UsePrecisionTimingMode { get; set; }

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

        public EditorPlaybackModel(VideoEditor editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.PlaybackTimer = new PrecisionTimer();
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetBool(nameof(this.UsePrecisionTimingMode), this.UsePrecisionTimingMode);
            data.SetBool(nameof(this.ZoomToCursor), this.ZoomToCursor);
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.UsePrecisionTimingMode = data.GetBool(nameof(this.UsePrecisionTimingMode));
            this.ZoomToCursor = data.GetBool(nameof(this.ZoomToCursor));
        }
    }
}