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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Timelines;

public class TimelineScrollableContentGrid : Grid {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TimelineScrollableContentGrid, Timeline?>(nameof(Timeline));

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public TimelineControl? TimelineControl { get; set; }

    public bool HandleBringIntoView {
        get => HandleRequestBringIntoView.GetIsEnabled(this);
        set => HandleRequestBringIntoView.SetIsEnabled(this, value);
    }

    static TimelineScrollableContentGrid() {
        TimelineProperty.Changed.AddClassHandler<TimelineScrollableContentGrid, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    public TimelineScrollableContentGrid() {
        this.HandleBringIntoView = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (!e.Handled && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && this.TimelineControl != null) {
            Point point = e.GetPosition(this);
            // bool isClickSequence = point.Y > this.TimelineControl.TimelineRuler.ActualHeight;
            // this.TimelineControl.SetPlayHeadToMouseCursor(point.X, isClickSequence);
            // if (isClickSequence)
            // {
            //     this.TimelineControl.Timeline?.ClearClipSelection();
            //     this.TimelineControl.UpdatePropertyEditorClipSelection();
            // }
        }
    }

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.PlayHeadChanged -= this.OnPlayHeadChanged;
        }

        if (newTimeline != null) {
            newTimeline.PlayHeadChanged += this.OnPlayHeadChanged;
        }
    }

    private void OnPlayHeadChanged(Timeline timeline, long oldvalue, long newvalue) {
        this.InvalidateMeasure();
    }

    protected override Size MeasureCore(Size availableSize) {
        Size size = base.MeasureCore(availableSize);
        if (this.TimelineControl != null && this.TimelineControl.Timeline == null)
            size = size.WithWidth(this.TimelineControl.Bounds.Width);
        return size;
    }

    protected override Size ArrangeOverride(Size arrangeSize) {
        if (this.TimelineControl != null && this.TimelineControl.Timeline == null)
            arrangeSize = arrangeSize.WithWidth(this.TimelineControl.Bounds.Width);
        Size arrange = base.ArrangeOverride(arrangeSize);
        return arrange.WithWidth(arrangeSize.Width);
    }
}