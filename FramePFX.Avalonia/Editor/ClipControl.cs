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
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using FramePFX.Editing;
using FramePFX.Editing.Audio;
using FramePFX.Editing.Scratch;
using FramePFX.Editing.Video;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

/// <summary>
/// Contains information about a clip's UI state
/// </summary>
public abstract class ClipControl {
    private static readonly Dictionary<Type, Func<ITrackControl, Clip, ClipControl>> s_RegisteredClips;
    private static readonly Type s_ClipType = typeof(Clip);

    private bool isDisposed;
    private string? myCachedFormattedTextRaw;
    private FormattedText? myCachedFormattedText;

    /// <summary>
    /// Gets the track associated with this clip control
    /// </summary>
    public ITrackControl Track { get; }

    /// <summary>
    /// Gets the clip associated with this clip control
    /// </summary>
    public Clip Clip { get; }

    protected ClipControl(ITrackControl track, Clip clip) {
        ArgumentNullException.ThrowIfNull(track);
        ArgumentNullException.ThrowIfNull(clip);
        this.Track = track;
        this.Clip = clip;
    }

    static ClipControl() {
        s_RegisteredClips = new Dictionary<Type, Func<ITrackControl, Clip, ClipControl>>();
    }

    public static void RegisterClip<TClip>(Func<ITrackControl, TClip, ClipControl> factory) where TClip : Clip {
        s_RegisteredClips[typeof(TClip)] = (t, c) => factory(t, (TClip) c);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ClipControl"/> for the given clip. If no custom type is
    /// registered for the type of clip, either a base type will be used or the default type will be used
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ClipControl CreateFor(ITrackControl track, Clip clip) {
        ClipControl control;
        for (Type? type = clip.GetType(); type != s_ClipType; type = type!.BaseType) {
            if (s_RegisteredClips.TryGetValue(type!, out Func<ITrackControl, Clip, ClipControl>? factory)) {
                control = factory(track, clip);
                goto ret;
            }
        }

        switch (clip) {
            case VideoClip videoClip:     control = new DefaultVideoClipControl(track, videoClip); break;
            case AudioClip audioClip:     control = new DefaultAudioClipControl(track, audioClip); break;
            case ScratchClip scratchClip: control = new DefaultScratchClipControl(track, scratchClip); break;
            default:                      throw new ArgumentException($"Unknown clip type: {clip.GetType()} ({clip.ClipType})");
        }

        ret:
        control.RegisterEvents();
        return control;
    }
    
    protected virtual void RegisterEvents() {
        this.Clip.DisplayNameChanged += this.ClipOnDisplayNameChanged;
        this.Clip.SpanChanged += this.ClipOnSpanChanged;
    }

    protected virtual void UnregisterEvents() {
        this.Clip.DisplayNameChanged -= this.ClipOnDisplayNameChanged;
        this.Clip.SpanChanged -= this.ClipOnSpanChanged;
    }
    
    public bool IsVisible() {
        TimelineControl timeline = this.Track.OwnerTrack.OwnerPanel!.TimelineControl;
        ClipSpan visibleSpan = TimelineUnits.GetVisibleRegion(timeline.HorizontalScroll, this.Track.ViewportWidth, timeline.Zoom);
        return visibleSpan.IntersectedBy(this.Clip.Span);
    }

    internal void Render(ITrackRenderSurface surface, DrawingContext context, Size visualSize, TimelineViewState timeline) {
        this.ValidateNotDisposed();

        using (context.PushRenderOptions(new RenderOptions() { EdgeMode = EdgeMode.Aliased })) {
            IPen pen = surface.GetClipBorderPen(this.Clip);
            const double BT = 1.0;
            double bt = pen.Thickness; // includes selected border thickness

            // draw background
            DrawRectangle(context, surface.ClipBackground, pen, new Rect(default, visualSize));

            // draw clip's header
            DrawRectangle(context, surface.ClipHeaderBackground, pen, new Rect(0, 0, visualSize.Width, TimelineUnits.ClipHeaderSize));

            FormattedText? formattedText = this.GetOrCreateFormattedText(surface);
            if (formattedText != null) {
                using (context.PushClip(new Rect(BT, BT, visualSize.Width - BT - BT, TimelineUnits.ClipHeaderSize - BT))) {
                    context.DrawText(formattedText, new Point(BT, BT));
                }
            }

            // Only do custom render when there's enough pixels visible
            Rect customBounds = new Rect(new Point(BT, TimelineUnits.ClipHeaderSize), new Point(visualSize.Width - BT, visualSize.Height - BT));
            if (customBounds.Height > 0.5 && customBounds.Width > 0.5) {
                // draw custom stuff
                using (context.PushClip(new Rect(default, visualSize))) {
                    this.OnRenderCustom(context, visualSize, customBounds, timeline.Zoom);
                }
            }
        }
    }

    /// <summary>
    /// Custom renderer for the clip
    /// </summary>
    /// <param name="context">The drawing context</param>
    /// <param name="clipSize">The size of the clip including border and header, in pixels</param>
    /// <param name="customBounds">
    ///     The bounds of the custom drawing area, positioned relative to the top-left corner of the clip. Push a transform
    ///     translation with <see cref="Rect.Position"/> and clip by <see cref="Rect.Size"/> if absolutely necessary
    /// </param>
    /// <param name="zoomed">The timeline zoom factor</param>
    protected virtual void OnRenderCustom(DrawingContext context, Size clipSize, Rect customBounds, double zoomed) {
        
    }

    public void Dispose() {
        if (!this.isDisposed) {
            this.isDisposed = true;
            try {
                this.UnregisterEvents();
            }
            finally {
                this.OnDisposed();
            }
        }
    }

    /// <summary>
    /// Dispose of any rendering data before this object is no longer used.
    /// <para>
    /// This happens when a clip is removed from a track (clip is deleted, moved to another track, etc.)
    /// </para>
    /// </summary>
    protected abstract void OnDisposed();

    private void ValidateNotDisposed() {
        ObjectDisposedException.ThrowIf(this.isDisposed, $"Cannot access a disposed {nameof(ClipControl)}");
    }

    private FormattedText? GetOrCreateFormattedText(ITrackRenderSurface surface) {
        string? text = this.Clip.DisplayName;
        if (this.myCachedFormattedTextRaw != text) {
            this.myCachedFormattedText = null;
            this.myCachedFormattedTextRaw = null;
        }

        if (string.IsNullOrWhiteSpace(text)) {
            return null;
        }

        if (this.myCachedFormattedText == null) {
            this.myCachedFormattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface.Default, 12.0, surface.ClipHeaderForeground);
            this.myCachedFormattedTextRaw = text;
        }

        return this.myCachedFormattedText;
    }

