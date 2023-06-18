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
                this.RaisePropertyChanged(nameof(this.IsSequenceEditorVisible));
                this.DeselectSequenceCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Whether or not the sequence editor should be shown or not
        /// </summary>
        public bool IsSequenceEditorVisible => this.SelectedSequence != null;

        public AutomationSequenceViewModel this[AutomationKey key] => this.dataMap[key];

        public AutomationData Model { get; }

        public RelayCommand ToggleAllOverridesCommand { get; }

        public RelayCommand DeselectSequenceCommand { get; }

        public AutomationDataViewModel(AutomationData model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.dataMap = new Dictionary<AutomationKey, AutomationSequenceViewModel>();
            this.sequences = new ObservableCollection<AutomationSequenceViewModel>();
            this.Sequences = new ReadOnlyObservableCollection<AutomationSequenceViewModel>(this.sequences);
            this.ToggleAllOverridesCommand = new RelayCommand(this.ToggleAllOverridesAction);
            this.DeselectSequenceCommand = new RelayCommand(this.DeselectSequenceAction, () => this.IsSequenceEditorVisible);
            foreach (AutomationSequence sequence in model.Sequences) {
                AutomationSequenceViewModel vm = new AutomationSequenceViewModel(sequence);
                this.dataMap[sequence.Key] = vm;
                this.sequences.Add(vm);
            }
        }

        public void ToggleAllOverridesAction() {
            bool state = !this.sequences.Any(x => x.IsOverrideEnabled);
            foreach (AutomationSequenceViewModel sequence in this.sequences) {
                sequence.IsOverrideEnabled = state;
            }
        }

        public void DeselectSequenceAction() {
            this.SelectedSequence = null;
        }
    }
}