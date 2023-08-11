using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.History;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.History {
    public class HistoryClipDrag : IHistoryAction {
        public readonly TimelineViewModel timeline;
        public readonly ClipDragHistoryData[] clips;

        public HistoryClipDrag(TimelineViewModel timeline, ClipDragHistoryData[] clips) {
            this.timeline = timeline;
            this.clips = clips;
        }

        public Task UndoAsync() {
            this.Apply(true);
            return Task.CompletedTask;
        }

        public Task RedoAsync() {
            this.Apply(false);
            return Task.CompletedTask;
        }

        public void OnRemoved() {
        }

        public void Apply(bool original) {
            foreach (ClipDragHistoryData clipData in this.clips) {
                if (!this.timeline.Model.GetTrackById(clipData.track.GetValue(original), out Track track)) {
                    continue;
                }

                if (!this.timeline.Model.GetClipById(clipData.id, out Clip clip)) {
                    continue;
                }

                if (clip.Track != track) {
                    clip.Track.viewModel.RemoveClipFromTrack(clip.viewModel);
                    track.viewModel.AddClip(clip.viewModel);
                }

                FrameSpan old = clip.FrameSpan;
                clip.FrameSpan = clipData.position.GetValue(original);
                clip.viewModel.OnFrameSpanChanged(old);
            }
        }
    }

    public class ClipDragHistoryData {
        public readonly long id;
        public readonly Transaction<long> track;
        public readonly Transaction<FrameSpan> position;

        public ClipDragHistoryData(ClipViewModel clip) {
            this.id = clip.Model.UniqueClipId;
            this.track = Transactions.ForBoth(clip.Track.Model.UniqueTrackId);
            this.position = Transactions.ForBoth(clip.FrameSpan);
        }
    }
}