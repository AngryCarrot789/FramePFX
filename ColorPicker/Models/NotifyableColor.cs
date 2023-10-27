using System;

namespace ColorPicker.Models {
    public class NotifyableColor : NotifyableObject {
        private readonly IColorStateStorage storage;

        public NotifyableColor(IColorStateStorage colorStateStorage) {
            this.storage = colorStateStorage;
        }

        private static bool NotEqual(double a, double b) {
            return Math.Abs(a - b) > 0.000001d;
        }

        public void UpdateEverything(ColorState oldValue) {
            ColorState currentValue = this.storage.ColorState;
            if (NotEqual(currentValue.A, oldValue.A))
                this.RaisePropertyChanged(nameof(this.A));

            if (NotEqual(currentValue.RGB_R, oldValue.RGB_R))
                this.RaisePropertyChanged(nameof(this.RGB_R));
            if (NotEqual(currentValue.RGB_G, oldValue.RGB_G))
                this.RaisePropertyChanged(nameof(this.RGB_G));
            if (NotEqual(currentValue.RGB_B, oldValue.RGB_B))
                this.RaisePropertyChanged(nameof(this.RGB_B));

            if (NotEqual(currentValue.HSV_H, oldValue.HSV_H))
                this.RaisePropertyChanged(nameof(this.HSV_H));
            if (NotEqual(currentValue.HSV_S, oldValue.HSV_S))
                this.RaisePropertyChanged(nameof(this.HSV_S));
            if (NotEqual(currentValue.HSV_V, oldValue.HSV_V))
                this.RaisePropertyChanged(nameof(this.HSV_V));

            if (NotEqual(currentValue.HSL_H, oldValue.HSL_H))
                this.RaisePropertyChanged(nameof(this.HSL_H));
            if (NotEqual(currentValue.HSL_S, oldValue.HSL_S))
                this.RaisePropertyChanged(nameof(this.HSL_S));
            if (NotEqual(currentValue.HSL_L, oldValue.HSL_L))
                this.RaisePropertyChanged(nameof(this.HSL_L));
        }

        public double A {
            get => this.storage.ColorState.A * 255;
            set {
                ColorState state = this.storage.ColorState;
                state.A = value / 255;
                this.storage.ColorState = state;
            }
        }

        public double RGB_R {
            get => this.storage.ColorState.RGB_R * 255;
            set {
                ColorState state = this.storage.ColorState;
                state.RGB_R = value / 255;
                this.storage.ColorState = state;
            }
        }

        public double RGB_G {
            get => this.storage.ColorState.RGB_G * 255;
            set {
                ColorState state = this.storage.ColorState;
                state.RGB_G = value / 255;
                this.storage.ColorState = state;
            }
        }

        public double RGB_B {
            get => this.storage.ColorState.RGB_B * 255;
            set {
                ColorState state = this.storage.ColorState;
                state.RGB_B = value / 255;
                this.storage.ColorState = state;
            }
        }

        public double HSV_H {
            get => this.storage.ColorState.HSV_H;
            set {
                ColorState state = this.storage.ColorState;
                state.HSV_H = value;
                this.storage.ColorState = state;
            }
        }

        public double HSV_S {
            get => this.storage.ColorState.HSV_S * 100;
            set {
                ColorState state = this.storage.ColorState;
                state.HSV_S = value / 100;
                this.storage.ColorState = state;
            }
        }

        public double HSV_V {
            get => this.storage.ColorState.HSV_V * 100;
            set {
                ColorState state = this.storage.ColorState;
                state.HSV_V = value / 100;
                this.storage.ColorState = state;
            }
        }

        public double HSL_H {
            get => this.storage.ColorState.HSL_H;
            set {
                ColorState state = this.storage.ColorState;
                state.HSL_H = value;
                this.storage.ColorState = state;
            }
        }

        public double HSL_S {
            get => this.storage.ColorState.HSL_S * 100;
            set {
                ColorState state = this.storage.ColorState;
                state.HSL_S = value / 100;
                this.storage.ColorState = state;
            }
        }

        public double HSL_L {
            get => this.storage.ColorState.HSL_L * 100;
            set {
                ColorState state = this.storage.ColorState;
                state.HSL_L = value / 100;
                this.storage.ColorState = state;
            }
        }
    }
}