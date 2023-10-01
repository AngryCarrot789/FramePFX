using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.Dragging
{
    /// <summary>
    /// A drag operation on one or more clips
    /// </summary>
    public sealed class ClipDragOperation
    {
        public readonly TimelineViewModel timeline; // timeline associated
        public readonly List<ClipDragHandleInfo> clips; // all clips including original clip
        public readonly ClipDragHandleInfo source;

        private ClipDragOperation(TimelineViewModel timeline, ClipViewModel source, IEnumerable<ClipViewModel> selectionWithoutSource)
        {
            this.timeline = timeline;
            this.source = new ClipDragHandleInfo(this, source);
            this.clips = new List<ClipDragHandleInfo> {this.source};
            this.clips.AddRange(selectionWithoutSource.Select(x => new ClipDragHandleInfo(this, x)));
        }

        public static ClipDragOperation ForClip(ClipViewModel source)
        {
            TimelineViewModel timeline = source.Timeline ?? throw new Exception("No timeline available from clip");
            List<ClipViewModel> clips = new List<ClipViewModel>();
            foreach (TrackViewModel track in timeline.Tracks)
            {
                clips.AddRange(track.SelectedClips);
            }

            int index = clips.IndexOf(source);
            if (index >= 0)
                clips.RemoveAt(index);
            return new ClipDragOperation(timeline, source, clips);
        }

        public void OnBegin()
        {
        }

        public void OnFinished(bool cancelled)
        {
        }

        public void OnDragToTrack(int index)
        {
            TrackViewModel track = this.clips[0].clip.Track;
            int target = Maths.Clamp(index, 0, this.timeline.Tracks.Count - 1);
            TrackViewModel targetTrack = this.timeline.Tracks[target];
            if (targetTrack == track || !targetTrack.IsClipTypeAcceptable(this.clips[0].clip))
            {
                return;
            }

            if (this.clips.Count != 1 && this.clips.Any(x => x.clip.Track != track))
            {
                return;
            }

            for (int i = this.clips.Count - 1; i >= 0; i--)
            {
                track.MoveClipToTrack(this.clips[i].clip, targetTrack);
            }
        }
    }
}