    private void ClipOnDisplayNameChanged(object? sender, EventArgs e) {
        this.Track.InvalidateRender();
    }

    private void ClipOnSpanChanged(object? sender, ValueChangedEventArgs<ClipSpan> e) {
        this.Track.InvalidateRender();
    }

    /// <summary>
    /// A helper method for drawing a bordered rectangle
    /// </summary>
    protected static void DrawRectangle(DrawingContext context, IBrush? brush, IPen? pen, Rect rect, in CornerRadius radius = default) {
        double d;
        if (pen != null && !Maths.IsZero(d = pen.Thickness)) {
            rect = rect.Deflate(d * 0.5);
        }

        context.DrawRectangle(brush, pen, new RoundedRect(in rect, in radius));
    }
}

public class DefaultVideoClipControl(ITrackControl track, VideoClip clip) : ClipControl(track, clip) {
    // This class does not do any custom drawing. The pre-renderer handles the background
    // of the clip so we don't need to draw that ourselves

    protected override void OnDisposed() {
    }
}

public class DefaultAudioClipControl(ITrackControl track, AudioClip clip) : ClipControl(track, clip) {
    // This class will (soon) render the audio waveform of the clip

    protected override void OnRenderCustom(DrawingContext context, Size clipSize, Rect customBounds, double zoomed) {
        base.OnRenderCustom(context, clipSize, customBounds, zoomed);
    }

    protected override void OnDisposed() {
    }
}

public class DefaultScratchClipControl(ITrackControl track, ScratchClip clip) : ClipControl(track, clip) {
    // This class does nothing special at all. Scratch clips are just fake clips that do nothing but plugin things

    protected override void OnDisposed() {
    }
}