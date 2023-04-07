using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Usage {
    public class MouseKeyboardShortcutUsage : IKeyboardShortcutUsage, IMouseShortcutUsage {
        private readonly MouseKeyboardShortcut shortcut;
        // private int clickCounter;

        private LinkedListNode<IInputStroke> currentStroke;
        public LinkedList<IInputStroke> Strokes { get; }

        public IKeyboardShortcut KeyboardShortcut => this.shortcut;

        public IMouseShortcut MouseShortcut => this.shortcut;

        public KeyStroke CurrentKeyStroke => this.currentStroke.Value is KeyStroke value ? value : default;

        public MouseStroke CurrentMouseStroke => this.currentStroke.Value is MouseStroke value ? value : default;

        public IShortcut Shortcut {
            get => this.KeyboardShortcut;
        }

        public bool IsCompleted => this.currentStroke == null;

        public IInputStroke PreviousStroke { get; private set; }

        public IInputStroke CurrentStroke => this.currentStroke?.Value;

        public IEnumerable<IInputStroke> RemainingStrokes {
            get {
                LinkedListNode<IInputStroke> stroke = this.currentStroke;
                while (stroke != null) {
                    yield return stroke.Value;
                    stroke = stroke.Next;
                }
            }
        }

        public bool IsCurrentStrokeMouseBased => this.currentStroke?.Value is MouseStroke;

        public bool IsCurrentStrokeKeyBased => this.currentStroke?.Value is KeyStroke;

        public MouseKeyboardShortcutUsage(MouseKeyboardShortcut shortcut) {
            this.shortcut = shortcut;
            this.Strokes = new LinkedList<IInputStroke>(shortcut.InputStrokes);
            this.currentStroke = this.Strokes.First.Next;
            this.PreviousStroke = this.Strokes.First.Value;
        }

        public bool OnKeyStroke(in KeyStroke stroke) {
            if (this.currentStroke == null) {
                return true;
            }

            if (stroke.Equals(this.currentStroke.Value)) {
                this.ProgressCurrentStroke();
                return true;
            }
            else {
                return false;
            }
        }

        public bool OnMouseStroke(in MouseStroke stroke) {
            if (this.currentStroke == null) {
                return true;
            }

            if (stroke.Equals(this.currentStroke.Value)) {
                this.ProgressCurrentStroke();
                return true;
            }
            else if (this.currentStroke.Value is MouseStroke cms && cms.EqualsWithoutClick(stroke)) {
                return true;
            }
            else {
                return false;
            }
        }

        public bool OnInputStroke(IInputStroke stroke) {
            if (this.currentStroke == null) {
                return true;
            }

            if (this.currentStroke.Value.Equals(stroke)) {
                this.ProgressCurrentStroke();
                return true;
            }
            else if (stroke is MouseStroke mouseStroke && this.currentStroke.Value is MouseStroke cms && cms.EqualsWithoutClick(mouseStroke)) {
                return true;
            }
            else {
                return false;
            }
        }

        private void ProgressCurrentStroke() {
            this.PreviousStroke = this.currentStroke.Value;
            this.currentStroke = this.currentStroke.Next;
        }
    }
}