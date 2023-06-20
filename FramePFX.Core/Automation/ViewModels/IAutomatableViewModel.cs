using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    public interface IAutomatableViewModel {
        IAutomatable AutomationModel { get; }

        AutomationDataViewModel AutomationData { get; }

        AutomationEngineViewModel AutomationEngine { get; }
    }
}