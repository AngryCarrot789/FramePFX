using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.Utils;

namespace FramePFX.Automation.ViewModels.Keyframe {
    public abstract class KeyFrameViewModel : BaseViewModel {
        private AutomationSequenceViewModel ownerSequence;

        /// <summary>
        /// The underlying key frame object
        /// </summary>
        public KeyFrame Model { get; }

        /// <summary>
        /// The sequence that this key frame is currently placed in
        /// </summary>
        public AutomationSequenceViewModel OwnerSequence {
            get => this.ownerSequence;
            set => this.RaisePropertyChanged(ref this.ownerSequence, value);
        }

        /// <summary>
        /// This key frame's current frame
        /// </summary>
        public long Frame {
            get => this.Model.frame;
            set {
                this.Model.frame = value;
                this.RaisePropertyChanged();
            }
        }

        public double CurveBendAmount {
            get => this.Model.curveBend;
            set {
                this.Model.curveBend = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(GetValuePropertyName(this));
            }
        }

        // protected virtual string ValuePropertyName => "Value";

        protected KeyFrameViewModel(KeyFrame keyFrame) {
            this.Model = keyFrame ?? throw new ArgumentNullException(nameof(keyFrame));
        }

        /// <summary>
        /// Returns the name of the property which stores the underlying value, used to listen to property changed events
        /// </summary>
        /// <param name="keyFrame"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetValuePropertyName(KeyFrameViewModel keyFrame) {
            // Use fixed name so that an abstract property is not required
            // return keyFrame.ValuePropertyName;
            return "Value";
        }

        /// <summary>
        /// Gets the float value and raises the <see cref="BaseViewModel.PropertyChanged"/> event for the value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetFloatValue(float value) => ((KeyFrameFloatViewModel) this).Value = ((KeyDescriptorFloat) this.Model.sequence.Key.Descriptor).Clamp(value);

        /// <summary>
        /// Gets the double value and raises the <see cref="BaseViewModel.PropertyChanged"/> event for the value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetDoubleValue(double value) => ((KeyFrameDoubleViewModel) this).Value = ((KeyDescriptorDouble) this.Model.sequence.Key.Descriptor).Clamp(value);

        /// <summary>
        /// Gets the long value and raises the <see cref="BaseViewModel.PropertyChanged"/> event for the value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetLongValue(long value) => ((KeyFrameLongViewModel) this).Value = ((KeyDescriptorLong) this.Model.sequence.Key.Descriptor).Clamp(value);

        /// <summary>
        /// Gets the bool value and raises the <see cref="BaseViewModel.PropertyChanged"/> event for the value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetBooleanValue(bool value) => ((KeyFrameBooleanViewModel) this).Value = value;

        /// <summary>
        /// Gets the Vector2 value and raises the <see cref="BaseViewModel.PropertyChanged"/> event for the value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetVector2Value(Vector2 value) => ((KeyFrameVector2ViewModel) this).Value = ((KeyDescriptorVector2) this.Model.sequence.Key.Descriptor).Clamp(value);

        public void SetValueFromObject(object value) {
            switch (this.Model.DataType) {
                case AutomationDataType.Float:   ((KeyFrameFloatViewModel) this).SetFloatValue((float) value); break;
                case AutomationDataType.Double:  ((KeyFrameDoubleViewModel) this).SetDoubleValue((double) value); break;
                case AutomationDataType.Long:    ((KeyFrameLongViewModel) this).SetLongValue((long) value); break;
                case AutomationDataType.Boolean: ((KeyFrameBooleanViewModel) this).SetBooleanValue((bool) value); break;
                case AutomationDataType.Vector2: ((KeyFrameVector2ViewModel) this).SetVector2Value((Vector2) value); break;
            }
        }

        public static KeyFrameViewModel NewInstance(KeyFrame keyFrame) {
            switch (keyFrame.DataType) {
                case AutomationDataType.Float:   return new KeyFrameFloatViewModel((KeyFrameFloat) keyFrame);
                case AutomationDataType.Double:  return new KeyFrameDoubleViewModel((KeyFrameDouble) keyFrame);
                case AutomationDataType.Long:    return new KeyFrameLongViewModel((KeyFrameLong) keyFrame);
                case AutomationDataType.Boolean: return new KeyFrameBooleanViewModel((KeyFrameBoolean) keyFrame);
                case AutomationDataType.Vector2: return new KeyFrameVector2ViewModel((KeyFrameVector2) keyFrame);
                default: throw new Exception("Unknown key frame type: " + keyFrame?.GetType());
            }
        }
    }

    public class KeyFrameFloatViewModel : KeyFrameViewModel {
        public new KeyFrameFloat Model => (KeyFrameFloat) base.Model;

        public float Value {
            get => this.Model.Value;
            set {
                this.Model.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameFloatViewModel(KeyFrameFloat keyFrame) : base(keyFrame) {
        }
    }

    public class KeyFrameDoubleViewModel : KeyFrameViewModel {
        public new KeyFrameDouble Model => (KeyFrameDouble) base.Model;

        public double Value {
            get => this.Model.Value;
            set {
                this.Model.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameDoubleViewModel(KeyFrameDouble keyFrame) : base(keyFrame) {
        }
    }

    public class KeyFrameLongViewModel : KeyFrameViewModel {
        public new KeyFrameLong Model => (KeyFrameLong) base.Model;

        public long Value {
            get => this.Model.Value;
            set {
                this.Model.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public int RoundingMode {
            get => this.Model.RoundingMode;
            set {
                this.Model.RoundingMode = Maths.Clamp(value, 0, 3);
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameLongViewModel(KeyFrameLong keyFrame) : base(keyFrame) {
        }
    }

    public class KeyFrameBooleanViewModel : KeyFrameViewModel {
        public new KeyFrameBoolean Model => (KeyFrameBoolean) base.Model;

        public bool Value {
            get => this.Model.Value;
            set {
                this.Model.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameBooleanViewModel(KeyFrameBoolean keyFrame) : base(keyFrame) {
        }
    }

    public class KeyFrameVector2ViewModel : KeyFrameViewModel {
        public new KeyFrameVector2 Model => (KeyFrameVector2) base.Model;

        public Vector2 Value {
            get => this.Model.Value;
            set {
                this.Model.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameVector2ViewModel(KeyFrameVector2 keyFrame) : base(keyFrame) {
        }
    }
}