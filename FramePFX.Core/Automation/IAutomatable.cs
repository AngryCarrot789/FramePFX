namespace FramePFX.Core.Automation {
    /// <summary>
    /// An interface applied to an object which contains parameters which can be automated
    /// </summary>
    public interface IAutomatable {
        /// <summary>
        /// The automation data for this instance, which stores a collection of automation sequences for storing the key frames for each type of automate-able parameters
        /// </summary>
        AutomationData AutomationData { get; }

        /// <summary>
        /// Whether or not a parameter's value is being set, and therefore, no other parameters should be changed, no history should be pushed, etc
        /// <para>
        /// This is typically modified by the automation engine itself, before setting the actual value
        /// </para>
        /// </summary>
        bool IsAutomationChangeInProgress { get; set; }
    }
}