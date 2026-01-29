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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Avalonia.Editor;

/// <summary>
/// The control responsible for rendering the play head(s)
/// </summary>
public sealed class PlayHeadRenderSurface : Control {
    public static readonly StyledProperty<TimelineViewState?> TimelineProperty = AvaloniaProperty.Register<PlayHeadRenderSurface, TimelineViewState?>(nameof(Timeline));
    public static readonly StyledProperty<IBrush?> PlayHeadBrushProperty = AvaloniaProperty.Register<PlayHeadRenderSurface, IBrush?>(nameof(PlayHeadBrush));

    public TimelineViewState? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public IBrush? PlayHeadBrush {
        get => this.GetValue(PlayHeadBrushProperty);
        set => this.SetValue(PlayHeadBrushProperty, value);
    }

    private IPen? cachedPen;

    public PlayHeadRenderSurface() {
        this.IsHitTestVisible = false;
    }

    static PlayHeadRenderSurface() {
        TimelineProperty.Changed.AddClassHandler<PlayHeadRenderSurface, TimelineViewState?>((s, e) => s.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        AffectsRender<PlayHeadRenderSurface>(TimelineProperty, PlayHeadBrushProperty);
    }

    public override void Render(DrawingContext context) {
        base.Render(context);
        TimelineViewState? tvs = this.Timeline;
        if (tvs == null) {
            return;
        }

        Rect bounds = this.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) {
            return;
        }

        double x = (tvs.Timeline.PlayHeadLocation.Ticks - tvs.HorizontalScroll.Ticks) * TimelineUnits.GetPixelsPerTickRatio(tvs.Zoom);
        if (x < 0 || x > bounds.Width) {
            return;
        }

        using (context.PushRenderOptions(new RenderOptions { EdgeMode = EdgeMode.Aliased })) {
            PenUtils.TryModifyOrCreate(ref this.cachedPen, this.PlayHeadBrush ?? Brushes.White, 1.0);
            context.DrawLine(
                this.cachedPen!,
                new Point(x, 0),
                new Point(x, bounds.Height)
            );
        }
    }


    private void OnTimelineChanged(TimelineViewState? oldTimeline, TimelineViewState? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.Timeline.PlayHeadLocationChanged -= this.TimelineOnPlayHeadLocationChanged;
            oldTimeline.ZoomChanged -= this.TimelineOnZoomChanged;
            oldTimeline.HorizontalScrollChanged -= this.TimelineOnHorizontalScrollChanged;
        }

        if (newTimeline != null) {
            newTimeline.Timeline.PlayHeadLocationChanged += this.TimelineOnPlayHeadLocationChanged;
            newTimeline.ZoomChanged += this.TimelineOnZoomChanged;
            newTimeline.HorizontalScrollChanged += this.TimelineOnHorizontalScrollChanged;
        }
    }

    private void TimelineOnPlayHeadLocationChanged(object? sender, ValueChangedEventArgs<TimeSpan> e) {
        this.InvalidateVisual();
    }

    private void TimelineOnZoomChanged(object? sender, ValueChangedEventArgs<double> e) {
        this.InvalidateVisual();
    }

    private void TimelineOnHorizontalScrollChanged(object? sender, ValueChangedEventArgs<TimeSpan> e) {
        this.InvalidateVisual();
    }
}