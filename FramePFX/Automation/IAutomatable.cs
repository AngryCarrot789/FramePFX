namespace FramePFX.Automation
{
    /// <summary>
    /// An interface implemented by an object which can be automated
    /// </summary>
    public interface IAutomatable
    {
        /// <summary>
        /// The automation data for this object, which stores a collection of automation
        /// sequences for storing the key frames for each type of automate-able parameters
        /// </summary>
        AutomationData AutomationData { get; }

        /// <summary>
        /// Gets or sets if a parameter's value is being set, and therefore, no other parameters
        /// should be changed, no history should be pushed, etc.
        /// <para>
        /// This is typically modified by the automation engine itself, before setting the actual value
        /// </para>
        /// <para>
        /// This is only really used for debugging, but it may be useful at some point
        /// </para>
        /// </summary>
        bool IsAutomationChangeInProgress { get; set; }
    }
}