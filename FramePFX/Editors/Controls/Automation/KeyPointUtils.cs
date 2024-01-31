using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Automation {
    public static class KeyPointUtils {
        [SwitchAutomationDataType]
        public static double GetY(KeyFrame keyFrame, double height) {
            Parameter key = keyFrame.sequence.Parameter;
            switch (keyFrame) {
                case KeyFrameFloat frame: {
                    ParameterDescriptorFloat desc = (ParameterDescriptorFloat) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameDouble frame: {
                    ParameterDescriptorDouble desc = (ParameterDescriptorDouble) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameLong frame: {
                    ParameterDescriptorLong desc = (ParameterDescriptorLong) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameBoolean frame: {
                    double offset = (height / 100) * 10;
                    return frame.Value ? (height - offset) : offset;
                }
                case KeyFrameVector2 _: {
                    return height / 2d;
                }
                default: {
                    throw new Exception($"Unknown key frame: {keyFrame}");
                }
            }
        }

        public static double GetYHelper(AutomationSequenceEditor editor, KeyFrame keyFrame, double height) {
            if (editor.IsValueRangeHuge) {
                return height / 2d;
            }
            else {
                return GetY(keyFrame, height);
            }
        }
    }
}