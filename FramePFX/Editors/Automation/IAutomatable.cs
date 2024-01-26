using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Automation {
    /// <summary>
    /// An interface implemented by an object which supports parameter automation
    /// </summary>
    public interface IAutomatable : IHaveTimeline {
        /// <summary>
        /// The automation data for this object, which stores a collection of automation
        /// sequences for storing the key frames for each type of automate-able parameters
        /// </summary>
        AutomationData AutomationData { get; }

        /// <summary>
        /// Checks if the given parameter has any key frames for this object
        /// </summary>
        /// <param name="parameter">The parameter to check</param>
        /// <returns>True or false</returns>
        bool IsAutomated(Parameter parameter);
    }
}