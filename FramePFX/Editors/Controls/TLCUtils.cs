using System;
using FramePFX.Editors.Controls.Timelines;
using FramePFX.Editors.Controls.Timelines.Tracks.Clips;
using Mouse = System.Windows.Input.Mouse;

namespace FramePFX.Editors.Controls {
    /// <summary>
    /// Timeline Control Utils, provides functions like getting frame from cursor relative to clip
    /// </summary>
    public static class TLCUtils {
        /// <summary>
        /// Gets the frame from the given clip, accounting for timeline zoom
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="useRounding">Round the frame if the cursor sits somewhere in between two frames</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// The clip does not have a timeline sequence control associated with
        /// it, meaning it is not placed in a valid timeline
        /// </exception>
        public static long GetCursorFrame(TimelineClipControl clip, bool useRounding = true) {
            TrackStoragePanel timeline = clip.Track?.OwnerPanel;
            if (timeline == null) {
                throw new Exception("Clip does not have a timeline sequence associated with it");
            }

            double cursor = Mouse.GetPosition(timeline).X;
            return TimelineUtils.PixelToFrame(cursor, timeline.Timeline?.Zoom ?? 1.0, useRounding);
        }
    }
}