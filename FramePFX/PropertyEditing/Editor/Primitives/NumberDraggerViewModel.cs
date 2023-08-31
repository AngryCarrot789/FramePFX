using System;
using System.Windows.Input;

namespace FramePFX.PropertyEditing.Editor.Primitives {
    public class NumberDraggerViewModel : BasePropertyEditorViewModel {
        private double value;

        public double Value {
            get => this.value;
            set {
                double oldValue = this.value;
                this.RaisePropertyChanged(ref this.value, value);
                this.OnValueChanged(oldValue, value);
            }
        }

        private double minValue;

        public double MinValue {
            get => this.minValue;
            set => this.RaisePropertyChanged(ref this.minValue, value);
        }

        private double maxValue;

        public double MaxValue {
            get => this.maxValue;
            set => this.RaisePropertyChanged(ref this.maxValue, value);
        }

        private bool isEditingValue; // the user has their mouse down

        public ICommand BeginValueModificationCommand { get; }
        public ICommand EndValueModificationCommand { get; }

        private readonly Func<object, double> getter;
        private readonly Action<object, double> setter;

        public NumberDraggerViewModel(Type type, Func<object, double> getter, Action<object, double> setter) : base(type) {
            this.BeginValueModificationCommand = new RelayCommand(() => this.isEditingValue = true, () => !this.isEditingValue);
            this.EndValueModificationCommand = new RelayCommand(() => this.isEditingValue = false, () => this.isEditingValue);
            this.getter = getter;
            this.setter = setter;
        }

        private void OnValueChanged(double oldValue, double newValue) {
            if (this.IsEmpty) {
                return;
            }

            if (this.Handlers.Count == 1) {
                this.setter(this.Handlers[0], newValue);
            }
            else if (this.isEditingValue) {
                double change = newValue - oldValue;
                foreach (object handler in this.Handlers) {
                    double val = this.getter(handler);
                    this.setter(handler, val + change);
                }
            }
            else {
                foreach (object handler in this.Handlers) {
                    this.setter(handler, newValue);
                }
            }
        }

        protected override PropertyHandler NewHandler(object target) => new NumberDragData(target);

        private class NumberDragData : PropertyHandler {
            // use accumulator in the event that there's a lower/upper bound to the value
            // this can be used to store the "excess" value. It can be added to the final
            // value and then clamped between the min/max to determine the absolute value
            public double accumulator;

            public NumberDragData(object target) : base(target) {
            }
        }
    }
}