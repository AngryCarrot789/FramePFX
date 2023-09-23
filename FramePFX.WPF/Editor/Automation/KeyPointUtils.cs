using System;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timeline.Utils;

namespace FramePFX.WPF.Editor.Automation {
    public static class KeyPointUtils {
        public static double GetY(KeyFrameViewModel keyFrame, double height) {
            AutomationKey key = keyFrame.OwnerSequence.Key;
            switch (keyFrame) {
                case KeyFrameFloatViewModel frame: {
                    KeyDescriptorFloat desc = (KeyDescriptorFloat) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameDoubleViewModel frame: {
                    KeyDescriptorDouble desc = (KeyDescriptorDouble) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameLongViewModel frame: {
                    KeyDescriptorLong desc = (KeyDescriptorLong) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameBooleanViewModel frame: {
                    double offset = (height / 100) * 10;
                    return frame.Value ? (height - offset) : offset;
                }
                case KeyFrameVector2ViewModel _: {
                    return height / 2d;
                }
                default: {
                    throw new Exception($"Unknown key frame: {keyFrame}");
                }
            }
        }

        public static double GetYHelper(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame, double height) {
            if (editor.IsValueRangeHuge) {
                return height / 2d;
            }
            else {
                return GetY(keyFrame, height);
            }
        }
    }
}