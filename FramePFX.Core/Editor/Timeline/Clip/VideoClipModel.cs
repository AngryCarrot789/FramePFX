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
        public override ClipSpan FrameSpan { get; set; }

        /// <summary>
        /// The number of frames that are skipped relative to <see cref="ClipStart"/>. This will be positive if the
        /// left grip of the clip is dragged to the right, and will be 0 when dragged to the left
        /// <para>
        /// Alternative name: MediaBegin
        /// </para>
        /// </summary>
        public long MediaFrameOffset { get; set; }

        /// <summary>
        /// Helper property for getting and setting the <see cref="ClipSpan.Begin"/> property
        /// </summary>
        public long FrameBegin {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.SetBegin(value);
        }

        /// <summary>
        /// Helper property for getting and setting the <see cref="ClipSpan.Duration"/> property
        /// </summary>
        public long FrameDuration {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.SetDuration(value);
        }

        /// <summary>
        /// Helper property for getting and setting the <see cref="ClipSpan.EndIndex"/> property
        /// </summary>
        public long FrameEndIndex {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.SetEndIndex(value);
        }

        /// <summary>
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        protected VideoClipModel() {
            this.MediaPosition = new Vector2();
            this.MediaScale = new Vector2(1f, 1f);
            this.MediaScaleOrigin = new Vector2(0.5f, 0.5f);
        }

        public virtual void InvalidateRender(bool schedule = true) {
            this.RenderInvalidated?.Invoke(this, schedule);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            data.SetStruct(nameof(this.MediaPosition), this.MediaPosition);
            data.SetStruct(nameof(this.MediaScale), this.MediaScale);
            data.SetStruct(nameof(this.MediaScaleOrigin), this.MediaScaleOrigin);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MediaFrameOffset = data.GetLong(nameof(this.MediaFrameOffset));
            this.MediaPosition = data.GetStruct<Vector2>(nameof(this.MediaPosition));
            this.MediaScale = data.GetStruct<Vector2>(nameof(this.MediaScale));
            this.MediaScaleOrigin = data.GetStruct<Vector2>(nameof(this.MediaScaleOrigin));
            this.FrameSpan = data.GetStruct<ClipSpan>(nameof(this.FrameSpan));
        }

        /// <summary>
        /// Gets the size of this video clip's media, which is used in the transformation phase
        /// using <see cref="MediaPosition"/>, <see cref="MediaScale"/> and <see cref="MediaScaleOrigin"/>
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetSize();

        public void Transform(SKCanvas canvas, out Vector2 size, out SKMatrix oldMatrix) {
            oldMatrix = canvas.TotalMatrix;
            this.Transform(canvas, out size);
        }

        public void Transform(SKCanvas canvas, out Vector2 size) {
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;
            size = this.GetSize();
            canvas.Translate(pos.X, pos.Y);
            canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
        }

        public void Transform(SKCanvas canvas) {
            this.Transform(canvas, out _);
        }

        public abstract void Render(RenderContext render, long frame, SKColorFilter alphaFilter);

        public override bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        protected abstract VideoClipModel NewInstance();

        protected virtual void LoadDataIntoClone(VideoClipModel clone) {
            clone.MediaFrameOffset = this.MediaFrameOffset;
            clone.MediaPosition = this.MediaPosition;
            clone.MediaScale = this.MediaScale;
            clone.MediaScaleOrigin = this.MediaScaleOrigin;
            clone.FrameSpan = this.FrameSpan;
            clone.DisplayName = this.DisplayName;
        }

        public override ClipModel CloneCore() {
            VideoClipModel clip = this.NewInstance();
            this.LoadDataIntoClone(clip);
            return clip;
        }
    }
}