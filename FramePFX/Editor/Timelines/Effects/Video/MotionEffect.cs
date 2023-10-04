using System.Numerics;
using FramePFX.Automation.Keys;
using FramePFX.Rendering;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Effects.Video
{
    /// <summary>
    /// An effect that deals with picture transformations (as in position, scale and scale origin)
    /// </summary>
    public class MotionEffect : VideoEffect
    {
        public static readonly AutomationKeyVector2 MediaPositionKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScaleOrigin), new Vector2(), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteScaleOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin));
        public static readonly AutomationKeyDouble MediaRotationKey = AutomationKey.RegisterDouble(nameof(MotionEffect), nameof(MediaRotation), 0d);
        public static readonly AutomationKeyVector2 MediaRotationOriginKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaRotationOrigin), new Vector2(0.5f, 0.5f), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteRotationOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteRotationOrigin));

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition;

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
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

        public MotionEffect()
        {
            this.AutomationData.AssignKey(MediaPositionKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaPosition = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScale = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScaleOrigin = s.GetVector2Value(f));
            this.AutomationData.AssignKey(UseAbsoluteScaleOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).UseAbsoluteScaleOrigin = s.GetBooleanValue(f));
            this.AutomationData.AssignKey(MediaRotationKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaRotation = s.GetDoubleValue(f));
            this.AutomationData.AssignKey(MediaRotationOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaRotationOrigin = s.GetVector2Value(f));
            this.AutomationData.AssignKey(UseAbsoluteRotationOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).UseAbsoluteRotationOrigin = s.GetBooleanValue(f));
        }

        // apply transformation

        public override void PreProcessFrame(long frame, RenderContext rc, Vector2? frameSize)
        {
            base.PreProcessFrame(frame, rc, frameSize);
            Vector2 pos = this.MediaPosition;
            Vector2 scale = this.MediaScale;
            Vector2 scaleOrigin = this.MediaScaleOrigin;
            Vector2 rotationOrigin = this.MediaRotationOrigin;
            float rotation = (float)(this.MediaRotation * System.Math.PI / 180d);

            scaleOrigin.X -= 0.5f;
            scaleOrigin.Y -= 0.5f;
            rotationOrigin.X -= 0.5f;
            rotationOrigin.Y -= 0.5f;

            rc.MatrixStack.PushMatrix();
            if (frameSize.HasValue)
            {
                // clip has size info that we can use to transform relative top-left corner
                Vector2 size = frameSize.Value;
                Vector2 center = new Vector2(size.X / 2.0f, size.Y / 2.0f);
                rc.MatrixStack.RotateZ(rotation,
                    this.UseAbsoluteRotationOrigin
                        ? new Vector3(rotationOrigin.X, rotationOrigin.Y, 0f)
                        : new Vector3(size.X * rotationOrigin.X, size.Y * rotationOrigin.Y, 0f));
                rc.MatrixStack.Scale(new Vector3(scale.X, scale.Y, 1f),
                    this.UseAbsoluteScaleOrigin
                        ? new Vector3(scaleOrigin.X, scaleOrigin.Y, 0f)
                        : new Vector3(size.X * scaleOrigin.X, size.Y * scaleOrigin.Y, 0f));
                rc.MatrixStack.Translate(new Vector3(pos.X + center.X, pos.Y + center.Y, 0f));
            }
            else
            {
                // worst case; clip has no size data so we assume it takes up 0 pixels
                rc.MatrixStack.RotateZ(rotation,
                    this.UseAbsoluteRotationOrigin
                        ? new Vector3(rotationOrigin.X, rotationOrigin.Y, 0f)
                        : Vector3.Zero);
                rc.MatrixStack.Scale(new Vector3(scale.X, scale.Y, 1f),
                    this.UseAbsoluteScaleOrigin
                        ? new Vector3(scaleOrigin.X, scaleOrigin.Y, 0f)
                        : Vector3.Zero);
                rc.MatrixStack.Translate(new Vector3(pos.X, pos.Y, 0f));
            }
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize)
        {
            base.PostProcessFrame(frame, rc, frameSize);
            rc.MatrixStack.PopMatrix();
        }
    }
}