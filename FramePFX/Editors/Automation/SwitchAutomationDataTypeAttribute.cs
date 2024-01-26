using System;

namespace FramePFX.Editors.Automation {
    /// <summary>
    /// An attribute that I use to keep track of which methods use a switch case on
    /// an <see cref="FramePFX.Editors.Automation.Keyframes.AutomationDataType"/>
    /// so that I know what to change if I add any new data types
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class SwitchAutomationDataTypeAttribute : Attribute {

    }
}