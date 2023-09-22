using System;
using FramePFX.Automation;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.History;

namespace FramePFX.Editor.ViewModels.Timelines.Effects {
    public class BaseEffectViewModel : BaseViewModel, IAutomatableViewModel, IProjectViewModelBound, IHistoryHolder, IStrictFrameRange {
        public BaseEffect Model { get; }

        public bool IsRemoveable => this.Model.IsRemoveable;

        public ClipViewModel OwnerClip { get; set; }

        public TimelineViewModel Timeline => this.OwnerClip.Timeline;

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public AutomationDataViewModel AutomationData { get; }

        public ProjectViewModel Project => this.OwnerClip.Project;

        public bool IsHistoryChanging { get; set; }

        public bool IsAutomationRefreshInProgress { get; set; }

        public BaseEffectViewModel(BaseEffect model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.ActiveSequenceChanged += this.OnActiveSequenceChanged;
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
        }

        // same behaviour as the BaseEffect versions

        /// <summary>
        /// Invoked when this effect is about to be added to <see cref="OwnerClip"/> (which is set prior to this call)
        /// </summary>
        protected virtual void OnAddingToClip() {
        }

        /// <summary>
        /// Invoked when this effect is added to the <see cref="OwnerClip"/>'s effect list
        /// </summary>
        protected virtual void OnAddedToClip() {
        }

        /// <summary>
        /// Invoked when this effect is about to be removed from the <see cref="OwnerClip"/>
        /// </summary>
        protected virtual void OnRemovingFromClip() {
        }

        /// <summary>
        /// Invoked when this effect has been removed from our previous owner (passed as a parameter)'s effect list
        /// </summary>
        /// <param name="clip">Our previous owner (<see cref="OwnerClip"/>, which is set to null prior to this call)</param>
        protected virtual void OnRemovedFromClip(ClipViewModel clip) {
        }

        // called when our automation data's active sequence changes
        private void OnActiveSequenceChanged(AutomationDataViewModel sender, ActiveSequenceChangedEventArgs e) {
            this.OwnerClip?.SetActiveAutomationSequence(e.Sequence, true);
        }

        public long ConvertRelativeToTimelineFrame(long relative) => this.Model.ConvertRelativeToTimelineFrame(relative);

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) => this.Model.ConvertTimelineToRelativeFrame(timeline, out inRange);

        public bool IsTimelineFrameInRange(long timeline) => this.Model.IsTimelineFrameInRange(timeline);

        public static void OnAddingToClip(BaseEffectViewModel effect) => effect.OnAddingToClip();
        public static void OnAddedToClip(BaseEffectViewModel effect) => effect.OnAddedToClip();
        public static void OnRemovingFromClip(BaseEffectViewModel effect) => effect.OnRemovingFromClip();
        public static void OnRemovedFromClip(BaseEffectViewModel effect, ClipViewModel clip) => effect.OnRemovedFromClip(clip);
    }
}