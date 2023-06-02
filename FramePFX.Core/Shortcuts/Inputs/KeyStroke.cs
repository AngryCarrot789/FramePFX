using System;
using System.Text;

namespace FramePFX.Core.Shortcuts.Inputs {
    /// <summary>
    /// Represents a key stroke, as in, a key press or release which may have modifier keys present
    /// <para>
    /// KeyStrokes typically do not represent modifier key strokes, meaning <see cref="KeyCode"/> would not equal the key code for SHIFT, CTRL, ALT, etc
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
        /// The modifier keys that were pressed during the key stroke
        /// </summary>
        public int Modifiers { get; }

        /// <summary>
        /// Whether this key stroke corresponds to a key release. False means key pressed
        /// </summary>
        public bool IsKeyRelease { get; }

        /// <summary>
        /// Whether this key stroke corresponds to a key press. This is the inverse of <see cref="IsKeyRelease"/>
        /// </summary>
        public bool IsKeyDown => !this.IsKeyRelease;

        public bool IsKeyboard => true;

        public bool IsMouse => false;

        public KeyStroke(int keyCode, int modifiers, bool isKeyRelease) {
            this.KeyCode = keyCode;
            this.Modifiers = modifiers;
            this.IsKeyRelease = isKeyRelease;
        }

        public bool Equals(IInputStroke stroke) {
            return stroke is KeyStroke other && this.Equals(other);
        }

        public bool Equals(KeyStroke stroke) {
            return this.KeyCode == stroke.KeyCode && this.Modifiers == stroke.Modifiers && this.IsKeyRelease == stroke.IsKeyRelease;
        }

        public override bool Equals(object obj) {
            return obj is KeyStroke other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = this.KeyCode;
                hash = (hash * 397) ^ this.Modifiers;
                hash = (hash * 397) ^ (this.IsKeyRelease ? 1 : 0);
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
                if (this.IsKeyRelease) {
                    sb.Append(" (Release)");
                }
            }
            else {
                sb.Append(this.IsKeyRelease ? " (Release)" : " (Press)");
            }

            return sb.ToString();
        }
    }
}