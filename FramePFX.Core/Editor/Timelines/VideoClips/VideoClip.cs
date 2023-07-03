using System;
using System.Numerics;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public abstract class VideoClip : Clip {
        public static readonly AutomationKey MediaPositionKey =    AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKey MediaScaleKey =       AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKey MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaScaleOrigin), new Vector2(0.5f, 0.5f), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKey OpacityKey =          AutomationKey.RegisterDouble(nameof(VideoClip), nameof(Opacity), 1d, 0d, 1d);

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition;

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale;

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin;

        /// <summary>
        /// When false, the <see cref="MediaScaleOrigin"/> is relative to the media size (see <see cref="GetSize"/>). When
        /// true, <see cref="GetSize"/> is not called, and the <see cref="MediaScaleOrigin"/> is used directly
        /// </summary>
        public bool UseAbsoluteScaleOrigin;

        /// <summary>
        /// The opacity; how much of this clip is visible when rendered. Ranges from 0 to 1
        /// </summary>
        public double Opacity;

        public byte OpacityByte => RenderUtils.DoubleToByte(this.Opacity);

        /// <summary>
        /// Whether or not this clip handles it's own opacity calculation to help with render performance. Default
        /// value is false, meaning an <see cref="Opacity"/> value that isn't 1d requires a temporary bitmap to render the clip
        /// </summary>
        public virtual bool UseCustomOpacityCalculation { get => false; }

        /// <summary>
        /// Whether or not this clip supports async rendering
        /// </summary>
        public virtual bool UseAsyncRendering { get => false; }

        /// <summary>
        /// The async render state. Should only be accessed after calling <see cref="BeginRender"/>
        /// </summary>
        public bool IsAsyncRenderReady;

        /// <summary>
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        protected VideoClip() {
            this.MediaPosition = Vector2.Zero;
            this.MediaScale = Vector2.One;
            this.MediaScaleOrigin = new Vector2(0.5f, 0.5f);
            this.Opacity = 1d;
            this.AutomationData.AssignKey(MediaPositionKey, (s, f) => this.MediaPosition = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleKey, (s, f) => this.MediaScale = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleOriginKey, (s, f) => this.MediaScaleOrigin = s.GetVector2Value(f));
            this.AutomationData.AssignKey(OpacityKey, (s, f) => this.Opacity = s.GetDoubleValue(f));
        }

        /// <summary>
        /// Signals the video editor associated with this clip to render the current frame again. Optionally allows the
        /// re-render to be scheduled, making it happen at some point in the very near future
        /// </summary>
        /// <param name="schedule">Schedule for the future and not in the current call</param>
        public virtual void InvalidateRender(bool schedule = true) {
            this.RenderInvalidated?.Invoke(this, schedule);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetStruct(nameof(this.MediaPosition), this.MediaPosition);
            data.SetStruct(nameof(this.MediaScale), this.MediaScale);
            data.SetStruct(nameof(this.MediaScaleOrigin), this.MediaScaleOrigin);
            data.SetDouble(nameof(this.Opacity), this.Opacity);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MediaPosition = data.GetStruct<Vector2>(nameof(this.MediaPosition));
            this.MediaScale = data.GetStruct<Vector2>(nameof(this.MediaScale));
            this.MediaScaleOrigin = data.GetStruct<Vector2>(nameof(this.MediaScaleOrigin));
            this.Opacity = data.GetDouble(nameof(this.Opacity));
        }

        /// <summary>
        /// Gets the size of this video clip's media, which is used in the scale phase of the
        /// <see cref="Transform(FramePFX.Core.Rendering.RenderContext,out System.Nullable{System.Numerics.Vector2},out SkiaSharp.SKMatrix)"/> call.
        /// If the value is unavailable, then the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns></returns>
        public abstract Vector2? GetSize();

        public void Transform(RenderContext rc, out Vector2? size, out SKMatrix oldMatrix) {
            oldMatrix = rc.Canvas.TotalMatrix;
            this.Transform(rc, out size);
        }

        public void Transform(RenderContext rc, out Vector2? size) {
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;

            rc.Canvas.Translate(pos.X, pos.Y);
            size = this.GetSize();
            if (this.UseAbsoluteScaleOrigin) {
                rc.Canvas.Scale(scale.X, scale.Y, origin.X, origin.Y);
            }
            else {
                if (!(size is Vector2 sz)) {
                    sz = new Vector2(rc.FrameInfo.Width, rc.FrameInfo.Height);
                }

                rc.Canvas.Scale(scale.X, scale.Y, sz.X * origin.X, sz.Y * origin.Y);
            }
        }

        public void Transform(RenderContext rc) {
            this.Transform(rc, out _);
        }

        /// <summary>
        /// Directly renders this clip, using the render context
        /// </summary>
        /// <param name="rc">The rendering context</param>
        /// <param name="frame">The frame being rendered. May be drastically different from the last render (frame seeked)</param>
        /// <exception cref="NotImplementedException">The clip does not support direct rendering (does not throw if <see cref="UseAsyncRendering"/> is false)</exception>
        public virtual void Render(RenderContext rc, long frame) {
            if (this.UseAsyncRendering)
                throw new NotImplementedException("This clip does not implement direct rendering; use async rendering");
        }

        /// <summary>
        /// The first stage of the async render process. This can be used to notify a decoder stream to seek the given frame
        /// <para>
        /// Clips that override this method should not call the base method
        /// </para>
        /// </summary>
        /// <param name="frame">The frame being rendered. May be drastically different from the last render (frame seeked)</param>
        /// <exception cref="NotImplementedException">The clip does not support async rendering (does not throw if <see cref="UseAsyncRendering"/> is true)</exception>
        public virtual void BeginRender(long frame) {
            if (!this.UseAsyncRendering)
                throw new NotImplementedException("This clip does not support async rendering; use direct rendering");
            this.IsAsyncRenderReady = true;
        }

        /// <summary>
        /// The second (last) stage of the async render process. This can be used to notify a decoder stream to seek the given frame
        /// <para>
        /// Clips that override this method should not call the base method
        /// </para>
        /// </summary>
        /// <param name="frame">The frame being rendered. May be drastically different from the last render (frame seeked)</param>
        /// <exception cref="NotImplementedException">The clip does not support async rendering (does not throw if <see cref="UseAsyncRendering"/> is true)</exception>
        public virtual void EndRender(RenderContext rc) {
            if (!this.UseAsyncRendering)
                throw new NotImplementedException("This clip does not support async rendering; use direct rendering");
            if (!this.IsAsyncRenderReady)
                throw new InvalidOperationException("Async render is not ready");
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
        }
    }
}