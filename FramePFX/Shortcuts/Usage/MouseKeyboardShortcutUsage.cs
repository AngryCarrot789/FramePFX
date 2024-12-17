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

namespace FramePFX.Shortcuts.Usage;

public class MouseKeyboardShortcutUsage : IKeyboardShortcutUsage, IMouseShortcutUsage
{
    private readonly MouseKeyboardShortcut shortcut;
    // private int clickCounter;

    private LinkedListNode<IInputStroke> currentStroke;
    public LinkedList<IInputStroke> Strokes { get; }

    public IKeyboardShortcut KeyboardShortcut => this.shortcut;

    public IMouseShortcut MouseShortcut => this.shortcut;

    public KeyStroke CurrentKeyStroke => this.currentStroke?.Value is KeyStroke value ? value : default;

    public MouseStroke CurrentMouseStroke => this.currentStroke?.Value is MouseStroke value ? value : default;

    public IShortcut Shortcut
    {
        get => this.KeyboardShortcut;
    }

    public bool IsCompleted => this.currentStroke == null;

    public IInputStroke PreviousStroke { get; private set; }

    public IInputStroke CurrentStroke => this.currentStroke?.Value;

    public IEnumerable<IInputStroke> RemainingStrokes
    {
        get
        {
            LinkedListNode<IInputStroke> stroke = this.currentStroke;
            while (stroke != null)
            {
                yield return stroke.Value;
                stroke = stroke.Next;
            }
        }
    }

    public bool IsCurrentStrokeMouseBased => this.currentStroke?.Value is MouseStroke;

    public bool IsCurrentStrokeKeyBased => this.currentStroke?.Value is KeyStroke;

    public MouseKeyboardShortcutUsage(MouseKeyboardShortcut shortcut)
    {
        this.shortcut = shortcut;
        this.Strokes = new LinkedList<IInputStroke>(shortcut.InputStrokes);
        this.currentStroke = this.Strokes.First.Next;
        this.PreviousStroke = this.Strokes.First.Value;
    }

    public bool OnKeyStroke(in KeyStroke stroke)
    {
        if (this.currentStroke == null)
        {
            return true;
        }

        if (stroke.Equals(this.currentStroke.Value))
        {
            this.ProgressCurrentStroke();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool OnMouseStroke(in MouseStroke stroke)
    {
        if (this.currentStroke == null)
        {
            return true;
        }
        else if (stroke.Equals(this.currentStroke.Value))
        {
            this.ProgressCurrentStroke();
            return true;
        }
        else if (this.currentStroke.Value is MouseStroke cms && cms.EqualsWithoutClickOrRelease(stroke))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool OnInputStroke(IInputStroke stroke)
    {
        if (this.currentStroke == null)
        {
            return true;
        }

        if (this.currentStroke.Value.Equals(stroke))
        {
            this.ProgressCurrentStroke();
            return true;
        }
        else if (stroke is MouseStroke mouseStroke && this.currentStroke.Value is MouseStroke cms && cms.EqualsWithoutClickOrRelease(mouseStroke))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ProgressCurrentStroke()
    {
        this.PreviousStroke = this.currentStroke.Value;
        this.currentStroke = this.currentStroke.Next;
    }
}