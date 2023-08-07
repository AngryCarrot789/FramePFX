using System;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Automation
{
    public static class KeyPointUtils
    {
        public static double GetY(KeyFrameViewModel keyFrame, double height)
        {
            AutomationKey key = keyFrame.OwnerSequence.Key;
            switch (keyFrame)
            {
                case KeyFrameFloatViewModel frame:
                {
                    KeyDescriptorFloat desc = (KeyDescriptorFloat) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameDoubleViewModel frame:
                {
                    KeyDescriptorDouble desc = (KeyDescriptorDouble) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameLongViewModel frame:
                {
                    KeyDescriptorLong desc = (KeyDescriptorLong) key.Descriptor;
                    return Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                }
                case KeyFrameBooleanViewModel frame:
                {
                    double offset = (height / 100) * 10;
                    return frame.Value ? (height - offset) : offset;
                }
                case KeyFrameVector2ViewModel _:
                {
                    return height / 2d;
                }
                default:
                {
                    throw new Exception($"Unknown key frame: {keyFrame}");
                }
            }
        }
    }
}