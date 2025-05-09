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
using Avalonia;
using Avalonia.Input;
using FramePFX.Avalonia.Editing.Timelines;
using FramePFX.Editing;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Playheads;

public class FlatLinePlayHeadControl : BasePlayHeadControl {
    private const int StateNone = 0;
    private const int StateInit = 1;
    private const int StateActive = 2;
    private const double MinDragInitPx = 5d;

    private Point clickPoint;
    private int dragState;

    public FlatLinePlayHeadControl() {
        this.Focusable = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        e.Handled = true;
        this.Focus();
        this.clickPoint = e.GetPosition(this);
        this.SetDragState(StateInit);
        if (this != e.Pointer.Captured)
            e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        e.Handled = true;
        if (this.dragState == StateActive) {
            this.myTimeline?.EndScrubPlayHead(this.PlayHeadType);
        }

        this.SetDragState(StateNone);
        if (this == e.Pointer.Captured) {
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        Point mPos = e.GetPosition(this);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            if (this.dragState == StateActive) {
                this.myTimeline?.EndScrubPlayHead(this.PlayHeadType);
            }

            this.SetDragState(StateNone);
            if (this == e.Pointer.Captured)
                e.Pointer.Capture(null);

            return;
        }

        if (this.TimelineControl is not TimelineControl control || control.Timeline is not Timeline timeline) {
            return;
        }

        if (this.dragState == StateInit) {
            if (Math.Abs(mPos.X - this.clickPoint.X) < MinDragInitPx) {
                return;
            }

            this.SetDragState(StateActive);
            this.myTimeline?.BeginScrubPlayHead(this.PlayHeadType);
        }

        if (this.dragState == StateNone) {
            return;
        }

        Point diff = mPos - this.clickPoint;
        long oldFrame = this.Frame;
        if (Math.Abs(diff.X) >= 1.0d) {
            long offset = (long) Math.Round(diff.X / control.Zoom);
            if (offset != 0) {
                // If begin is 2 and offset is -5, this sets offset to -2
                // and since newBegin = begin+offset (2 + -2)
                // this ensures begin never drops below 0
                if ((oldFrame + offset) < 0) {
                    offset = -oldFrame;
                }

                if (offset != 0) {
                    long newFrame = oldFrame + offset;
                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0) {
                        if (SnapHelper.SnapPlayHeadToClipEdge(control.Timeline, newFrame, out long snapFrame)) {
                            newFrame = snapFrame;
                        }
                    }

                    this.Frame = Math.Min(newFrame, timeline.MaxDuration - 1);
                }
            }
        }
    }

    private void SetDragState(int state) {
        this.dragState = state;
    }
}