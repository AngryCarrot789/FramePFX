using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines {
    public class Timeline : IAutomatable, IRBESerialisable {
        /// <summary>
        /// The project associated with this timeline
        /// </summary>
        public Project Project { get; }

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
        public List<Track> Tracks { get; }

        public AutomationData AutomationData { get; }

        public AutomationEngine AutomationEngine => this.Project.AutomationEngine;

        public bool IsAutomationChangeInProgress { get; set; }

        // view model can access this
        public Dictionary<Clip, Exception> ExceptionsLastRender { get; } = new Dictionary<Clip, Exception>();

        public Timeline(Project project) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Tracks = new List<Track>();
            this.AutomationData = new AutomationData(this);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong(nameof(this.PlayHeadFrame), this.PlayHeadFrame);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (Track track in this.Tracks) {
                if (!(track.RegistryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Track.RegistryId), registryId);
                track.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.PlayHeadFrame = data.GetLong(nameof(this.PlayHeadFrame));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).OfType<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(Track.RegistryId));
                Track track = TrackRegistry.Instance.CreateModel(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.Tracks.Count < 1 ? 0 : this.Tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public void AddTrack(Track track) {
            this.Tracks.Add(track);
            Track.SetTimeline(track, this);
        }

        public bool RemoveTrack(Track track) {
            int index = this.Tracks.IndexOf(track);
            if (index < 0) {
                return false;
            }

            this.RemoveTrack(index);
            return true;
        }

        public void RemoveTrack(int index) {
            Track track = this.Tracks[index];
            Validate.Exception(ReferenceEquals(track.Timeline, this), "Expected track's timeline and the current timeline instance to be equal");
            this.Tracks.RemoveAt(index);
            Track.SetTimeline(track, null);
        }

        public void ClearTracks() {
            try {
                foreach (Track track in this.Tracks) {
                    Track.SetTimeline(track, null);
                }
            }
            #if DEBUG
            catch (Exception e) {
                Debugger.Break();
                throw;
            }
            #endif
            finally {
                this.Tracks.Clear();
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

        private static void EndOpacityLayer(RenderContext render, int count, ref SKPaint paint) {
            render.Canvas.RestoreToCount(count);
            if (paint != null) {
                paint.Dispose();
                paint = null;
            }
        }

        public void RenderAudio(long frame) {

        }

        public void Render(RenderContext render, long frame) {
            List<Track> tracks = this.Tracks;
            SKPaint trackPaint = null, clipPaint = null;
            List<VideoClip> bufferList = new List<VideoClip>();
            int i, c;
            for (i = tracks.Count - 1; i >= 0; i--) {
                if (tracks[i] is VideoTrack track && track.IsActuallyVisible) {
                    VideoClip clip = (VideoClip) track.GetClipAtFrame(frame);
                    if (clip == null) {
                        continue;
                    }

                    #if DEBUG
                    try {
                        #endif
                        if (clip.UseAsyncRendering) {
                            clip.IsAsyncRenderReady = false;
                            clip.BeginRender(frame);
                            if (bufferList.Count < 1 && clip.IsAsyncRenderReady) {
                                int trackSaveCount = BeginTrackOpacityLayer(render, track, ref trackPaint);
                                int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                                try {
                                    clip.EndRender(render);
                                }
                                finally {
                                    EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                                    EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                                    clip.IsAsyncRenderReady = false;
                                }
                            }
                            else {
                                bufferList.Add(clip);
                            }
                        }
                        else if (bufferList.Count < 1) {
                            int trackSaveCount = BeginTrackOpacityLayer(render, track, ref trackPaint);
                            int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                            try {
                                if (clip is OGLRenderTarget) {
                                    ((OGLRenderTarget) clip).RenderGL(frame);
                                    GLUtils.CleanGL();
                                }
                                else {
                                    clip.Render(render, frame);
                                }
                            }
                            finally {
                                EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                                EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                            }
                        }
                        else {
                            bufferList.Add(clip);
                        }
                        #if DEBUG
                    }
                    catch (Exception e) {
                        this.ExceptionsLastRender[clip] = e;
                    }
                    #endif
                }
            }

            for (i = 0, c = bufferList.Count; i < c; i++) {
                VideoClip clip = bufferList[i];
                if (clip.UseAsyncRendering) {
                    while (!clip.IsAsyncRenderReady) {
                        Thread.Sleep(1);
                    }

                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    int trackSaveCount = BeginTrackOpacityLayer(render, (VideoTrack) clip.Track, ref trackPaint);
                    try {
                        clip.EndRender(render);
                    }
                    finally {
                        clip.IsAsyncRenderReady = false;
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
                else {
                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    int trackSaveCount = BeginTrackOpacityLayer(render, (VideoTrack) clip.Track, ref trackPaint);
                    try {
                        if (clip is OGLRenderTarget) {
                            ((OGLRenderTarget) clip).RenderGL(frame);
                            GLUtils.CleanGL();
                        }
                        else {
                            clip.Render(render, frame);
                        }
                    }
                    finally {
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
            }
        }

        public async Task RenderAsync(RenderContext render, long frame) {
            List<Track> tracks = this.Tracks;
            SKPaint trackPaint = null, clipPaint = null;
            List<VideoClip> bufferList = new List<VideoClip>();
            for (int i = tracks.Count - 1; i >= 0; i--) {
                if (tracks[i] is VideoTrack track && track.IsActuallyVisible) {
                    VideoClip clip = (VideoClip) track.GetClipAtFrame(frame);
                    if (clip == null) {
                        continue;
                    }

                    #if DEBUG
                    try {
                        #endif
                        if (clip.UseAsyncRendering) {
                            clip.IsAsyncRenderReady = false;
                            clip.BeginRender(frame);
                            if (bufferList.Count < 1 && clip.IsAsyncRenderReady) {
                                int trackSaveCount = BeginTrackOpacityLayer(render, track, ref trackPaint);
                                int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                                try {
                                    clip.EndRender(render);
                                }
                                finally {
                                    EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                                    EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                                    clip.IsAsyncRenderReady = false;
                                }
                            }
                            else {
                                bufferList.Add(clip);
                            }
                        }
                        else if (bufferList.Count < 1) {
                            int trackSaveCount = BeginTrackOpacityLayer(render, track, ref trackPaint);
                            int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                            try {
                                if (clip is OGLRenderTarget) {
                                    ((OGLRenderTarget) clip).RenderGL(frame);
                                    GLUtils.CleanGL();
                                }
                                else {
                                    clip.Render(render, frame);
                                }
                            }
                            finally {
                                EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                                EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                            }
                        }
                        else {
                            bufferList.Add(clip);
                        }
                        #if DEBUG
                    }
                    catch (Exception e) {
                        this.ExceptionsLastRender[clip] = e;
                    }
                    #endif
                }
            }

            foreach (VideoClip clip in bufferList) {
                if (clip.UseAsyncRendering) {
                    while (!clip.IsAsyncRenderReady) {
                        await Task.Delay(1);
                    }

                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    int trackSaveCount = BeginTrackOpacityLayer(render, (VideoTrack) clip.Track, ref trackPaint);
                    try {
                        clip.EndRender(render);
                    }
                    finally {
                        clip.IsAsyncRenderReady = false;
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
                else {
                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    int trackSaveCount = BeginTrackOpacityLayer(render, (VideoTrack) clip.Track, ref trackPaint);
                    try {
                        if (clip is OGLRenderTarget) {
                            ((OGLRenderTarget) clip).RenderGL(frame);
                            GLUtils.CleanGL();
                        }
                        else {
                            clip.Render(render, frame);
                        }
                    }
                    finally {
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
                }
            }
        }
    }
}