using FramePFX.Editor;
using FramePFX.History;

namespace FramePFX.Automation.ViewModels
{
    /// <summary>
    /// An interface for automatable view model objects
    /// </summary>
    public interface IAutomatableViewModel : ITimelineViewModelBound, IHistoryHolder
    {
        /// <summary>
        /// Gets a reference to this automatable view model's backing model
        /// </summary>
        IAutomatable AutomationModel { get; }

        /// <summary>
        /// Gets the automation data view model for this object
        /// </summary>
        AutomationDataViewModel AutomationData { get; }

        /// <summary>
        /// Gets or sets if the automation engine is currently refreshing a value in this object
        /// </summary>
        bool IsAutomationRefreshInProgress { get; set; }
    }
}