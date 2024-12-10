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
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing.Automation;

public class AutomationSequenceEditorControl : Control
{
    public const double EllipseRadius = 4.0;
    public const double EllipseHitRadius = 12d;
    public const double LineThickness = 2d;
    public const double LineHitThickness = 10d;
    public const double MaximumFloatingPointRange = 10000;

    public static readonly StyledProperty<double> HorizontalZoomProperty = AvaloniaProperty.Register<AutomationSequenceEditorControl, double>(nameof(HorizontalZoom));
    public static readonly StyledProperty<AutomationSequence?> AutomationSequenceProperty = AvaloniaProperty.Register<AutomationSequenceEditorControl, AutomationSequence?>(nameof(AutomationSequence));
    public static readonly StyledProperty<IBrush?> ShapeBrushProperty = AvaloniaProperty.Register<AutomationSequenceEditorControl, IBrush?>(nameof(ShapeBrush), Brushes.OrangeRed);
    public static readonly StyledProperty<IBrush?> OverrideShapeBrushProperty = AvaloniaProperty.Register<AutomationSequenceEditorControl, IBrush?>(nameof(OverrideShapeBrush), Brushes.DarkGray);
    public static readonly StyledProperty<long> FrameDurationProperty = AvaloniaProperty.Register<AutomationSequenceEditorControl, long>(nameof(FrameDuration), long.MaxValue);

    public double HorizontalZoom
    {
        get => this.GetValue(HorizontalZoomProperty);
        set => this.SetValue(HorizontalZoomProperty, value);
    }

