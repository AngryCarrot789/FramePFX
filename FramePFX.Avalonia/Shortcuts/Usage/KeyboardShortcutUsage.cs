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

using System.Collections.Generic;
using FramePFX.Avalonia.Shortcuts.Inputs;

namespace FramePFX.Avalonia.Shortcuts.Usage;

public class KeyboardShortcutUsage : IKeyboardShortcutUsage {
    private LinkedListNode<KeyStroke> currentStroke;
    private LinkedListNode<KeyStroke> previousStroke;

    public IKeyboardShortcut KeyboardShortcut { get; }

    public LinkedList<KeyStroke> Strokes { get; }

    public KeyStroke CurrentKeyStroke => this.currentStroke?.Value ?? default;

    public IShortcut Shortcut {
        get => this.KeyboardShortcut;
    }

    public bool IsCompleted => this.currentStroke == null;

    public IInputStroke PreviousStroke { get; private set; }

    public IInputStroke CurrentStroke => this.currentStroke?.Value;

    public IEnumerable<IInputStroke> RemainingStrokes {
        get {
            LinkedListNode<KeyStroke> stroke = this.currentStroke;
            while (stroke != null) {
                yield return stroke.Value;
                stroke = stroke.Next;
            }
        }
    }

    public bool IsCurrentStrokeMouseBased => false;

    public bool IsCurrentStrokeKeyBased => true;

    public KeyboardShortcutUsage(IKeyboardShortcut shortcut) {
        this.KeyboardShortcut = shortcut;
        this.Strokes = new LinkedList<KeyStroke>(shortcut.KeyStrokes);
        this.currentStroke = this.Strokes.First.Next;
        this.PreviousStroke = this.Strokes.First.Value;
    }

    public bool OnKeyStroke(in KeyStroke stroke) {
        if (this.currentStroke == null) {
            return true;
        }

        if (this.currentStroke.Value.Equals(stroke)) {
            this.PreviousStroke = stroke;
            this.currentStroke = this.currentStroke.Next;
            return true;
        }
        else {
            return false;
        }
    }

    public bool OnInputStroke(IInputStroke stroke) {
        return stroke is KeyStroke keyStroke && this.OnKeyStroke(keyStroke);
    }
}