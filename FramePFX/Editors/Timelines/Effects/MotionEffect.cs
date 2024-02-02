using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using SkiaSharp;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.Editors.Timelines.Effects {
    public class MotionEffect : VideoEffect, ITransformationEffect {
        public static readonly ParameterVector2 MediaPositionParameter =             Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaPosition),             ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaPosition)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterVector2 MediaScaleParameter =                Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScale), Vector2.One,   ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaScale)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterVector2 MediaScaleOriginParameter =          Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleOrigin),          ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaScaleOrigin)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterBoolean UseAbsoluteScaleOriginParameter =    Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin),    ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteScaleOrigin)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterDouble MediaRotationParameter =              Parameter.RegisterDouble(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotation),              ValueAccessors.LinqExpression<double>(typeof(MotionEffect), nameof(MediaRotation)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterVector2 MediaRotationOriginParameter =       Parameter.RegisterVector2(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotationOrigin),       ValueAccessors.LinqExpression<Vector2>(typeof(MotionEffect), nameof(MediaRotationOrigin)), ParameterFlags.InvalidatesRender);
        public static readonly ParameterBoolean UseAbsoluteRotationOriginParameter = Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteRotationOrigin), ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteRotationOrigin)), ParameterFlags.InvalidatesRender);

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
            MotionEffect fx = (MotionEffect) clone;

            // fx.MediaPosition = this.MediaPosition;
            // fx.MediaScale = this.MediaScale;
            // fx.MediaScaleOrigin = this.MediaScaleOrigin;
            // fx.MediaRotation = this.MediaRotation;
            // fx.MediaRotationOrigin = this.MediaRotationOrigin;
            // fx.UseAbsoluteScaleOrigin = this.UseAbsoluteScaleOrigin;
            // fx.UseAbsoluteRotationOrigin = this.UseAbsoluteRotationOrigin;
            // fx.internalTransformationMatrix = this.internalTransformationMatrix;
        }

        public override void PrepareRender(PreRenderContext ctx, long frame) {
            base.PrepareRender(ctx, frame);
            this.render_TransformationMatrix = this.TransformationMatrix;
        }

        public override void PreProcessFrame(RenderContext rc) {
            base.PreProcessFrame(rc);
            rc.Canvas.SetMatrix(rc.Canvas.TotalMatrix.PreConcat(this.render_TransformationMatrix));
        }

        private static void OnParameterValueChanged(AutomationSequence sequence) {
            MotionEffect effect = (MotionEffect) sequence.AutomationData.Owner;
            effect.isMatrixDirty = true;
            if (effect.Owner is VideoClip) {
                effect.OwnerClip.InvalidateTransformationMatrix();
            }
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
            if (this.Owner is VideoClip)
                this.OwnerClip.InvalidateTransformationMatrix();
        }

        protected override void OnRemoved() {
            base.OnRemoved();
            if (this.Owner is VideoClip)
                this.OwnerClip.InvalidateTransformationMatrix();
        }
    }
}