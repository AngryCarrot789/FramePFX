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

using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Editing.ViewStates;

public sealed class TimelineViewState {
    public const double MinimumZoom = 0.001;
    public const double MaximumZoom = 100.0;
    public const double DefaultZoom = 1.0;

    /// <summary>
    /// Gets the timeline model associated with this view state
    /// </summary>
    public Timeline Timeline { get; }
    
    /// <summary>
    /// Gets the identifier of the top level that this view state belongs to
    /// </summary>
    public TopLevelIdentifier TopLevelIdentifier { get; }

    /// <summary>
    /// Gets or sets the zoom factor relative to <see cref="HorizontalScroll"/> on the left side of the view bounds
    /// </summary>
    public double Zoom {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, Math.Clamp(value, MinimumZoom, MaximumZoom), this, this.ZoomChanged);
    } = DefaultZoom;
    
    /// <summary>
    /// Gets or sets the horizontal scroll value of the timeline
    /// </summary>
    public TimeSpan HorizontalScroll {
        get => field;
        set {
            this.Timeline.MaximumDuration = new TimeSpan(Math.Max(value.Ticks, this.Timeline.MaximumDuration.Ticks));
            PropertyHelper.SetAndRaiseINE(ref field, new TimeSpan(Math.Max(value.Ticks, 0)), this, this.HorizontalScrollChanged);
        }
    }

    /// <summary>
    /// Gets the observable list of selected tracks
    /// </summary>
    public SelectionSet<Track> SelectedTracks { get; }
    
    public event EventHandler<ValueChangedEventArgs<TimeSpan>>? HorizontalScrollChanged;
    public event EventHandler<ValueChangedEventArgs<double>>? ZoomChanged;
    
    private TimelineViewState(Timeline timeline, TopLevelIdentifier topLevelIdentifier) {
        this.Timeline = timeline;
        this.TopLevelIdentifier = topLevelIdentifier;
        this.SelectedTracks = new SelectionSet<Track>();
    }
    
    public static TimelineViewState GetInstance(Timeline timeline, TopLevelIdentifier topLevel) => TopLevelDataMap.GetInstance(timeline).GetOrCreate(topLevel, timeline, (t, i) => new TimelineViewState((Timeline) t!, i));
}