using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    /// <summary>
    /// A view model wrapper for <see cref="AutomationData"/>. This class contains a collection
    /// of <see cref="AutomationSequenceViewModel"/> instances. The collection is designed to be immutable
    /// </summary>
    public class AutomationDataViewModel : BaseViewModel {
        private readonly Dictionary<AutomationKey, AutomationSequenceViewModel> dataMap;
        private readonly ObservableCollection<AutomationSequenceViewModel> sequences;
        private AutomationSequenceViewModel activeSequence;

        /// <summary>
        /// A read only collection of sequences registered for this automation data
        /// </summary>
        public ReadOnlyObservableCollection<AutomationSequenceViewModel> Sequences { get; }

        /// <summary>
        /// The primary automation sequence currently being modified/viewed
        /// </summary>
        public AutomationSequenceViewModel ActiveSequence {
            get => this.activeSequence;
            set {
                if (this.activeSequence != null) {
                    this.activeSequence.IsActive = false;
                }

                this.Model.ActiveKeyFullId = value?.Key.FullId;
                this.RaisePropertyChanged(ref this.activeSequence, value);
                if (value != null) {
                    value.IsActive = true;
                }

                this.RaisePropertyChanged(nameof(this.ActiveSequenceKey));
                this.RaisePropertyChanged(nameof(this.IsSequenceEditorVisible));

                this.DeselectSequenceCommand.RaiseCanExecuteChanged();
                this.ToggleOverrideCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The active sequence's key, or null, if there is no active sequence
        /// </summary>
        public AutomationKey ActiveSequenceKey => this.ActiveSequence?.Key;

        /// <summary>
        /// Whether or not the sequence editor should be shown or not
        /// </summary>
        public bool IsSequenceEditorVisible => this.ActiveSequence != null;

        public AutomationSequenceViewModel this[AutomationKey key] {
            get {
                if (this.dataMap.TryGetValue(key ?? throw new ArgumentNullException(nameof(key), "Key cannot be null"), out AutomationSequenceViewModel sequence))
                    return sequence;

                #if DEBUG
                System.Diagnostics.Debugger.Break();
                #endif

                throw new Exception($"Key has not been assigned: {key}");
            }
        }

        public AutomationData Model { get; }

        public RelayCommand ToggleOverrideCommand { get; }

        public RelayCommand DeselectSequenceCommand { get; }

        public IAutomatableViewModel Owner { get; }

        public event EventHandler OverrideStateChanged;

        public AutomationDataViewModel(IAutomatableViewModel owner, AutomationData model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.dataMap = new Dictionary<AutomationKey, AutomationSequenceViewModel>();
            this.sequences = new ObservableCollection<AutomationSequenceViewModel>();
            this.Sequences = new ReadOnlyObservableCollection<AutomationSequenceViewModel>(this.sequences);
            this.ToggleOverrideCommand = new RelayCommand(this.ToggleOverrideAction, this.CanToggleOverride);
            this.DeselectSequenceCommand = new RelayCommand(this.DeselectSequenceAction, () => this.IsSequenceEditorVisible);
            foreach (AutomationSequence sequence in model.Sequences) {
                AutomationSequenceViewModel vm = new AutomationSequenceViewModel(this, sequence);
                this.dataMap[sequence.Key] = vm;
                this.sequences.Add(vm);
            }

            string activeId = model.ActiveKeyFullId;
            AutomationSequenceViewModel foundSequence;
            if (model.ActiveKeyFullId != null && (foundSequence = this.sequences.FirstOrDefault(x => x.Key.FullId == activeId)) != null) {
                this.ActiveSequence = foundSequence;
            }
        }

        public void AssignRefreshHandler(AutomationKey key, RefreshAutomationValueEventHandler handler) {
            this.dataMap[key].RefreshValue += handler;
        }

        public void OnOverrideStateChanged(AutomationSequenceViewModel sequence) {
            if (!ReferenceEquals(this, sequence.AutomationData)) {
                throw new Exception("Invalid sequence; not owned by this instance");
            }

            this.ToggleOverrideCommand.RaiseCanExecuteChanged();
            this.DeselectSequenceCommand.RaiseCanExecuteChanged(); // just in case
            this.OverrideStateChanged?.Invoke(this, EventArgs.Empty);
            AutomationEngineViewModel engine = this.Owner.AutomationEngine;
            if (engine != null) {
                engine.OnOverrideStateChanged(this, sequence);
            }
            #if DEBUG
            else {
                Debugger.Break();
                Debug.WriteLine("No automation engine available");
            }
            #endif
        }

        public void OnKeyFrameChanged(AutomationSequenceViewModel sequence, KeyFrameViewModel keyFrame) {
            if (!ReferenceEquals(this, sequence.AutomationData)) {
                throw new Exception("Invalid sequence; not owned by this instance");
            }

            AutomationEngineViewModel engine = this.Owner.AutomationEngine;
            if (engine != null) {
                engine.OnKeyFrameChanged(this, sequence, keyFrame);
            }
            #if DEBUG
            else {
                Debugger.Break();
                Debug.WriteLine("No automation engine available");
            }
            #endif
        }

        public bool CanToggleOverride() {
            return this.ActiveSequence != null || this.sequences.Any(x => x.IsOverrideEnabled);
        }

        public void ToggleOverrideAction() {
            if (this.ActiveSequence != null) {
                this.ActiveSequence.IsOverrideEnabled = !this.ActiveSequence.IsOverrideEnabled;
            }
            else {
                bool anyEnabled = this.sequences.Any(x => x.IsOverrideEnabled);
                foreach (AutomationSequenceViewModel sequence in this.sequences) {
                    sequence.IsOverrideEnabled = !anyEnabled;
                }
            }

            this.ToggleOverrideCommand.RaiseCanExecuteChanged();
        }

        public void DeselectSequenceAction() {
            this.ActiveSequence = null;
        }
    }
}