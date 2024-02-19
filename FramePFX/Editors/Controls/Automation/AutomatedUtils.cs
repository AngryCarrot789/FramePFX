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

using System.Windows;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Automation {
    public static class AutomatedUtils {
        public static void SetDefaultKeyFrameOrAddNew(IAutomatable automatable, DependencyObject control, Parameter parameter, DependencyProperty property) {
            SetDefaultKeyFrameOrAddNew(automatable, parameter, control.GetValue(property));
        }

        public static void SetDefaultKeyFrameOrAddNew(AutomationSequence sequence, object value) {
            if (sequence.IsEmpty || sequence.IsOverrideEnabled) {
                sequence.DefaultKeyFrame.SetValueFromObject(value);
            }
            else {
                if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead)) {
                    // Either get the last key frame at the playhead or create a new one at that location
                    KeyFrame keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _);
                    keyFrame.SetValueFromObject(value);
                }
                else {
                    // when the object is has a strict frame range, e.g. clip, effect, and it is not in range,
                    // enable override and set the default key frame
                    sequence.DefaultKeyFrame.SetValueFromObject(value);
                    sequence.IsOverrideEnabled = true;
                }
            }
        }

        public static void GetDefaultKeyFrameOrAddNew(AutomationSequence sequence, out KeyFrame keyFrame, bool enableOverrideIfOutOfRange = true) {
            if (sequence.IsEmpty || sequence.IsOverrideEnabled) {
                keyFrame = sequence.DefaultKeyFrame;
            }
            else if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead)) {
                // Either get the last key frame at the playhead or create a new one at that location
                keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _);
            }
            else {
                // when the object is has a strict frame range, e.g. clip, effect, and it is not in range,
                // enable override and set the default key frame
                keyFrame = sequence.DefaultKeyFrame;
                if (enableOverrideIfOutOfRange)
                    sequence.IsOverrideEnabled = true;
            }
        }

        public static void SetDefaultKeyFrameOrAddNew(IAutomatable automatable, Parameter parameter, object value) {
            AutomationSequence sequence = automatable.AutomationData[parameter];
            if (sequence.IsEmpty || sequence.IsOverrideEnabled) {
                sequence.DefaultKeyFrame.SetValueFromObject(value);
            }
            else if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead)) {
                // Either get the last key frame at the playhead or create a new one at that location
                KeyFrame keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _);
                keyFrame.SetValueFromObject(value);
            }
            else {
                // when the object is has a strict frame range, e.g. clip, effect, and it is not in range,
                // enable override and set the default key frame
                sequence.DefaultKeyFrame.SetValueFromObject(value);
                sequence.IsOverrideEnabled = true;
            }
        }

        public static bool TryAddKeyFrameAtLocation(AutomationSequence sequence, out KeyFrame keyFrame) {
            if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead)) {
                keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _, true);
                return true;
            }
            else {
                keyFrame = null;
                return false;
            }
        }
    }
}