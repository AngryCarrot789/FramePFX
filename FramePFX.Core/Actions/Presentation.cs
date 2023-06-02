using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.Actions {
    public readonly struct Presentation {
        private readonly int flags;

        /// <summary>
        /// Whether this button is visible on screen or not
        /// </summary>
        public bool IsVisible => (this.flags & 0b001) != 0;

        /// <summary>
        /// Whether this button is actually enabled and clickable
        /// </summary>
        public bool IsEnabled => (this.flags & 0b010) != 0;

        public static readonly Presentation VisibleAndEnabled = new Presentation(true, true);
        public static readonly Presentation VisibleAndDisabled = new Presentation(true, false);
        public static readonly Presentation Invisible = new Presentation(false, false);

        public Presentation(bool isVisible, bool isEnabled) {
            this.flags = Bits.Join(isVisible, isEnabled);
        }

        public static Presentation BoolToEnabled(bool isVisible) {
            return isVisible ? VisibleAndEnabled : VisibleAndDisabled;
        }
    }
}