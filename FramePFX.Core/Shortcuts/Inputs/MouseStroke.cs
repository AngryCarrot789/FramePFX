using System;
using System.Text;

namespace SharpPadV2.Core.Shortcuts.Inputs {
    public readonly struct MouseStroke : IInputStroke {
        /// <summary>
        /// A non-null function for converting a mouse button into a string representation
        /// </summary>
        public static Func<int, string> MouseButtonToStringProvider { get; set; } = (x) => new StringBuilder(20).Append("MOUSE(").Append(x).Append(')').ToString();

        /// <summary>
        /// A non-null function for converting a keyboard modifier flag set into a string representation
        /// </summary>
        public static Func<int, bool, string> ModifierToStringProvider { get; set; } = (x, s) => new StringBuilder(16).Append("MOD(").Append(x).Append(')').ToString();

        /// <summary>
        /// The mouse button that was clicked. Special care must be taken for mouse wheel inputs
        /// </summary>
        public int MouseButton { get; }

        /// <summary>
        /// The modifier keys that were pressed during the mouse input
        /// </summary>
        public int Modifiers { get; }

        /// <summary>
        /// The number of times the mouse was clicked during this stroke. This number is usually calculated
        /// by the operating system per mouse input within a certain interval time between inputs
        /// <para>
        /// This means that,
        /// for example, in order for this instance to contain a <see cref="ClickCount"/> of 3, 3 mouse inputs must
        /// have occurred previously within a certain time frame (typically less than 500ms per input)
        /// </para>
        /// </summary>
        public int ClickCount { get; }

        /// <summary>
        /// The current mouse wheel's delta. This will typically be 0 if this mouse stroke is not a mouse wheel input
        /// <para>
        /// On windows for example, each mouse wheel input has a delta value of 120, meaning this value will most
        /// likely be a multiple of 120. This was to allow "freely-rotating mouse wheels without notches"
        /// </para>
        /// </summary>
        public int WheelDelta { get; }

        /// <summary>
        /// A custom parameter for this mouse stroke. This will be used during <see cref="MouseStroke"/> equality
        /// testing and hashing like all the other fields. This can store any custom data with this mouse input
        /// </summary>
        public int CustomParam { get; }

        public bool IsKeyboard => false;

        public bool IsMouse => true;

        public MouseStroke(int mouseButton, int modifiers, int clickCount = -1, int wheelDelta = 0, int customParam = 0) {
            this.MouseButton = mouseButton;
            this.Modifiers = modifiers;
            this.ClickCount = clickCount;
            this.WheelDelta = wheelDelta;
            this.CustomParam = customParam;
        }

        public bool Equals(IInputStroke stroke) {
            return stroke is MouseStroke other && this.Equals(other);
        }

        public override bool Equals(object obj) {
            return obj is MouseStroke other && this.Equals(other);
        }

        public bool Equals(MouseStroke other) {
            return this.MouseButton == other.MouseButton &&
                   this.Modifiers == other.Modifiers &&
                   (this.ClickCount == -1 || other.ClickCount == -1 || this.ClickCount == other.ClickCount) &&
                   this.WheelDelta == other.WheelDelta &&
                   this.CustomParam == other.CustomParam;
        }

        public bool EqualsWithoutClick(MouseStroke other) {
            return this.MouseButton == other.MouseButton &&
                   this.Modifiers == other.Modifiers &&
                   this.WheelDelta == other.WheelDelta &&
                   this.CustomParam == other.CustomParam;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = this.MouseButton;
                hashCode = (hashCode * 397) ^ this.Modifiers;
                hashCode = (hashCode * 397) ^ this.ClickCount;
                hashCode = (hashCode * 397) ^ this.WheelDelta;
                hashCode = (hashCode * 397) ^ this.CustomParam;
                return hashCode;
            }
        }

        public override string ToString() {
            return this.ToString(true, true, true);
        }

        public string ToString(bool appendClickCount, bool appendDelta, bool useSpacers) {
            StringBuilder sb = new StringBuilder();
            string mod = ModifierToStringProvider(this.Modifiers, useSpacers);
            if (mod.Length > 0) {
                sb.Append(mod).Append(useSpacers ? " + " : "+");
            }

            sb.Append(MouseButtonToStringProvider(this.MouseButton));
            if (appendClickCount && this.ClickCount >= 0) {
                sb.Append(" (x").Append(this.ClickCount).Append(')');
            }

            if (appendDelta && this.WheelDelta != 0) {
                sb.Append(" (ROT ").Append(this.WheelDelta).Append(')');
            }

            return sb.ToString();
        }
    }
}