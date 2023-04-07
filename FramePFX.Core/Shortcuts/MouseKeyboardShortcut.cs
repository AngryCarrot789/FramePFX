using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Usage;

namespace FramePFX.Core.Shortcuts {
    public class MouseKeyboardShortcut : IMouseShortcut, IKeyboardShortcut {
        public static MouseShortcut EmptyMouseKeyboardShortcut = new MouseShortcut();

        private readonly List<IInputStroke> inputStrokes;

        public MouseStroke PrimaryMouseStroke => this.PrimaryStroke is MouseStroke stroke ? stroke : throw new Exception("Primary stroke is not a mouse stroke");

        public KeyStroke PrimaryKeyStroke => this.PrimaryStroke is KeyStroke stroke ? stroke : throw new Exception("Primary stroke is not a key stroke");

        public IEnumerable<MouseStroke> MouseStrokes => this.inputStrokes.OfType<MouseStroke>();

        public IEnumerable<KeyStroke> KeyStrokes => this.inputStrokes.OfType<KeyStroke>();

        public IInputStroke PrimaryStroke => this.inputStrokes[0];

        public IEnumerable<IInputStroke> InputStrokes {
            get => this.inputStrokes;
        }

        public bool IsKeyboard => true;

        public bool IsMouse => true;

        public bool IsEmpty => this.inputStrokes.Count < 1;

        public bool HasSecondaryStrokes => this.inputStrokes.Count > 1;

        public MouseKeyboardShortcut() {
            this.inputStrokes = new List<IInputStroke>();
        }

        public MouseKeyboardShortcut(params IInputStroke[] secondMouseStrokes) {
            this.inputStrokes = new List<IInputStroke>(secondMouseStrokes);
        }

        public MouseKeyboardShortcut(IEnumerable<IInputStroke> secondMouseStrokes) {
            this.inputStrokes = new List<IInputStroke>(secondMouseStrokes);
        }

        public MouseKeyboardShortcut(List<IInputStroke> inputStrokes) {
            this.inputStrokes = inputStrokes;
        }

        public IMouseShortcutUsage CreateMouseUsage() {
            return (IMouseShortcutUsage) this.CreateUsage();
        }


        public IKeyboardShortcutUsage CreateKeyUsage() {
            return (IKeyboardShortcutUsage) this.CreateUsage();
        }

        public IShortcutUsage CreateUsage() {
            return this.IsEmpty ? throw new InvalidOperationException("Shortcut is empty. Cannot create a usage") : new MouseKeyboardShortcutUsage(this);
        }

        public override string ToString() {
            return string.Join(", ", this.inputStrokes);
        }

        public override bool Equals(object obj) {
            if (obj is MouseKeyboardShortcut shortcut) {
                if (this.PrimaryStroke.Equals(shortcut.PrimaryStroke)) {
                    int lenA = this.inputStrokes.Count;
                    int lenB = shortcut.inputStrokes.Count;
                    if (lenA != lenB) {
                        return false;
                    }

                    for (int i = 0; i < lenA; i++) {
                        if (!this.inputStrokes[i].Equals(shortcut.inputStrokes[i])) {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode() {
            int code = 0;
            foreach (IInputStroke stroke in this.inputStrokes)
                code += stroke.GetHashCode();
            return code;
        }
    }
}