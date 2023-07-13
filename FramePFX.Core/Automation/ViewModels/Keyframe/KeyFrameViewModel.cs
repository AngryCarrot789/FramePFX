using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation.ViewModels.Keyframe {
    public abstract class KeyFrameViewModel : BaseViewModel {
        public KeyFrame Model { get; }

        private AutomationSequenceViewModel ownerSequence;
        public AutomationSequenceViewModel OwnerSequence {
            get => this.ownerSequence;
            set => this.RaisePropertyChanged(ref this.ownerSequence, value);
        }

        public long Timestamp {
            get => this.Model.time;
            set {
                this.Model.time = value;
                this.RaisePropertyChanged();
            }
        }

        public double CurveBendAmount {
            get => this.Model.curveBend;
            set {
                this.Model.curveBend = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(GetPropertyName(this));
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
        public static string GetPropertyName(KeyFrameViewModel keyFrame) {
            // Use fixed name so that an abstract property is not required
            // return keyFrame.ValuePropertyName;
            return "Value";
        }

        public void SetFloatValue(float value) => ((KeyFrameFloatViewModel) this).Value = ((KeyDescriptorFloat) this.Model.sequence.Key.Descriptor).Clamp(value);
        public void SetDoubleValue(double value) => ((KeyFrameDoubleViewModel) this).Value = ((KeyDescriptorDouble) this.Model.sequence.Key.Descriptor).Clamp(value);
        public void SetLongValue(long value) => ((KeyFrameLongViewModel) this).Value = ((KeyDescriptorLong) this.Model.sequence.Key.Descriptor).Clamp(value);
        public void SetBooleanValue(bool value) => ((KeyFrameBooleanViewModel) this).Value = value;
        public void SetVector2Value(Vector2 value) => ((KeyFrameVector2ViewModel) this).Value = ((KeyDescriptorVector2) this.Model.sequence.Key.Descriptor).Clamp(value);

        public static KeyFrameViewModel NewInstance(KeyFrame keyFrame) {
            switch (keyFrame) {
                case KeyFrameFloat f:   return new KeyFrameFloatViewModel(f);
                case KeyFrameDouble f:  return new KeyFrameDoubleViewModel(f);
                case KeyFrameLong f:    return new KeyFrameLongViewModel(f);
                case KeyFrameBoolean f: return new KeyFrameBooleanViewModel(f);
                case KeyFrameVector2 f: return new KeyFrameVector2ViewModel(f);
                default:
                    throw new Exception("Unknown key frame type: " + keyFrame?.GetType());
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