using System.Numerics;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public abstract class VideoClipModel : ClipModel {
        /// <summary>
        /// The number of frames that are skipped relative to <see cref="ClipStart"/>. This will be positive if the
        /// left grip of the clip is dragged to the right, and will be 0 when dragged to the left
        /// <para>
        /// Alternative name: MediaBegin
        /// </para>
        /// </summary>
        public long MediaOffset { get; set; }

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
        public ClipSpan ClipPos { get; set; }

        public long FrameBegin {
            get => this.ClipPos.Begin;
            set => this.ClipPos = this.ClipPos.SetBegin(value);
        }

        public long FrameDuration {
            get => this.ClipPos.Duration;
            set => this.ClipPos = this.ClipPos.SetDuration(value);
        }

        public long FrameEndIndex {
            get => this.ClipPos.EndIndex;
            set => this.ClipPos = this.ClipPos.SetEndIndex(value);
        }

        protected VideoClipModel() {

        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MediaOffset = data.GetLong(nameof(this.MediaOffset));
            this.MediaPosition = data.GetStruct<Vector2>(nameof(this.MediaPosition));
            this.MediaScale = data.GetStruct<Vector2>(nameof(this.MediaScale));
            this.MediaScaleOrigin = data.GetStruct<Vector2>(nameof(this.MediaScaleOrigin));
            this.ClipPos = data.GetStruct<ClipSpan>(nameof(this.ClipPos));
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.MediaOffset), this.MediaOffset);
            data.SetStruct(nameof(this.MediaPosition), this.MediaPosition);
            data.SetStruct(nameof(this.MediaScale), this.MediaScale);
            data.SetStruct(nameof(this.MediaScaleOrigin), this.MediaScaleOrigin);
            data.SetStruct(nameof(this.ClipPos), this.ClipPos);
        }

        /// <summary>
        /// Gets the size of this video clip's media, which is used in the transformation phase
        /// using <see cref="MediaPosition"/>, <see cref="MediaScale"/> and <see cref="MediaScaleOrigin"/>
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetSize();

        public void Transform(SKCanvas canvas, out Vector2 size, out SKMatrix oldMatrix) {
            Vector2 pos = this.MediaPosition;
            Vector2 scale = this.MediaScale;
            Vector2 origin = this.MediaScaleOrigin;
            size = this.GetSize();

            oldMatrix = canvas.TotalMatrix;
            canvas.Translate(pos.X, pos.Y);
            canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
        }

        public abstract void Render(RenderContext ctx, long frame);
    }
}