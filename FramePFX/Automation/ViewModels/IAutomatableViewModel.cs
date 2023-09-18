using FramePFX.Editor;

namespace FramePFX.Automation.ViewModels {
    public interface IAutomatableViewModel : ITimelineViewModelBound {
        /// <summary>
        /// A reference to this automatable view model's backing model
        /// </summary>
        IAutomatable AutomationModel { get; }

        /// <summary>
        /// This automatable instance's data
        /// </summary>
        AutomationDataViewModel AutomationData { get; }

        bool IsAutomationRefreshInProgress { get; set; }
    }
}