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
                if (this.IsOverrideEnabled == value) {
                    return;
                }

                this.Model.IsOverrideEnabled = value;
                this.RaisePropertyChanged();
                this.AutomationData.OnOverrideStateChanged(this);
            }
        }

        public KeyFrameViewModel OverrideKeyFrame { get; }

        public AutomationSequence Model { get; }

        public AutomationKey Key => this.Model.Key;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key frames present, meaning the automation engine is operating in normal operation
        /// </summary>
        public bool IsAutomationInUse => this.Model.IsAutomationInUse;

        /// <summary>
        /// The automation data instance that owns this sequence
        /// </summary>
        public AutomationDataViewModel AutomationData { get; }

        public event RefreshAutomationValueEventHandler RefreshValue;

        public AutomationSequenceViewModel(AutomationDataViewModel automationData, AutomationSequence model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = automationData ?? throw new ArgumentNullException(nameof(automationData));
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
            if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameDoubleViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.IsOverrideEnabled = true;
                ((KeyFrameDoubleViewModel) this.OverrideKeyFrame).Value = value;
            }
        }

        public void SetLongValue(long timestamp, long value) {
            AutomationSequence.ValidateType(AutomationDataType.Long, this.Model.DataType);
            value = ((KeyDescriptorLong) this.Model.Key.Descriptor).Clamp(value);
            if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameLongViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.IsOverrideEnabled = true;
                ((KeyFrameLongViewModel) this.OverrideKeyFrame).Value = value;
            }
        }

        public void SetBooleanValue(long timestamp, bool value) {
            AutomationSequence.ValidateType(AutomationDataType.Boolean, this.Model.DataType);
            if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameBooleanViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.IsOverrideEnabled = true;
                ((KeyFrameBooleanViewModel) this.OverrideKeyFrame).Value = value;
            }
        }

        public void SetVector2Value(long timestamp, Vector2 value) {
            AutomationSequence.ValidateType(AutomationDataType.Vector2, this.Model.DataType);
            value = ((KeyDescriptorVector2) this.Model.Key.Descriptor).Clamp(value);
            if (this.GetLastFrameExactlyAt(timestamp) is KeyFrameVector2ViewModel keyFrame) {
                keyFrame.Value = value;
            }
            else {
                this.IsOverrideEnabled = true;
                ((KeyFrameVector2ViewModel) this.OverrideKeyFrame).Value = value;
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

        public void AddKeyFrame(KeyFrameViewModel newKeyFrame) {
            long timeStamp = newKeyFrame.Timestamp;
            if (timeStamp < 0) {
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(newKeyFrame));
            }

            if (newKeyFrame.Model.DataType != this.Model.DataType) {
                throw new ArgumentException($"Invalid key frame data type. Expected {this.Model.DataType}, got {newKeyFrame.Model.DataType}", nameof(newKeyFrame));
            }

            newKeyFrame.OwnerSequence = this;

            int i = 0;
            foreach (KeyFrameViewModel existingFrame in this.keyFrames) {
                if (timeStamp < existingFrame.Timestamp) {
                    this.keyFrames.Insert(i == 0 ? 0 : (i - 1), newKeyFrame);
                }
                else if (timeStamp == existingFrame.Timestamp) {
                    if (existingFrame.Model.Equals(newKeyFrame.Model)) {
                        existingFrame.OwnerSequence = null;
                        this.keyFrames.RemoveAt(i);
                        this.Model.RemoveKeyFrame(existingFrame.Model);
                    }

                    this.keyFrames.Insert(i, newKeyFrame);
                }
                else {
                    i++;
                    continue;
                }

                return;
            }

            this.keyFrames.Add(newKeyFrame);
            this.Model.AddKeyFrame(newKeyFrame.Model);
        }

        /// <summary>
        /// A helper function for enabling the key frame override mode (setting <see cref="IsOverrideEnabled"/> to true) and returning the override key frame, for convenience
        /// </summary>
        /// <returns></returns>
        public KeyFrameViewModel GetOverride() {
            if (!this.IsOverrideEnabled)
                this.IsOverrideEnabled = true;
            return this.OverrideKeyFrame;
        }

        public void DoRefreshValue(AutomationEngineViewModel engine, long frame, bool isPlaybackSource) {
            this.RefreshValue?.Invoke(this, new RefreshAutomationValueEventArgs(frame, isPlaybackSource));
        }
    }
}