using System;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.RBC.Events;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A model that represents a timeline track clip, such as a video or audio clip
    /// </summary>
    public abstract class Clip : IClip, IStrictFrameRange, IAutomatable, IDisposable {
        private readonly List<BaseEffect> internalEffectList;

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
        public string FactoryId => ClipFactory.Instance.GetTypeIdForModel(this.GetType());

        public bool IsDisposing { get; private set; }

        /// <summary>
        /// The position of this clip in terms of video frames, in the form of a <see cref="Utils.FrameSpan"/> which
        /// has a begin and duration property. This should only be set directly if the clip is not placed in a track,
        /// otherwise, use <see cref="SetFrameSpan"/> in order to update the track's clip position cache
        /// </summary>
        public FrameSpan FrameSpan;

        FrameSpan IClip.FrameSpan => this.FrameSpan;

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

        /// <summary>
        /// Gets or sets if this clip be rendered (drawn for video clips, or played for audio clips).
        /// When true, the clip can be drawn to the view port or have audio play to the speakers (both including exporting).
        /// False means the clip is not drawn and no audio is played
        /// <para>
        /// True by default
        /// </para>
        /// </summary>
        public bool IsRenderingEnabled { get; set; }

        public IReadOnlyList<BaseEffect> Effects => this.internalEffectList;

        /// <summary>
        /// Gets this clip's resource helper class
        /// </summary>
        public ResourceHelper ResourceHelper { get; }
        
        public event TrackChangedEventHandler TrackChanged;
        public event TimelineChangedEventHandler TrackTimelineChanged;
        public event ProjectChangedEventHandler TrackTimelineProjectChanged;
        public event FrameSeekedEventHandler FrameSeeked;
        public event WriteToRBEEventHandler SerialiseExtension;
        public event ReadFromRBEEventHandler DeserialiseExtension;

        protected Clip() {
            this.AutomationData = new AutomationData(this);
            this.internalEffectList = new List<BaseEffect>();
            this.ResourceHelper = new ResourceHelper(this);
            this.IsRenderingEnabled = true;
        }

        public KeyFrame GetDefaultKeyFrame(AutomationKey key) {
            return this.AutomationData[key].DefaultKeyFrame;
        }

        public void SetFrameSpan(FrameSpan span) {
            FrameSpan oldSpan = this.FrameSpan;
            this.FrameSpan = span;
            this.Track?.OnClipFrameSpanChanged(this, oldSpan);
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
            this.ResourceHelper.SetManager(newTrack?.Timeline?.Project?.ResourceManager);
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
            this.ResourceHelper.SetManager(newTimeline?.Project?.ResourceManager);
            this.TrackTimelineChanged?.Invoke(oldTimeline, newTimeline);
        }

        protected virtual void OnTrackTimelineProjectChanged(Project oldProject, Project newProject) {
            this.ResourceHelper.SetManager(newProject?.ResourceManager);
            this.TrackTimelineProjectChanged?.Invoke(oldProject, newProject);
        }

        public long GetRelativeFrame(long playhead) => playhead - this.FrameBegin;

        public bool GetRelativeFrame(long playhead, out long frame) {
            frame = this.ConvertTimelineToRelativeFrame(playhead, out bool valid);
            return valid;
        }

        public void AddEffect(BaseEffect effect) => BaseEffect.AddEffectToClip(this, effect);
        public void InsertEffect(BaseEffect effect, int index) => BaseEffect.InsertEffectIntoClip(this, effect, index);

        public bool RemoveEffect(BaseEffect effect) {
            if (effect.OwnerClip != null && !ReferenceEquals(effect.OwnerClip, this))
                throw new Exception("Effect does not belong to this clip");
            return BaseEffect.RemoveEffectFromOwner(effect);
        }

        public void RemoveEffectAt(int index) => BaseEffect.RemoveEffectAt(this, index);

        public void ClearEffects() => BaseEffect.ClearEffects(this);

        protected virtual void OnEffectAdded(BaseEffect effect, int index) {
        }

        protected virtual void OnEffectRemoved(BaseEffect effect) {
        }

        /// <summary>
        /// Writes this clip's data
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            data.SetBool(nameof(this.IsRenderingEnabled), this.IsRenderingEnabled);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList("Effects");
            foreach (BaseEffect effect in this.Effects) {
                if (!(effect.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + effect.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(BaseEffect.FactoryId), id);
                effect.WriteToRBE(dictionary.CreateDictionary("Data"));
            }

            this.ResourceHelper.WriteToRootRBE(data);
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
            this.IsRenderingEnabled = data.GetBool(nameof(this.IsRenderingEnabled), true);
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            this.ClearEffects(); // this shouldn't be necessary... but just in case
            foreach (RBEBase entry in data.GetList("Effects").List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Effect resource dictionary contained a non dictionary child: {entry.Type}");
                string factoryId = dictionary.GetString(nameof(BaseEffect.FactoryId));
                BaseEffect effect = EffectFactory.Instance.CreateModel(factoryId);
                effect.ReadFromRBE(dictionary.GetDictionary("Data"));
                BaseEffect.AddEffectToClip(this, effect);
            }

            this.ResourceHelper.ReadFromRootRBE(data);
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

        #region Cloning

        /// <summary>
        /// Creates a clone of this clip, referencing the same resources, same display name, media
        /// transformations, etc (but not the same Clip ID, if one is present). This is typically called when
        /// splitting or duplicating clips, or even duplicating a track
        /// </summary>
        /// <returns></returns>
        public Clip Clone(ClipCloneFlags flags = ClipCloneFlags.DefaultFlags) {
            Clip clone = this.NewInstanceForClone();
            clone.DisplayName = this.DisplayName;
            clone.FrameSpan = this.FrameSpan;
            clone.MediaFrameOffset = this.MediaFrameOffset;
            if ((flags & ClipCloneFlags.AutomationData) != 0) {
                this.AutomationData.LoadDataIntoClone(clone.AutomationData);
            }

            if ((flags & ClipCloneFlags.Effects) != 0) {
                foreach (BaseEffect effect in this.internalEffectList) {
                    BaseEffect.AddEffectToClip(clone, effect.Clone());
                }
            }

            this.LoadUserDataIntoClone(clone, flags);
            clone.AutomationData.UpdateBackingStorage();
            foreach (BaseEffect effect1 in clone.Effects)
                effect1.AutomationData.UpdateBackingStorage();

            if (clone is CompositionVideoClip composition && composition.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
                AutomationEngine.UpdateBackingStorage(resource.Timeline);
            }

            return clone;
        }

        protected abstract Clip NewInstanceForClone();

        /// <summary>
        /// Loads user-specific data into the cloned clip
        /// </summary>
        /// <param name="clone">The new clip instance</param>
        /// <param name="flags">Cloning flags</param>
        protected virtual void LoadUserDataIntoClone(Clip clone, ClipCloneFlags flags) {
            if ((flags & ClipCloneFlags.ResourceHelper) != 0) {
                this.ResourceHelper.LoadDataIntoClone(clone.ResourceHelper);
            }
        }

        #endregion

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
        /// <para>
        /// This function clears all effects and releases any resource handles
        /// </para>
        /// </summary>
        public void Dispose() {
            this.OnBeginDispose();
            this.OnDisposeCore();
            this.OnEndDispose();
        }

        /// <summary>
        /// Called just before <see cref="OnDisposeCore"/>. This should not throw any exceptions
        /// </summary>
        protected virtual void OnBeginDispose() {
            this.IsDisposing = true;
        }

        /// <summary>
        /// Disposes this clip's resources, if necessary. Shared resources (e.g. stored in <see cref="ResourceItem"/>
        /// instances) shouldn't be disposed as other clips may reference the same data
        /// <para>
        /// Exceptions should not be thrown from this method, and instead, added to the given <see cref="ErrorList"/>
        /// </para>
        /// </summary>
        protected virtual void OnDisposeCore() {
            this.ClearEffects();
            this.ResourceHelper.Dispose();
        }

        /// <summary>
        /// Called just after <see cref="OnDisposeCore"/>. This should not throw any exceptions
        /// </summary>
        protected virtual void OnEndDispose() {
            this.IsDisposing = false;
        }

        #endregion

        #region Serialisation

        public static Clip ReadSerialisedWithId(RBEDictionary dictionary) {
            string id = dictionary.GetString(nameof(FactoryId));
            Clip clip = ClipFactory.Instance.CreateModel(id);
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

        public long ConvertRelativeToTimelineFrame(long relative) => this.FrameBegin + relative;

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) {
            FrameSpan span = this.FrameSpan;
            long frame = timeline - span.Begin;
            inRange = frame >= 0 && frame < span.Duration;
            return frame;
        }

        public bool IsTimelineFrameInRange(long timeline) {
            FrameSpan span = this.FrameSpan;
            long frame = timeline - span.Begin;
            return frame >= 0 && frame < span.Duration;
        }

        /// <summary>
        /// Whether or not this clip can accept the given effect. Video clips can only accept video effects, etc.
        /// This method MUST return the same value that <see cref="IsEffectTypeAllowed(Type)"/> would return for
        /// the type of an effect instance passed to this method
        /// </summary>
        /// <param name="effect">The non-null effect to check</param>
        /// <returns>True if the effect can be added, otherwise false</returns>
        public virtual bool IsEffectTypeAllowed(BaseEffect effect) {
            return this.IsEffectTypeAllowed(effect.GetType());
        }

        /// <summary>
        /// Whether or not this clip can accept the given effect type. This MUST return the same value
        /// that <see cref="IsEffectTypeAllowed(BaseEffect)"/> would return for an effect instance of
        /// the exact type passed to this method
        /// </summary>
        /// <param name="effect">The non-null effect to check</param>
        /// <returns>True if the effect can be added, otherwise false</returns>
        public abstract bool IsEffectTypeAllowed(Type effectType);

        #region Static Helpers

        internal static void InternalSetTrack(Clip clip, Track track) {
            Track oldTrack = clip.Track;
            if (!ReferenceEquals(oldTrack, track)) {
                clip.Track = track;
                clip.OnTrackChanged(oldTrack, track);
            }
        }

        internal static void InternalOnTrackTimelineChanged(Clip clip, Timeline oldTimeline, Timeline newTimeline) {
            clip.OnTrackTimelineChanged(oldTimeline, newTimeline);
        }

        internal static void InternalOnTrackTimelineProjectChanged(Clip clip, Project oldProject, Project newProject) {
            clip.OnTrackTimelineProjectChanged(oldProject, newProject);
        }

        internal static void InternalOnInsertingEffect(Clip clip, int index, BaseEffect effect) {
            clip.internalEffectList.Insert(index, effect);
        }

        internal static void InternalOnEffectAdded(Clip clip, int index, BaseEffect effect) {
            clip.OnEffectAdded(effect, index);
        }

        internal static void InternalOnRemovingEffect(Clip clip, int index) {
            clip.internalEffectList.RemoveAt(index);
        }

        internal static void InternalOnEffectRemoved(Clip clip, BaseEffect effect) {
            clip.OnEffectRemoved(effect);
        }

        #endregion
    }
}