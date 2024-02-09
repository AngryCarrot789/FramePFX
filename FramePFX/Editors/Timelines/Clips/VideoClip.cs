using System;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    /// <summary>
    /// The base class for all clips that produce video data, whether that be an image, a video frame at some time, text, particles, etc.
    /// <para>
    /// When rendering a video clip, there are 3 phases:
    /// </para>
    /// <para>
    /// Preparation on AMT (application main thread). This is where all tracks are processed (bottom to top), the
    /// clip (or clips with a transition) are calculated and then those clips' setup functions are called.
    /// The clip method is <see cref="PrepareRenderFrame"/>, which lets the clip store its current state in
    /// a proxy object which is accessible via the render thread
    /// </para>
    /// <para>
    /// Rendering (on a randomly assigned rendering thread). This is where the clip actually renders its contents.
    /// Since this is done off the main thread on a render-specific thread, it's very important that the clip does
    /// not access any un-synchronised data. The render data should be calculated in the preparation phase
    /// </para>
    /// <para>
    /// Final frame assembly (on render thread). This is where all of the rendered data is assembled into a final
    /// frame. An event (<see cref="RenderManager.FrameRendered"/>) is fired on the application main thread, and the view
    /// port then presents the fully rendered frame to the user
    /// </para>
    /// </summary>
    public abstract class VideoClip : Clip {
        public static readonly ParameterDouble OpacityParameter =
            Parameter.RegisterDouble(
                typeof(VideoClip),
                nameof(VideoClip),
                nameof(Opacity),
                new ParameterDescriptorDouble(1, 0, 1),
                ValueAccessors.LinqExpression<double>(typeof(VideoClip), nameof(Opacity)),
                ParameterFlags.StandardProjectVisual);

        public static readonly ParameterVector2 MediaPositionParameter =             Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaPosition),             ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaPosition)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaScaleParameter =                Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaScale), Vector2.One,   ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaScale)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaScaleOriginParameter =          Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaScaleOrigin),          ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaScaleOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterBoolean UseAbsoluteScaleOriginParameter =    Parameter.RegisterBoolean(typeof(VideoClip), nameof(VideoClip), nameof(UseAbsoluteScaleOrigin),    ValueAccessors.Reflective<bool>(typeof(VideoClip), nameof(UseAbsoluteScaleOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterDouble MediaRotationParameter =              Parameter.RegisterDouble(typeof(VideoClip), nameof(VideoClip), nameof(MediaRotation),              ValueAccessors.LinqExpression<double>(typeof(VideoClip), nameof(MediaRotation)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaRotationOriginParameter =       Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaRotationOrigin),       ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaRotationOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterBoolean UseAbsoluteRotationOriginParameter = Parameter.RegisterBoolean(typeof(VideoClip), nameof(VideoClip), nameof(UseAbsoluteRotationOrigin), ValueAccessors.Reflective<bool>(typeof(VideoClip), nameof(UseAbsoluteRotationOrigin)), ParameterFlags.StandardProjectVisual);

        public static readonly DataParameterBoolean IsVisibleParameter = DataParameter.Register(new DataParameterBoolean(typeof(VideoClip), nameof(IsVisible), true, ValueAccessors.Reflective<bool>(typeof(VideoClip), nameof(IsVisible)), DataParameterFlags.StandardProjectVisual));

        // Transformation data
        private Vector2 MediaPosition;
        private Vector2 MediaScale;
        private Vector2 MediaScaleOrigin;
        private double MediaRotation;
        private Vector2 MediaRotationOrigin;
        private bool UseAbsoluteScaleOrigin;
        private bool UseAbsoluteRotationOrigin;
        private SKMatrix clipTransformationMatrix;
        private SKMatrix finalTransformationMatrix;
        private bool isMatrixDirty;
        private bool IsVisible;

        // video clip stuff
        private double Opacity;

        /// <summary>
        /// Updated by the rendering engine when a clip begins rendering. This is a thread safe proxy of <see cref="Opacity"/>
        /// </summary>
        public double RenderOpacity;

        /// <summary>
        /// This is <see cref="RenderOpacity"/> converted to a byte
        /// </summary>
        public byte RenderOpacityByte => RenderUtils.DoubleToByte255(this.RenderOpacity);

        /// <summary>
        /// Returns true if this clip handles its own opacity calculations in order for a more
        /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
        /// </summary>
        public bool UsesCustomOpacityCalculation { get; protected set; }

        /// <summary>
        /// Gets (or calculates, if dirty) this clip's final transformation matrix, which is a concatenation of
        /// our <see cref="ClipTransformationMatrix"/> and our parent track's transformation matrix (or identity, if not in a track yet)
        /// </summary>
        public SKMatrix ClipAndTrackTransformationMatrix {
            get {
                if (this.isMatrixDirty)
                    this.GenerateMatrices();
                return this.finalTransformationMatrix;
            }
        }

        /// <summary>
        /// Gets (or calculates, if dirty) this clip's transformation matrix entirely based on our transformation properties (therefore does not contain our track's matrix)
        /// </summary>
        public SKMatrix ClipTransformationMatrix {
            get {
                if (this.isMatrixDirty)
                    this.GenerateMatrices();
                return this.clipTransformationMatrix;
            }
        }

        protected VideoClip() {
            this.isMatrixDirty = true;
            this.Opacity = OpacityParameter.Descriptor.DefaultValue;
            this.IsVisible = IsVisibleParameter.DefaultValue;
            this.MediaPosition = MediaPositionParameter.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleParameter.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginParameter.Descriptor.DefaultValue;
            this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
            this.MediaRotationOrigin = MediaRotationOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteRotationOrigin = UseAbsoluteRotationOriginParameter.Descriptor.DefaultValue;
        }

        static VideoClip() {
            Parameter.AddMultipleHandlers(s => ((VideoClip) s.AutomationData.Owner).InvalidateTransformationMatrix(), MediaPositionParameter, MediaScaleParameter, MediaScaleOriginParameter, UseAbsoluteScaleOriginParameter, MediaRotationParameter, MediaRotationOriginParameter, UseAbsoluteRotationOriginParameter);
        }

        private void GenerateMatrices() {
            this.clipTransformationMatrix = MatrixUtils.CreateTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, this.MediaScaleOrigin, this.MediaRotationOrigin);
            this.finalTransformationMatrix = this.Track is VideoTrack vidTrack ? vidTrack.TransformationMatrix.PreConcat(this.clipTransformationMatrix) : this.clipTransformationMatrix;
            this.isMatrixDirty = false;
        }

        protected override void OnTrackChanged(Track oldTrack, Track newTrack) {
            base.OnTrackChanged(oldTrack, newTrack);
            this.InvalidateTransformationMatrix();
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetBool(nameof(this.IsVisible), this.IsVisible);
            // we don't write Opacity here since it's an automatable parameter and therefore
            // the value is saved either in the default key frame or in key frames
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.IsVisible = data.GetBool(nameof(this.IsVisible));
            this.isMatrixDirty = true;
        }

        /// <summary>
        /// Gets the raw amount of space this clip takes up on screen, unaffected by standard transformation matrices.
        /// If the value is unavailable, then typically the render viewport's width and height are used as a fallback
        /// <para>
        /// This value also may only be available after the clip has rendered at least once, and it may also be
        /// completely different after each render phase due to the nature of the clip itself, so this should just
        /// be treated as a general hint
        /// </para>
        /// </summary>
        /// <returns>The size, if applicable, otherwise null</returns>
        public virtual Vector2? GetRenderSize() => null;

        public override bool IsEffectTypeAccepted(Type effectType) => typeof(VideoEffect).IsAssignableFrom(effectType);

        /// <summary>
        /// Propagates the render invalidated state to our project's <see cref="RenderManager"/>
        /// </summary>
        public void InvalidateRender() {
            this.Project?.RenderManager.InvalidateRender();
        }

        protected override void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {
            base.OnFrameSpanChanged(oldSpan, newSpan);
            this.InvalidateRender();
        }

        /// <summary>
        /// Prepares this clip for rendering. This is called on the main thread, and allows rendering data
        /// to be cached locally so that it can be accessed safely by a render thread in <see cref="RenderFrame"/>.
        /// </summary>
        /// <param name="rc">The pre-render setup context</param>
        /// <param name="frame">The play head frame, relative to this clip. This will always be within range of our span</param>
        /// <returns>True if this clip can be rendered (meaning <see cref="RenderFrame"/> may be called after this call)</returns>
        public abstract bool PrepareRenderFrame(PreRenderContext rc, long frame);

        /// <summary>
        /// Renders this clip using the given rendering context data. This is called on a randomly
        /// assigned rendering thread, therefore, this method should not access un-synchronised clip data
        /// </summary>
        /// <param name="rc">The rendering context, containing things such as the surface and canvas to draw to</param>
        /// <param name="renderArea">
        /// Used as an optimisation to know where this clip actually drew data, and only that area will be used.
        /// This defaults to the destination surface's frame size (calculated via the render context's image info),
        /// meaning it is unoptimised by default
        /// </param>
        public abstract void RenderFrame(RenderContext rc, ref SKRect renderArea);

        public void InvalidateTransformationMatrix() {
            this.isMatrixDirty = true;
            this.InvalidateRender();
        }
    }
}