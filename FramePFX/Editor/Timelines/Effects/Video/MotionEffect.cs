using System.Numerics;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video {
    /// <summary>
    /// An effect that deals with picture transformations (as in position, scale and scale origin)
    /// </summary>
    public class MotionEffect : VideoEffect, ITransformationEffect {
        public static readonly AutomationKeyVector2 MediaPositionKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScaleOrigin), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteScaleOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin));
        public static readonly AutomationKeyDouble MediaRotationKey = AutomationKey.RegisterDouble(nameof(MotionEffect), nameof(MediaRotation), 0d);
        public static readonly AutomationKeyVector2 MediaRotationOriginKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaRotationOrigin), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteRotationOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteRotationOrigin));

        private SKMatrix __internalTransformationMatrix;

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition;

        /// <summary>
        /// The x and y scale of the 0video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale;

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0,0 (top-left corner of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin;

        /// <summary>
        /// When false, the <see cref="MediaScaleOrigin"/> is relative to the media size (see <see cref="GetSize"/>). When
        /// true, <see cref="GetSize"/> is not called, and the <see cref="MediaScaleOrigin"/> is used directly
        /// </summary>
        public bool UseAbsoluteScaleOrigin;

        /// <summary>
        /// The clockwise rotation of the frame, in degrees
        /// </summary>
        public double MediaRotation;

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0,0 (top-left corner of the frame)
        /// </summary>
        public Vector2 MediaRotationOrigin;

        /// <summary>
        /// When false, the <see cref="MediaScaleOrigin"/> is relative to the media size (see <see cref="GetSize"/>). When
        /// true, <see cref="GetSize"/> is not called, and the <see cref="MediaScaleOrigin"/> is used directly
        /// </summary>
        public bool UseAbsoluteRotationOrigin;
        private bool isMatrixDirty;

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

        public event MatrixChangedEventHandler MatrixChanged;

        public MotionEffect() {
            this.isMatrixDirty = true;
            UpdateAutomationValueEventHandler throwABrickAtMeThisSucks = (s, f) => this.InvalidateMatrix();
            this.AutomationData.AssignKey(MediaPositionKey, this.CreateAssignment(MediaPositionKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(MediaScaleKey, this.CreateAssignment(MediaScaleKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(MediaScaleOriginKey, this.CreateAssignment(MediaScaleOriginKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(UseAbsoluteScaleOriginKey, this.CreateAssignment(UseAbsoluteScaleOriginKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(MediaRotationKey, this.CreateAssignment(MediaRotationKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(MediaRotationOriginKey, this.CreateAssignment(MediaRotationOriginKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
            this.AutomationData.AssignKey(UseAbsoluteRotationOriginKey, this.CreateAssignment(UseAbsoluteRotationOriginKey)).AddUpdateHandler(throwABrickAtMeThisSucks);
        }

        private void InvalidateMatrix() {
            this.isMatrixDirty = true;
            this.OwnerClip?.InvalidateTransformationMatrix();
            this.MatrixChanged?.Invoke(this);
        }

        /// <summary>
        /// Creates a transformation matrix based on the current state of this effect
        /// </summary>
        /// <returns></returns>
        private SKMatrix CreateTransformationMatrix() {
            Vector2 pos = this.MediaPosition;
            Vector2 scale = this.MediaScale;
            Vector2 scaleOrigin = this.MediaScaleOrigin;
            double rotation = this.MediaRotation;
            Vector2 rotOrigin = this.MediaRotationOrigin;

            SKMatrix matrix = SKMatrix.Identity;
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(pos.X, pos.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateScale(scale.X, scale.Y, scaleOrigin.X, scaleOrigin.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) rotation, rotOrigin.X, rotOrigin.Y));
            return matrix;
        }

        protected override void OnRemovedFromClip() {
            base.OnRemovedFromClip();
            this.OwnerClip.InvalidateTransformationMatrix();
        }

        protected override void OnAddedToClip() {
            base.OnAddedToClip();
            this.OwnerClip.InvalidateTransformationMatrix();
        }

        public override void PreProcessFrame(long frame, RenderContext rc) {
            base.PreProcessFrame(frame, rc);
            rc.Canvas.SetMatrix(rc.Canvas.TotalMatrix.PreConcat(this.TransformationMatrix));
        }
    }
}