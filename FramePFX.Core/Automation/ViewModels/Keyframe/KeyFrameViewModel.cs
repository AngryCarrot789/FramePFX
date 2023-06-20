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

        protected KeyFrameViewModel(KeyFrame keyFrame) {
            this.Model = keyFrame ?? throw new ArgumentNullException(nameof(keyFrame));
        }

        public void SetDoubleValue(double value) => this.Model.SetDoubleValue(value);

        public void SetLongValue(long value) => this.Model.SetLongValue(value);

        public void SetBooleanValue(bool value) => this.Model.SetBooleanValue(value);

        public void SetVector2Value(Vector2 value) => this.Model.SetVector2Value(value);

        public static KeyFrameViewModel NewInstance(KeyFrame keyFrame) {
            switch (keyFrame) {
                case KeyFrameDouble frame:  return new KeyFrameDoubleViewModel(frame);
                case KeyFrameLong frame:    return new KeyFrameLongViewModel(frame);
                case KeyFrameBoolean frame: return new KeyFrameBooleanViewModel(frame);
                case KeyFrameVector2 frame: return new KeyFrameVector2ViewModel(frame);
                default:
                    throw new Exception("Unknown key frame type: " + keyFrame?.GetType());
            }
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