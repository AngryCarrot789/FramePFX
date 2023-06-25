using System;
using System.Diagnostics;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// A model that represents a timeline track clip, such as a video or audio clip
    /// </summary>
    public abstract class ClipModel : IAutomatable, IRBESerialisable, IDisposable {
        /// <summary>
        /// Returns the track that this clip is currently in. When this changes, <see cref="OnTrackChanged"/> is always called
        /// </summary>
        public TrackModel Track { get; private set; }

        /// <summary>
        /// Returns the timeline associated with this clip. This is fetched from the <see cref="Track"/> property, so this returns null if that is null
        /// </summary>
        public TimelineModel Timeline => this.Track?.Timeline;

        /// <summary>
        /// Returns the project associated with this clip. This is fetched from the <see cref="Track"/> property, so this returns null if that is null
        /// </summary>
        public ProjectModel Project => this.Track?.Timeline.Project;

        /// <summary>
        /// Returns the resource manager associated with this clip. This is fetched from the <see cref="Track"/> property, so this returns null if that is null
        /// </summary>
        public ResourceManager ResourceManager => this.Track?.Timeline.Project.ResourceManager;

        public long TimelinePlayhead => this.Track?.Timeline.PlayHeadFrame ?? 0;

        /// <summary>
        /// This clip's display name, which the user can chose to identify it
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// This clip's factory ID, used for creating a new instance dynamically via reflection
        /// </summary>
        public string FactoryId => ClipRegistry.Instance.GetTypeIdForModel(this.GetType());

        public bool IsDisposing { get; private set; }

        /// <summary>
        /// A unique identifier for this clip, relative to the project. Returns -1 if the clip does not have an
        /// ID assigned and is not associated with a track yet (otherwise, a new id is assigned through the project model)
        /// </summary>
        public long UniqueClipId {
            get => this.clipId >= 0 ? this.clipId : (this.clipId = this.Track?.Timeline.Project.GetNextClipId() ?? -1);
            set => this.clipId = value;
        }

        /// <summary>
        /// Whether or not this clip has an ID assigned or not
        /// </summary>
        public bool HasClipId => this.clipId >= 0;

        /// <summary>
        /// The position of this clip in terms of video frames, in the form of a <see cref="Utils.FrameSpan"/> which has a begin and duration property
        /// </summary>
        public FrameSpan FrameSpan { get; set; }

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.Begin"/> property
        /// </summary>
        public long FrameBegin {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.WithBegin(value);
        }

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.Duration"/> property
        /// </summary>
        public long FrameDuration {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.WithDuration(value);
        }

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.EndIndex"/> property
        /// </summary>
        public long FrameEndIndex {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.WithEndIndex(value);
        }

        /// <summary>
        /// The number of frames (offset relative to <see cref="FrameBegin"/>) where the media originally begun
        /// <para>
        /// When the left thumb is dragged left, this value is decremented. Whereas, dragging right increments this value
        /// </para>
        /// </summary>
        public long MediaFrameOffset { get; set; }

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        private long clipId = -1;

        protected ClipModel() {
            this.AutomationData = new AutomationData(this);
        }

        /// <summary>
        /// Sets the given clip's track
        /// </summary>
        /// <param name="model">Model to set the track of</param>
        /// <param name="track">New track</param>
        /// <param name="fireTrackChangedEvent">Whether to invoke the model's <see cref="OnTrackChanged"/> function. If false, it must be called manually</param>
        public static void SetTrack(ClipModel model, TrackModel track, bool fireTrackChangedEvent = true) {
            if (fireTrackChangedEvent) {
                TrackModel oldTrack = model.Track;
                if (ReferenceEquals(oldTrack, track)) {
                    Debug.WriteLine("Attempted to set the track to the same instance.\n" + new Exception().GetToString());
                }
                else {
                    model.Track = track;
                    model.OnTrackChanged(oldTrack, track);
                }
            }
            else {
                model.Track = track;
            }
        }

        /// <summary>
        /// Writes this clip's data
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
            if (this.clipId >= 0)
                data.SetLong(nameof(this.UniqueClipId), this.clipId);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
        }

        /// <summary>
        /// Reads this clip's data
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            if (data.TryGetLong(nameof(this.UniqueClipId), out long id) && id >= 0)
                this.clipId = id;
            this.FrameSpan = data.GetStruct<FrameSpan>(nameof(this.FrameSpan));
            this.MediaFrameOffset = data.GetLong(nameof(this.MediaFrameOffset));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
        }

        /// <summary>
        /// Whether or not this clip (video, audio, etc) intersects the given video frame
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <returns>Intersection</returns>
        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        /// <summary>
        /// Called when this clip's track changes, either by the clip being added to it for the first time (in which case,
        /// <paramref name="oldTrack"/> will be null and <paramref name="newTrack"/> will not be null), or by a user dragging
        /// this clip from track to track
        /// </summary>
        /// <param name="oldTrack"></param>
        /// <param name="newTrack"></param>
        protected virtual void OnTrackChanged(TrackModel oldTrack, TrackModel newTrack) {

        }

        /// <summary>
        /// Creates a clone of this clip, referencing the same resources, same display name, media
        /// transformations, etc (but not the same Clip ID, if one is present). This is typically called when
        /// splitting or duplicating clips, or even duplicating a track
        /// </summary>
        /// <returns></returns>
        public ClipModel Clone() {
            ClipModel clip = this.NewInstance();
            this.LoadDataIntoClone(clip);
            return clip;
        }

        protected abstract ClipModel NewInstance();

        protected virtual void LoadDataIntoClone(ClipModel clone) {
            clone.DisplayName = this.DisplayName;
            clone.FrameSpan = this.FrameSpan;
            clone.MediaFrameOffset = this.MediaFrameOffset;
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
        }

        #region Dispose

        /// <summary>
        /// Disposes this clip, releasing any resources it is using. This is called when a clip is about to no longer reachable
        /// <para>
        /// When clips are removed from the timeline, they are serialised and stored in the history manager, and then deserialised when
        /// a user undoes the clip deletion. This means that, dispose is called just after a clip is serialised
        /// </para>
        /// <para>
        /// Dispose only throws an exception in truely exceptional cases
        /// </para>
        /// </summary>
        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                this.OnBeginDispose();
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception($"{nameof(this.DisposeCore)} threw an unexpected exception", e));
                }
                this.OnEndDispose();
            }
        }

        /// <summary>
        /// Called just before <see cref="DisposeCore(ExceptionStack)"/>. This should not throw any exceptions
        /// </summary>
        public virtual void OnBeginDispose() {
            this.IsDisposing = true;
        }

        /// <summary>
        /// Disposes this clip's resources, if necessary. Shared resources (e.g. stored in <see cref="ResourceItem"/>
        /// instances) shouldn't be disposed as other clips may reference the same data
        /// <para>
        /// Exceptions should not be thrown from this method, and instead, added to the given <see cref="ExceptionStack"/>
        /// </para>
        /// </summary>
        /// <param name="stack">The exception stack in which to add any encountered exceptions during disposal</param>
        protected virtual void DisposeCore(ExceptionStack stack) {

        }

        /// <summary>
        /// Called just after <see cref="DisposeCore(ExceptionStack)"/>. This should not throw any exceptions
        /// </summary>
        public virtual void OnEndDispose() {
            this.IsDisposing = false;
        }

        #endregion
    }
}