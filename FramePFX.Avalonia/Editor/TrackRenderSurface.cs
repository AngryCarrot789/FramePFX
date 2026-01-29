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
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils.Destroying;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

/// <summary>
/// A control which handles rendering of a single track's clips
/// </summary>
public sealed class TrackRenderSurface : Control, ITrackRenderSurface, ITrackControl {
    private static readonly DynamicAvaloniaColourBrush s_ClipBackgroundBrush = (DynamicAvaloniaColourBrush) BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone3.Background.Static");
    private static readonly DynamicAvaloniaColourBrush s_ClipHeaderForegroundBrush = (DynamicAvaloniaColourBrush) BrushManager.Instance.GetDynamicThemeBrush("ABrush.Foreground.Static");
    private static readonly List<Clip> s_TmpRenderList = new List<Clip>(1024);

    /// <summary>
    /// </summary>
    /// <returns>True to continue scanning, False to stop looping</returns>
    public delegate bool AcceptClipHitTest(Clip clip, object? state);

    // Maps clips to the visual clip info. Lazily generated, in case of erratic clip adds/removes
    private readonly ModelControlMap<Clip, ClipControl> visualClipInfo;
    private TrackViewState? viewState;
    private IDisposable? myClipBackgroundSubscription;
    private IDisposable? myClipHeaderForegroundSubscription;
    private ImmutablePen? defaultClipPen, selectedClipPen;

