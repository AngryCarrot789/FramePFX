using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using SkiaSharp;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.Editors.Timelines.Effects {
    public class MotionEffect : VideoEffect, ITransformationEffect {
        public static readonly ParameterVector2 MediaPositionParameter =             Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaPosition),             ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaPosition)), ParameterFlags.AffectsRender);
        public static readonly ParameterVector2 MediaScaleParameter =                Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScale), Vector2.One,   ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaScale)), ParameterFlags.AffectsRender);
        public static readonly ParameterVector2 MediaScaleOriginParameter =          Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleOrigin),          ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaScaleOrigin)), ParameterFlags.AffectsRender);
        public static readonly ParameterBoolean UseAbsoluteScaleOriginParameter =    Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin),    ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteScaleOrigin)), ParameterFlags.AffectsRender);
        public static readonly ParameterDouble MediaRotationParameter =              Parameter.RegisterDouble(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotation),              ValueAccessors.LinqExpression<double>(typeof(MotionEffect), nameof(MediaRotation)), ParameterFlags.AffectsRender);
        public static readonly ParameterVector2 MediaRotationOriginParameter =       Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotationOrigin),       ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaRotationOrigin)), ParameterFlags.AffectsRender);
        public static readonly ParameterBoolean UseAbsoluteRotationOriginParameter = Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteRotationOrigin), ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteRotationOrigin)), ParameterFlags.AffectsRender);

        public Vector2 MediaPosition;
        public Vector2 MediaScale;
        public Vector2 MediaScaleOrigin;
        public double MediaRotation;
        public Vector2 MediaRotationOrigin;
        public bool UseAbsoluteScaleOrigin;
        public bool UseAbsoluteRotationOrigin;
        private SKMatrix internalTransformationMatrix;
        private SKMatrix render_TransformationMatrix;
        private bool isMatrixDirty;
        private SKMatrix oldMatrix;
        private int restoreCount;

        public event MatrixChangedEventHandler MatrixChanged;

        /// <summary>
        /// Our pre-computed matrix, which is updated when a parameter (that is used during the calculation of the matrix) is modified, triggered by automation updates
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty) {
                    this.isMatrixDirty = false;
                    this.internalTransformationMatrix = this.CreateTransformationMatrix();
                }

                return this.internalTransformationMatrix;
            }
        }

        public MotionEffect() {
            this.isMatrixDirty = true;
            this.MediaPosition = MediaPositionParameter.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleParameter.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginParameter.Descriptor.DefaultValue;
            this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
            this.MediaRotationOrigin = MediaRotationOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteRotationOrigin = UseAbsoluteRotationOriginParameter.Descriptor.DefaultValue;
        }

        static MotionEffect() {
            Parameter.AddMultipleHandlers(OnParameterValueChanged, MediaPositionParameter, MediaScaleParameter, MediaScaleOriginParameter, UseAbsoluteScaleOriginParameter, MediaRotationParameter, MediaRotationOriginParameter, UseAbsoluteRotationOriginParameter);
        }

        protected override void LoadDataIntoClone(BaseEffect clone) {
            base.LoadDataIntoClone(clone);
        }

        public override void PrepareRender(PreRenderContext ctx, long frame) {
            base.PrepareRender(ctx, frame);
            this.render_TransformationMatrix = this.TransformationMatrix;
        }

        public override void PreProcessFrame(RenderContext rc) {
            base.PreProcessFrame(rc);
            this.restoreCount = rc.Canvas.Save();
            rc.Canvas.SetMatrix(rc.Canvas.TotalMatrix.PreConcat(this.render_TransformationMatrix));
        }

        public override void PostProcessFrame(RenderContext rc, ref SKRect renderArea) {
            base.PostProcessFrame(rc, ref renderArea);
            rc.Canvas.RestoreToCount(this.restoreCount);
        }

        private static void OnParameterValueChanged(AutomationSequence sequence) {
            MotionEffect effect = (MotionEffect) sequence.AutomationData.Owner;
            effect.isMatrixDirty = true;
            effect.InvalidateOwnerMatrix();
        }

        /// <summary>
        /// Creates a transformation matrix based on the current state of this effect
        /// </summary>
        /// <returns></returns>
        private SKMatrix CreateTransformationMatrix() {
            SKMatrix matrix = SKMatrix.Identity;
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(this.MediaPosition.X, this.MediaPosition.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateScale(this.MediaScale.X, this.MediaScale.Y, this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) this.MediaRotation, this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
            return matrix;
        }

        protected override void OnAdded() {
            base.OnAdded();
            this.InvalidateOwnerMatrix();
        }

        protected override void OnRemoved() {
            base.OnRemoved();
            this.InvalidateOwnerMatrix();
        }

        public void InvalidateOwnerMatrix() {
            switch (this.Owner) {
                case VideoClip clip: clip.InvalidateTransformationMatrix();
                    break;
                case VideoTrack track: track.InvalidateTransformationMatrix();
                    break;
            }
        }
    }
}