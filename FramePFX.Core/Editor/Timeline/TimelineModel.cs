using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineModel : IAutomatable, IRBESerialisable {
        public ProjectModel Project { get; }

        public long PlayHeadFrame { get; set; }

        public long MaxDuration { get; set; }

        public List<TrackModel> Tracks { get; }

        public AutomationData AutomationData { get; }

        public AutomationEngine AutomationEngine => this.Project.AutomationEngine;

        public bool IsAutomationChangeInProgress { get; set; }

        // view model can access this
        public Dictionary<ClipModel, Exception> ExceptionsLastRender { get; } = new Dictionary<ClipModel, Exception>();

        public TimelineModel(ProjectModel project) {
            this.Project = project;
            this.Tracks = new List<TrackModel>();
            this.AutomationData = new AutomationData(this);
        }

        long IAutomatable.GetRelativeFrame(long frame) => frame;

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong(nameof(this.PlayHeadFrame), this.PlayHeadFrame);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (TrackModel track in this.Tracks) {
                if (!(track.RegistryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(TrackModel.RegistryId), registryId);
                track.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.PlayHeadFrame = data.GetLong(nameof(this.PlayHeadFrame));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEBase entry in data.GetList(nameof(this.Tracks)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                string registryId = dictionary.GetString(nameof(TrackModel.RegistryId));
                TrackModel track = TrackRegistry.Instance.CreateModel(this, registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.Tracks.Count < 1 ? 0 : this.Tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public void AddTrack(TrackModel track) {
            Validate.Exception(ReferenceEquals(track.Timeline, this), "Expected track's timeline and the current timeline instance to be equal");
            this.Tracks.Add(track);
        }

        public bool RemoveTrack(TrackModel track) {
            Validate.Exception(ReferenceEquals(track.Timeline, this), "Expected track's timeline and the current timeline instance to be equal");
            int index = this.Tracks.IndexOf(track);
            if (index < 0) {
                return false;
            }

            this.Tracks.RemoveAt(index);
            return true;
        }

        public void RemoveTrack(int index) {
            TrackModel track = this.Tracks[index];
            Validate.Exception(ReferenceEquals(track.Timeline, this), "Expected track's timeline and the current timeline instance to be equal");
            this.Tracks.RemoveAt(index);
        }

        public void ClearTracks() {
            this.Tracks.Clear();
        }

        public void Render(RenderContext render) {
            this.Render(render, this.PlayHeadFrame);
        }

        private static int SaveLayerForOpacity(SKCanvas canvas, double opacity, ref SKPaint transparency) {
            return canvas.SaveLayer(transparency ?? (transparency = new SKPaint {Color = new SKColor(255, 255, 255, (byte) Maths.Clamp(opacity * 255F, 0, 255F))}));
        }

        public void Render(RenderContext render, long frame) {
            SKPaint trackTransparencyPaint = null, clipPaint = null;
            for (int i = this.Tracks.Count - 1; i >= 0; i--) {
                if (!(this.Tracks[i] is VideoTrackModel track) || !track.IsActuallyVisible) {
                    continue;
                }

                List<ClipModel> clips = track.Clips.Where(x => x.IntersectsFrameAt(frame)).ToList();
                if (clips.Count <= 0) {
                    continue;
                }

                // SaveLayer requires a temporary drawing bitmap, which can slightly
                // decrease performance, so only SaveLayer when absolutely necessary
                int trackSaveCount = !Maths.Equals(track.Opacity, 1d) ? SaveLayerForOpacity(render.Canvas, track.Opacity, ref trackTransparencyPaint) : render.Canvas.Save();

                foreach (ClipModel clip in clips) {
                    int clipSaveCount;
                    VideoClipModel videoClip = (VideoClipModel) clip;
                    if (videoClip.UseCustomOpacityCalculation || Maths.Equals(videoClip.Opacity, 1d)) {
                        clipSaveCount = render.Canvas.Save();
                    }
                    else {
                        clipSaveCount = SaveLayerForOpacity(render.Canvas, videoClip.Opacity, ref clipPaint);
                    }

                    #if DEBUG
                    videoClip.Render(render, frame);
                    #else
                    try {
                        videoClip.Render(render, frame);
                    }
                    catch (Exception e) {
                        this.ExceptionsLastRender[clip] = e;
                    }
                    #endif

                    render.Canvas.RestoreToCount(clipSaveCount);
                    if (clipPaint != null) {
                        clipPaint.Dispose();
                        clipPaint = null;
                    }
                }

                render.Canvas.RestoreToCount(trackSaveCount);
                if (trackTransparencyPaint != null) {
                    trackTransparencyPaint.Dispose();
                    trackTransparencyPaint = null;
                }
            }
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return Enumerable.Reverse(this.Tracks).SelectMany(track => track.GetClipsAtFrame(frame));
        }
    }
}