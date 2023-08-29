using System.Numerics;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines.Effects.Video {
    public class MotionEffect : VideoEffect {
        public static readonly AutomationKeyVector2 MediaPositionKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaPosition), Vector2.Zero, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScale), Vector2.One, Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyVector2 MediaScaleOriginKey = AutomationKey.RegisterVec2(nameof(MotionEffect), nameof(MediaScaleOrigin), new Vector2(0.5f, 0.5f), Vectors.MinValue, Vectors.MaxValue);
        public static readonly AutomationKeyBoolean UseAbsoluteScaleOriginKey = AutomationKey.RegisterBool(nameof(MotionEffect), nameof(UseAbsoluteScaleOrigin));

        // saves using closure allocation for each clip
        private static readonly UpdateAutomationValueEventHandler UpdateMediaPosition = (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaPosition = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateMediaScale = (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScale = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateMediaScaleOrigin = (s, f) => ((MotionEffect) s.AutomationData.Owner).MediaScaleOrigin = s.GetVector2Value(f);
        private static readonly UpdateAutomationValueEventHandler UpdateUseAbsoluteScaleOrigin = (s, f) => ((MotionEffect) s.AutomationData.Owner).UseAbsoluteScaleOrigin = s.GetBooleanValue(f);

        public Vector2? FrameSize;

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

        public MotionEffect(VideoClip clip) {
            this.OwnerClip = clip;
            this.CanRemove = false;

            this.MediaPosition = MediaPositionKey.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleKey.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginKey.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(MediaPositionKey, UpdateMediaPosition);
            this.AutomationData.AssignKey(MediaScaleKey, UpdateMediaScale);
            this.AutomationData.AssignKey(MediaScaleOriginKey, UpdateMediaScaleOrigin);
            this.AutomationData.AssignKey(UseAbsoluteScaleOriginKey, UpdateUseAbsoluteScaleOrigin);
        }

        public override void ProcessFrame(RenderContext rc) {
            base.ProcessFrame(rc);
            Vector2 pos = this.MediaPosition, scale = this.MediaScale, origin = this.MediaScaleOrigin;
            rc.Canvas.Translate(pos.X, pos.Y);
            Vector2 sz = this.FrameSize ?? new Vector2(rc.FrameInfo.Width, rc.FrameInfo.Height);
            if (this.UseAbsoluteScaleOrigin) {
                rc.Canvas.Scale(scale.X, scale.Y, origin.X, origin.Y);
            }
            else {
                rc.Canvas.Scale(scale.X, scale.Y, sz.X * origin.X, sz.Y * origin.Y);
            }
        }
    }
}