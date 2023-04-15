using System;
using SharpPadV2.Core.Shortcuts.Inputs;

namespace SharpPadV2.Core.Shortcuts.ViewModels {
    public abstract class InputStrokeViewModel : BaseViewModel {
        public static Func<KeyStrokeViewModel, string> KeyToReadableString { get; set; } = (x) => x.ToKeyStroke().ToString();
        public static Func<MouseStrokeViewModel, string> MouseToReadableString { get; set; } = (x) => x.ToMouseStroke().ToString();

        public static InputStrokeViewModel CreateFrom(IInputStroke stroke) {
            if (stroke is MouseStroke mouseStroke) {
                return new MouseStrokeViewModel(mouseStroke);
            }
            else if (stroke is KeyStroke keyStroke) {
                return new KeyStrokeViewModel(keyStroke);
            }
            else {
                throw new Exception("Unknown input stroke type: " + stroke?.GetType());
            }
        }

        public abstract IInputStroke ToInputStroke();

        public override string ToString() {
            return $"{this.GetType()} ({this.ToInputStroke()})";
        }

        public abstract string ToReadableString();
    }

    public class KeyStrokeViewModel : InputStrokeViewModel {
        private int keyCode;
        public int KeyCode {
            get => this.keyCode;
            set => this.RaisePropertyChanged(ref this.keyCode, value);
        }

        private int modifiers;
        public int Modifiers {
            get => this.modifiers;
            set => this.RaisePropertyChanged(ref this.modifiers, value);
        }

        private bool isKeyRelease;
        public bool IsKeyRelease {
            get => this.isKeyRelease;
            set => this.RaisePropertyChanged(ref this.isKeyRelease, value);
        }

        public KeyStrokeViewModel() {

        }

        public KeyStrokeViewModel(KeyStroke stroke) {
            this.keyCode = stroke.KeyCode;
            this.modifiers = stroke.Modifiers;
            this.isKeyRelease = stroke.IsKeyRelease;
        }

        public KeyStroke ToKeyStroke() {
            return new KeyStroke(this.keyCode, this.modifiers, this.isKeyRelease);
        }

        public override IInputStroke ToInputStroke() {
            return this.ToKeyStroke();
        }

        public override string ToReadableString() {
            return KeyToReadableString(this);
        }
    }

    public class MouseStrokeViewModel : InputStrokeViewModel {
        private int mouseButton;
        public int MouseButton {
            get => this.mouseButton;
            set => this.RaisePropertyChanged(ref this.mouseButton, value);
        }

        private int modifiers;
        public int Modifiers {
            get => this.modifiers;
            set => this.RaisePropertyChanged(ref this.modifiers, value);
        }

        private int clickCount;
        public int ClickCount {
            get => this.clickCount;
            set => this.RaisePropertyChanged(ref this.clickCount, value);
        }

        private int wheelDelta;
        public int WheelDelta {
            get => this.wheelDelta;
            set => this.RaisePropertyChanged(ref this.wheelDelta, value);
        }

        private int customParam;
        public int CustomParam {
            get => this.customParam;
            set => this.RaisePropertyChanged(ref this.customParam, value);
        }

        public MouseStrokeViewModel() {

        }

        public MouseStrokeViewModel(MouseStroke stroke) {
            this.mouseButton = stroke.MouseButton;
            this.modifiers = stroke.Modifiers;
            this.clickCount = stroke.ClickCount;
            this.wheelDelta = stroke.WheelDelta;
            this.customParam = stroke.CustomParam;
        }

        public MouseStroke ToMouseStroke() {
            return new MouseStroke(this.mouseButton, this.modifiers, this.clickCount, this.wheelDelta, this.customParam);
        }

        public override IInputStroke ToInputStroke() {
            return this.ToMouseStroke();
        }

        public override string ToReadableString() {
            return MouseToReadableString(this);
        }
    }
}