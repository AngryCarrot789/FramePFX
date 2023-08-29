using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timelines.Effects.Video;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public abstract class VideoClip : Clip {
        public static readonly AutomationKeyVector2 MediaPositionKey = AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleKey = AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(VideoClip), nameof(MediaScaleOrigin), new Vector2(0.5f, 0.5f), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoClip), nameof(Opacity), 1d, 0d, 1d);

        // saves using closure allocation for each clip
        private static readonly UpdateAutomationValueEventHandler UpdateMediaPosition = (s, f) => ((VideoClip) s.AutomationData.Owner).MediaPosition = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateMediaScale = (s, f) => ((VideoClip) s.AutomationData.Owner).MediaScale = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateMediaScaleOrigin = (s, f) => ((VideoClip) s.AutomationData.Owner).MediaScaleOrigin = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateOpacity = (s, f) => ((VideoClip) s.AutomationData.Owner).Opacity = s.GetDoubleValue(f);

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
        /// An event invoked when this video clip changes in some way that affects its render. 
        /// Typically handled by the view model, which schedules the video editor window's view port to render at some point in the furture
        /// </summary>
        public event ClipRenderInvalidatedEventHandler RenderInvalidated;

        public List<VideoEffect> Effects { get; }

        protected VideoClip() {
            this.Effects = new List<VideoEffect>();
            this.Effects.Add(new MotionEffect(this));
            this.MediaPosition = MediaPositionKey.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleKey.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginKey.Descriptor.DefaultValue;
            this.Opacity = OpacityKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(MediaPositionKey, UpdateMediaPosition);
            this.AutomationData.AssignKey(MediaScaleKey, UpdateMediaScale);
            this.AutomationData.AssignKey(MediaScaleOriginKey, UpdateMediaScaleOrigin);
            this.AutomationData.AssignKey(OpacityKey, UpdateOpacity);
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
        /// Gets the amount of space this clip takes up on screen (unaffected by <see cref="MediaPosition"/> or <see cref="MediaScale"/>).
        /// If the value is unavailable, then the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns>A nullable vector (null indicating to use the current view port size)</returns>
        public virtual Vector2? GetSize() => null;

        public void Transform(RenderContext rc, out Vector2? size, out SKMatrix oldMatrix) {
            oldMatrix = rc.Canvas.TotalMatrix;
            this.Transform(rc, out size);
        }

        public void Transform(RenderContext rc, out Vector2? size) {
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;
            rc.Canvas.Translate(pos.X, pos.Y);
            Vector2 sz;
            size = this.GetSize();
            if (size.HasValue) {
                sz = size.Value;
            }
            else {
                size = sz = new Vector2(rc.FrameInfo.Width, rc.FrameInfo.Height);
            }

            if (this.UseAbsoluteScaleOrigin) {
                rc.Canvas.Scale(scale.X, scale.Y, origin.X, origin.Y);
            }
            else {
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
            for (int i = 0, c = this.Effects.Count; i < c; i++) {
                this.Effects[i].ProcessFrame(rc);
            }
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
        }
    }
}