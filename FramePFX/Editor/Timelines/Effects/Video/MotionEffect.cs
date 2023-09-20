using System.Numerics;
using FramePFX.Automation.Keys;
using FramePFX.Rendering;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Effects.Video {
    /// <summary>
    /// An effect that deals with picture transformations (as in position, scale and scale origin)
    /// </summary>
    public class MotionEffect : VideoEffect {
        public static readonly AutomationKeyVector2 MediaPositionKey          = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleKey             = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleOriginKey       = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScaleOrigin), new Vector2(), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteScaleOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin));

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

        public MotionEffect() {
            this.AutomationData.AssignKey(MediaPositionKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaPosition = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScale = s.GetVector2Value(f));
            this.AutomationData.AssignKey(MediaScaleOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScaleOrigin = s.GetVector2Value(f));
            this.AutomationData.AssignKey(UseAbsoluteScaleOriginKey, (s, f) => ((MotionEffect) s.AutomationData.Owner).UseAbsoluteScaleOrigin = s.GetBooleanValue(f));

            this.MediaPosition = MediaPositionKey.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleKey.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginKey.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginKey.Descriptor.DefaultValue;
        }

        // apply transformation

        public override void PreProcessFrame(RenderContext rc, Vector2? frame) {
            base.PreProcessFrame(rc, frame);
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;
            rc.Canvas.Translate(pos.X, pos.Y);
            if (this.UseAbsoluteScaleOrigin) {
                rc.Canvas.Scale(scale.X, scale.Y, origin.X, origin.Y);
            }
            else if (frame.HasValue) {
                Vector2 size = frame.Value;
                rc.Canvas.Scale(scale.X, scale.Y, size.X * origin.X, size.Y * origin.Y);
            }
        }
    }
}