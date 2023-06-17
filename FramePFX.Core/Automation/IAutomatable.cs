namespace FramePFX.Core.Automation {
    /// <summary>
    /// An interface applied to an object which contains parameters which can be automated
    /// </summary>
    public interface IAutomatable {
        AutomationData AutomationData { get; }
    }
}