// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

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