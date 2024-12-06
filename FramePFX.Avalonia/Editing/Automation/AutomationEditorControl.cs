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
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.Automation;

public class AutomationEditorControl : Control {
    public const double EllipseRadius = 2.5d;
    public const double EllipseThickness = 1d;
    public const double EllipseHitRadius = 12d;
    public const double LineThickness = 2d;
    public const double LineHitThickness = 12d;
    public const double MaximumFloatingPointRange = 10000;

    public static readonly StyledProperty<double> HorizontalZoomProperty = AvaloniaProperty.Register<AutomationEditorControl, double>(nameof(HorizontalZoom));
    public static readonly StyledProperty<AutomationSequence?> AutomationSequenceProperty = AvaloniaProperty.Register<AutomationEditorControl, AutomationSequence?>(nameof(AutomationSequence));
    public static readonly StyledProperty<IBrush?> ShapeBrushProperty = AvaloniaProperty.Register<AutomationEditorControl, IBrush?>(nameof(ShapeBrush), Brushes.OrangeRed);
    public static readonly StyledProperty<IBrush?> OverrideShapeBrushProperty = AvaloniaProperty.Register<AutomationEditorControl, IBrush?>(nameof(OverrideShapeBrush), Brushes.DarkGray);

    public double HorizontalZoom {
        get => this.GetValue(HorizontalZoomProperty);
        set => this.SetValue(HorizontalZoomProperty, value);
    }

    public AutomationSequence? AutomationSequence {
        get => this.GetValue(AutomationSequenceProperty);
        set => this.SetValue(AutomationSequenceProperty, value);
    }
    
    public IBrush? ShapeBrush {
        get => this.GetValue(ShapeBrushProperty);
        set => this.SetValue(ShapeBrushProperty, value);
    }
    
    public IBrush? OverrideShapeBrush {
        get => this.GetValue(OverrideShapeBrushProperty);
        set => this.SetValue(OverrideShapeBrushProperty, value);
    }

    public bool IsValueRangeHuge { get; private set; }

    private readonly KeyFrameEventHandler keyFrameValueChangeHandler;

    public AutomationEditorControl() {
        this.IsHitTestVisible = true;
        this.keyFrameValueChangeHandler = this.OnKeyFrameValueChanged;
        this.UseLayoutRounding = true;
    }

