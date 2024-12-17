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

/// <summary>
/// A shortcut that accepts a combination of key and mouse strokes
/// </summary>
public class MouseKeyboardShortcut : IMouseShortcut, IKeyboardShortcut
{
    public static readonly MouseShortcut EmptyMouseKeyboardShortcut = new MouseShortcut();

    private readonly List<IInputStroke> inputStrokes;

    public IEnumerable<MouseStroke> MouseStrokes => this.inputStrokes.OfType<MouseStroke>();

    public IEnumerable<KeyStroke> KeyStrokes => this.inputStrokes.OfType<KeyStroke>();

    public IInputStroke PrimaryStroke => this.inputStrokes[0];

    public IEnumerable<IInputStroke> InputStrokes
    {
        get => this.inputStrokes;
    }

    public bool IsKeyboard => true;

    public bool IsMouse => true;

    public bool IsEmpty => this.inputStrokes.Count <= 0;

    public bool HasSecondaryStrokes => this.inputStrokes.Count > 1;

    public MouseKeyboardShortcut()
    {
        this.inputStrokes = new List<IInputStroke>();
    }

    public MouseKeyboardShortcut(params IInputStroke[] secondMouseStrokes)
    {
        this.inputStrokes = new List<IInputStroke>(secondMouseStrokes);
    }

    public MouseKeyboardShortcut(IEnumerable<IInputStroke> secondMouseStrokes)
    {
        this.inputStrokes = new List<IInputStroke>(secondMouseStrokes);
    }

    public MouseKeyboardShortcut(List<IInputStroke> inputStrokes)
    {
        Validate.NotNull(inputStrokes);
        this.inputStrokes = inputStrokes;
    }

    public IMouseShortcutUsage CreateMouseUsage()
    {
        return (IMouseShortcutUsage) this.CreateUsage();
    }


    public IKeyboardShortcutUsage CreateKeyUsage()
    {
        return (IKeyboardShortcutUsage) this.CreateUsage();
    }

    public IShortcutUsage CreateUsage()
    {
        return this.IsEmpty ? throw new InvalidOperationException("Shortcut is empty. Cannot create a usage") : new MouseKeyboardShortcutUsage(this);
    }

    public bool IsPrimaryStroke(IInputStroke input)
    {
        return this.PrimaryStroke.Equals(input);
    }

    public override string ToString()
    {
        return string.Join(", ", this.inputStrokes);
    }

    public override bool Equals(object? obj)
    {
        if (obj is MouseKeyboardShortcut shortcut)
        {
            int lenA = this.inputStrokes.Count;
            int lenB = shortcut.inputStrokes.Count;
            if (lenA != lenB)
            {
                return false;
            }

            for (int i = 0; i < lenA; i++)
            {
                if (!this.inputStrokes[i].Equals(shortcut.inputStrokes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        int code = 0;
        foreach (IInputStroke stroke in this.inputStrokes)
            code += stroke.GetHashCode();
        return code;
    }
}