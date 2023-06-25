using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    public interface IAutomatableViewModel {
        /// <summary>
        /// A reference to this automatable view model's backing model
        /// </summary>
        IAutomatable AutomationModel { get; }

        AutomationDataViewModel AutomationData { get; }

        AutomationEngineViewModel AutomationEngine { get; }
    }
}