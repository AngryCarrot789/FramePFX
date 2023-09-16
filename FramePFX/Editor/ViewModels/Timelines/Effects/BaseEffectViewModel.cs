using FramePFX.Automation;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.History;

namespace FramePFX.Editor.ViewModels.Timelines.Effects {
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
            this.AutomationData.ActiveSequenceChanged += this.OnActiveSequenceChanged;
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
        }

        // same behaviour as the BaseEffect versions

        /// <summary>
        /// Called when this effect is added to <see cref="OwnerClip"/> (which was set to a non-null value before this call)
        /// </summary>
        public virtual void OnAddedToClip() {
        }

        /// <summary>
        /// Called when this effect is removed from <see cref="OwnerClip"/> (which is set to null after this call)
        /// </summary>
        public virtual void OnRemovedFromClip() {
        }

        // called when our automation data's active sequence changes
        private void OnActiveSequenceChanged(AutomationDataViewModel sender, ActiveSequenceChangedEventArgs e) {
            this.OwnerClip?.SetActiveAutomationSequence(e.Sequence, true);
        }
    }
}