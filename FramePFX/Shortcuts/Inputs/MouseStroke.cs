// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Text;

namespace FramePFX.Shortcuts.Inputs;

public readonly struct MouseStroke : IInputStroke, IEquatable<MouseStroke> {
    /// <summary>
    /// A non-null function for converting a mouse button into a string representation
    /// </summary>
    public static Func<int, string> MouseButtonToStringProvider { get; set; } = (x) => new StringBuilder(20).Append("MOUSE(").Append(x).Append(')').ToString();

    /// <summary>
    /// The mouse button that was clicked. Special care must be taken for mouse wheel inputs
    /// </summary>
    public int MouseButton { get; }

    /// <summary>
    /// The modifier keys that were pressed during the mouse input
    /// </summary>
    public int Modifiers { get; }

    /// <summary>
    /// Whether or not the mouse input was released. At the moment, this field is not used for normal shortcut processing, because of the
    /// complications with managing both mouse up and down. Any mouse stroke is classes as a "Click" which can technically mean this property
    /// is true in that case
    /// <para>
    /// This is however used by the input state system, where a mouse down can activate a state, and mouse up can deactivate it
    /// </para>
    /// </summary>
    public bool IsRelease { get; }

    /// <summary>
    /// The number of times the mouse was clicked during this stroke. This number is usually calculated
    /// by the operating system per mouse input within a certain interval time between inputs
    /// <para>
    /// This means that, for example, in order for this instance to contain a <see cref="ClickCount"/> of 3,
    /// 3 mouse inputs must have occurred previously within a certain time frame (typically less than 500ms per input)
    /// </para>
    /// <para>
    /// Set to -1 to disable
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

    public bool IsKeyboard => false;

    public bool IsMouse => true;

    public MouseStroke(int mouseButton, int modifiers, bool isRelease, int clickCount = -1, int wheelDelta = 0) {
        this.MouseButton = mouseButton;
        this.Modifiers = modifiers;
        this.IsRelease = isRelease;
        this.ClickCount = clickCount;
        this.WheelDelta = wheelDelta;
    }

    /// <summary>
    /// Gets whether the given stroke is a mouse stroke and it matches this instance
    /// </summary>
    /// <param name="stroke">The stroke to compare</param>
    /// <returns>The current instance and the given stroke are "equal/match"</returns>
    public bool Equals(IInputStroke stroke) => stroke is MouseStroke other && this.Equals(other);

    public override bool Equals(object obj) => obj is MouseStroke other && this.Equals(other);

    public bool Equals(MouseStroke other) {
        return this.EqualsExceptRelease(other) && this.IsRelease == other.IsRelease;
    }

    public bool EqualsWithoutClickOrRelease(MouseStroke other) {
        return this.MouseButton == other.MouseButton && this.Modifiers == other.Modifiers && this.WheelDelta == other.WheelDelta;
    }

    /// <summary>
    /// Whether or not the current and other mouse strokes are equal (same button, mods,
    /// click count and wheel delta) while ignoring the pressed/release value
    /// </summary>
    /// <param name="stroke">Other stroke to compare</param>
    /// <returns>True or false... see above</returns>
    public bool EqualsExceptRelease(MouseStroke stroke) {
        return this.MouseButton == stroke.MouseButton &&
               this.Modifiers == stroke.Modifiers &&
               (this.ClickCount == -1 || stroke.ClickCount == -1 || this.ClickCount == stroke.ClickCount) &&
               this.WheelDelta == stroke.WheelDelta;
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = this.MouseButton;
            hashCode = (hashCode * 397) ^ this.Modifiers;
            hashCode = (hashCode * 397) ^ this.ClickCount;
            hashCode = (hashCode * 397) ^ this.WheelDelta;
            return hashCode;
        }
    }

    public override string ToString() {
        return this.ToString(true, true, false);
    }

    public string ToString(bool appendClickCount, bool appendDelta, bool useModSpacers) {
        StringBuilder sb = new StringBuilder();
        string mod = KeyStroke.ModifierToStringProvider(this.Modifiers, useModSpacers);
        if (mod.Length > 0) {
            sb.Append(mod).Append(useModSpacers ? " + " : "+");
        }

        sb.Append(MouseButtonToStringProvider(this.MouseButton));
        if (appendClickCount && this.ClickCount >= 0) {
            sb.Append(" (x").Append(this.ClickCount).Append(')');
        }

        if (appendDelta && this.WheelDelta != 0) {
            sb.Append(" (Delta ").Append(this.WheelDelta).Append(')');
        }

        return sb.ToString();
    }

    public static bool operator ==(MouseStroke left, MouseStroke right) => left.Equals(right);

    public static bool operator !=(MouseStroke left, MouseStroke right) => !(left == right);
}