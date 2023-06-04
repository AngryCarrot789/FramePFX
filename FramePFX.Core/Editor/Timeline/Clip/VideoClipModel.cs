using System.Numerics;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public abstract class VideoClipModel : ClipModel {
        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition { get; set; }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale { get; set; }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin { get; set; }

        /// <summary>
        /// The position of this video clip, in the form of a <see cref="ClipSpan"/> which has a begin and duration property
        /// </summary>
        public ClipSpan FrameSpan { get; set; }

        /// <summary>
        /// The number of frames that are skipped relative to <see cref="ClipStart"/>. This will be positive if the
        /// left grip of the clip is dragged to the right, and will be 0 when dragged to the left
        /// <para>
        /// Alternative name: MediaBegin
        /// </para>
        /// </summary>
        public long MediaFrameOffset { get; set; }

        public long FrameBegin {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.SetBegin(value);
        }

        public long FrameDuration {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.SetDuration(value);
        }

        public long FrameEndIndex {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.SetEndIndex(value);
        }

        protected VideoClipModel() {
            this.MediaPosition = new Vector2();
            this.MediaScale = new Vector2(1f, 1f);
            this.MediaScaleOrigin = new Vector2(0.5f, 0.5f);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MediaFrameOffset = data.GetLong(nameof(this.MediaFrameOffset));
            this.MediaPosition = data.GetStruct<Vector2>(nameof(this.MediaPosition));
            this.MediaScale = data.GetStruct<Vector2>(nameof(this.MediaScale));
            this.MediaScaleOrigin = data.GetStruct<Vector2>(nameof(this.MediaScaleOrigin));
            this.FrameSpan = data.GetStruct<ClipSpan>(nameof(this.FrameSpan));
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            data.SetStruct(nameof(this.MediaPosition), this.MediaPosition);
            data.SetStruct(nameof(this.MediaScale), this.MediaScale);
            data.SetStruct(nameof(this.MediaScaleOrigin), this.MediaScaleOrigin);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
        }

        /// <summary>
        /// Gets the size of this video clip's media, which is used in the transformation phase
        /// using <see cref="MediaPosition"/>, <see cref="MediaScale"/> and <see cref="MediaScaleOrigin"/>
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetSize();

        public void Transform(SKCanvas canvas, out Rect rect, out SKMatrix oldMatrix) {
            Vector2 size = this.GetSize();
            Vector2 scale = this.MediaScale, pos = this.MediaPosition, origin = this.MediaScaleOrigin;
            oldMatrix = canvas.TotalMatrix;
            canvas.Translate(pos.X, pos.Y);
            canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
            rect = new Rect(0, 0, size.X, size.Y);
        }

        public void Transform(SKCanvas canvas, out Rect rect) {
            Vector2 size = this.GetSize();
            Vector2 scale = this.MediaScale, pos = this.MediaPosition, origin = this.MediaScaleOrigin;
            canvas.Translate(pos.X, pos.Y);
            canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
            rect = new Rect(0, 0, size.X, size.Y);
        }

        public abstract void Render(RenderContext render, long frame);

        public override bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }
    }
}