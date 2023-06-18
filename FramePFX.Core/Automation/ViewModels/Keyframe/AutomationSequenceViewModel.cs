using System.Collections.ObjectModel;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationSequenceViewModel : BaseViewModel {
        private readonly ObservableCollection<KeyFrameViewModel> sequences;
        public ReadOnlyObservableCollection<KeyFrameViewModel> Sequences { get; }

        public AutomationSequence Model { get; }

        public AutomationSequenceViewModel(AutomationSequence model) {
            this.Model = model;
            this.sequences = new ObservableCollection<KeyFrameViewModel>();
            this.Sequences = new ReadOnlyObservableCollection<KeyFrameViewModel>(this.sequences);
        }
    }
}