namespace FramePFX.Automation.ViewModels {
    public interface IAutomatableViewModel {
        /// <summary>
        /// A reference to this automatable view model's backing model
        /// </summary>
        IAutomatable AutomationModel { get; }

        /// <summary>
        /// This automatable instance's data
        /// </summary>
        AutomationDataViewModel AutomationData { get; }

        /// <summary>
        /// The automation engine view model associated with this automatable instance
        /// </summary>
        AutomationEngineViewModel AutomationEngine { get; }
    }
}