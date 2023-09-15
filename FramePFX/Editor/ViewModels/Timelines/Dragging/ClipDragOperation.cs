using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.History;
using FramePFX.History.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.Dragging {
    public class ClipDragData {
        public readonly TimelineViewModel timeline;
        public readonly List<ClipDragInfo> clips;
        public HistoryClipDrag history;

        public HistoryManagerViewModel HistoryManager => this.timeline.Project.Editor.HistoryManager;

        public ClipDragData(TimelineViewModel timeline, IReadOnlyCollection<ClipViewModel> clips) {
            this.clips = clips.Select(x => new ClipDragInfo(x)).ToList();
            this.timeline = timeline;
        }

        public void OnBegin() {
            if (this.history != null) {
                throw new Exception("This instance cannot be used when already finished finished");
            }

            this.history = new HistoryClipDrag(this.timeline, this.clips.Select(x => x.history).ToArray());
        }

        public virtual void OnFinished(bool cancelled) {
            if (cancelled) {
                this.history.Apply(true);
            }
            else {
                this.HistoryManager.AddAction(this.history);
            }
        }

        public virtual void OnLeftThumbDelta(long offset) {
            if (offset == 0) {
                return;
            }

            foreach (ClipDragInfo handle in this.clips) {
                long newFrameBegin = handle.clip.FrameBegin + offset;
                if (newFrameBegin < 0) {
                    offset += -newFrameBegin;
                    newFrameBegin = 0;
                }

                long duration = handle.clip.FrameDuration - offset;
                if (duration < 1) {
                    newFrameBegin += (duration - 1);
                    duration = 1;
                    if (newFrameBegin < 0) {
                        continue;
                    }
                }

                handle.clip.MediaFrameOffset += (newFrameBegin - handle.clip.FrameBegin);
                handle.clip.FrameSpan = new FrameSpan(newFrameBegin, duration);
                handle.history.position.SetCurrent(handle.clip.FrameSpan);
            }
        }

        public virtual void OnRightThumbDelta(long offset) {
            if (offset == 0) {
                return;
            }

            foreach (ClipDragInfo handle in this.clips) {
                FrameSpan span = handle.clip.FrameSpan;
                long newEndIndex = Math.Max(span.EndIndex + offset, span.Begin + 1);
                if (newEndIndex > this.timeline.MaxDuration) {
                    this.timeline.MaxDuration = newEndIndex + 300;
                }

                handle.clip.FrameSpan = span.WithEndIndex(newEndIndex);
                handle.history.position.SetCurrent(handle.clip.FrameSpan);
            }
        }

        public virtual void OnDragDelta(long offset) {
            if (offset == 0) {
                return;
            }

            foreach (ClipDragInfo handle in this.clips) {
                FrameSpan span = handle.clip.FrameSpan;
                long begin = (span.Begin + offset) - handle.accumulator;
                handle.accumulator = 0L;
                if (begin < 0) {
                    handle.accumulator = -begin;
                    begin = 0;
                }

                long endIndex = begin + span.Duration;
                if (this.timeline != null) {
                    if (endIndex > this.timeline.MaxDuration) {
                        this.timeline.MaxDuration = endIndex + 300;
                    }
                }

                handle.clip.FrameSpan = new FrameSpan(begin, span.Duration);
                handle.history.position.SetCurrent(handle.clip.FrameSpan);
            }
        }

        public virtual void OnDragToTrack(int index) {
            TrackViewModel track = this.clips[0].clip.Track;
            int target = Maths.Clamp(index, 0, this.timeline.Tracks.Count - 1);
            TrackViewModel targetTrack = this.timeline.Tracks[target];
            if (targetTrack == track || !targetTrack.IsClipTypeAcceptable(this.clips[0].clip)) {
                return;
            }

            if (this.clips.Count != 1 && this.clips.Any(x => x.clip.Track != track)) {
                return;
            }

            for (int i = this.clips.Count - 1; i >= 0; i--) {
                ClipDragInfo info = this.clips[i];
                info.history.track.SetCurrent(targetTrack.Model.UniqueTrackId);
                this.timeline.MoveClip(info.clip, track, targetTrack);
            }
        }
    }
}