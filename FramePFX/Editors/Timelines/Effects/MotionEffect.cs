using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Effects {
    public class MotionEffect : VideoEffect, ITransformationEffect {
        public static readonly ParameterFloat MediaPositionXParameter =               Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaPositionX),           0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaPositionX)));
        public static readonly ParameterFloat MediaPositionYParameter =               Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaPositionY),           0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaPositionY)));
        public static readonly ParameterFloat MediaScaleXParameter =                  Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleX),              1, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaScaleX)));
        public static readonly ParameterFloat MediaScaleYParameter =                  Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleY),              1, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaScaleY)));
        public static readonly ParameterFloat MediaScaleOriginXParameter =            Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleOriginX),        0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaScaleOriginX)));
        public static readonly ParameterFloat MediaScaleOriginYParameter =            Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaScaleOriginY),        0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaScaleOriginY)));
        public static readonly ParameterBoolean UseAbsoluteScaleOriginParameter =     Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin),    ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteScaleOrigin)));
        public static readonly ParameterDouble MediaRotationParameter =               Parameter.RegisterDouble(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotation),           0, ValueAccessors.LinqExpression<double>(typeof(MotionEffect), nameof(MediaRotation)));
        public static readonly ParameterFloat MediaRotationOriginXParameter =         Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotationOriginX),     0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaRotationOriginX)));
        public static readonly ParameterFloat MediaRotationOriginYParameter =         Parameter.RegisterFloat(typeof(MotionEffect), nameof(MotionEffect), nameof(MediaRotationOriginY),     0, ValueAccessors.LinqExpression<float>(typeof(MotionEffect), nameof(MediaRotationOriginY)));
        public static readonly ParameterBoolean UseAbsoluteRotationOriginParameter =  Parameter.RegisterBoolean(typeof(MotionEffect), nameof(MotionEffect), nameof(UseAbsoluteRotationOrigin), ValueAccessors.Reflective<bool>(typeof(MotionEffect), nameof(UseAbsoluteRotationOrigin)));

        public float MediaPositionX;
        public float MediaPositionY;
        public float MediaScaleX;
        public float MediaScaleY;
        public float MediaScaleOriginX;
        public float MediaScaleOriginY;
        public double MediaRotation;
        public float MediaRotationOriginX;
        public float MediaRotationOriginY;
        public bool UseAbsoluteScaleOrigin;
        public bool UseAbsoluteRotationOrigin;
        private SKMatrix __internalTransformationMatrix;
        private SKMatrix renderMatrixProxy;
        private bool isMatrixDirty;

        public event MatrixChangedEventHandler MatrixChanged;

        /// <summary>
        /// Our pre-computed matrix, which is updated when a parameter (that is used during the calculation of the matrix) is modified, triggered by automation updates
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty) {
                    this.isMatrixDirty = false;
                    this.__internalTransformationMatrix = this.CreateTransformationMatrix();
                }

                return this.__internalTransformationMatrix;
            }
        }

        public MotionEffect() {
            this.isMatrixDirty = true;
            this.MediaPositionX = MediaPositionXParameter.Descriptor.DefaultValue;
            this.MediaPositionY = MediaPositionYParameter.Descriptor.DefaultValue;
            this.MediaScaleX = MediaScaleXParameter.Descriptor.DefaultValue;
            this.MediaScaleY = MediaScaleYParameter.Descriptor.DefaultValue;
            this.MediaScaleOriginX = MediaScaleOriginXParameter.Descriptor.DefaultValue;
            this.MediaScaleOriginY = MediaScaleOriginYParameter.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginParameter.Descriptor.DefaultValue;
            this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
            this.MediaRotationOriginX = MediaRotationOriginXParameter.Descriptor.DefaultValue;
            this.MediaRotationOriginY = MediaRotationOriginYParameter.Descriptor.DefaultValue;
            this.UseAbsoluteRotationOrigin = UseAbsoluteRotationOriginParameter.Descriptor.DefaultValue;
        }

        static MotionEffect() {
            Parameter.AddMultipleHandlers(OnParameterValueChanged, MediaPositionXParameter, MediaPositionYParameter, MediaScaleXParameter, MediaScaleYParameter, MediaScaleOriginXParameter, MediaScaleOriginYParameter, UseAbsoluteScaleOriginParameter, MediaRotationParameter, MediaRotationOriginXParameter, MediaRotationOriginYParameter, UseAbsoluteRotationOriginParameter);
        }

        public override void PrepareRender(PreRenderContext ctx, long frame) {
            base.PrepareRender(ctx, frame);
            this.renderMatrixProxy = this.TransformationMatrix;
        }

        public override void PreProcessFrame(RenderContext rc) {
            base.PreProcessFrame(rc);
            rc.Canvas.SetMatrix(rc.Canvas.TotalMatrix.PreConcat(this.renderMatrixProxy));
        }

        private static void OnParameterValueChanged(AutomationSequence sequence) {
            MotionEffect effect = (MotionEffect) sequence.AutomationData.Owner;
            effect.isMatrixDirty = true;
            if (effect.IsClipEffect) {
                effect.OwnerClip.InvalidateTransformationMatrix();
            }
        }

        /// <summary>
        /// Creates a transformation matrix based on the current state of this effect
        /// </summary>
        /// <returns></returns>
        private SKMatrix CreateTransformationMatrix() {
            SKMatrix matrix = SKMatrix.Identity;
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(this.MediaPositionX, this.MediaPositionY));
            matrix = matrix.PreConcat(SKMatrix.CreateScale(this.MediaScaleX, this.MediaScaleY, this.MediaScaleOriginX, this.MediaScaleOriginY));
            matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) this.MediaRotation, this.MediaRotationOriginX, this.MediaRotationOriginY));
            return matrix;
        }

        protected override void OnAdded() {
            base.OnAdded();
            if (this.IsClipEffect)
                this.OwnerClip.InvalidateTransformationMatrix();
        }

        protected override void OnRemoved() {
            base.OnRemoved();
            if (this.IsClipEffect)
                this.OwnerClip.InvalidateTransformationMatrix();
        }
    }
}