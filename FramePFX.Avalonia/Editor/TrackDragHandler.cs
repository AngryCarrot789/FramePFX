// 
// Copyright (c) 2026-2026 REghZy
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
using FramePFX.Editing;

namespace FramePFX.Avalonia.Editor;

internal sealed class TrackDragHandler {
    private int dragState; // 0 = none, 1 = init, 2 = dragging
    private Clip? draggingClip;
    private TimeSpan originalLocation;
    private TimeSpan originalDuration;
    private IPointer? dragPointer;
    private Point clickPos;
    private double offsetToClipStart;

    public TimelineTrackControl Track { get; }

    private double Zoom => this.Track.OwnerPanel!.TimelineControl.Zoom;
    
    public bool IsDraggingOrWaitingForRegion => this.dragState > 0;
    
    public bool IsDragging => this.dragState == 2;

    public TrackDragHandler(TimelineTrackControl track) {
        this.Track = track;
    }

    public void OnLeftPointerPressed(Point mPos, Clip? hitClip, PointerPressedEventArgs e) {
        if (hitClip == null) {
            return;
        }

        this.ClearDrag(true);

        this.dragState = 1;
        this.draggingClip = hitClip;
        this.originalLocation = hitClip.Span.Start;
        this.originalDuration = hitClip.Span.Duration;
        this.dragPointer = e.Pointer;
        this.clickPos = mPos;
        this.offsetToClipStart = TimelineUnits.TicksToPixels(hitClip.Span.Start.Ticks, this.Zoom) - mPos.X;
        e.Pointer.Capture(this.Track);
    }

    public void OnPointerReleased(PointerReleasedEventArgs e) {
        if (!e.Properties.IsLeftButtonPressed) {
            this.ClearDrag(false);
        }
    }

    public void OnPointerMoved(PointerEventArgs e) {
        if (!e.Properties.IsLeftButtonPressed) {
            this.ClearDrag(false);
            return;
        }

        if (this.dragState == 1) {
            Point mPos = e.GetPosition(this.Track);
            if (Math.Abs(mPos.X - this.clickPos.X) >= 5.0) {
                this.dragState = 2;
            }
        }

        if (this.dragState == 2) {
            Point mPos = e.GetPosition(this.Track);
            long location = TimelineUnits.PixelsToTicks(Math.Max(0, mPos.X + this.offsetToClipStart), this.Zoom);
            this.draggingClip!.Span = ClipSpan.FromDuration(new TimeSpan(location), this.originalDuration);
        }
    }

    private void ClearDrag(bool cancel) {
        if (this.dragState == 2) {
            if (cancel) {
                this.draggingClip!.Span = ClipSpan.FromDuration(this.originalLocation, this.originalDuration);
            }
        }

        this.dragState = 0;
        this.draggingClip = null;
        this.originalLocation = TimeSpan.Zero;
        this.originalDuration = TimeSpan.Zero;
        if (this.dragPointer != null && ReferenceEquals(this.dragPointer!.Captured, this.Track)) {
            this.dragPointer!.Capture(null);
        }

        this.dragPointer = null;
    }
}