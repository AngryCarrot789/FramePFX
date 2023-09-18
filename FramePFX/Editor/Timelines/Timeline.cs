using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A timeline or sequence, which contains a collection of tracks (which contain a collection of clips)
    /// </summary>
    public class Timeline : IAutomatable, IRBESerialisable {
        private readonly List<Track> tracks;
        private readonly List<VideoClip> RenderList = new List<VideoClip>();

        // not a chance anyone's creating more than 9 quintillion clips or tracks
        private long nextClipId;
        private long nextTrackId;

        /// <summary>
        /// The project associated with this timeline
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// The current play head
        /// </summary>
        public long PlayHeadFrame { get; set; }

        /// <summary>
        /// This timeline's maximum duration
        /// </summary>
        public long MaxDuration { get; set; }

        /// <summary>
        /// A list of tracks that this timeline contains
        /// </summary>
        public IReadOnlyList<Track> Tracks => this.tracks;

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        public Timeline() {
            this.tracks = new List<Track>();
            this.AutomationData = new AutomationData(this);
        }

        public void UpdateAutomationBackingStorage() {
            this.AutomationData.UpdateBackingStorage();
            foreach (Track track in this.Tracks) {
                track.AutomationData.UpdateBackingStorage();
                foreach (Clip clip in track.Clips) {
                    clip.AutomationData.UpdateBackingStorage();
                }
            }
        }

        public FrameSpan GetUsedFrameSpan() {
            return FrameSpan.UnionAll(this.Tracks.SelectMany(x => x.Clips.Select(y => y.FrameSpan)));
        }

        public void GetClipIndicesAt(long frame, ICollection<int> indices) {
            foreach (Track track in this.tracks) {
                track.GetClipIndicesAt(frame, indices);
            }
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong("NextClipId", this.nextClipId);
            data.SetLong("NextTrackId", this.nextTrackId);
            data.SetLong(nameof(this.PlayHeadFrame), this.PlayHeadFrame);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (Track track in this.tracks) {
                if (!(track.RegistryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Track.RegistryId), registryId);
                track.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            if ((this.nextClipId = data.GetLong("NextClipId")) < 0)
                throw new Exception("Invalid next clip id");
            if ((this.nextTrackId = data.GetLong("NextTrackId")) < 0)
                throw new Exception("Invalid next track id");
            this.ClearTracks();
            this.PlayHeadFrame = data.GetLong(nameof(this.PlayHeadFrame));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            this.AutomationData.UpdateBackingStorage();
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).OfType<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(Track.RegistryId));
                Track track = TrackRegistry.Instance.CreateModel(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.tracks.Count < 1 ? 0 : this.tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public void AddTrack(Track track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, Track track) {
            if (this.tracks.Contains(track))
                throw new Exception("Track already stored in timeline");
            this.tracks.Insert(index, track);
            Track.SetTimeline(track, this);
        }

        public bool RemoveTrack(Track track) {
            int index = this.tracks.IndexOf(track);
            if (index < 0)
                return false;
            this.RemoveTrackAt(index);
            return true;
        }

        public void RemoveTrackAt(int index) {
            Track track = this.tracks[index];
            Debug.Assert(track.Timeline == this, "Expected track's timeline and the current timeline instance to be equal");
            this.tracks.RemoveAt(index);
            Track.SetTimeline(track, null);
        }

        public void MoveTrackIndex(int oldIndex, int newIndex) {
            this.tracks.MoveItem(oldIndex, newIndex);
        }

        public void MoveTrackToTimeline(Track track, Timeline timeline) {
            int index = this.tracks.IndexOf(track);
            if (index == -1)
                throw new Exception("Track is not stored in the current instance");
            this.MoveTrackToTimeline(index, timeline);
        }

        public void MoveTrackToTimeline(int index, Timeline timeline) {
            Track track = this.tracks[index];
            Debug.Assert(track.Timeline == this, "Track is stored in current timeline, but its parent timeline does not match");
            if (timeline.tracks.Contains(track))
                throw new Exception("Target timeline already contains the track");
            this.tracks.RemoveAt(index);
            timeline.tracks.Insert(index, track);
            Track.SetTimeline(track, timeline);
        }

        public void ClearTracks() {
            using (ErrorList list = new ErrorList()) {
                try {
                    for (int i = this.tracks.Count - 1; i >= 0; i--) {
                        this.tracks[i].Clear();
                        this.RemoveTrackAt(i);
                    }
                }
                catch (Exception e) {
                    list.Add(e);
                }
            }
        }

        private static void EndOpacityLayer(RenderContext render, int count, ref SKPaint paint) {
            render.Canvas.RestoreToCount(count);
            if (paint != null) {
                paint.Dispose();
                paint = null;
            }
        }

        // I did some testing and found that awaiting in async is about 10x slower than regular
        // method invocation, when in debug mode (which uses class-based async state machines)

        // Which means that async probably won't affect the render performance all that much, and
        // if anything, stuff like waiting for video decoders to finish would take longer

        // Anyway this method is extremely messy but a lot of it is just avoiding the usage of enumerators
        // in order to re-use local variables... because I like fast apps ;-)
        public async Task RenderAsync(RenderContext render, long frame, CancellationToken token) {
            if (!this.BeginCompositeRender(frame, token))
                throw new TaskCanceledException("Begin render took too long to complete");
            await this.EndCompositeRenderAsync(render, frame, token);
        }

        public void CancelCompositeRenderList(long frame, int i = 0) {
            using (ErrorList list = new ErrorList("Failed to cancel one or more clip renders")) {
                List<VideoClip> renderList = this.RenderList;
                for (int k = renderList.Count; i < k; i++) {
                    try {
                        renderList[i].OnRenderCancelled(frame);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }

                renderList.Clear();
            }
        }

        public bool BeginCompositeRender(long frame, CancellationToken token) {
            List<VideoClip> renderList = this.RenderList;
            List<Track> trackList = this.tracks;
            // Render timeline from the bottom to the top
            for (int i = trackList.Count - 1; i >= 0; i--) {
                if (token.IsCancellationRequested) {
                    // BeginRender phase must have taken too long; call cancel to all clips that were prepared
                    this.CancelCompositeRenderList(frame);
                    return false;
                }

                if (trackList[i] is VideoTrack track && track.IsActuallyVisible) {
                    VideoClip clip = (VideoClip) track.GetClipAtFrame(frame);
                    if (clip == null) {
                        continue;
                    }

                    bool render;
                    try {
                        render = clip.BeginRender(frame);
                    }
                    catch (Exception e) {
                        try {
                            this.CancelCompositeRenderList(frame);
                        }
                        catch (Exception ex) {
                            e.AddSuppressed(ex);
                        }

                        throw new Exception("Failed to invoke " + nameof(clip.BeginRender) + " for clip", e);
                    }

                    if (render) {
                        renderList.Add(clip);
                    }
                }
            }

            return true;
        }

        public async Task EndCompositeRenderAsync(RenderContext render, long frame, CancellationToken token) {
            List<VideoClip> renderList = this.RenderList;
            SKPaint trackPaint = null, clipPaint = null;
            try {
                int j, m;
                for (int i = 0, k = renderList.Count; i < k; i++) {
                    if (token.IsCancellationRequested) {
                        // Rendering took too long. Some clips may have already drawn. Cancel the rest
                        this.CancelCompositeRenderList(i);
                        throw new TaskCanceledException();
                    }

                    VideoClip clip = renderList[i];
                    int trackSaveCount = BeginTrackOpacityLayer(render, (VideoTrack) clip.Track, ref trackPaint);
                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    try {
                        List<BaseEffect> effects = clip.Effects;
                        m = effects.Count;
                        BaseEffect effect;

                        Vector2? frameSize = clip.GetSize();
                        // pre-process clip effects, such as translation, scale, etc.
                        for (j = 0; j < m; j++) {
                            if ((effect = effects[j]) is VideoEffect) {
                                ((VideoEffect) effect).PreProcessFrame(render, frameSize);
                            }
                        }

                        // actual render the clip
                        await clip.EndRender(render, frame);

                        // post process clip, e.g. twirl or whatever
                        for (j = 0; j < m; j++) {
                            if ((effect = effects[j]) is VideoEffect) {
                                ((VideoEffect) effect).PostProcessFrame(render);
                            }
                        }
                    }
                    catch (TaskCanceledException) {
                        // do nothing
                    }
                    catch (Exception e) {
                        try {
                            this.CancelCompositeRenderList(frame, i);
                        }
                        catch (Exception ex) {
                            e.AddSuppressed(ex);
                        }

                        throw new Exception("Failed to render clip", e);
                    }
                    finally {
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
            }
            finally {
                renderList.Clear();
            }
        }

        // SaveLayer requires a temporary drawing bitmap, which can slightly
        // decrease performance, so only SaveLayer when absolutely necessary
        private static int SaveLayerForOpacity(SKCanvas canvas, double opacity, ref SKPaint transparency) {
            return canvas.SaveLayer(transparency ?? (transparency = new SKPaint {Color = new SKColor(255, 255, 255, (byte) Maths.Clamp(opacity * 255F, 0, 255F))}));
        }

        private static int BeginClipOpacityLayer(RenderContext render, VideoClip clip, ref SKPaint paint) {
            if (clip.UseCustomOpacityCalculation || Maths.Equals(clip.Opacity, 1d)) {
                return render.Canvas.Save();
            }
            else {
                return SaveLayerForOpacity(render.Canvas, clip.Opacity, ref paint);
            }
        }

        private static int BeginTrackOpacityLayer(RenderContext render, VideoTrack track, ref SKPaint paint) {
            return !Maths.Equals(track.Opacity, 1d) ? SaveLayerForOpacity(render.Canvas, track.Opacity, ref paint) : render.Canvas.Save();
        }
    }
}