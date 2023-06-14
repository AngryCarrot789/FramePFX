using System.Numerics;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
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
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity { get; set; }

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
            data.SetStruct(nameof(this.MediaPosition), this.MediaPosition);
            data.SetStruct(nameof(this.MediaScale), this.MediaScale);
            data.SetStruct(nameof(this.MediaScaleOrigin), this.MediaScaleOrigin);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MediaPosition = data.GetStruct<Vector2>(nameof(this.MediaPosition));
            this.MediaScale = data.GetStruct<Vector2>(nameof(this.MediaScale));
            this.MediaScaleOrigin = data.GetStruct<Vector2>(nameof(this.MediaScaleOrigin));
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

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
            if (clone is VideoClipModel vc) {
                vc.MediaPosition = this.MediaPosition;
                vc.MediaScale = this.MediaScale;
                vc.MediaScaleOrigin = this.MediaScaleOrigin;
            }
        }
    }
}