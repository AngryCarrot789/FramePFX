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

using PFXToolKitUI.Utils.Events;

namespace FramePFX.Editing.Video;

public sealed class VideoTrack : Track {
    /// <summary>
    /// Returns <see cref="ClipType.Video"/>
    /// </summary>
    public override ClipType AcceptedClipType => ClipType.Video;

    public double Opacity {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static t => {
            t.OpacityChanged?.Invoke(t, EventArgs.Empty);
            t.RaiseRenderInvalidated();
        });
    }

    public event EventHandler? OpacityChanged;
    
    /// <summary>
    /// An event fired when the render state of this video track becomes invalid, such as from <see cref="Opacity"/> changing
    /// </summary>
    public event EventHandler<VideoTrackRenderInvalidatedEventArgs>? RenderInvalidated;
    
    public VideoTrack() {
    }

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event spanning <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public void RaiseRenderInvalidated() => this.RaiseRenderInvalidated(ClipSpan.MaxValue);

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event
    /// </summary>
    /// <param name="span">The span that was invalidated</param>
    public void RaiseRenderInvalidated(ClipSpan span) {
        this.RenderInvalidated?.Invoke(this, new VideoTrackRenderInvalidatedEventArgs(span));
        this.Timeline?.RaiseRenderInvalidated(this, span);
    }
}

public readonly struct VideoTrackRenderInvalidatedEventArgs(ClipSpan span) {
    /// <summary>
    /// Gets the invalidated range. May be <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public ClipSpan Span { get; } = span;
}