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
        private readonly Dictionary<long, Clip> idToClip = new Dictionary<long, Clip>();
        private readonly Dictionary<long, Track> idToTrack = new Dictionary<long, Track>();

        // not a chance anyone's creating more than 9 quintillion clips or tracks
        private long nextClipId;
        private long nextTrackId;

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

        public event ClipIdAssignedEventHandler ClipIdAssigned;
        public event TrackIdAssignedEventHandler TrackIdAssigned;

        public Timeline(Project project) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Tracks = new List<Track>();
            this.AutomationData = new AutomationData(this);
        }

        public bool GetClipById(long id, out Clip clip) {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Id must be non-negative");
            return this.idToClip.TryGetValue(id, out clip);
        }

        public bool GetTrackById(long id, out Track track) {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Id must be non-negative");
            return this.idToTrack.TryGetValue(id, out track);
        }

        public long GetNextClipId(Clip clip) {
            long id = this.nextClipId++;
            this.idToClip[id] = clip;
            this.ClipIdAssigned?.Invoke(clip, id);
            return id;
        }

        public long GetNextTrackId(Track track) {
            long id = this.nextTrackId++;
            this.idToTrack[id] = track;
            this.TrackIdAssigned?.Invoke(track, id);
            return id;
        }

        public void GetUsedFrameSpan(ref long begin, ref long endIndex) {
            foreach (Track track in this.Tracks) {
                track.GetUsedFrameSpan(ref begin, ref endIndex);
            }
        }

        public void GetClipIndicesAt(long frame, ICollection<int> indices) {
            foreach (Track track in this.Tracks) {
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
            foreach (Track track in this.Tracks) {
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
            this.PlayHeadFrame = data.GetLong(nameof(this.PlayHeadFrame));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            HashSet<long> usedIds = new HashSet<long>();
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).OfType<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(Track.RegistryId));
                Track track = TrackRegistry.Instance.CreateModel(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));

                // check for duplicate track ids, caused by external modification or corruption
                if (track.internalTrackId >= 0) {
                    if (usedIds.Contains(track.internalTrackId))
                        throw new Exception("Track ID already in use: " + track.internalTrackId);
                    usedIds.Add(track.internalTrackId);
                }

                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.Tracks.Count < 1 ? 0 : this.Tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public void AddTrack(Track track) => this.InsertTrack(this.Tracks.Count, track);

        public void InsertTrack(int index, Track track) {
            this.Tracks.Insert(index, track);
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
            if (!ReferenceEquals(track.Timeline, this)) {
                throw new Exception("Expected track's timeline and the current timeline instance to be equal");
            }

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

        /// <summary>
        /// Called when a clip is added to a track in this timeline
        /// </summary>
        /// <param name="track"></param>
        /// <param name="clip"></param>
        public void OnClipAdded(Track track, Clip clip) {
        }

        /// <summary>
        /// Called when a clip is removed from a track in this timeline
        /// </summary>
        /// <param name="track"></param>
        /// <param name="clip"></param>
        public void OnClipRemoved(Track track, Clip clip) {
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

        public Task RenderAsync(RenderContext render, long frame) {
            CancellationTokenSource source = new CancellationTokenSource(1000);
            return this.RenderAsync(render, frame, source.Token);
        }

        public Task RenderAsync(RenderContext render, long frame, CancellationToken token) {
            List<Track> tracks = this.Tracks;
            SKPaint trackPaint = null, clipPaint = null;
            int i = tracks.Count - 1;
            for (; i >= 0; i--) {
                if (token.IsCancellationRequested) {
                    return Task.FromCanceled(token);
                }

                if (tracks[i] is VideoTrack track && track.IsActuallyVisible) {
                    VideoClip clip = (VideoClip) track.GetClipAtFrame(frame);
                    if (clip == null) {
                        continue;
                    }

#if !DEBUG
                    try {
#endif
                    int trackSaveCount = BeginTrackOpacityLayer(render, track, ref trackPaint);
                    int clipSaveCount = BeginClipOpacityLayer(render, clip, ref clipPaint);
                    try {
                        clip.Render(render, frame);
                    }
                    finally {
                        EndOpacityLayer(render, clipSaveCount, ref clipPaint);
                        EndOpacityLayer(render, trackSaveCount, ref trackPaint);
                    }
#if !DEBUG
                    }
                    catch (Exception e) {
                        this.ExceptionsLastRender[clip] = e;
                    }
#endif
                }
            }

            return Task.CompletedTask;
        }
    }
}