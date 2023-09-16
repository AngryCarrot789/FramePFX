using System;
using System.Text;

namespace FramePFX.Shortcuts.Inputs {
    /// <summary>
    /// Represents a key stroke, as in, a key press or release which may have modifier keys present
    /// <para>
    /// KeyStrokes can represent modifier key strokes too, meaning <see cref="KeyCode"/> could equal the key
    /// code for SHIFT, CTRL, ALT, etc. In this case, <see cref="Modifiers"/> will always be 0
    /// </para>
    /// </summary>
    public readonly struct KeyStroke : IInputStroke {
        /// <summary>
        /// A non-null function for converting a key code into a string representation
        /// </summary>
        public static Func<int, string> KeyCodeToStringProvider { get; set; } = (x) => new StringBuilder(16).Append("KEY(").Append(x).Append(')').ToString();

        /// <summary>
        /// A non-null function for converting a keyboard modifier flag set into a string representation
        /// </summary>
        public static Func<int, bool, string> ModifierToStringProvider { get; set; } = (x, s) => new StringBuilder(16).Append("MOD(").Append(x).Append(')').ToString();

        /// <summary>
        /// The key code involved (cannot be a modifier key). This key code is relative to whatever key system the platform is running on
        /// </summary>
        public int KeyCode { get; }

        /// <summary>
        /// The modifier keys (bitflags) that were pressed during the key stroke, or 0, if <see cref="KeyCode"/> represents a modifier key
        /// </summary>
        public int Modifiers { get; }

        /// <summary>
        /// Whether this key stroke corresponds to a key release. False means key pressed
        /// </summary>
        public bool IsRelease { get; }

        /// <summary>
        /// Whether this key stroke corresponds to a key press. This is the inverse of <see cref="IsRelease"/>
        /// </summary>
        public bool IsKeyDown => !this.IsRelease;

        public bool IsKeyboard => true;

        public bool IsMouse => false;

        public KeyStroke(int keyCode, int modifiers, bool isRelease) {
            this.KeyCode = keyCode;
            this.Modifiers = modifiers;
            this.IsRelease = isRelease;
        }

        /// <summary>
        /// Gets whether the given stroke is a key stroke and it matches this instance
        /// </summary>
        /// <param name="stroke">The stroke to compare</param>
        /// <returns>The current instance and the given stroke are "equal/match"</returns>
        public bool Equals(IInputStroke stroke) => stroke is KeyStroke other && this.Equals(other);

        public override bool Equals(object obj) => obj is KeyStroke other && this.Equals(other);

        public bool Equals(KeyStroke stroke) {
            return this.KeyCode == stroke.KeyCode && this.Modifiers == stroke.Modifiers && this.IsRelease == stroke.IsRelease;
        }

        public bool EqualsExceptRelease(KeyStroke stroke) {
            return this.KeyCode == stroke.KeyCode && this.Modifiers == stroke.Modifiers;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = this.KeyCode;
                hash = (hash * 397) ^ this.Modifiers;
                hash = (hash * 397) ^ (this.IsRelease ? 1 : 0);
                return hash;
            }
        }

        public override string ToString() {
            return this.ToString(false, true);
        }

        public string ToString(bool appendIsReleaseOnly, bool useSpacers) {
            StringBuilder sb = new StringBuilder();
            string mod = ModifierToStringProvider(this.Modifiers, useSpacers);
            if (mod.Length > 0) {
                sb.Append(mod).Append(useSpacers ? " + " : "+");
            }

            sb.Append(KeyCodeToStringProvider(this.KeyCode));
            if (appendIsReleaseOnly) {
                if (this.IsRelease) {
                    sb.Append(" (Release)");
                }
            }
            else {
                sb.Append(this.IsRelease ? " (Release)" : " (Press)");
            }

            return sb.ToString();
        }
    }
}