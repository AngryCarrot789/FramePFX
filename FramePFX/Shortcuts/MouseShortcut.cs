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

using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Usage;
using FramePFX.Utils;

namespace FramePFX.Shortcuts;

public class MouseShortcut : IMouseShortcut {
    public static MouseShortcut EmptyMouseShortcut = new MouseShortcut();

    private readonly List<MouseStroke> mouseStrokes;

    public IInputStroke PrimaryStroke => this.mouseStrokes[0];

    public MouseStroke PrimaryMouseStroke => this.mouseStrokes[0];
    
    public IEnumerable<IInputStroke> InputStrokes {
        get => this.mouseStrokes.Cast<IInputStroke>();
    }

    public IEnumerable<MouseStroke> MouseStrokes => this.mouseStrokes;

    public bool IsKeyboard => false;

    public bool IsMouse => true;

    public bool IsEmpty => this.mouseStrokes.Count <= 0;

    public bool HasSecondaryStrokes => this.mouseStrokes.Count > 1;

    public MouseShortcut() {
        this.mouseStrokes = new List<MouseStroke>();
    }

    public MouseShortcut(params MouseStroke[] secondMouseStrokes) {
        this.mouseStrokes = new List<MouseStroke>(secondMouseStrokes);
    }

    public MouseShortcut(IEnumerable<MouseStroke> secondMouseStrokes) {
        this.mouseStrokes = new List<MouseStroke>(secondMouseStrokes);
    }

    public MouseShortcut(List<MouseStroke> mouseStrokes) {
        Validate.NotNull(mouseStrokes);
        this.mouseStrokes = mouseStrokes;
    }

    public IMouseShortcutUsage CreateMouseUsage() {
        return this.IsEmpty ? throw new InvalidOperationException("Shortcut is empty. Cannot create a usage") : new MouseShortcutUsage(this);
    }

    public IShortcutUsage CreateUsage() {
        return this.CreateMouseUsage();
    }

    public bool IsPrimaryStroke(IInputStroke input) {
        return input is MouseStroke stroke && this.mouseStrokes[0].Equals(stroke);
    }

    public override string ToString() {
        return string.Join(", ", this.mouseStrokes);
    }

    public override bool Equals(object? obj) {
        if (obj is MouseShortcut shortcut) {
            int lenA = this.mouseStrokes.Count;
            int lenB = shortcut.mouseStrokes.Count;
            if (lenA != lenB) {
                return false;
            }

            for (int i = 0; i < lenA; i++) {
                if (!this.mouseStrokes[i].Equals(shortcut.mouseStrokes[i])) {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override int GetHashCode() {
        int code = 0;
        foreach (MouseStroke stroke in this.mouseStrokes)
            code += stroke.GetHashCode();
        return code;
    }
}