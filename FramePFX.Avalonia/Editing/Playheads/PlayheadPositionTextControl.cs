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
using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.Bindings;
using FramePFX.Editing.Timelines;

namespace FramePFX.Avalonia.Editing.Playheads;

public class PlayheadPositionTextControl : TemplatedControl {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<PlayheadPositionTextControl, Timeline?>(nameof(Timeline));
    public static readonly StyledProperty<long> PlayHeadPositionProperty = AvaloniaProperty.Register<PlayheadPositionTextControl, long>("PlayHeadPosition");
    public static readonly StyledProperty<long> TotalFrameDurationProperty = AvaloniaProperty.Register<PlayheadPositionTextControl, long>("TotalFrameDuration");
    public static readonly StyledProperty<long> LargestFrameInUseProperty = AvaloniaProperty.Register<PlayheadPositionTextControl, long>("LargestFrameInUse");

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public long PlayHeadPosition {
        get => this.GetValue(PlayHeadPositionProperty);
        set => this.SetValue(PlayHeadPositionProperty, value);
    }

    public long TotalFrameDuration {
        get => this.GetValue(TotalFrameDurationProperty);
        set => this.SetValue(TotalFrameDurationProperty, value);
    }

    public long LargestFrameInUse {
        get => this.GetValue(LargestFrameInUseProperty);
        set => this.SetValue(LargestFrameInUseProperty, value);
    }

    private readonly AvaloniaPropertyToEventPropertyGetSetBinder<Timeline> playHeadBinder = new AvaloniaPropertyToEventPropertyGetSetBinder<Timeline>(PlayHeadPositionProperty, nameof(PlayheadPositionTextControl.Timeline.PlayHeadChanged), (b) => b.Model.PlayHeadPosition, (b, v) => b.Model.PlayHeadPosition = (long) v);
    private readonly AvaloniaPropertyToEventPropertyGetSetBinder<Timeline> totalFramesBinder = new AvaloniaPropertyToEventPropertyGetSetBinder<Timeline>(TotalFrameDurationProperty, nameof(PlayheadPositionTextControl.Timeline.MaxDurationChanged), (b) => b.Model.MaxDuration, (b, v) => b.Model.MaxDuration = (long) v);
    private readonly AvaloniaPropertyToEventPropertyBinder<Timeline> largestFrameInUseBinder = new AvaloniaPropertyToEventPropertyBinder<Timeline>(LargestFrameInUseProperty, nameof(PlayheadPositionTextControl.Timeline.LargestFrameInUseChanged), obj => obj.Control.SetValue(LargestFrameInUseProperty, obj.Model.LargestFrameInUse), null);

    public PlayheadPositionTextControl() {
    }

    static PlayheadPositionTextControl() {
        TimelineProperty.Changed.AddClassHandler<PlayheadPositionTextControl, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            this.totalFramesBinder.Detach();
            this.playHeadBinder.Detach();
            this.largestFrameInUseBinder.Detach();
        }

        if (newTimeline != null) {
            this.totalFramesBinder.Attach(this, newTimeline);
            this.playHeadBinder.Attach(this, newTimeline);
            this.largestFrameInUseBinder.Attach(this, newTimeline);
        }
        else {
            this.PlayHeadPosition = 0;
            this.TotalFrameDuration = 0;
            this.LargestFrameInUse = 0;
        }
    }
}