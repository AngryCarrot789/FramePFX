using System.Numerics;
using FramePFX.Automation.Events;
using FramePFX.Editor.ZSystem;

namespace FramePFX.Automation
{
    public static class AutomationEventUtils
    {
        public static UpdateAutomationValueEventHandler ForZProperty(ZProperty<float> property)
        {
            return (sequence, frame) => ((ZObject) sequence.AutomationData.Owner).SetValueU(property, sequence.GetFloatValue(frame));
        }

        public static UpdateAutomationValueEventHandler ForZProperty(ZProperty<double> property)
        {
            return (sequence, frame) => ((ZObject) sequence.AutomationData.Owner).SetValueU(property, sequence.GetDoubleValue(frame));
        }

        public static UpdateAutomationValueEventHandler ForZProperty(ZProperty<long> property)
        {
            return (sequence, frame) => ((ZObject) sequence.AutomationData.Owner).SetValueU(property, sequence.GetLongValue(frame));
        }

        public static UpdateAutomationValueEventHandler ForZProperty(ZProperty<bool> property)
        {
            return (sequence, frame) => ((ZObject) sequence.AutomationData.Owner).SetValueU(property, sequence.GetBooleanValue(frame));
        }

        public static UpdateAutomationValueEventHandler ForZProperty(ZProperty<Vector2> property)
        {
            return (sequence, frame) => ((ZObject) sequence.AutomationData.Owner).SetValueU(property, sequence.GetVector2Value(frame));
        }
    }
}