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

public sealed class TrackViewState {
    public const double MinTrackHeight = 20;
    public const double MaxTrackHeight = 400;
    public const double DefaultTrackHeight = 60;
    
    /// <summary>
    /// Gets the track associated with this view state
    /// </summary>
    public Track Track { get; }

    /// <summary>
    /// Gets the top level that this track view state is associated with
    /// </summary>
    public TopLevelIdentifier TopLevelIdentifier { get; }

    /// <summary>
    /// Gets or sets the track height. Default value is 60. Minimum value is 20. Maximum value is 400
    /// </summary>
    public double Height {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, Math.Clamp(value, MinTrackHeight, MaxTrackHeight), this, this.HeightChanged);
    } = DefaultTrackHeight;

    /// <summary>
    /// Gets the observable list of selected clips
    /// </summary>
    public SelectionSet<Clip> SelectedClips { get; }
    
    public event EventHandler? HeightChanged;
    
    private TrackViewState(Track track, TopLevelIdentifier topLevelIdentifier) {
        this.Track = track;
        this.TopLevelIdentifier = topLevelIdentifier;
        this.SelectedClips = new SelectionSet<Clip>();
    }
    
    public static TrackViewState GetInstance(Track track, TopLevelIdentifier topLevel) => TopLevelDataMap.GetInstance(track).GetOrCreate(topLevel, track, (t, i) => new TrackViewState((Track) t!, i));
}