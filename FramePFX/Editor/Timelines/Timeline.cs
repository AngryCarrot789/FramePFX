using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Rendering;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.Editor.Timelines {
    public delegate void PlayHeadPositionChangedEventHandler(Timeline timeline, long oldPos, long newPos);

    public delegate void TimelineEventHandler(Timeline timeline);

    /// <summary>
    /// A timeline or sequence, which contains a collection of tracks (which contain a collection of clips)
    /// </summary>
    public class Timeline : IProjectBound, IAutomatable {
        private readonly List<Track> tracks;
        private readonly TimelineRenderState renderState;
        private volatile bool isBeginningRender;
        private long nextClipId;
        private long nextTrackId;
        private long playHeadFrame;
        private long maxDuration;
        private long largestFrameInUse;
        private string displayName;

        /// <summary>
        /// The project associated with this timeline. This may change for things like composition timelines
        /// when a resource is added to the resource manager and a project becomes associated with the resource.
        /// </summary>
        public Project Project { get; private set; }

        /// <summary>
        /// The current play head
        /// </summary>
        public long PlayHeadFrame {
            get => this.playHeadFrame;
            set {
                if (this.playHeadFrame == value)
                    return;
                long oldFrame = this.playHeadFrame;
                this.playHeadFrame = value;
                this.PlayHeadPositionChanged?.Invoke(this, oldFrame, value);
            }
        }

        /// <summary>
        /// This timeline's maximum duration
        /// </summary>
        public long MaxDuration {
            get => this.maxDuration;
            set {
                if (this.maxDuration == value)
                    return;
                this.maxDuration = value;
                this.MaxDurationChanged?.Invoke(this);
            }
        }

        public long LargestFrameInUse {
            get => this.largestFrameInUse;
            private set {
                if (this.largestFrameInUse == value)
                    return;
                this.largestFrameInUse = value;
                this.LargestFrameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// A list of tracks that this timeline contains
        /// </summary>
        public IReadOnlyList<Track> Tracks => this.tracks;

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets the number of ticks the previous render took
        /// </summary>
        public long LastRenderDurationTicks { get; private set; }

        /// <summary>
        /// An event fired when this timeline's project has changed
        /// </summary>
        public event ProjectChangedEventHandler ProjectChanged;

        /// <summary>
        /// An event fired when this timeline's play head position changes
        /// </summary>
        public event PlayHeadPositionChangedEventHandler PlayHeadPositionChanged;

        /// <summary>
        /// An event fired when this timeline's display name changes, aka readable name
        /// </summary>
        public event TimelineEventHandler DisplayNameChanged;

        /// <summary>
        /// An event fired when this timeline's maximum duration changes
        /// </summary>
        public event TimelineEventHandler MaxDurationChanged;

        /// <summary>
        /// An event fired when the largest frame changes
        /// </summary>
        public event TimelineEventHandler LargestFrameChanged;

        private readonly TrackEventHandler TrackLargestFrameChangedHandler;

        public Timeline() {
            this.tracks = new List<Track>();
            this.AutomationData = new AutomationData(this);
            this.renderState = new TimelineRenderState(this);
            this.TrackLargestFrameChangedHandler = track => {
                this.UpdateLargestFrame();
            };
        }

        public void UpdateLargestFrame() {
            IReadOnlyList<Track> list = this.Tracks;
            int count = list.Count;
            if (count > 0) {
                long max = list[0].LargestFrameInUse;
                for (int i = 1; i < count; i++) {
                    max = Math.Max(max, list[i].LargestFrameInUse);
                }

                this.LargestFrameInUse = max;
            }
            else {
                this.LargestFrameInUse = 0;
            }
        }

        /// <summary>
        /// Sets the project associated with this timeline. This will notify all tracks, clips and effects of a project change
        /// </summary>
        /// <param name="project">The new project</param>
        public void SetProject(Project project) {
            if (!ReferenceEquals(this.Project, project)) {
                ProjectChangedEventArgs args = new ProjectChangedEventArgs(this.Project, project);
                this.Project = project;
                foreach (Track track in this.Tracks) {
                    Track.OnTimelineProjectChanged(track, args);
                }

                this.ProjectChanged?.Invoke(this, args);
            }
        }

        public FrameSpan GetUsedFrameSpan() {
            return FrameSpan.UnionAll(this.Tracks.SelectMany(x => x.Clips.Select(y => y.FrameSpan)));
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong("NextClipId", this.nextClipId);
            data.SetLong("NextTrackId", this.nextTrackId);
            data.SetLong(nameof(this.PlayHeadFrame), this.PlayHeadFrame);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (Track track in this.tracks) {
                if (!(track.FactoryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Track.FactoryId), registryId);
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
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).Cast<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(Track.FactoryId));
                Track track = TrackFactory.Instance.CreateModel(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.tracks.Count < 1 ? 0 : this.tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public bool TryGetIndexOfTrack(Track track, out int index) {
            return (index = this.IndexOfTrack(track)) != -1;
        }

        public int IndexOfTrack(Track track) {
            List<Track> list = this.tracks;
            for (int i = 0; i < list.Count; i++) {
                if (ReferenceEquals(track, list[i])) {
                    return i;
                }
            }

            return -1;
        }

        // true = adding, false = removing, null = moving track from timeline A to B
        private void ValidateTrack(Track track, bool isAdding) {
            if (isAdding) {
                if (track.Timeline != null) {
                    throw new Exception($"Track already belongs to another timeline: '{track.Timeline.DisplayName}'");
                }

                if (this.IndexOfTrack(track) != -1) {
                    throw new Exception($"Track has no timeline associated but is stored in our track list?");
                }
            }
            else if (track.Timeline != this) {
                throw new Exception($"Expected track's timeline to equal the current instance, but instead {(track.Timeline == null ? "it's null" : $" equals {track.Timeline.DisplayName}")}");
            }
        }

        public void AddTrack(Track track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, Track track) {
            this.ValidateTrack(track, true);
            this.tracks.Insert(index, track);
            Track.SetTimeline(track, this);
            track.LargestFrameChanged += this.TrackLargestFrameChangedHandler;
            this.UpdateLargestFrame();
        }

        public bool RemoveTrack(Track track) {
            if (!this.TryGetIndexOfTrack(track, out int index))
                return false;
            this.RemoveTrackAt(index);
            return true;
        }

        public void RemoveTrackAt(int index) {
            Track track = this.tracks[index];
            this.ValidateTrack(track, false);
            track.LargestFrameChanged -= this.TrackLargestFrameChangedHandler;
            Track.SetTimeline(track, null);
            this.tracks.RemoveAt(index);
            this.UpdateLargestFrame();
        }

        public void MoveTrackUnsafe(int oldIndex, int newIndex) {
            this.tracks.MoveItem(oldIndex, newIndex);
        }

        public void MoveTrackToTimeline(Track track, Timeline timeline) {
            if (!this.TryGetIndexOfTrack(track, out int index))
                throw new Exception("Track is not stored in the current instance");
            this.MoveTrackToTimeline(index, timeline);
        }

        public void MoveTrackToTimeline(int index, Timeline timeline) {
            Track track = this.tracks[index];
            this.ValidateTrack(track, false);
            if (timeline.tracks.Contains(track)) {
                throw new Exception("Target timeline is already storing the track?");
            }

            this.tracks.RemoveAt(index);
            timeline.tracks.Add(track);
            Track.SetTimeline(track, timeline);
        }

        public void ClearTracks() {
            using (ErrorList list = new ErrorList()) {
                for (int i = this.tracks.Count - 1; i >= 0; i--) {
                    try {
                        this.tracks[i].RemoveAllClips();
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }

                    try {
                        this.RemoveTrackAt(i);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
            }
        }

        // I did some testing and found that invoking methods declared with async is generally
        // about 10x slower than regular method invocation, when in debug mode (which uses
        // class-based async state machines, AFAIK)

        // And most rendering is probably fully syncrhronous, meaning async is not required (they
        // can return Task.CompletedTask), which means that async probably won't affect the render
        // performance all that much

        // Anyway these methods are really messy but a lot of it is just avoiding the usage of enumerators
        // in order to re-use local variables... because I like fast apps ;-)
        public async Task RenderAsync(RenderContext render, long frame, CancellationToken token) {
            if (!this.BeginCompositeRender(frame, token))
                throw new TaskCanceledException("Begin render took too long to complete");
            await this.EndCompositeRenderAsync(render, frame, token);
        }

        private void CompleteRenderList(long frame, bool isCancelled, int i = 0) {
            using (ErrorList list = new ErrorList("Failed to " + (isCancelled ? "cancel" : "finalize") + " one or more clip renders")) {
                List<VideoClip> renderList = this.renderState.RenderList;
                for (int count = renderList.Count; i < count; i++) {
                    try {
                        renderList[i].OnRenderCompleted(frame, isCancelled);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }

                this.renderState.Reset();
            }
        }

        private void CompleteRenderList(long frame, bool isCancelled, Exception e, int i = 0) {
            try {
                this.CompleteRenderList(frame, isCancelled, i);
            }
            catch (Exception ex) {
                e.AddSuppressed(ex);
            }
        }

        public bool BeginCompositeRender(long frame, CancellationToken token) {
            if (this.isBeginningRender)
                throw new Exception("Already rendering. Possible asynchronous render");

            // the same composition resource is associated with 2 clips, and
            // those 2 clips are on 2 tracks and intersect the same playhead frame,
            // meaning we are trying to render the same timeline at, most likely,
            // 2 different frame times, which is difficult to managed so.... not allowed for now
            if (this.renderState.IsRendering)
                return false;
            try {
                this.isBeginningRender = true;
                return this.renderState.IsRendering = this.BeginCompositeRenderInternal(frame, token);
            }
            finally {
                this.isBeginningRender = false;
            }
        }

        // TODO: really gotta replace this by rendering each track in their own tasks
        // This is why I added the ZProperty system, so that the state of the clips and tracks
        // could be safely mirrored between "update" and "render" parts

        private bool BeginCompositeRenderInternal(long frame, CancellationToken token) {
            List<AdjustmentVideoClip> adjustments = this.renderState.AdjustmentStack;
            List<VideoClip> renderList = this.renderState.RenderList;
            List<Track> trackList = this.tracks;
            // Render timeline from the bottom to the top
            for (int i = trackList.Count - 1; i >= 0; i--) {
                if (token.IsCancellationRequested) {
                    // BeginRender phase must have taken too long; call cancel to all clips that were prepared
                    this.CompleteRenderList(frame, true);
                    return false;
                }

                if (trackList[i] is VideoTrack track && track.IsActuallyVisible) {
                    Clip clip = track.GetClipAtFrame(frame);
                    if (clip == null || !clip.IsRenderingEnabled) {
                        continue;
                    }

                    if (clip is AdjustmentVideoClip) {
                        adjustments.Add((AdjustmentVideoClip) clip);
                    }
                    else {
                        bool canRender;
                        try {
                            canRender = ((VideoClip) clip).OnBeginRender(frame);
                        }
                        catch (Exception e) {
                            this.CompleteRenderList(frame, true, e);
                            throw new Exception("Failed to invoke " + nameof(VideoClip.OnBeginRender) + " for clip", e);
                        }

                        if (canRender) {
                            renderList.Add((VideoClip) clip);
                        }
                    }
                }
            }

            return true;
        }

        public async Task EndCompositeRenderAsync(RenderContext render, long frame, CancellationToken token) {
            try {
                render.Depth++;
                long a = Time.GetSystemTicks();
                List<VideoClip> renderList = this.renderState.RenderList;
                SKPaint trackPaint = null, clipPaint = null;
                int timelineCanvasSaveIndex = render.Canvas.Save();
                render.Canvas.ClipRect(render.FrameSize.ToRectAsSize(0, 0));
                int i = 0, count = renderList.Count;
                try {
                    this.PreProcessAjustments(frame, render);
                    for (; i < count; i++) {
                        if (token.IsCancellationRequested) {
                            // Rendering took too long. Some clips may have already drawn. Cancel the rest
                            this.CompleteRenderList(frame, true, i);
                            throw new TaskCanceledException();
                        }

                        VideoClip clip = renderList[i];
                        int trackSaveCount = BeginTrackOpacityLayer(render, clip.Track, ref trackPaint);
                        int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);

                        try {
                            BaseEffect.ProcessEffectList(clip.Effects, frame, render, true);
                        }
                        catch (Exception e) {
                            this.CompleteRenderList(frame, true, e, i);
                            throw new RenderException("Failed to pre-process effects", e);
                        }

                        try {
                            // actual render the clip
                            Task task = clip.OnEndRender(render, frame);
                            if (!task.IsCompleted) {
                                // possibly help with performance a tiny bit
                                await task;
                            }
                        }
                        catch (TaskCanceledException) {
                            // do nothing
                        }
                        catch (Exception e) {
                            this.CompleteRenderList(frame, true, e, i);
                            throw new RenderException($"Failed to render '{clip}'", e);
                        }

                        try {
                            BaseEffect.ProcessEffectList(clip.Effects, frame, render, false);
                        }
                        catch (Exception e) {
                            try {
                                clip.OnRenderCompleted(frame, false);
                            }
                            catch (Exception ex) {
                                e.AddSuppressed(new Exception("Failed to finalize clip just after post effect error", ex));
                            }

                            this.CompleteRenderList(frame, true, e, i + 1);
                            throw new RenderException("Failed to post-process effects", e);
                        }

                        try {
                            clip.OnRenderCompleted(frame, false);
                        }
                        catch (Exception e) {
                            this.CompleteRenderList(frame, true, e, i + 1);
                            throw new RenderException($"Failed to call {nameof(clip.OnRenderCompleted)} for '{clip}'", e);
                        }

                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
                catch (TaskCanceledException) {
                    throw;
                }
                catch (RenderException) {
                    throw;
                }
                catch (Exception e) {
                    this.CompleteRenderList(frame, true, e, i + 1);
                    throw new Exception("Unexpected exception occurred during render", e);
                }

                this.PostProcessAjustments(frame, render);
                this.renderState.Reset();
                render.Canvas.RestoreToCount(timelineCanvasSaveIndex);
                this.LastRenderDurationTicks = Time.GetSystemTicks() - a;
            }
            finally {
                this.renderState.IsRendering = false;
                render.Depth--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PreProcessAjustments(long frame, RenderContext render) {
            List<AdjustmentVideoClip> list = this.renderState.AdjustmentStack;
            int count = list.Count;
            if (count == 0)
                return;

            Vector2 size = render.FrameSize;
            try {
                for (int i = 0; i < count; i++) {
                    BaseEffect.ProcessEffectList(list[i].Effects, frame, render, true);
                }
            }
            catch (Exception e) {
                this.CompleteRenderList(frame, true, e);
                throw new Exception("Failed to pre-process adjustment layer effects", e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostProcessAjustments(long frame, RenderContext render) {
            List<AdjustmentVideoClip> list = this.renderState.AdjustmentStack;
            int count = list.Count;
            if (count == 0)
                return;

            Vector2 size = render.FrameSize;
            try {
                for (int i = count - 1; i >= 0; i--) {
                    BaseEffect.ProcessEffectList(list[i].Effects, frame, render, false);
                }
            }
            catch (Exception e) {
                throw new Exception("Failed to post-process adjustment layer effects", e);
            }
        }

        // SaveLayer requires a temporary drawing bitmap, which can slightly
        // decrease performance, so only SaveLayer when absolutely necessary
        private static int SaveLayerForOpacity(SKCanvas canvas, double opacity, ref SKPaint transparency) {
            return canvas.SaveLayer(transparency ?? (transparency = new SKPaint {
                Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(opacity))
            }));
        }

        private static int BeginClipOpacityLayer(RenderContext render, VideoClip clip, ref SKPaint paint) {
            if (clip.UsesCustomOpacityCalculation || Maths.Equals(clip.Opacity, 1d)) {
                return render.Canvas.Save();
            }
            else {
                return SaveLayerForOpacity(render.Canvas, clip.Opacity, ref paint);
            }
        }

        private static int BeginTrackOpacityLayer(RenderContext render, VideoTrack track, ref SKPaint paint) {
            return !Maths.Equals(track.Opacity, 1d)
                // TODO: optimise this, because it adds about 3ms of extra lag per layer with an opacity less than 1
                // (due to bitmap allocation obviously). Not even
                ? SaveLayerForOpacity(render.Canvas, track.Opacity, ref paint)
                : render.Canvas.Save();
        }

        private static void EndOpacityLayer(RenderContext render, int count, ref SKPaint paint) {
            render.Canvas.RestoreToCount(count);
            if (paint != null) {
                paint.Dispose();
                paint = null;
            }
        }

        public override string ToString() {
            return $"{this.GetType().Name} ({this.Tracks.Count.ToString()} tracks, {this.tracks.Sum(x => x.Clips.Count).ToString()} total clips)";
        }
    }
}