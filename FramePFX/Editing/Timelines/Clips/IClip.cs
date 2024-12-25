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

using FramePFX.DataTransfer;
using FramePFX.Editing.Automation;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity;

namespace FramePFX.Editing.Timelines.Clips;

/// <summary>
/// An interface for all types of clip models
/// </summary>
public interface IClip : IHaveEffects, IStrictFrameRange, IAutomatable, ITransferableData, IResourceHolder, IDisplayName {
    /// <summary>
    /// Gets or sets this clip's frame span, that is, a beginning and duration
    /// property contain in a single struct that represents the location and
    /// duration of a clip within a track
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Begin or duration were negative</exception>
    FrameSpan FrameSpan { get; set; }

    /// <summary>
    /// Gets the track that this clip is placed in
    /// </summary>
    Track? Track { get; }

    /// <summary>
    /// An event fired when this clip's <see cref="FrameSpan"/> changed
    /// </summary>
    event ClipSpanChangedEventHandler? FrameSpanChanged;

    /// <summary>
    /// An event fired when this clip's track changes. This may be called when:
    /// </summary>
    event ClipTrackChangedEventHandler? TrackChanged;
}