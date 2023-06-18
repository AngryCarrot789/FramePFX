using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;

namespace FramePFX.Core.Automation.ViewModels.Keyframe {
    public class AutomationSequenceViewModel : BaseViewModel {
        private readonly ObservableCollection<KeyFrameViewModel> keyFrames;
        public ReadOnlyObservableCollection<KeyFrameViewModel> KeyFrames { get; }

        public bool IsOverrideEnabled {
            get => this.Model.IsOverrideEnabled;
            set {
                this.Model.IsOverrideEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyFrameViewModel OverrideKeyFrame { get; }

        public AutomationSequence Model { get; }

        public AutomationSequenceViewModel(AutomationSequence model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.OverrideKeyFrame = KeyFrameViewModel.NewInstance(model.OverrideKeyFrame);
            this.keyFrames = new ObservableCollection<KeyFrameViewModel>();
            this.KeyFrames = new ReadOnlyObservableCollection<KeyFrameViewModel>(this.keyFrames);
            foreach (KeyFrame frame in model.KeyFrames) {
                this.keyFrames.Add(KeyFrameViewModel.NewInstance(frame));
            }
        }

        public void SetDoubleValue(long timestamp, double value) {
            AutomationSequence.ValidateType(AutomationDataType.Double, this.Model.DataType);
            value = ((KeyDescriptorDouble) this.Model.Key.Descriptor).Clamp(value);
            if (this.IsOverrideEnabled || this.keyFrames.Count < 1) {
                ((KeyFrameDoubleViewModel) this.OverrideKeyFrame).Value = value;
            }
            else if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameDoubleViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.AddKeyFrame(timestamp, new KeyFrameDoubleViewModel(new KeyFrameDouble {Value = value, Timestamp = timestamp}));
            }
        }

        public void SetLongValue(long timestamp, long value) {
            AutomationSequence.ValidateType(AutomationDataType.Long, this.Model.DataType);
            value = ((KeyDescriptorLong) this.Model.Key.Descriptor).Clamp(value);
            if (this.IsOverrideEnabled || this.keyFrames.Count < 1) {
                ((KeyFrameLongViewModel) this.OverrideKeyFrame).Value = value;
            }
            else if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameLongViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.AddKeyFrame(timestamp, new KeyFrameLongViewModel(new KeyFrameLong {Value = value, Timestamp = timestamp}));
            }
        }

        public void SetBooleanValue(long timestamp, bool value) {
            AutomationSequence.ValidateType(AutomationDataType.Boolean, this.Model.DataType);
            if (this.IsOverrideEnabled || this.keyFrames.Count < 1) {
                ((KeyFrameBooleanViewModel) this.OverrideKeyFrame).Value = value;
            }
            else if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameBooleanViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.AddKeyFrame(timestamp, new KeyFrameBooleanViewModel(new KeyFrameBoolean {Value = value, Timestamp = timestamp}));
            }
        }

        public void SetVector2Value(long timestamp, Vector2 value) {
            AutomationSequence.ValidateType(AutomationDataType.Vector2, this.Model.DataType);
            value = ((KeyDescriptorVector2) this.Model.Key.Descriptor).Clamp(value);
            if (this.IsOverrideEnabled || this.keyFrames.Count < 1) {
                ((KeyFrameVector2ViewModel) this.OverrideKeyFrame).Value = value;
            }
            else if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameVector2ViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.AddKeyFrame(timestamp, new KeyFrameVector2ViewModel(new KeyFrameVector2 {Value = value, Timestamp = timestamp}));
            }
        }

        public IEnumerable<KeyFrameViewModel> GetFrameExactlyAt(long frame) {
            foreach (KeyFrameViewModel keyFrame in this.keyFrames) {
                if (keyFrame.Timestamp == frame) {
                    yield return keyFrame;
                }
                else if (keyFrame.Timestamp > frame) {
                    yield break;
                }
            }
        }

        public KeyFrameViewModel GetLastFrameExactlyAt(long frame) {
            KeyFrameViewModel last = null;
            foreach (KeyFrameViewModel keyFrame in this.keyFrames) {
                if (keyFrame.Timestamp == frame) {
                    last = keyFrame;
                }
                else if (keyFrame.Timestamp > frame) {
                    return last;
                }
            }

            return null;
        }

        public bool TryGetLastFrameExactlyAt(long timestamp, out KeyFrameViewModel keyFrame) {
            KeyFrame kf = this.Model.GetLastFrameExactlyAt(timestamp);
            if (kf == null) {
                keyFrame = null;
                return false;
            }

            return (keyFrame = this.keyFrames.FirstOrDefault(x => ReferenceEquals(x.Model, kf))) != null;
        }

        public bool RemoveKeyFrame(KeyFrameViewModel keyFrame) {
            int index = this.keyFrames.IndexOf(keyFrame);
            if (index == -1) {
                return false;
            }

            keyFrame.OwnerSequence = null;
            this.keyFrames.RemoveAt(index);
            Debug.Assert(this.Model.RemoveKeyFrame(keyFrame.Model), "WOT!");
            return true;
        }

        public void AddKeyFrame(long timestamp, KeyFrameViewModel keyFrame) {
            keyFrame.Timestamp = timestamp;
            this.AddKeyFrame(keyFrame);
        }

        public void AddKeyFrame(KeyFrameViewModel keyFrame) {
            long timeStamp = keyFrame.Timestamp;
            if (timeStamp < 0) {
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(keyFrame));
            }

            if (keyFrame.Model.DataType != this.Model.DataType) {
                throw new ArgumentException($"Invalid key frame data type. Expected {this.Model.DataType}, got {keyFrame.Model.DataType}", nameof(keyFrame));
            }

            keyFrame.OwnerSequence = this;

            int i = 0;
            foreach (KeyFrameViewModel frame in this.keyFrames) {
                if (timeStamp < frame.Timestamp) {
                    this.keyFrames.Insert(i == 0 ? 0 : (i - 1), frame);
                }
                else if (timeStamp == frame.Timestamp) {
                    if (frame.Model.Equals(keyFrame.Model)) {
                        frame.OwnerSequence = null;
                        this.keyFrames.RemoveAt(i);
                        Debug.Assert(this.Model.RemoveKeyFrame(frame.Model), "WOT!");
                    }

                    this.keyFrames.Insert(i, frame);
                }
                else {
                    i++;
                    continue;
                }

                return;
            }

            this.keyFrames.Add(keyFrame);
            this.Model.AddKeyFrame(keyFrame.Model);
        }
    }
}