using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;
using FramePFX.History.ViewModels;

namespace FramePFX.Editor.Timelines.Effects.ViewModels {
    public class BaseEffectViewModel : BaseViewModel, IAutomatableViewModel, IProjectViewModelBound, IHistoryHolder {
        public BaseEffect Model { get; }

        public bool IsRemoveable => this.Model.IsRemoveable;

        public ClipViewModel OwnerClip { get; set; }

        public TimelineViewModel Timeline => this.OwnerClip.Timeline;

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public AutomationDataViewModel AutomationData { get; }

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        public ProjectViewModel Project => this.OwnerClip.Project;

        public bool IsHistoryChanging { get; set; }

        public bool IsAutomationRefreshInProgress { get; set; }

        public BaseEffectViewModel(BaseEffect model) {
            this.Model = model;
            this.AutomationData = new AutomationDataViewModel(this, this.Model.AutomationData);
        }
    }
}