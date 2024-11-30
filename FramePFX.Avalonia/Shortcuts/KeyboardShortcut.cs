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
using System.Collections.Generic;
using System.Linq;
using FramePFX.Avalonia.Shortcuts.Inputs;
using FramePFX.Avalonia.Shortcuts.Usage;

namespace FramePFX.Avalonia.Shortcuts;

/// <summary>
/// Represents a keyboard-based shortcut. This consists of 1 or more key strokes required to activate it
/// <para>
/// When the shortcut only consists of 1 key stroke, the shortcut may be activated immediately.
/// </para>
/// </summary>
public class KeyboardShortcut : IKeyboardShortcut {
    public static readonly KeyboardShortcut EmptyKeyboardShortcut = new KeyboardShortcut();

    private readonly List<KeyStroke> keyStrokes;

    public IInputStroke PrimaryStroke => this.keyStrokes[0];

    public IEnumerable<IInputStroke> InputStrokes {
        get => this.keyStrokes.Cast<IInputStroke>();
    }

    public IEnumerable<KeyStroke> KeyStrokes => this.keyStrokes;

    public bool IsKeyboard => true;

    public bool IsMouse => false;

    public bool IsEmpty => this.keyStrokes.Count <= 0;

    public bool HasSecondaryStrokes => this.keyStrokes.Count > 1;

    public KeyboardShortcut() {
        this.keyStrokes = new List<KeyStroke>();
    }

    public KeyboardShortcut(params KeyStroke[] secondKeyStrokes) {
        this.keyStrokes = new List<KeyStroke>(secondKeyStrokes);
    }

    public KeyboardShortcut(IEnumerable<KeyStroke> strokes) {
        this.keyStrokes = new List<KeyStroke>(strokes);
    }

    public KeyboardShortcut(List<KeyStroke> keyStrokes) {
        this.keyStrokes = keyStrokes;
    }

    public IKeyboardShortcutUsage CreateKeyUsage() {
        return this.IsEmpty ? throw new InvalidOperationException("Shortcut is empty. Cannot create a usage") : new KeyboardShortcutUsage(this);
    }

    public IShortcutUsage CreateUsage() {
        return this.CreateKeyUsage();
    }

    public bool IsPrimaryStroke(IInputStroke input) {
        return input is KeyStroke stroke && this.keyStrokes[0].Equals(stroke);
    }

    public override string ToString() {
        return string.Join(", ", this.keyStrokes);
    }

    public override bool Equals(object obj) {
        if (obj is KeyboardShortcut shortcut) {
            int lenA = this.keyStrokes.Count;
            int lenB = shortcut.keyStrokes.Count;
            if (lenA != lenB) {
                return false;
            }

            for (int i = 0; i < lenA; i++) {
                if (!this.keyStrokes[i].Equals(shortcut.keyStrokes[i])) {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override int GetHashCode() {
        int code = 0;
        foreach (KeyStroke stroke in this.keyStrokes)
            code += stroke.GetHashCode();
        return code;
    }
}