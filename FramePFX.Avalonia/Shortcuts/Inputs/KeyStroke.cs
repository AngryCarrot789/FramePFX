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

using System;
using System.Text;

namespace FramePFX.Avalonia.Shortcuts.Inputs;

/// <summary>
/// Represents a key stroke, as in, a key press or release which may have modifier keys present
/// <para>
/// KeyStrokes can represent modifier key strokes too, meaning <see cref="KeyCode"/> could equal the key
/// code for SHIFT, CTRL, ALT, etc., and <see cref="Modifiers"/> will always be 0 in this case
/// </para>
/// </summary>
public readonly struct KeyStroke : IInputStroke, IEquatable<KeyStroke> {
    /// <summary>
    /// A non-null function for converting a key code into a string representation
    /// </summary>
    public static Func<int, string> KeyCodeToStringProvider { get; set; } = (x) => new StringBuilder(16).Append("KEY(").Append(x).Append(')').ToString();

    /// <summary>
    /// A non-null function for converting a keyboard modifier flag set into a string representation
    /// </summary>
    public static Func<int, bool, string> ModifierToStringProvider { get; set; } = (x, s) => new StringBuilder(16).Append("MOD(").Append(x).Append(')').ToString();

    /// <summary>
    /// The key code involved. This key code is relative to whatever key system the platform is running on
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

    // Cannot do since avalonia is a bucket full of . and can't detect repeated keys apparently
    // /// <summary>
    // /// Gets the repeat mode. 0 = ignored, 1 = Repeat Only, 2 = No Repeat
    // /// </summary>
    // public byte RepeatMode { get; }

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

    public static bool operator ==(KeyStroke left, KeyStroke right) => left.Equals(right);

    public static bool operator !=(KeyStroke left, KeyStroke right) => !(left == right);
}