using FramePFX.Editors.Automation.Keyframes;

namespace FramePFX.Editors.Automation.Params {
    /// <summary>
    /// A delegate for when a parameter's value changes
    /// <param name="sequence">The sequence whose parameter value changed</param>
    /// </summary>
    public delegate void ParameterChangedEventHandler(AutomationSequence sequence);
}