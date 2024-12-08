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

namespace FramePFX.Editing.Timelines.Clips;

/// <summary>
/// An interface for a clip that has an adjustable playback speed, represented as a double.
/// It is expected that when the playback speed is changed, the clip's underlying <see cref="Clip.FrameSpan"/> will change too
/// </summary>
public interface IHavePlaybackSpeedClip : IClip
{
    const double MinimumSpeed = 0.001;
    const double MaximumSpeed = 1000.0;
    
    /// <summary>
    /// Gets whether there is a playback speed set that is not 1.0
    /// </summary>
    bool HasSpeedApplied { get; }
    
    /// <summary>
    /// Gets the playback speed. 1.0 is the default
    /// </summary>
    double PlaybackSpeed { get; }
    
    /// <summary>
    /// Sets the playback speed. This method also updates the <see cref="Clip.FrameSpan"/> to accomodate.
    /// If set to 1.0, <see cref="HasSpeedApplied"/> becomes 0 and our span is set back to the original
    /// </summary>
    /// <param name="speed"></param>
    void SetPlaybackSpeed(double speed);
    
    /// <summary>
    /// Sets the playback speed back to 1.0. This is a helper method to calling <see cref="SetPlaybackSpeed"/> with 1.0
    /// </summary>
    void ClearPlaybackSpeed();
}