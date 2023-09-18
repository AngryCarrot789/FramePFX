using System;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.RBC.Events;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A model that represents a timeline track clip, such as a video or audio clip
    /// </summary>
    public abstract class Clip : IClip, IStrictFrameRange, IAutomatable, IDisposable {
        /// <summary>
        /// Returns the track that this clip is currently in. When this changes, <see cref="OnTrackChanged"/> is always called
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        /// The project associated with this clip. This is fetched from the <see cref="Track"/> property, so this returns null if that is null
        /// </summary>
        public Project Project => this.Track?.Timeline?.Project;

        /// <summary>
        /// Returns the resource manager associated with this clip. This is fetched from the <see cref="Track"/> property, so this returns null if that is null
        /// </summary>
        public ResourceManager ResourceManager => this.Project?.ResourceManager;

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
        /// The position of this clip in terms of video frames, in the form of a <see cref="Utils.FrameSpan"/> which has a begin and duration property
        /// </summary>
        public FrameSpan FrameSpan;

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.Begin"/> property
        /// </summary>
        public long FrameBegin => this.FrameSpan.Begin;

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.Duration"/> property
        /// </summary>
        public long FrameDuration => this.FrameSpan.Duration;

        /// <summary>
        /// Helper property for getting and setting the <see cref="Utils.FrameSpan.EndIndex"/> property
        /// </summary>
        public long FrameEndIndex => this.FrameSpan.EndIndex;

        /// <summary>
        /// The number of frames (offset relative to <see cref="FrameBegin"/>) where the media originally begun
        /// <para>
        /// When the left thumb is dragged left, this value is decremented. Whereas, dragging right increments this value
        /// </para>
        /// </summary>
        public long MediaFrameOffset { get; set; }

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        public List<BaseEffect> Effects { get; }

        public event TrackChangedEventHandler TrackChanged;
        public event TimelineChangedEventHandler TrackTimelineChanged;
        public event FrameSeekedEventHandler FrameSeeked;
        public event WriteToRBEEventHandler SerialiseExtension;
        public event ReadFromRBEEventHandler DeserialiseExtension;

        protected Clip() {
            this.AutomationData = new AutomationData(this);
            this.Effects = new List<BaseEffect>();
        }

        /// <summary>
        /// Called when the user moves the timeline play head over this clip
        /// </summary>
        /// <param name="oldFrame">The previous play head position</param>
        /// <param name="newFrame">The new/current play head position</param>
        public virtual void OnFrameSeeked(long oldFrame, long newFrame) {
            this.FrameSeeked?.Invoke(this, oldFrame, newFrame);
        }

        /// <summary>
        /// Called when this clip is added to, removed from, or moved between tracks
        /// </summary>
        /// <param name="oldTrack">The track this clip was originally in (not in by the time this method is called)</param>
        /// <param name="newTrack">The track that this clip now exists in</param>
        protected virtual void OnTrackChanged(Track oldTrack, Track newTrack) {
            this.TrackChanged?.Invoke(oldTrack, newTrack);
        }

        /// <summary>
        /// Called only when this clip's track's timeline changes. This is called after
        /// <see cref="Timelines.Track.OnTimelineChanging"/> but before <see cref="Timelines.Track.OnTimelineChanged"/>
        /// <para>
        /// This is only called when the track (that holds us) timeline changes, but not when not when this clip
        /// is moved between tracks with differing timelines; that should be handled in <see cref="OnTrackChanged"/>
        /// </para>
        /// </summary>
        /// <param name="oldTimeline">Previous timeline</param>
        /// <param name="newTimeline">The new timeline, associated with our track</param>
        protected virtual void OnTrackTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            this.TrackTimelineChanged?.Invoke(oldTimeline, newTimeline);
        }

        public long GetRelativeFrame(long playhead) => playhead - this.FrameBegin;

        public bool GetRelativeFrame(long playhead, out long frame) {
            frame = this.ConvertTimelineToRelativeFrame(playhead, out bool valid);
            return valid;
        }

        public void AddEffect(BaseEffect effect) => BaseEffect.AddEffectToClip(this, effect);
        public void InsertEffect(BaseEffect effect, int index) => BaseEffect.InsertEffectIntoClip(this, effect, index);
        public bool RemoveEffect(BaseEffect effect) => BaseEffect.RemoveEffectFromClip(this, effect);
        public void RemoveEffectAt(int index) => BaseEffect.RemoveEffectAt(this, index);

        /// <summary>
        /// Writes this clip's data
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList("Effects");
            foreach (BaseEffect effect in this.Effects) {
                if (!(effect.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + effect.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(BaseEffect.FactoryId), id);
                effect.WriteToRBE(dictionary.CreateDictionary("Data"));
            }

            this.SerialiseExtension?.Invoke(this, data);
        }

        /// <summary>
        /// Reads this clip's data
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            this.FrameSpan = data.GetStruct<FrameSpan>(nameof(this.FrameSpan));
            this.MediaFrameOffset = data.GetLong(nameof(this.MediaFrameOffset));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            this.AutomationData.UpdateBackingStorage();
            foreach (RBEBase entry in data.GetList("Effects").List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Effect resource dictionary contained a non dictionary child: {entry.Type}");
                string factoryId = dictionary.GetString(nameof(BaseEffect.FactoryId));
                BaseEffect effect = EffectRegistry.Instance.CreateModel(factoryId);
                effect.ReadFromRBE(dictionary.GetDictionary("Data"));
                effect.OwnerClip = this;
                this.Effects.Add(effect);
            }

            this.DeserialiseExtension?.Invoke(this, data);
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
        /// Creates a clone of this clip, referencing the same resources, same display name, media
        /// transformations, etc (but not the same Clip ID, if one is present). This is typically called when
        /// splitting or duplicating clips, or even duplicating a track
        /// </summary>
        /// <returns></returns>
        public Clip Clone() {
            Clip clip = this.NewInstance();
            this.LoadDataIntoClone(clip);
            return clip;
        }

        protected abstract Clip NewInstance();

        protected virtual void LoadDataIntoClone(Clip clone) {
            clone.DisplayName = this.DisplayName;
            clone.FrameSpan = this.FrameSpan;
            clone.MediaFrameOffset = this.MediaFrameOffset;
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
        }

        #region Dispose

        /// <summary>
        /// Disposes this clip, releasing any resources it is using. This is called when a clip is about to be no longer reachable (e.g. when the user deletes it).
        /// <para>
        /// When clips are removed from the timeline, they are serialised (and stored in the history manager) then disposed.
        /// When the user undoes that, they're deserialised and a new instance is created. This means that the original reference
        /// of the deleted clip is not reused for simplicity sakes.
        /// </para>
        /// <para>
        /// Dispose only throws an exception in truly exceptional cases that should result in the app crashing
        /// </para>
        /// </summary>
        public void Dispose() {
            using (ErrorList stack = new ErrorList()) {
                this.OnBeginDispose();
                this.DisposeCore(stack);
                this.OnEndDispose();
            }
        }

        /// <summary>
        /// Called just before <see cref="DisposeCore(ErrorList)"/>. This should not throw any exceptions
        /// </summary>
        public virtual void OnBeginDispose() {
            this.IsDisposing = true;
        }

        /// <summary>
        /// Disposes this clip's resources, if necessary. Shared resources (e.g. stored in <see cref="ResourceItem"/>
        /// instances) shouldn't be disposed as other clips may reference the same data
        /// <para>
        /// Exceptions should not be thrown from this method, and instead, added to the given <see cref="ErrorList"/>
        /// </para>
        /// </summary>
        /// <param name="list">
        /// The exception stack in which to add any encountered exceptions during disposal.
        /// Any exceptions added to this may result in the app crashing and showing a crash report screen
        /// </param>
        protected virtual void DisposeCore(ErrorList list) {
            for (int i = this.Effects.Count - 1; i >= 0; i--) {
                this.RemoveEffectAt(i);
            }

            if (this is IBaseResourceClip resourceClip) {
                resourceClip.ResourceHelper.Dispose();
            }
        }

        /// <summary>
        /// Called just after <see cref="DisposeCore(ErrorList)"/>. This should not throw any exceptions
        /// </summary>
        public virtual void OnEndDispose() {
            this.IsDisposing = false;
        }

        #endregion

        #region Serialisation

        public static Clip ReadSerialisedWithId(RBEDictionary dictionary) {
            string id = dictionary.GetString(nameof(FactoryId));
            Clip clip = ClipRegistry.Instance.CreateModel(id);
            clip.ReadFromRBE(dictionary.GetDictionary("Data"));
            return clip;
        }

        public static void WriteSerialisedWithId(RBEDictionary dictionary, Clip clip) {
            if (!(clip.FactoryId is string id))
                throw new Exception("Unknown clip type: " + clip.GetType());
            dictionary.SetString(nameof(FactoryId), id);
            clip.WriteToRBE(dictionary.CreateDictionary("Data"));
        }

        public static RBEDictionary WriteSerialisedWithId(Clip clip) {
            RBEDictionary dictionary = new RBEDictionary();
            WriteSerialisedWithId(dictionary, clip);
            return dictionary;
        }

        #endregion

        #region Static Helpers

        public static void SetTrack(Clip clip, Track track) {
            Track oldTrack = clip.Track;
            if (!ReferenceEquals(oldTrack, track)) {
                clip.Track = track;
                clip.OnTrackChanged(oldTrack, track);
            }
        }

        public static void OnTrackTimelineChanged(Clip clip, Timeline oldTimeline, Timeline newTimeline) {
            clip.OnTrackTimelineChanged(oldTimeline, newTimeline);
        }

        #endregion

        public long ConvertRelativeToTimelineFrame(long relative) => this.FrameBegin + relative;

        public long ConvertTimelineToRelativeFrame(long timeline, out bool valid) {
            FrameSpan span = this.FrameSpan;
            long frame = timeline - span.Begin;
            valid = frame >= 0 && frame < span.Duration;
            return frame;
        }

        public bool IsTimelineFrameInRange(long timeline) {
            FrameSpan span = this.FrameSpan;
            long frame = timeline - span.Begin;
            return frame >= 0 && frame < span.Duration;
        }
    }
}