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
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Playheads;

public class GrippedPlayHeadControl : BasePlayHeadControl {
    private Thumb? PART_ThumbHead;
    private Thumb? PART_ThumbBody;
    private long lastFrame;
    private double lastDeltaX;
    private bool isShiftPressed;

    public GrippedPlayHeadControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ThumbHead = e.NameScope.Find("PART_ThumbHead") as Thumb;
        this.PART_ThumbBody = e.NameScope.Find("PART_ThumbBody") as Thumb;
        if (this.PART_ThumbHead != null) {
            this.PART_ThumbHead.DragStarted += this.PART_ThumbHeadOnDragStarted;
            this.PART_ThumbHead.DragCompleted += this.PART_ThumbHeadOnDragCompleted;
            this.PART_ThumbHead.DragDelta += this.PART_ThumbOnDragDelta;

            this.PART_ThumbHead.AddHandler(KeyDownEvent, this.OnKey, RoutingStrategies.Tunnel, true);
            this.PART_ThumbHead.AddHandler(KeyUpEvent, this.OnKey, RoutingStrategies.Tunnel, true);
        }

        if (this.PART_ThumbBody != null) {
            this.PART_ThumbBody.DragStarted += this.PART_ThumbHeadOnDragStarted;
            this.PART_ThumbBody.DragCompleted += this.PART_ThumbHeadOnDragCompleted;
            this.PART_ThumbBody.DragDelta += this.PART_ThumbOnDragDelta;
            this.PART_ThumbBody.AddHandler(KeyDownEvent, this.OnKey, RoutingStrategies.Tunnel, true);
            this.PART_ThumbBody.AddHandler(KeyUpEvent, this.OnKey, RoutingStrategies.Tunnel, true);
        }
    }

    private void PART_ThumbHeadOnDragStarted(object? sender, VectorEventArgs e) {
        this.myTimeline?.BeginScrubPlayHead(this.PlayHeadType);
    }

    private void PART_ThumbHeadOnDragCompleted(object? sender, VectorEventArgs e) {
        this.myTimeline?.EndScrubPlayHead(this.PlayHeadType);
    }

    private void OnKey(object? sender, KeyEventArgs e) {
        this.isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
    }

    private void PART_ThumbOnDragDelta(object? sender, VectorEventArgs e) {
        if (!(this.TimelineControl is TimelineControl control) || !(control.Timeline is Timeline timeline)) {
            return;
        }

        long change = (long) Math.Round(e.Vector.X / control.Zoom);
        // long change = (long) Math.Round(e.Vector.X * control.Zoom);
        if (change != 0) {
            long oldFrame = this.Frame;
            long newPlayHeadFrame = Math.Max(oldFrame + change, 0);
            if (newPlayHeadFrame >= timeline.MaxDuration) {
                newPlayHeadFrame = timeline.MaxDuration - 1;
            }

            if (this.isShiftPressed && SnapHelper.SnapPlayHeadToClipEdge(control.Timeline, newPlayHeadFrame, out long snappedFrame)) {
                newPlayHeadFrame = snappedFrame;
            }

            this.lastFrame = this.Frame;
            this.lastDeltaX = e.Vector.X;
            this.Frame = newPlayHeadFrame;
        }
    }

    protected override void SetPixelMargin(Thickness thickness) {
        thickness = new Thickness(thickness.Left - 7, thickness.Top, thickness.Right, thickness.Bottom);
        base.SetPixelMargin(thickness);
        // CompositionVisual? compositionVisual = ElementComposition.GetElementVisual(this);
        // if (compositionVisual != null) {
        //     Vector3D offset = compositionVisual.Offset;
        //     compositionVisual.Offset = new Vector3D(thickness.Left, offset.Y, offset.Z);
        // 
        //     // Compositor compositor = compositionVisual.Compositor;
        //     // Vector3DKeyFrameAnimation animation = compositor.CreateVector3DKeyFrameAnimation();
        //     // animation.InsertKeyFrame(0f, offset);
        //     // animation.InsertKeyFrame(1f, new Vector3D(thickness.Left, offset.Y, offset.Z));
        //     // animation.Duration = TimeSpan.FromMilliseconds(1);
        //     // compositionVisual.StartAnimation("Offset", animation);
        // }
        // else {
        //     base.SetPixelMargin(thickness);
        // }
    }

    protected static readonly FieldInfo LastPointField = typeof(Thumb).GetField("_lastPoint", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public void EnableDragging(PointerEventArgs e) {
        Thumb thumb = this.PART_ThumbBody!;
        e.Handled = true;
        e.Pointer.Capture(thumb);

        Point pt = new Point(thumb.Bounds.Width / 2.0, thumb.Bounds.Height / 2.0);
        LastPointField.SetValue(thumb, pt);

        VectorEventArgs ev = new VectorEventArgs {
            RoutedEvent = Thumb.DragStartedEvent,
            Vector = pt,
        };

        e.PreventGestureRecognition();
        thumb.RaiseEvent(ev);
    }
}