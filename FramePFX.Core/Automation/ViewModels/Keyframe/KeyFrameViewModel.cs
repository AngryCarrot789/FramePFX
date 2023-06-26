using System;
using System.Numerics;
using FramePFX.Core.Automation.Keyframe;
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
            get => this.Model.Timestamp;
            set {
                this.Model.Timestamp = value;
                this.RaisePropertyChanged();
            }
        }

        public double CurveBendAmount {
            get => this.Model.CurveBendAmount;
            set {
                this.Model.CurveBendAmount = value;
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
        public static string GetPropertyName(KeyFrameViewModel keyFrame) {
            // return keyFrame.ValuePropertyName;
            return "Value";
        }

        public void SetFloatValue(float value) => ((KeyFrameFloatViewModel) this).Value = value;

        public void SetDoubleValue(double value) => ((KeyFrameDoubleViewModel) this).Value = value;

        public void SetLongValue(long value) => ((KeyFrameLongViewModel) this).Value = value;

        public void SetBooleanValue(bool value) => ((KeyFrameBooleanViewModel) this).Value = value;

        public void SetVector2Value(Vector2 value) => ((KeyFrameVector2ViewModel) this).Value = value;

        public static KeyFrameViewModel NewInstance(KeyFrame keyFrame) {
            switch (keyFrame) {
                case KeyFrameFloat frame:   return new KeyFrameFloatViewModel(frame);
                case KeyFrameDouble frame:  return new KeyFrameDoubleViewModel(frame);
                case KeyFrameLong frame:    return new KeyFrameLongViewModel(frame);
                case KeyFrameBoolean frame: return new KeyFrameBooleanViewModel(frame);
                case KeyFrameVector2 frame: return new KeyFrameVector2ViewModel(frame);
                default:
                    throw new Exception("Unknown key frame type: " + keyFrame?.GetType());
            }
        }
    }

    public class KeyFrameFloatViewModel : KeyFrameViewModel {
        public new KeyFrameFloat KeyFrame => (KeyFrameFloat) base.Model;

        public float Value {
            get => this.KeyFrame.Value;
            set {
                this.KeyFrame.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameFloatViewModel(KeyFrameFloat keyFrame) : base(keyFrame) {

        }
    }

    public class KeyFrameDoubleViewModel : KeyFrameViewModel {
        public new KeyFrameDouble KeyFrame => (KeyFrameDouble) base.Model;

        public double Value {
            get => this.KeyFrame.Value;
            set {
                this.KeyFrame.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameDoubleViewModel(KeyFrameDouble keyFrame) : base(keyFrame) {

        }
    }

    public class KeyFrameLongViewModel : KeyFrameViewModel {
        public new KeyFrameLong KeyFrame => (KeyFrameLong) base.Model;

        public long Value {
            get => this.KeyFrame.Value;
            set {
                this.KeyFrame.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public int RoundingMode {
            get => this.KeyFrame.RoundingMode;
            set {
                this.KeyFrame.RoundingMode = Maths.Clamp(value, 0, 3);
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameLongViewModel(KeyFrameLong keyFrame) : base(keyFrame) {

        }
    }

    public class KeyFrameBooleanViewModel : KeyFrameViewModel {
        public new KeyFrameBoolean KeyFrame => (KeyFrameBoolean) base.Model;

        public bool Value {
            get => this.KeyFrame.Value;
            set {
                this.KeyFrame.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameBooleanViewModel(KeyFrameBoolean keyFrame) : base(keyFrame) {

        }
    }

    public class KeyFrameVector2ViewModel : KeyFrameViewModel {
        public new KeyFrameVector2 KeyFrame => (KeyFrameVector2) base.Model;

        public Vector2 Value {
            get => this.KeyFrame.Value;
            set {
                this.KeyFrame.Value = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameVector2ViewModel(KeyFrameVector2 keyFrame) : base(keyFrame) {

        }
    }
}