    public AutomationSequence? AutomationSequence
    {
        get => this.GetValue(AutomationSequenceProperty);
        set => this.SetValue(AutomationSequenceProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw key frames and connector lines 
    /// </summary>
    public IBrush? ShapeBrush
    {
        get => this.GetValue(ShapeBrushProperty);
        set => this.SetValue(ShapeBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used to draw key frames and connector lines when override is enabled 
    /// </summary>
    public IBrush? OverrideShapeBrush
    {
        get => this.GetValue(OverrideShapeBrushProperty);
        set => this.SetValue(OverrideShapeBrushProperty, value);
    }
    
    public long FrameDuration
    {
        get => this.GetValue(FrameDurationProperty);
        set => this.SetValue(FrameDurationProperty, value);
    }

    public bool IsValueRangeHuge { get; private set; }

    public Point ScreenOffset => this.TemplatedParent is TimelineClipControl ? new Point(-1.0, 0) : default;

    public readonly List<KeyFrameUI> keyFrameUIs;
    private KeyFrameUI? defaultKeyFrameUI;
    private KeyFrameMultiElement? mouseOverElement;
    private KeyFrameMultiElement? capturedElement;

    private Point leftClickPos;
    private DragState dragState;
    private bool flagHasCreatedKeyFrameForPress;

    private enum DragState { None, Initiated, Active }

    private readonly struct KeyFrameMultiElement
    {
        public readonly KeyFrameUI keyFrame;
        public readonly KeyFrameElementPart part;

        public KeyFrameMultiElement(KeyFrameUI keyFrame, KeyFrameElementPart part)
        {
            this.keyFrame = keyFrame;
            this.part = part;
        }
    }

    public AutomationSequenceEditorControl()
    {
        this.IsHitTestVisible = true;
        this.UseLayoutRounding = true;
        this.keyFrameUIs = new List<KeyFrameUI>();
    }

    static AutomationSequenceEditorControl()
    {
        AffectsRender<AutomationSequenceEditorControl>(HorizontalZoomProperty, AutomationSequenceProperty);
        AutomationSequenceProperty.Changed.AddClassHandler<AutomationSequenceEditorControl, AutomationSequence?>((d, e) => d.OnAutomationSequenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        HorizontalZoomProperty.Changed.AddClassHandler<AutomationSequenceEditorControl, double>((d, e) => d.OnHorizontalZoomChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnHorizontalZoomChanged(double oldValue, double newValue)
    {
        foreach (KeyFrameUI keyFrame in this.keyFrameUIs)
            keyFrame.OnZoomChanged();

        this.InvalidateVisual();
    }

    private void OnAutomationSequenceChanged(AutomationSequence? oldSequence, AutomationSequence? newSequence)
    {
        if (oldSequence != null)
        {
            oldSequence.OverrideStateChanged -= this.OnIsOverriddenEnabledChanged;
            oldSequence.KeyFrameAdded -= this.OnKeyFrameAdded;
            oldSequence.KeyFrameRemoved -= this.OnKeyFrameRemoved;
            for (int i = this.keyFrameUIs.Count - 1; i >= 0; i--)
            {
                this.OnKeyFrameRemoved(true, i);
            }

            this.defaultKeyFrameUI!.OnRemoved();
            this.defaultKeyFrameUI = null;
        }

        if (newSequence != null)
        {
            newSequence.OverrideStateChanged += this.OnIsOverriddenEnabledChanged;
            newSequence.KeyFrameAdded += this.OnKeyFrameAdded;
            newSequence.KeyFrameRemoved += this.OnKeyFrameRemoved;

            int i = 0;
            foreach (KeyFrame keyFrame in newSequence.KeyFrames)
            {
                this.OnKeyFrameAdded(true, keyFrame, i++);
            }

            this.defaultKeyFrameUI = new KeyFrameUI(this, newSequence.DefaultKeyFrame);
            this.defaultKeyFrameUI.OnAdded();
        }

        this.SetupRenderingInfo(newSequence);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (this.mouseOverElement is KeyFrameMultiElement mouseOver)
        {
            this.leftClickPos = e.GetPosition(this);
            this.dragState = DragState.Initiated;
            if (mouseOver.part == KeyFrameElementPart.LeftLine || mouseOver.part == KeyFrameElementPart.RightLine)
            {
                Point pos = e.GetPosition(this);
                long frame = TimelineUtils.PixelToFrame(pos.X, this.HorizontalZoom);
                int index = mouseOver.keyFrame.sequence.AddNewKeyFrame(frame, out KeyFrame keyFrame);
                KeyFrameUI newKeyFrameUI = this.keyFrameUIs[index];
                this.capturedElement = this.mouseOverElement = new KeyFrameMultiElement(newKeyFrameUI, KeyFrameElementPart.KeyFrame);
                newKeyFrameUI.MouseOverPart = KeyFrameElementPart.KeyFrame;
                newKeyFrameUI.SetValueForMousePoint(pos);
                this.flagHasCreatedKeyFrameForPress = true;
            }
            else
            {
                this.capturedElement = mouseOver;
            }
            
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        bool flag = this.flagHasCreatedKeyFrameForPress;
        this.flagHasCreatedKeyFrameForPress = false;
        
        if (this.capturedElement is KeyFrameMultiElement captured)
        {
            DragState oldState = this.dragState;
            this.capturedElement = null;
            this.dragState = DragState.None;
            if (ReferenceEquals(this, e.Pointer.Captured))
                e.Pointer.Capture(null);

            if (!flag && oldState == DragState.Initiated && captured.part == KeyFrameElementPart.KeyFrame)
            {
                captured.keyFrame.sequence.RemoveKeyFrame(captured.keyFrame.keyFrame, out _);
            }

            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        PointerPoint pointer = e.GetCurrentPoint(this);
        Point mPos = e.GetPosition(this);
        if (this.capturedElement is KeyFrameMultiElement captured && captured.part == KeyFrameElementPart.KeyFrame)
        {
            if (!pointer.Properties.IsLeftButtonPressed)
            {
                return;
            }
            
            const double minDragX = 5.0;
            const double minDragY = 5.0;
            if (Math.Abs(mPos.X - this.leftClickPos.X) < minDragX && Math.Abs(mPos.Y - this.leftClickPos.Y) < minDragY)
            {
                return;
            }

            this.dragState = DragState.Active;
            
            KeyFrameUI? prev = captured.keyFrame.Prev;
            KeyFrameUI? next = captured.keyFrame.Next;

            long min = prev?.keyFrame.Frame ?? 0;
            long max = next?.keyFrame.Frame ?? this.FrameDuration;

            // Update X position
            long newTime = Math.Max(0, (long) Math.Round(mPos.X / this.HorizontalZoom));
            long oldTime = captured.keyFrame.keyFrame.Frame;
            if ((oldTime + newTime) < 0)
            {
                newTime = -oldTime;
            }
            
            captured.keyFrame.keyFrame.Frame = Maths.Clamp(newTime, min, max);

            // Try update Y position
            if (!this.IsValueRangeHuge)
            {
                captured.keyFrame.SetValueForMousePoint(mPos);
            }

            return;
        }

        bool hasNotifiedDirty = this.ClearMouseOverElement();

        object? keyFrameOrList = null;
        foreach (KeyFrameUI keyFrame in this.keyFrameUIs)
        {
            if (IsMouseOverKeyFramePoint(keyFrame, mPos))
            {
                if (keyFrameOrList == null)
                {
                    keyFrameOrList = keyFrame;
                }
                else
                {
                    if (!(keyFrameOrList is List<KeyFrameUI> list))
                    {
                        list = new List<KeyFrameUI>();
                        list.Add((KeyFrameUI) keyFrameOrList);
                        keyFrameOrList = list;
                    }
                    
                    list.Add(keyFrame);
                }
            }
        }

        if (keyFrameOrList != null)
        {
            if (!(keyFrameOrList is KeyFrameUI keyFrame))
            {
                List<KeyFrameUI> list = (List<KeyFrameUI>) keyFrameOrList;
                keyFrame = list[0];
                double min = GetKeyFramePointOverDistance(list[0], mPos);
                for (int i = 1; i < list.Count; i++)
                {
                    double nextMin = GetKeyFramePointOverDistance(list[i], mPos);
                    if (nextMin < min)
                    {
                        min = nextMin;
                        keyFrame = list[i];
                    }
                }
            }
            
            this.mouseOverElement = new KeyFrameMultiElement(keyFrame, KeyFrameElementPart.KeyFrame);
            keyFrame.MouseOverPart = KeyFrameElementPart.KeyFrame;
            
            if (!hasNotifiedDirty)
                this.InvalidateVisual();

            return;
        }
        
        foreach (KeyFrameUI keyFrame in this.keyFrameUIs)
        {
            if (GetMouseOverLinePart(keyFrame, mPos) is bool part)
            {
                this.mouseOverElement = new KeyFrameMultiElement(keyFrame, part ? KeyFrameElementPart.LeftLine : KeyFrameElementPart.RightLine);
                if (part)
                {
                    keyFrame.MouseOverPart = KeyFrameElementPart.LeftLine;
                    if (keyFrame.Prev is KeyFrameUI prev)
                        prev.MouseOverPart = KeyFrameElementPart.RightLine;
                }
                else
                {
                    keyFrame.MouseOverPart = KeyFrameElementPart.RightLine;
                    if (keyFrame.Next is KeyFrameUI next)
                        next.MouseOverPart = KeyFrameElementPart.LeftLine;
                }

                if (!hasNotifiedDirty)
                    this.InvalidateVisual();
                
                return;
            }
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        this.ClearMouseOverElement();
    }

    private bool ClearMouseOverElement()
    {
        if (this.mouseOverElement is KeyFrameMultiElement mouseOver)
        {
            KeyFrameUI? prev = mouseOver.keyFrame.Prev;
            if (prev != null)
                prev.MouseOverPart = null;
            
            KeyFrameUI? next = mouseOver.keyFrame.Next;
            if (next != null)
                next.MouseOverPart = null;
            
            mouseOver.keyFrame.MouseOverPart = null;
            this.mouseOverElement = null;
            this.capturedElement = null;
            this.InvalidateVisual();
            return true;
        }

        return false;
    }

    private static bool IsMouseOverKeyFramePoint(KeyFrameUI keyFrame, Point mPos) => GetKeyFramePointOverDistance(keyFrame, mPos) <= EllipseHitRadius;

    private static double GetKeyFramePointOverDistance(KeyFrameUI keyFrame, Point mPos)
    {
        Point pos = keyFrame.GetPosition();
        double a = mPos.Y - pos.Y;
        double b = mPos.X - pos.X;
        return Math.Sqrt((a * a) + (b * b));
    }
    
    // Null = none, True = left, False = right
    private static bool? GetMouseOverLinePart(KeyFrameUI keyFrame, Point mPos)
    {
        KeyFrameUI? prev = keyFrame.Prev;
        KeyFrameUI? next = keyFrame.Next;
        Point lineP2 = keyFrame.GetPosition();
        Point lineP1 = prev?.GetPosition() ?? lineP2.WithX(0);
        Point lineP3 = next?.GetPosition() ?? lineP2.WithX(keyFrame.editor.Bounds.Width);

        if (IsMouseOverLine(ref mPos, ref lineP1, ref lineP2, LineHitThickness))
            return true;
        
        if (IsMouseOverLine(ref mPos, ref lineP2, ref lineP3, LineHitThickness))
            return false;
        
        return null;
    }
    
    public static bool IsMouseOverLine(ref Point p, ref Point a, ref Point b, double thickness)
    {
        double c1 = Math.Abs((b.X - a.X) * (a.Y - p.Y) - (a.X - p.X) * (b.Y - a.Y));
        double c2 = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        if ((c1 / c2) > (thickness / 2))
        {
            return false;
        }

        double ht = thickness / 2d;
        double minX = Math.Min(a.X, b.X) - ht;
        double maxX = Math.Max(a.X, b.X) + ht;
        double minY = Math.Min(a.Y, b.Y) - ht;
        double maxY = Math.Max(a.Y, b.Y) + ht;
        return p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        Size size = this.Bounds.Size;
        if (!(this.AutomationSequence is AutomationSequence sequence))
        {
            return;
        }

        // Screen offset is used to offset all rendering due to margins and extra borders on the parent.
        // Clips are -1,0 because clips have a natural border thickness of 1,0 so we need to move back to 0,0.
        // Tracks are 0,0 since they have no borders. The track panel has a spacing of 1 but that doesn't count
        IBrush? overrideBrush = sequence.IsOverrideEnabled ? (this.OverrideShapeBrush ?? Brushes.Gray) : null;
        IBrush theBrush = overrideBrush ?? this.ShapeBrush ?? Brushes.OrangeRed;
        IBrush? theMouseOverBrush = null;
        Pen transparentLinePen = new Pen(Brushes.Transparent, LineHitThickness);
        Pen linePen = new Pen(theBrush, LineThickness);
        Pen? mouseOverLinePen = null;

        if (sequence.IsEmpty)
        {
            // TODO: uncomment when we can actually modify key frames via this UI 
            // DrawDottedLine(ctx, theBrush, sequence.IsOverrideEnabled ? this.GetYHelper(sequence.DefaultKeyFrame, size.Height) : (size.Height / 2.0), size.Width);
            return;
        }

        List<Point> points = this.keyFrameUIs.Select(x => x.GetPosition()).ToList();

        // Draw lines first so that ellipses can be drawn on top of them all
        Point firstPoint = points[0].WithX(0), prevPoint = firstPoint;
        for (int i = 0; i < points.Count; i++)
        {
            Point pt = points[i];
            IPen pen = this.keyFrameUIs[i].MouseOverPart == KeyFrameElementPart.LeftLine ? (mouseOverLinePen ??= new Pen(Brushes.White, LineThickness)) : linePen;
            ctx.DrawLine(transparentLinePen, prevPoint, pt);
            ctx.DrawLine(pen, prevPoint, pt);
            prevPoint = pt;
        }

        {
            Point endPos = points[points.Count - 1].WithX(size.Width);
            ctx.DrawLine(transparentLinePen, prevPoint, endPos);
            ctx.DrawLine(this.keyFrameUIs[points.Count - 1].MouseOverPart == KeyFrameElementPart.RightLine ? (mouseOverLinePen ?? new Pen(Brushes.White, LineThickness)) : linePen, prevPoint, endPos);
        }
        
        for (int i = 0; i < points.Count; i++)
        {
            IBrush brush = this.keyFrameUIs[i].MouseOverPart == KeyFrameElementPart.KeyFrame ? (theMouseOverBrush ??= Brushes.White) : theBrush;
            ctx.DrawEllipse(Brushes.Transparent, null, points[i], EllipseHitRadius, EllipseHitRadius);
            ctx.DrawEllipse(brush, null, points[i], EllipseRadius, EllipseRadius);
        }

        if (sequence.IsOverrideEnabled)
        {
            DrawDottedLine(ctx, theBrush, this.defaultKeyFrameUI!.GetPosition().Y, size.Width);
        }
    }

    private static void DrawDottedLine(DrawingContext ctx, IBrush brush, double y, double width)
    {
        Pen pen = new Pen(brush, LineThickness, new ImmutableDashStyle([2, 3], 0), PenLineCap.Square);
        ctx.DrawLine(pen, new Point(0, y), new Point(width, y));
    }

    [SwitchAutomationDataType]
    private void SetupRenderingInfo(AutomationSequence? sequence)
    {
        this.IsValueRangeHuge = false;
        if (sequence == null)
        {
            return;
        }

        switch (sequence.Parameter.DataType)
        {
            case AutomationDataType.Float:
            {
                ParameterDescriptorFloat desc = (ParameterDescriptorFloat) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = KeyFrameUI.IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Double:
            {
                ParameterDescriptorDouble desc = (ParameterDescriptorDouble) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = KeyFrameUI.IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Long:
            {
                ParameterDescriptorLong desc = (ParameterDescriptorLong) sequence.Parameter.Descriptor;
                this.IsValueRangeHuge = KeyFrameUI.IsValueTooLarge(desc.Minimum, desc.Maximum);
                break;
            }
            case AutomationDataType.Boolean: break;
            case AutomationDataType.Vector2: break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void OnIsOverriddenEnabledChanged(AutomationSequence sequence) => this.InvalidateVisual();

    private void OnKeyFrameAdded(AutomationSequence sequence, KeyFrame keyframe, int index) => this.OnKeyFrameAdded(false, keyframe, index);

    private void OnKeyFrameRemoved(AutomationSequence sequence, KeyFrame keyframe, int index) => this.OnKeyFrameRemoved(false, index);

    private void OnKeyFrameAdded(bool isOnSequenceChange, KeyFrame keyFrame, int index)
    {
        this.keyFrameUIs.Insert(index, new KeyFrameUI(this, keyFrame)
        {
            Index = index
        });
        
        this.keyFrameUIs[index].OnAdded();

        if (!isOnSequenceChange)
        {
            UpdateIndexForInsertionOrRemoval(this.keyFrameUIs, index);
            this.InvalidateVisual();
        }
    }

    private void OnKeyFrameRemoved(bool isOnSequenceChange, int index)
    {
        this.ClearMouseOverElement();
        KeyFrameUI removing = this.keyFrameUIs[index];
        removing.OnRemoved();
        this.keyFrameUIs.RemoveAt(index);

        if (!isOnSequenceChange)
        {
            UpdateIndexForInsertionOrRemoval(this.keyFrameUIs, index);
            this.InvalidateVisual();
        }
    }
    
    private static void UpdateIndexForInsertionOrRemoval(List<KeyFrameUI> keyFrames, int index)
    {
        for (int i = keyFrames.Count - 1; i >= index; i--)
        {
            keyFrames[i].Index = i;
        }
    }
}