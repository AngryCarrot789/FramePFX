using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Usage
{
    public class MouseShortcutUsage : IMouseShortcutUsage
    {
        private LinkedListNode<MouseStroke> currentStroke;
        // private int clickCounter;

        public IMouseShortcut MouseShortcut { get; }

        public LinkedList<MouseStroke> Strokes { get; }

        public MouseStroke CurrentMouseStroke => this.currentStroke?.Value ?? default;

        public IShortcut Shortcut
        {
            get => this.MouseShortcut;
        }

        public bool IsCompleted => this.currentStroke == null;

        public IInputStroke PreviousStroke { get; private set; }

        public IInputStroke CurrentStroke => this.currentStroke?.Value;

        public IEnumerable<IInputStroke> RemainingStrokes
        {
            get
            {
                LinkedListNode<MouseStroke> stroke = this.currentStroke;
                while (stroke != null)
                {
                    yield return stroke.Value;
                    stroke = stroke.Next;
                }
            }
        }

        public bool IsCurrentStrokeMouseBased => true;

        public bool IsCurrentStrokeKeyBased => false;

        public MouseShortcutUsage(IMouseShortcut shortcut)
        {
            this.MouseShortcut = shortcut;
            this.Strokes = new LinkedList<MouseStroke>(shortcut.MouseStrokes);
            this.currentStroke = this.Strokes.First.Next;
            this.PreviousStroke = this.Strokes.First.Value;
        }

        public bool OnMouseStroke(in MouseStroke stroke)
        {
            if (this.currentStroke == null)
            {
                return true;
            }

            if (this.currentStroke.Value.Equals(stroke))
            {
                this.PreviousStroke = stroke;
                this.currentStroke = this.currentStroke.Next;
                return true;
            }
            else if (this.currentStroke.Value.EqualsWithoutClick(stroke))
            {
                // this allows double or triple clicking
                // this.clickCounter++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool OnInputStroke(IInputStroke stroke)
        {
            return stroke is MouseStroke keyStroke && this.OnMouseStroke(keyStroke);
        }
    }
}