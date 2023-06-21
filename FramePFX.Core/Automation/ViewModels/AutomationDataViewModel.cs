using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationDataViewModel : BaseViewModel {
        private readonly Dictionary<AutomationKey, AutomationSequenceViewModel> dataMap;

        private readonly ObservableCollection<AutomationSequenceViewModel> sequences;
        public ReadOnlyObservableCollection<AutomationSequenceViewModel> Sequences { get; }

        private AutomationSequenceViewModel selectedSequence;
        public AutomationSequenceViewModel SelectedSequence {
            get => this.selectedSequence;
            set {
                this.RaisePropertyChanged(ref this.selectedSequence, value);
                this.RaisePropertyChanged(nameof(this.SelectedSequenceKey));
                this.RaisePropertyChanged(nameof(this.IsSequenceEditorVisible));
                this.DeselectSequenceCommand.RaiseCanExecuteChanged();
                this.DisableOverrideCommand.RaiseCanExecuteChanged();
            }
        }

        public AutomationKey SelectedSequenceKey => this.selectedSequence?.Key;

        /// <summary>
        /// Whether or not the sequence editor should be shown or not
        /// </summary>
        public bool IsSequenceEditorVisible => this.SelectedSequence != null;

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

        public RelayCommand DisableOverrideCommand { get; }

        public RelayCommand DeselectSequenceCommand { get; }

        public IAutomatableViewModel Owner { get; }

        public AutomationDataViewModel(IAutomatableViewModel owner, AutomationData model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.dataMap = new Dictionary<AutomationKey, AutomationSequenceViewModel>();
            this.sequences = new ObservableCollection<AutomationSequenceViewModel>();
            this.Sequences = new ReadOnlyObservableCollection<AutomationSequenceViewModel>(this.sequences);
            this.DisableOverrideCommand = new RelayCommand(this.DisableOverrideAction, this.CanDisableOverrideAction);
            this.DeselectSequenceCommand = new RelayCommand(this.DeselectSequenceAction, () => this.IsSequenceEditorVisible);
            foreach (AutomationSequence sequence in model.Sequences) {
                AutomationSequenceViewModel vm = new AutomationSequenceViewModel(this, sequence);
                this.dataMap[sequence.Key] = vm;
                this.sequences.Add(vm);
            }
        }

        public void AssignRefreshHandler(AutomationKey key, RefreshAutomationValueEventHandler handler) {
            this.dataMap[key].RefreshValue += handler;
        }

        public void OnOverrideStateChanged(AutomationSequenceViewModel sequence) {
            if (!ReferenceEquals(this, sequence.AutomationData)) {
                throw new Exception("Invalid sequence; not owned by this instance");
            }

            AutomationEngineViewModel engine = this.Owner.AutomationEngine;
            if (engine != null) {
                engine.OnOverrideStateChanged(this, sequence);
            }

            this.DisableOverrideCommand.RaiseCanExecuteChanged();
            this.DeselectSequenceCommand.RaiseCanExecuteChanged(); // just in case
        }

        public bool CanDisableOverrideAction() {
            if (this.selectedSequence != null) {
                return this.selectedSequence.IsOverrideEnabled;
            }
            else {
                return this.sequences.Any(x => x.IsOverrideEnabled);
            }
        }

        public void DisableOverrideAction() {
            if (this.selectedSequence != null) {
                this.selectedSequence.IsOverrideEnabled = false;
            }
            else {
                foreach (AutomationSequenceViewModel sequence in this.sequences) {
                    if (sequence.IsOverrideEnabled) {
                        sequence.IsOverrideEnabled = false;
                    }
                }
            }

            this.DisableOverrideCommand.RaiseCanExecuteChanged();
        }

        public void DeselectSequenceAction() {
            this.SelectedSequence = null;
        }
    }
}