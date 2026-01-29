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
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;

namespace FramePFX.Avalonia.Editor;

/// <summary>
/// Provides constants and conversion functions for converting between pixels, seconds and TimeSpan units
/// </summary>
public static class TimelineUnits {
    /// <summary>
    /// The baseline amount of pixels taken up by a 1-second clip in the timeline with a zoom factor of 1
    /// </summary>
    public const double PixelsPerSecond = 60.0;

    /// <summary>
    /// The height of a clip's header
    /// </summary>
    public const double ClipHeaderSize = TrackViewState.MinTrackHeight;

    /// <summary>
    /// Gets a ratio for converting <see cref="zoomAmount"/> units into pixels
    /// </summary>
    /// <param name="zoomAmount">An additional zoom multiplier</param>
    /// <returns>A ratio to be multiplied by <see cref="TimeSpan"/> to produce a pixel value</returns>
    public static double GetPixelsPerTickRatio(double zoomAmount) {
        return (PixelsPerSecond * zoomAmount) / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Gets a ratio for converting pixels into <see cref="TimeSpan"/> units
    /// </summary>
    /// <param name="zoomAmount">An additional zoom multiplier</param>
    /// <returns>A ratio to be multiplied by pixel values to produce a value suitable for <see cref="TimeSpan"/></returns>
    public static double GetTicksPerPixelRatio(double zoomAmount) {
        return TimeSpan.TicksPerSecond / (PixelsPerSecond * zoomAmount);
    }

    public static double TicksToPixels(long ticks, double zoomAmount) {
        return ticks * GetPixelsPerTickRatio(zoomAmount);
    }

    public static long PixelsToTicks(double pixels, double zoomAmount) {
        return (long) (pixels * GetTicksPerPixelRatio(zoomAmount));
    }

    public static ClipSpan GetVisibleRegion(TimeSpan horizontalScroll, double width, double zoomAmount) {
        return ClipSpan.FromDuration(horizontalScroll, (long) (width / GetPixelsPerTickRatio(zoomAmount)));
    }

    public static ClipSpan GetVisibleRegion(double x, double width, double zoomAmount) {
        double ratio = GetPixelsPerTickRatio(zoomAmount);
        long offset = PixelsToTicks(x, zoomAmount);
        return ClipSpan.FromDuration(offset, (long) (width / ratio));
    }

    public static (double X, double Width) GetClipRect(Clip clip, TimeSpan scrollOffset, double zoomAmount) {
        double pixelsPerTick = GetPixelsPerTickRatio(zoomAmount);
        return ((clip.Span.Start.Ticks - scrollOffset.Ticks) * pixelsPerTick, clip.Span.Duration.Ticks * pixelsPerTick);
    }
}