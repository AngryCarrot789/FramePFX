using System;
using System.Collections.Generic;

namespace FramePFX.Core.PropertyEditing.Editors.Primitives {
    public class CheckBoxEditorViewModel : BasePropertyEditorViewModel {
        private bool? isChecked;

        public bool? IsChecked {
            get => this.isChecked;
            set {
                // probably an overglorfied way of checking if the 2 are equal
                bool? old = this.isChecked;
                if (!old.HasValue && !value.HasValue || old.HasValue && value.HasValue && old.Value == value.Value) {
                    return;
                }

                this.RaisePropertyChanged(ref this.isChecked, value);
                foreach (PropertyHandler handler in this.HandlerData) {
                    this.setter(handler.Target, value ?? ((CBHandlerData) handler).OriginalValue);
                }
            }
        }

        private string label;

        public string Label {
            get => this.label;
            set => this.RaisePropertyChanged(ref this.label, value);
        }

        private string trueLabel;

        public string TrueLabel {
            get => this.trueLabel;
            set => this.RaisePropertyChanged(ref this.trueLabel, value);
        }

        private string falseLabel;

        public string FalseLabel {
            get => this.falseLabel;
            set => this.RaisePropertyChanged(ref this.falseLabel, value);
        }

        public RelayCommand ResetValueCommand { get; }

        private readonly Func<object, bool> getter;
        private readonly Action<object, bool> setter;

        public CheckBoxEditorViewModel(string label, Type applicableType, Func<object, bool> getter, Action<object, bool> setter) : base(applicableType) {
            this.label = label;
            this.getter = getter;
            this.setter = setter;
            this.ResetValueCommand = new RelayCommand(() => {
                if (this.IsMultiSelection) {
                    foreach (PropertyHandler data in base.HandlerData) {
                        bool value = ((CBHandlerData) data).OriginalValue;
                        this.setter(data.Target, value);
                    }
                }
                else if (!this.IsEmpty) {
                    bool value = ((CBHandlerData) this.GetHandlerData(0)).OriginalValue;
                    this.setter(this.Handlers[0], value);
                }
                else {
                    return;
                }

                this.isChecked = this.CalculateDefaultValue();
                this.RaisePropertyChanged(nameof(this.IsChecked));
            });
        }

        public bool? CalculateDefaultValue() {
            if (this.IsMultiSelection) {
                bool lastValue = this.getter(this.Handlers[0]);
                for (int i = 1; i < this.Handlers.Count; i++) {
                    if (this.getter(base.Handlers[i]) != lastValue) {
                        return null;
                    }
                }

                return lastValue;
            }
            else {
                return this.IsEmpty ? (bool?) null : this.getter(this.Handlers[0]);
            }
        }

        public static bool? GetDefaultBool(IReadOnlyList<object> objects, Func<object, bool> getter) {
            if (objects == null || objects.Count < 1) {
                return null;
            }
            else if (objects.Count > 1) {
                bool lastValue = getter(objects[0]);
                for (int i = 1; i < objects.Count; i++) {
                    if (getter(objects[i]) != lastValue) {
                        return null;
                    }
                }

                return lastValue;
            }
            else {
                return getter(objects[0]);
            }
        }

        public static CheckBoxEditorViewModel ForGeneric<T>(string label, Func<T, bool> getter, Action<T, bool> setter) {
            return new CheckBoxEditorViewModel(label, typeof(T), x => getter((T) x), (x, v) => setter((T) x, v));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.PreallocateHandlerData();
            if (!this.IsEmpty) {
                this.isChecked = this.CalculateDefaultValue();
                this.RaisePropertyChanged(nameof(this.IsChecked));
            }
        }

        protected override PropertyHandler NewHandler(object target) => new CBHandlerData(target, this.getter(target));

        private class CBHandlerData : PropertyHandler {
            public readonly bool OriginalValue;

            public CBHandlerData(object target, bool originalValue) : base(target) {
                this.OriginalValue = originalValue;
            }
        }
    }
}