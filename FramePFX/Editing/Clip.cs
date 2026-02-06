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

using FramePFX.Editing.Video;
using PFXToolKitUI.Composition;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Editing;

/// <summary>
/// The base class for a clip within a track
/// </summary>
public abstract class Clip : IComponentManager, ITransferableData {
    public ComponentStorage ComponentStorage => field ??= new ComponentStorage(this);

    /// <summary>
    /// Gets or sets a short description of the clip, displayed in the clip's header
    /// </summary>
    public string? DisplayName {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DisplayNameChanged);
    }

    /// <summary>
    /// Gets or sets the media offset of this clip. This is used to fix a clip's true starting point when it is resized using the left grip.
    /// </summary>
    public long MediaOffset {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => {
            t.OnMediaOffsetChanged(o, n);
            if (t.Track is VideoTrack videoTrack) {
                videoTrack.RaiseRenderInvalidated(t.Span);
            }
        });
    }

    /// <summary>
    /// Gets or sets the location of this clip within the track, in ticks. There are 10 million ticks in one second.
    /// </summary>
    public ClipSpan Span {
        get => field;
        set {
            PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => {
                t.Track?.InternalOnClipSpanChanged(t, o, n);
                t.SpanChanged?.Invoke(t, new ValueChangedEventArgs<ClipSpan>(o, n));
                if (t.Track is VideoTrack videoTrack) {
                    videoTrack.Timeline?.RaiseRenderInvalidated(videoTrack, ClipSpan.Union(o, n));
                }
            });
        }
    }

    /// <summary>
    /// Gets the track that this clip resides in.
    /// </summary>
    public Track? Track {
        get => field;
        internal set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.TrackChanged);
    }

    /// <summary>
    /// Gets the type of this clip
    /// </summary>
    public ClipType ClipType => this.InternalClipType;

    internal abstract ClipType InternalClipType { get; }

    public TransferableData TransferableData => field ??= new TransferableData(this);

    public event EventHandler? DisplayNameChanged;
    public event EventHandler<ValueChangedEventArgs<long>>? MediaOffsetChanged;
    public event EventHandler<ValueChangedEventArgs<ClipSpan>>? SpanChanged;
    public event EventHandler<ValueChangedEventArgs<Track?>>? TrackChanged;

    internal Clip() {
    }

    public bool IsPointInRange(long timelineLocation) {
        return this.Span.IntersectedBy(timelineLocation);
    }
    
    protected virtual void OnMediaOffsetChanged(long oldOffset, long newOffset) {
        this.MediaOffsetChanged?.Invoke(this, new ValueChangedEventArgs<long>(oldOffset, newOffset));
    }
}