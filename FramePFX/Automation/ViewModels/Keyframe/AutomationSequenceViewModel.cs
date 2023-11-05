using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FFmpeg.AutoGen;
using FramePFX.Automation.Events;
using FramePFX.Automation.History;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.History;
using FramePFX.History.ViewModels;
using FramePFX.Utils;

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

        public KeyFrameViewModel DefaultKeyFrame { get; }

        public AutomationSequence Model { get; }

        public AutomationKey Key => this.Model.Key;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key frames present,
        /// meaning the automation engine can operate upon this sequence normally
        /// </summary>
        public bool IsAutomationAllowed => this.Model.IsAutomationAllowed;

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

        private KeyFrameViewModel KFVMBeingAdded; // work around for Models modifying to ViewModels

        public AutomationSequenceViewModel(AutomationDataViewModel automationData, AutomationSequence model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = automationData ?? throw new ArgumentNullException(nameof(automationData));
            this.keyFramePropertyChangedHandler = this.OnKeyFrameOnPropertyChanged;
            this.DefaultKeyFrame = KeyFrameViewModel.NewInstance(model.DefaultKeyFrame);
            this.DefaultKeyFrame.OwnerSequence = this;
            this.DefaultKeyFrame.PropertyChanged += this.keyFramePropertyChangedHandler;
            this.keyFrames = new ObservableCollection<KeyFrameViewModel>();
            this.KeyFrames = new ReadOnlyObservableCollection<KeyFrameViewModel>(this.keyFrames);
            foreach (KeyFrame frame in model.KeyFrames) {
                this.AddInternalUnsafe(this.keyFrames.Count, KeyFrameViewModel.NewInstance(frame));
            }

            model.KeyFrameAdded += this.OnKeyFrameAdded;
            model.KeyFrameRemoved += this.OnKeyFrameRemoved;
        }

        private void OnKeyFrameAdded(AutomationSequence sequence, KeyFrame keyframe, int index) {
            KeyFrameViewModel vm = Helper.Exchange(ref this.KFVMBeingAdded, null) ?? KeyFrameViewModel.NewInstance(keyframe);
            this.AddInternalUnsafe(index, vm);
        }

        private void OnKeyFrameRemoved(AutomationSequence sequence, KeyFrame keyframe, int index) {
            if (this.keyFrames[index].Model != keyframe)
                throw new Exception("Model-ViewModel unsynced");
            this.RemoveInternalUnsafe(index);
        }

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

        internal static void SetIsActiveInternal(AutomationSequenceViewModel sequence, bool isActive) {
            sequence.RaisePropertyChanged(ref sequence.isActiveSequence, isActive, nameof(sequence.IsActiveSequence));
        }

        public void UpdateKeyFrameCollectionProperties() {
            this.RaisePropertyChanged(nameof(this.HasKeyFrames));
            this.RaisePropertyChanged(nameof(this.IsAutomationAllowed));
        }

        public void DoRefreshValue(long tlframe, bool isDuringPlayback, bool isPlaybackTick) {
            this.DoRefreshValue(new AutomationUpdateEventArgs(tlframe, isDuringPlayback, isPlaybackTick));
        }

        public void DoRefreshValue(AutomationUpdateEventArgs e) => this.RefreshValue?.Invoke(this, e);

        /// <summary>
        /// Assigns the default key frame (for our key) to its default value
        /// </summary>
        public void AssignDefaultValue() {
            KeyFrameViewModel keyFrame = this.AutomationData[this.Key].DefaultKeyFrame;
            keyFrame.Model.AssignDefaultValue(this.Key.Descriptor);
            this.RaiseKeyFrameChanged(keyFrame);
        }

        public KeyFrameViewModel GetLastFrameExactlyAt(long frame) {
            int index = this.Model.GetLastFrameExactlyAt(frame);
            return index == -1 ? null : this.keyFrames[index];
        }

        public bool RemoveKeyFrame(KeyFrameViewModel keyFrame) {
            return this.Model.RemoveKeyFrame(keyFrame.Model, out _);
        }

        public void AddKeyFrame(KeyFrameViewModel newKeyFrame) {
            if (newKeyFrame.OwnerSequence != null && newKeyFrame.OwnerSequence.Model.GetIndexOf(newKeyFrame.Model) != -1)
                throw new InvalidOperationException("Key frame was already added to another sequence");

            this.KFVMBeingAdded = newKeyFrame;
            this.Model.AddKeyFrame(newKeyFrame.Model);
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

            return this.DefaultKeyFrame;
        }

        public KeyFrameViewModel GetActiveKeyFrameOrCreateNew(long frame) {
            KeyFrameViewModel keyFrame = this.GetLastFrameExactlyAt(frame);
            if (keyFrame == null)
                keyFrame = this.keyFrames[this.Model.AddNewKeyFrame(frame, out _)];
            return keyFrame;
        }

        private void OnKeyFrameOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            KeyFrameViewModel keyFrame = (KeyFrameViewModel) sender;
            if (e.PropertyName == KeyFrameViewModel.GetValuePropertyName(keyFrame) || e.PropertyName == nameof(KeyFrameViewModel.Frame)) {
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

        public void ToggleOverrideAction() => this.IsOverrideEnabled = !this.IsOverrideEnabled;
    }
}