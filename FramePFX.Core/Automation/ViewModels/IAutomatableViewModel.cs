using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Core.Automation.ViewModels {
    public interface IAutomatableViewModelUpdateHandler {
        void OnOverrideStateChanged(AutomationSequenceViewModel sequence);
    }
}