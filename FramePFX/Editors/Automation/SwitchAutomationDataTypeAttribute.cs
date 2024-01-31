using System;
using FramePFX.Editors.Automation.Keyframes;

namespace FramePFX.Editors.Automation {
    /// <summary>
    /// An attribute that I use to keep track of which methods use a switch or a bunch of if statements on
    /// an <see cref="AutomationDataType"/>, a key frame type, parameter type, etc, so that I know what to
    /// change if I add any new data types
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class SwitchAutomationDataTypeAttribute : Attribute {

    }
}