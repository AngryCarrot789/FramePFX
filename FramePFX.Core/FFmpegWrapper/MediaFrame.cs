using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper {

    public abstract unsafe class MediaFrame : FFObject {
        internal AVFrame* frame;
        protected bool _ownsFrame = true;

        public AVFrame* Handle {
            get {
                this.ValidateNotDisposed();
                return this.frame;
            }
        }

        public long? BestEffortTimestamp => FFUtils.GetPTS(this.frame->best_effort_timestamp);

        public long? PresentationTimestamp {
            get => FFUtils.GetPTS(this.frame->pts);
            set => FFUtils.SetPTS(ref this.frame->pts, value);
        }

        protected override void Free() {
            if (this.frame != null && this._ownsFrame) {
                fixed (AVFrame** ppFrame = &this.frame) {
                    ffmpeg.av_frame_free(ppFrame);
                }
            }

            this.frame = null;
        }

        protected void ValidateNotDisposed() {
            if (this.frame == null) {
                throw new ObjectDisposedException(nameof(MediaFrame));
            }
        }
    }
}