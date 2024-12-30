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
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Playheads;

public class GrippedPlayHeadControl : BasePlayHeadControl {
    private Thumb? PART_ThumbHead;
    private Thumb? PART_ThumbBody;
    private long lastFrame;
    private double lastDeltaX;

    public GrippedPlayHeadControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ThumbHead = e.NameScope.Find("PART_ThumbHead") as Thumb;
        this.PART_ThumbBody = e.NameScope.Find("PART_ThumbBody") as Thumb;
        if (this.PART_ThumbHead != null) {
            this.PART_ThumbHead.DragDelta += this.PART_ThumbOnDragDelta;
        }

        if (this.PART_ThumbBody != null) {
            this.PART_ThumbBody.DragDelta += this.PART_ThumbOnDragDelta;
        }
    }

    private void PART_ThumbOnDragDelta(object? sender, VectorEventArgs e) {
        if (!(this.TimelineControl is TimelineControl control) || !(control.Timeline is Timeline timeline)) {
            return;
        }

        // long change = (long) Math.Round(e.Vector.X / control.Zoom);
        long change = (long) Math.Round(e.Vector.X * control.Zoom);
        if (change != 0) {
            long oldFrame = this.Frame;
            long newFrame = Math.Max(oldFrame + change, 0);
            if (newFrame >= timeline.MaxDuration) {
                newFrame = timeline.MaxDuration - 1;
            }

            this.lastFrame = this.Frame;
            this.lastDeltaX = e.Vector.X;
            if (newFrame != oldFrame) {
                // this.Frame = (long) Maths.Clamp(newFrame - (this.lastFrame), 0, timeline.MaxDuration - 1);
                this.Frame = newFrame;
            }
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