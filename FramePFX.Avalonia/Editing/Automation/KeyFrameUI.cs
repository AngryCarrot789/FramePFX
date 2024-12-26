// 
// Copyright (c) 2024-2024 REghZy
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
using System.Collections.Generic;
using Avalonia;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.Automation;

/// <summary>
/// A class which contains UI information about a key frame
/// </summary>
public class KeyFrameUI {
    public readonly AutomationSequenceEditorControl editor;
    public readonly AutomationSequence sequence;
    public readonly KeyFrame keyFrame;
    public readonly bool IsDefaultKeyFrame;
    private Point? myPosition;

    public int Index = -1;

    public KeyFrameElementPart? MouseOverPart;

    public KeyFrameUI? Next {
        get {
            if (this.Index == -1)
                return null;

            int index = this.Index + 1;
            List<KeyFrameUI> list = this.editor.keyFrameUIs;
            return index > 0 && index < list.Count ? list[index] : null;
        }
    }

    public KeyFrameUI? Prev {
        get {
            if (this.Index == -1)
                return null;

            int index = this.Index - 1;
            List<KeyFrameUI> list = this.editor.keyFrameUIs;
            return index < list.Count && index >= 0 ? list[index] : null;
        }
    }

    public KeyFrameUI(AutomationSequenceEditorControl editor, KeyFrame keyFrame) {
        Validate.NotNull(keyFrame);

        this.editor = editor;
        this.keyFrame = keyFrame;
        this.sequence = keyFrame.Sequence ?? throw new InvalidOperationException("Key frame does not exist in a sequence");
        this.IsDefaultKeyFrame = ReferenceEquals(keyFrame, this.sequence.DefaultKeyFrame);
    }

    public Point GetPosition() {
        if (this.myPosition.HasValue)
            return this.myPosition.Value;

        Point offset = this.editor.ScreenOffset;
        double height = this.editor.Bounds.Height + offset.Y;
        double px = TimelineUtils.FrameToPixel(this.keyFrame.Frame, this.editor.HorizontalZoom) + offset.X;
        double py = GetYHelper(this.editor.IsValueRangeHuge, this.keyFrame, height);
        return new Point(px, DoubleUtils.IsValid(py) ? py : 0.0);
    }

    public void OnAdded() {
        this.keyFrame.ValueChanged += this.OnKeyFrameValueChange;
        this.keyFrame.FrameChanged += this.OnKeyFrameLocationChange;
    }

    public void OnRemoved() {
        this.keyFrame.ValueChanged -= this.OnKeyFrameValueChange;
        this.keyFrame.FrameChanged -= this.OnKeyFrameLocationChange;
    }

    public void OnZoomChanged() {
        this.myPosition = null;
    }

    private void OnKeyFrameValueChange(KeyFrame keyframe) {
        this.myPosition = default;
        this.editor.InvalidateVisual();
    }

    private void OnKeyFrameLocationChange(KeyFrame keyframe, long oldframe, long newframe) {
        this.myPosition = default;
        this.editor.InvalidateVisual();
    }

    public static double GetYHelper(bool isHugeRange, KeyFrame keyFrame, double height) {
        return isHugeRange ? (height / 2d) : (height - GetY(keyFrame, height));
    }

    [SwitchAutomationDataType]
    public static double GetY(KeyFrame keyFrame, double height) {
        Parameter key = keyFrame.Sequence!.Parameter;
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
            case KeyFrameBool frame: {
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

    public static bool IsValueTooLarge(float min, float max) {
        return float.IsInfinity(min) || float.IsInfinity(max) || Maths.GetRange(min, max) > AutomationSequenceEditorControl.MaximumFloatingPointRange;
    }

    public static bool IsValueTooLarge(double min, double max) {
        return double.IsInfinity(min) || double.IsInfinity(max) || Maths.GetRange(min, max) > AutomationSequenceEditorControl.MaximumFloatingPointRange;
    }

    public static bool IsValueTooLarge(long min, long max) {
        return Maths.GetRange(min, max) > AutomationSequenceEditorControl.MaximumFloatingPointRange;
    }

    [SwitchAutomationDataType]
    public bool SetValueForMousePoint(Point point) {
        double height = this.editor.Bounds.Height;
        if (double.IsNaN(height) || height <= 0d) {
            return false;
        }

        Parameter key = this.keyFrame.Sequence!.Parameter;
        switch (this.keyFrame) {
            case KeyFrameFloat frame when key.Descriptor is ParameterDescriptorFloat fd:   frame.SetFloatValue((float) Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd); break;
            case KeyFrameDouble frame when key.Descriptor is ParameterDescriptorDouble fd: frame.SetDoubleValue(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd); break;
            case KeyFrameLong frame when key.Descriptor is ParameterDescriptorLong fd:     frame.SetLongValue((long) Math.Round(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum)), fd); break;
            case KeyFrameBool frame:
                double offset = (height / 100) * 30;
                double bound_b = height - offset;
                if (point.Y >= bound_b) {
                    frame.SetBoolValue(false);
                }
                else if (point.Y < offset) {
                    frame.SetBoolValue(true);
                }
                else {
                    return false;
                }

                return true;
            // case KeyFrameVector2 frame when key.Descriptor is ParameterDescriptorVector2 fd:
            //     double x = Maths.Clamp(Maths.Map(point.X, height, 0, fd.Minimum.X, fd.Maximum.X), fd.Minimum.X, fd.Maximum.X) / this.editor.UnitZoom;
            //     double y = Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum.Y, fd.Maximum.Y), fd.Minimum.Y, fd.Maximum.Y);
            //     frame.Value = new Vector2((float) x, (float) y);
            //     break;
            default: return false;
        }

        return true;
    }
}