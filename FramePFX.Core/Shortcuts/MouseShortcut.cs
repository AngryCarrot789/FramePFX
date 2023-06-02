using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Usage;

namespace FramePFX.Core.Shortcuts {
    public class MouseShortcut : IMouseShortcut {
        public static MouseShortcut EmptyMouseShortcut = new MouseShortcut();

        private readonly List<MouseStroke> mouseStrokes;

        public IInputStroke PrimaryStroke => this.mouseStrokes[0];

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

        public override bool Equals(object obj) {
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
}