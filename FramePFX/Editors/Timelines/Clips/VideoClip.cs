using System;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.RBC;
using FramePFX.Utils;
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
                "Opacity",
                new ParameterDescriptorDouble(1, 0, 1),
                ValueAccessors.LinqExpression<double>(typeof(VideoClip), nameof(Opacity)),
                ParameterFlags.InvalidatesRender);

        private SKMatrix internalTransformationMatrix;
        private bool isMatrixDirty;

        /// <summary>
        /// The actual live opacity of this clip. This is updated by the automation engine, and is not thread-safe (see <see cref="InternalRenderOpacity"/>)
        /// </summary>
        public double Opacity;

        public byte OpacityByte => RenderUtils.DoubleToByte255(this.Opacity);

        /// <summary>
        /// Updated by the rendering engine when a clip begins rendering. This is a thread safe proxy of <see cref="Opacity"/>
        /// </summary>
        public double InternalRenderOpacity;

        public byte InternalRenderOpacityByte => RenderUtils.DoubleToByte255(this.InternalRenderOpacity);

        /// <summary>
        /// Returns true if this clip handles its own opacity calculations in order for a more
        /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
        /// </summary>
        public bool UsesCustomOpacityCalculation { get; protected set; }

        /// <summary>
        /// This video clip's transformation matrix, which is applied before it is rendered (if
        /// <see cref="OnBeginRender"/> returns true of course). This is calculated by one or
        /// more <see cref="MotionEffect"/> instances, where each instances' matrix is concatenated
        /// in their orders in our effect list
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty)
                    this.CookTransformationMatrix();
                return this.internalTransformationMatrix;
            }
        }

        protected VideoClip() {
            this.Opacity = OpacityParameter.Descriptor.DefaultValue;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            // we don't write Opacity here since it's an automatable parameter and therefore
            // the value is saved either in the default key frame or in key frames
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.InvalidateTransformationMatrix();
        }

        /// <summary>
        /// Gets the raw amount of space this clip takes up on screen, unaffected by standard transformation matrices.
        /// If the value is unavailable, then typically the render viewport's width and height are used as a fallback
        /// </summary>
        /// <returns>The size, if applicable, otherwise null</returns>
        public virtual Vector2? GetRenderSize() => null;

        public override bool IsEffectTypeAccepted(Type effectType) {
            return effectType.instanceof(typeof(VideoEffect));
        }

        /// <summary>
        /// Propagates the render invalidated state to our project's <see cref="RenderManager"/>
        /// </summary>
        public void InvalidateRender() {
            this.Track?.InvalidateRender();
        }

        protected override void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {
            base.OnFrameSpanChanged(oldSpan, newSpan);
            this.InvalidateRender();
        }

        /// <summary>
        /// Prepares this clip for rendering. This is called on the main thread, and allows rendering data
        /// to be cached locally so that it can be accessed safely by a render thread in <see cref="RenderFrame"/>.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="frame">The play head frame, relative to this clip. This will always be within range of our span</param>
        public abstract bool PrepareRenderFrame(PreRenderContext ctx, long frame);

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

        private void CookTransformationMatrix() {
            SKMatrix matrix = SKMatrix.Identity;
            foreach (BaseEffect effect in this.Effects) {
                if (effect is ITransformationEffect tfx) {
                    matrix = matrix.PreConcat(tfx.TransformationMatrix);
                }
            }

            this.internalTransformationMatrix = matrix;
            this.isMatrixDirty = false;
        }
    }
}