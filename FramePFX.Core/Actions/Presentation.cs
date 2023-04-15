using SharpPadV2.Core.Utils;

namespace SharpPadV2.Core.Actions {
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

        public static Presentation VisibleAndEnabled { get; } = new Presentation(true, true);
        public static Presentation VisibleAndDisabled { get; } = new Presentation(true, false);
        public static Presentation Invisible { get; } = new Presentation(false, false);

        public Presentation(bool isVisible, bool isEnabled) {
            this.flags = Bits.Join(isVisible, isEnabled);
        }

        public static Presentation BoolToEnabled(bool isVisible) {
            return isVisible ? VisibleAndEnabled : VisibleAndDisabled;
        }
    }
}