    static AutomationEditorControl() {
        AffectsRender<AutomationEditorControl>(HorizontalZoomProperty, AutomationSequenceProperty);
        AutomationSequenceProperty.Changed.AddClassHandler<AutomationEditorControl, AutomationSequence?>((d, e) => d.OnAutomationSequenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnAutomationSequenceChanged(AutomationSequence? oldSequence, AutomationSequence? newSequence) {
        if (oldSequence != null) {
            oldSequence.OverrideStateChanged -= this.OnIsOverriddenEnabledChanged;
            oldSequence.DefaultKeyFrame.FrameChanged -= this.OnKeyFramePositionChanged;
            oldSequence.DefaultKeyFrame.ValueChanged -= this.keyFrameValueChangeHandler;
            oldSequence.KeyFrameAdded -= this.OnKeyFrameAdded;
            oldSequence.KeyFrameRemoved -= this.OnKeyFrameRemoved;
            
            for (int i = oldSequence.KeyFrames.Count - 1; i >= 0; i--) {
                this.OnKeyFrameRemoved(true, oldSequence, oldSequence.KeyFrames[i], i);
            }
        }

        if (newSequence != null) {
            newSequence.OverrideStateChanged += this.OnIsOverriddenEnabledChanged;
            newSequence.DefaultKeyFrame.FrameChanged += this.OnKeyFramePositionChanged;
            newSequence.DefaultKeyFrame.ValueChanged += this.keyFrameValueChangeHandler;
            newSequence.KeyFrameAdded += this.OnKeyFrameAdded;
            newSequence.KeyFrameRemoved += this.OnKeyFrameRemoved;

            int i = 0;
            foreach (KeyFrame keyFrame in newSequence.KeyFrames) {
                this.OnKeyFrameAdded(true, newSequence, keyFrame, i++);
            }
        }

        this.SetupRenderingInfo(newSequence);
    }

    private void OnIsOverriddenEnabledChanged(AutomationSequence sequence) {
        this.InvalidateVisual();
    }

    private void OnKeyFramePositionChanged(KeyFrame keyframe, long oldFrame, long newFrame) {
        this.InvalidateVisual();
    }

    private void OnKeyFrameValueChanged(KeyFrame keyframe) {
        this.InvalidateVisual();
    }

    private void OnKeyFrameAdded(AutomationSequence sequence, KeyFrame keyframe, int index) {
        this.OnKeyFrameAdded(false, sequence, keyframe, index);
    }

    private void OnKeyFrameRemoved(AutomationSequence sequence, KeyFrame keyframe, int index) {
        this.OnKeyFrameRemoved(false, sequence, keyframe, index);
    }
    
    private void OnKeyFrameAdded(bool isOnSequenceChange, AutomationSequence sequence, KeyFrame keyFrame, int index) {
        if (!isOnSequenceChange)
            this.InvalidateVisual();
        keyFrame.ValueChanged += this.keyFrameValueChangeHandler;
    }

    private void OnKeyFrameRemoved(bool isOnSequenceChange, AutomationSequence sequence, KeyFrame keyFrame, int index) {
        if (!isOnSequenceChange)
            this.InvalidateVisual();
        keyFrame.ValueChanged -= this.keyFrameValueChangeHandler;
    }
    
    public Point ScreenOffset => this.TemplatedParent is TimelineClipControl ? new Point(-1.0, 0) : default;
    
    public override void Render(DrawingContext ctx) {
        base.Render(ctx);

        Size size = this.Bounds.Size;
        if (!(this.AutomationSequence is AutomationSequence sequence)) {
            return;
        }

        // Screen offset is used to offset all rendering due to margins and extra borders on the parent.
        // Clips are -1,0 because clips have a natural border thickness of 1,0 so we need to move back to 0,0.
        // Tracks are 0,0 since they have no borders. The track panel has a spacing of 1 but that doesn't count
        Point offset = this.ScreenOffset;
        double zoom = this.HorizontalZoom;
        
        // TODO: cache maybe
        IBrush? overrideBrush = sequence.IsOverrideEnabled ? (this.OverrideShapeBrush ?? Brushes.Gray) : null;
        IBrush theBrush = overrideBrush ?? this.ShapeBrush ?? Brushes.OrangeRed;
        Pen ellipsePen = new Pen(theBrush, EllipseThickness);
        Pen linePen = new Pen(theBrush, LineThickness);

        if (sequence.IsEmpty) {
            // TODO: uncomment when we can actually modify key frames via this UI 
            // DrawDottedLine(ctx, theBrush, sequence.IsOverrideEnabled ? this.GetYHelper(sequence.DefaultKeyFrame, size.Height) : (size.Height / 2.0), size.Width);
            return;
        }

        List<KeyFrame> list = sequence.KeyFrames.ToList();
        List<Point> points = list.Select(Selector).ToList();

        // Draw lines first so that ellipses can be drawn on top of them all
        Point firstPoint = points[0].WithX(0), prevPoint = firstPoint;
        foreach (Point pt in points) {
            ctx.DrawLine(linePen, prevPoint, pt);
            prevPoint = pt;
        }

        ctx.DrawLine(linePen, prevPoint, this.PointForKeyFrame(list[list.Count - 1], size, offset, zoom).WithX(size.Width));
        foreach (Point pt in points) {
            ctx.DrawEllipse(theBrush, ellipsePen, pt, EllipseRadius, EllipseRadius);
        }

        if (sequence.IsOverrideEnabled) {
            double y = this.GetYHelper(sequence.DefaultKeyFrame, size.Height);
            DrawDottedLine(ctx, theBrush, y, size.Width);
        }

        return;

        Point Selector(KeyFrame x) => this.PointForKeyFrame(x, size, offset, zoom);
    }

    private static void DrawDottedLine(DrawingContext ctx, IBrush brush, double y, double width) {
        Pen pen = new Pen(brush, LineThickness, new ImmutableDashStyle([2, 3], 0), PenLineCap.Square);
        ctx.DrawLine(pen, new Point(0, y), new Point(width, y));
    }

    public Point PointForKeyFrame(KeyFrame keyFrame, Size size, Point offset, double zoom) {
        double height = size.Height + offset.Y;
        double px = TimelineUtils.FrameToPixel(keyFrame.Frame, zoom) + offset.X;
        double py = this.GetYHelper(keyFrame, height);
        if (double.IsNaN(py)) {
            py = 0;
        }

        return new Point(px, py);
    }

    public double GetYHelper(KeyFrame keyFrame, double height) {
        return this.IsValueRangeHuge ? (height / 2d) : (height - GetY(keyFrame, height));
    }

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

    [SwitchAutomationDataType]
    private void SetupRenderingInfo(AutomationSequence? sequence) {
        this.IsValueRangeHuge = false;
        if (sequence == null) {
            return;
        }

        switch (sequence.Parameter.DataType) {
            case AutomationDataType.Float: {
                ParameterDescriptorFloat desc = (ParameterDescriptorFloat) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Double: {
                ParameterDescriptorDouble desc = (ParameterDescriptorDouble) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Long: {
                ParameterDescriptorLong desc = (ParameterDescriptorLong) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Boolean: break;
            case AutomationDataType.Vector2: break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private static bool IsValueTooLarge(float min, float max) {
        return float.IsInfinity(min) || float.IsInfinity(max) || Maths.GetRange(min, max) > MaximumFloatingPointRange;
    }

    private static bool IsValueTooLarge(double min, double max) {
        return double.IsInfinity(min) || double.IsInfinity(max) || Maths.GetRange(min, max) > MaximumFloatingPointRange;
    }

    private static bool IsValueTooLarge(long min, long max) {
        return Maths.GetRange(min, max) > MaximumFloatingPointRange;
    }
}