    public TrackViewState? Track {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnTrackChanged(o, n));
    }

    TrackViewState ITrackRenderSurface.Track => this.Track ?? throw new InvalidOperationException($"Attempt to use {nameof(ITrackRenderSurface)} incorrectly");

    TrackViewState ITrackControl.Track => this.Track ?? throw new InvalidOperationException($"Attempt to use {nameof(ITrackControl)} incorrectly");
    
    public IBrush ClipBackground { get; private set; } = Brushes.Transparent;
    public IBrush ClipHeaderForeground { get; private set; } = Brushes.Black;
    public IBrush ClipHeaderBackground { get; private set; } = Brushes.Transparent;

    public TimelineTrackControl OwnerTrack { get; internal set; } = null!;

    public double ViewportWidth => this.OwnerTrack.Bounds.Width;

    public IPen GetClipBorderPen(Clip clip) {
        TrackViewState? track = this.Track;
        if (track != null && track.SelectedClips.IsSelected(clip)) {
            return this.selectedClipPen ??= new ImmutablePen(Brushes.YellowGreen, 2.0);
        }

        return this.defaultClipPen ??= new ImmutablePen(Brushes.DodgerBlue, 1.0);
    }

    public TrackRenderSurface() {
        this.visualClipInfo = new ModelControlMap<Clip, ClipControl>();
        this.UseLayoutRounding = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.myClipBackgroundSubscription = s_ClipBackgroundBrush.Subscribe(static (b, s) => {
            TrackRenderSurface trs = ((TrackRenderSurface) s!);
            trs.InvalidateVisual();
            trs.ClipBackground = b.CurrentBrush ?? Brushes.Transparent;
        }, this);

        this.myClipHeaderForegroundSubscription = s_ClipHeaderForegroundBrush.Subscribe(static (b, s) => {
            TrackRenderSurface trs = ((TrackRenderSurface) s!);
            trs.InvalidateVisual();
            trs.ClipHeaderForeground = b.CurrentBrush ?? Brushes.Transparent;
        }, this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        DisposableUtils.Dispose(ref this.myClipBackgroundSubscription);
        DisposableUtils.Dispose(ref this.myClipHeaderForegroundSubscription);
    }

    private void OnTrackChanged(TrackViewState? oldTrack, TrackViewState? newTrack) {
        this.InvalidateVisual();
        if (oldTrack != null) {
            this.viewState = null;
            oldTrack.Track.ClipRemoved -= this.TrackOnClipRemoved;
            oldTrack.Track.ColourChanged -= this.TrackOnColourChanged;
        }

        if (newTrack != null) {
            this.viewState = newTrack;
            newTrack.Track.ClipRemoved += this.TrackOnClipRemoved;
            newTrack.Track.ColourChanged += this.TrackOnColourChanged;
            this.OnTrackColourChanged(newTrack.Track);
        }
    }

    private void TrackOnColourChanged(object? sender, EventArgs e) {
        this.OnTrackColourChanged((Track) sender!);
    }

    private void OnTrackColourChanged(Track track) {
        this.ClipHeaderBackground = new ImmutableSolidColorBrush((uint) track.Colour);
        this.InvalidateVisual();
    }

    public override void Render(DrawingContext context) {
        base.Render(context);

        TrackViewState? track = this.Track;
        Timeline? timeline;
        if (track == null || (timeline = track.Track.Timeline) == null) {
            return;
        }

        TimelineViewState tvs = TimelineViewState.GetInstance(timeline, track.TopLevelIdentifier);
        double zoomRatio = TimelineUnits.GetPixelsPerTickRatio(tvs.Zoom);

        Rect trackBounds = this.Bounds;
        s_TmpRenderList.Clear();
        track.Track.GetClipsInRange(s_TmpRenderList, ClipSpan.FromDuration(tvs.HorizontalScroll, (long) (trackBounds.Width / zoomRatio)));
        foreach (Clip clip in s_TmpRenderList) {
            double clipWidth = clip.Span.Duration.Ticks * zoomRatio;
            if (clipWidth <= 1.0) {
                // Don't render clips that are too small to even be useful
                continue;
            }

            double clipX = (clip.Span.Start.Ticks - tvs.HorizontalScroll.Ticks) * zoomRatio;
            using (context.PushTransform(Matrix.CreateTranslation(clipX, 0))) {
                this.GetVisualClipInfo(clip).Render(this, context, new Size(clipWidth, trackBounds.Height), tvs);
            }
        }

        s_TmpRenderList.Clear();
    }

    internal ClipControl GetVisualClipInfo(Clip clip) {
        if (!this.visualClipInfo.TryGetControl(clip, out ClipControl? info))
            this.visualClipInfo.AddMapping(clip, info = ClipControl.CreateFor(this, clip));

        return info;
    }

    internal bool TryGetVisualClipInfo(Clip clip, [NotNullWhen(true)] out ClipControl? info) {
        return this.visualClipInfo.TryGetControl(clip, out info);
    }

    /// <summary>
    /// Tries to get the first clip that intersects the point
    /// </summary>
    /// <param name="point">The point</param>
    /// <param name="hitClip">The clip</param>
    /// <returns></returns>
    public bool TryGetHitClip(Point point, [NotNullWhen(true)] out Clip? hitClip) {
        Clip?[] array = new Clip?[1];
        this.HitTestClips(point, (clip, state) => {
            ((Clip?[]) state!)[0] = clip;
            return false;
        }, array);

        return (hitClip = array[0]) != null;
    }

    public int HitTestClips(Point point, AcceptClipHitTest accept, object? state) {
        int i = 0;
        TrackViewState? track = this.Track;
        Timeline? timeline;
        if (track != null && (timeline = track.Track.Timeline) != null) {
            TimelineViewState tvs = TimelineViewState.GetInstance(timeline, track.TopLevelIdentifier);
            double pixelsPerTick = TimelineUnits.GetPixelsPerTickRatio(tvs.Zoom);
            Rect trackBounds = this.Bounds;
            List<Clip> clips = new List<Clip>(32);
            track.Track.GetClipsInRange(clips, ClipSpan.FromDuration(tvs.HorizontalScroll, (long) (trackBounds.Width / pixelsPerTick)));

            for (; i < clips.Count; i++) {
                Clip clip = clips[i];
                Rect rect = new Rect((clip.Span.Start.Ticks - tvs.HorizontalScroll.Ticks) * pixelsPerTick, 0, clip.Span.Duration.Ticks * pixelsPerTick, trackBounds.Height);
                if (rect.Contains(point)) {
                    bool continueLoop = accept(clip, state);
                    if (!continueLoop) {
                        return i;
                    }
                }
            }
        }

        return i;
    }

    private void TrackOnClipRemoved(object? sender, ClipEventArgs e) {
        if (this.visualClipInfo.TryGetControl(e.Clip, out ClipControl? info)) {
            info.Dispose();

            this.visualClipInfo.RemoveMapping(e.Clip, info);
        }
    }

    public void OnClipSelectionChanged(IList<Clip> removedClips, IList<Clip> addedClips) {
        // TODO: only redraw if a visible clip becomes selected or deselected
        // bool invalidate = false;
        // foreach (Clip clip in removedClips) {
        //     if (this.TryGetVisualClipInfo(clip, out ClipControl? info) && info.IsVisible()) {
        //         invalidate = true;
        //     }
        // }

        this.InvalidateVisual();
    }

    public void InvalidateRender() {
        this.InvalidateVisual();
    }
}