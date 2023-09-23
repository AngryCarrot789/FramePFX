using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FramePFX.Automation.Events;
using FramePFX.Automation.History;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.History;
using FramePFX.History.ViewModels;

namespace FramePFX.Automation.ViewModels.Keyframe {
    public class AutomationSequenceViewModel : BaseViewModel, IHistoryHolder {
        private readonly ObservableCollection<KeyFrameViewModel> keyFrames;
        internal bool isActiveSequence;

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
                this.UpdateKeyFrameCollectionProperties();
            }
        }

        public KeyFrameViewModel OverrideKeyFrame { get; }

        public AutomationSequence Model { get; }

        public AutomationKey Key => this.Model.Key;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key frames present,
        /// meaning the automation engine can operate upon this sequence normally
        /// </summary>
        public bool IsAutomationReady => this.Model.IsAutomationReady;

        /// <summary>
        /// Gets or sets if this sequence is currently active. Setting this property will modify our owner's active sequence
        /// </summary>
        public bool IsActiveSequence {
            get => this.isActiveSequence;
            set {
                if (this.isActiveSequence == value) {
                    return;
                }

                if (value) {
                    this.AutomationData.ActiveSequence = this;
                }
                else if (ReferenceEquals(this.AutomationData.ActiveSequence, this)) {
                    this.AutomationData.ActiveSequence = null;
                }
                else {
                    // this case shouldn't really be reachable...
                    this.RaisePropertyChanged(ref this.isActiveSequence, false);
                }
            }
        }

        public bool HasKeyFrames => this.Model.HasKeyFrames;

        /// <summary>
        /// The automation data instance that owns this sequence
        /// </summary>
        public AutomationDataViewModel AutomationData { get; }

        public bool IsHistoryChanging { get; set; }

        // there will most likely only be 1 handler, being the owner to the automation data
        public event RefreshAutomationValueEventHandler RefreshValue;

        private readonly PropertyChangedEventHandler keyFramePropertyChangedHandler;

        public AutomationSequenceViewModel(AutomationDataViewModel automationData, AutomationSequence model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = automationData ?? throw new ArgumentNullException(nameof(automationData));
            this.OverrideKeyFrame = KeyFrameViewModel.NewInstance(model.OverrideKeyFrame);
            this.OverrideKeyFrame.OwnerSequence = this;
            this.keyFrames = new ObservableCollection<KeyFrameViewModel>();
            this.KeyFrames = new ReadOnlyObservableCollection<KeyFrameViewModel>(this.keyFrames);
            this.keyFramePropertyChangedHandler = this.OnKeyFrameOnPropertyChanged;
            foreach (KeyFrame frame in model.KeyFrames) {
                this.AddInternalUnsafe(this.keyFrames.Count, KeyFrameViewModel.NewInstance(frame));
            }
        }

        internal static void SetIsActiveInternal(AutomationSequenceViewModel sequence, bool isActive) {
            sequence.RaisePropertyChanged(ref sequence.isActiveSequence, isActive, nameof(sequence.IsActiveSequence));
        }

        public void UpdateKeyFrameCollectionProperties() {
            this.RaisePropertyChanged(nameof(this.HasKeyFrames));
            this.RaisePropertyChanged(nameof(this.IsAutomationReady));
        }

        public void DoRefreshValue(long tlframe, bool isDuringPlayback, bool isPlaybackTick) {
            this.DoRefreshValue(new RefreshAutomationValueEventArgs(tlframe, isDuringPlayback, isPlaybackTick));
        }

        public void DoRefreshValue(RefreshAutomationValueEventArgs e) => this.RefreshValue?.Invoke(this, e);

        private void AddInternalUnsafe(int index, KeyFrameViewModel keyFrame) {
            keyFrame.OwnerSequence = this;
            keyFrame.PropertyChanged += this.keyFramePropertyChangedHandler;
            this.keyFrames.Insert(index, keyFrame);
            this.UpdateKeyFrameCollectionProperties();
        }

        private void RemoveInternalUnsafe(int index) {
            KeyFrameViewModel keyFrame = this.keyFrames[index];
            keyFrame.OwnerSequence = null;
            keyFrame.PropertyChanged -= this.keyFramePropertyChangedHandler;
            this.keyFrames.RemoveAt(index);
            this.UpdateKeyFrameCollectionProperties();
        }

        public KeyFrameViewModel GetLastFrameExactlyAt(long frame) {
            int index = this.Model.GetLastFrameExactlyAt(frame);
            return index == -1 ? null : this.keyFrames[index];
        }

        public bool RemoveKeyFrame(KeyFrameViewModel keyFrame, bool applyHistory = true) {
            int index = this.keyFrames.IndexOf(keyFrame);
            if (index == -1)
                return false;
            this.RemoveKeyFrameAt(index, applyHistory);
            return true;
        }

        public void RemoveKeyFrameAt(int index, bool applyHistory = true) {
            KeyFrameViewModel removed = this.keyFrames[index];
            if (!ReferenceEquals(removed.Model, this.Model.GetKeyFrameAtIndex(index))) {
                throw new Exception("Model-ViewModel de-sync");
            }

            this.Model.RemoveKeyFrame(index);
            this.RemoveInternalUnsafe(index);

            if (applyHistory && !this.IsHistoryChanging) {
                HistoryManagerViewModel.Instance.AddAction(new HistoryKeyFrameRemove(this, new KeyFrameViewModel[] {removed}), "Add key frame");
            }
        }

        public void AddKeyFrame(long frame, KeyFrameViewModel keyFrame, bool applyHistory = true) {
            keyFrame.Frame = frame;
            this.AddKeyFrame(keyFrame, applyHistory);
        }

        public void AddKeyFrame(KeyFrameViewModel newKeyFrame, bool applyHistory = true) {
            long frame = newKeyFrame.Frame;
            if (frame < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + frame, nameof(newKeyFrame));
            if (newKeyFrame.Model.DataType != this.Model.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.Model.DataType}, got {newKeyFrame.Model.DataType}", nameof(newKeyFrame));

            this.AddInternalUnsafe(this.Model.AddKeyFrame(newKeyFrame.Model), newKeyFrame);
            if (applyHistory && !HistoryManagerViewModel.Instance.IsOperationActive) {
                HistoryKeyFrameAdd action = new HistoryKeyFrameAdd(this);
                action.unsafeKeyFrameList.Add(newKeyFrame);
                HistoryManagerViewModel.Instance.AddAction(action, "Add key frame");
            }
        }

        /// <summary>
        /// A helper function for enabling the key frame override mode (setting <see cref="IsOverrideEnabled"/> to true) if
        /// there are key frames present, and returning the override key frame for convenience
        /// </summary>
        /// <returns></returns>
        public KeyFrameViewModel GetOverride() {
            if (!this.IsOverrideEnabled && this.keyFrames.Count > 0) {
                this.IsOverrideEnabled = true;
            }

            return this.OverrideKeyFrame;
        }

        public KeyFrameViewModel GetActiveKeyFrameOrCreateNew(long frame) {
            KeyFrameViewModel keyFrame = this.GetLastFrameExactlyAt(frame);
            if (keyFrame != null) {
                return keyFrame;
            }

            this.AddKeyFrame(frame, keyFrame = KeyFrameViewModel.NewInstance(this.Key.CreateKeyFrame()));
            return keyFrame;
        }

        private void OnKeyFrameOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            KeyFrameViewModel keyFrame = (KeyFrameViewModel) sender;
            if (e.PropertyName == KeyFrameViewModel.GetPropertyName(keyFrame) || e.PropertyName == nameof(KeyFrameViewModel.Frame)) {
                this.RaiseKeyFrameChanged(keyFrame);
            }
        }

        /// <summary>
        /// Invokes the <see cref="AutomationDataViewModel.OnKeyFrameChanged"/> method
        /// </summary>
        /// <param name="keyFrame">The key frame whose value has been modified</param>
        public void RaiseKeyFrameChanged(KeyFrameViewModel keyFrame) {
            this.AutomationData.OnKeyFrameChanged(this, keyFrame);
        }

        /// <summary>
        /// Invokes <see cref="RaiseKeyFrameChanged"/>, passing the override key frame.
        /// <para>
        /// By default, the override key frame's value changed events are
        /// not listened to, so a notification must be manually fired
        /// </para>
        /// </summary>
        public void RaiseOverrideValueChanged() {
            this.AutomationData.OnKeyFrameChanged(this, this.OverrideKeyFrame);
        }

        public void ToggleOverrideAction() => this.IsOverrideEnabled = !this.IsOverrideEnabled;
    }
}