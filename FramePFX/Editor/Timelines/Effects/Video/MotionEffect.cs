using System.Numerics;
using FramePFX.Automation.Keys;
using FramePFX.Rendering;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;

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
            Vector2 rotOrigin = this.MediaRotationOrigin;

            scaleOrigin.X -= 0.5f;
            scaleOrigin.Y -= 0.5f;
            rotOrigin.X -= 0.5f;
            rotOrigin.Y -= 0.5f;

            float rotation = (float) (this.MediaRotation * System.Math.PI / 180d);

            Matrix4x4 matrix = Matrix4x4.Identity;
            if (frameSize.HasValue)
            {
                // clip has size info that we can use to transform relative top-left corner
                Vector2 size = frameSize.Value;
                if (this.UseAbsoluteRotationOrigin)
                {
                    matrix *= Matrix4x4.CreateRotationZ(rotation, new Vector3(rotOrigin.X, rotOrigin.Y, 0f));
                }
                else
                {
                    matrix *= Matrix4x4.CreateRotationZ(rotation, new Vector3(size.X * rotOrigin.X, size.Y * rotOrigin.Y, 0f));
                }

                if (this.UseAbsoluteScaleOrigin)
                {
                    matrix *= Matrix4x4.CreateScale(scale.X, scale.Y, 1f, new Vector3(scaleOrigin.X, scaleOrigin.Y, 0f));
                }
                else
                {
                    matrix *= Matrix4x4.CreateScale(scale.X, scale.Y, 1f, new Vector3(size.X * scaleOrigin.X, size.Y * scaleOrigin.Y, 0f));
                }

                matrix *= Matrix4x4.CreateTranslation(pos.X + (size.X / 2), pos.Y + (size.Y / 2), 0f);
            }
            else
            {
                // worst case; clip has no size data so we assume it takes up 0 pixels
                if (this.UseAbsoluteRotationOrigin)
                {
                    matrix *= Matrix4x4.CreateRotationZ(rotation, new Vector3(rotOrigin.X, rotOrigin.Y, 0f));
                }

                if (this.UseAbsoluteScaleOrigin)
                {
                    matrix *= Matrix4x4.CreateScale(scale.X, scale.Y, 1f, new Vector3(scaleOrigin.X, scaleOrigin.Y, 0f));
                }

                matrix *= Matrix4x4.CreateTranslation(pos.X, pos.Y, 0f);
            }

            rc.MatrixStack.PushMatrix(ref matrix);
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize)
        {
            base.PostProcessFrame(frame, rc, frameSize);
            rc.MatrixStack.PopMatrix();
        }
    }
}