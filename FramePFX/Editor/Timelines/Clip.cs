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
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// A model that represents a timeline track clip, such as a video or audio clip
    /// </summary>
    public abstract class Clip : IResourceHolder, IProjectBound, IStrictFrameRange, IAutomatable {
        public static readonly SerialisationRegistry Serialisation;
        private readonly List<BaseEffect> internalEffectList;
        private FrameSpan frameSpan;
        public long LastSeekedFrame;

        /// <summary>
        /// Gets the track that this clip is currently placed in
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

        /// <summary>
        /// The position of this clip in terms of video frames, in the form of a
        /// <see cref="Utils.FrameSpan"/> which has a begin and duration property
        /// </summary>
        public FrameSpan FrameSpan {
            get => this.frameSpan;
            set {
                FrameSpan oldSpan = this.frameSpan;
                if (oldSpan != value) {
                    this.frameSpan = value;
                    this.FrameSpanChanged?.Invoke(this, oldSpan, value);
                }
            }
        }

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

        /// <summary>
        /// An event fired when this clip was either removed from a
        /// track, added to a track, or moved from one track to another
        /// </summary>
        public event TrackChangedEventHandler TrackChanged;

        /// <summary>
        /// An event fired when this clip's <see cref="FrameSpan"/> changes
        /// </summary>
        public event FrameSpanChangedEventHandler FrameSpanChanged;

        protected Clip() {
            this.AutomationData = new AutomationData(this);
            this.internalEffectList = new List<BaseEffect>();
            this.ResourceHelper = new ResourceHelper(this);
            this.IsRenderingEnabled = true;
        }

        static Clip() {
            Serialisation = new SerialisationRegistry();
            Serialisation.Register<Clip>("1.0.0", (clip, data, ctx) => {
                AppLogger.WriteLine("Serialising Clip 1.0.0: " + ctx.CurrentVersion);
                if (!string.IsNullOrEmpty(clip.DisplayName))
                    data.SetString(nameof(clip.DisplayName), clip.DisplayName);
                data.SetStruct(nameof(clip.FrameSpan), clip.FrameSpan);
                data.SetLong(nameof(clip.MediaFrameOffset), clip.MediaFrameOffset);
                data.SetBool(nameof(clip.IsRenderingEnabled), clip.IsRenderingEnabled);
                clip.AutomationData.WriteToRBE(data.CreateDictionary(nameof(clip.AutomationData)));
                RBEList list = data.CreateList("Effects");
                foreach (BaseEffect effect in clip.Effects) {
                    if (!(effect.FactoryId is string id))
                        throw new Exception("Unknown clip type: " + effect.GetType());
                    RBEDictionary dictionary = list.AddDictionary();
                    dictionary.SetString(nameof(BaseEffect.FactoryId), id);
                    effect.WriteToRBE(dictionary.CreateDictionary("Data"));
                }

                clip.ResourceHelper.WriteToRootRBE(data);
            }, (clip, data, ctx) => {
                AppLogger.WriteLine("Deserialising Clip 1.0.0: " + ctx.CurrentVersion);
                clip.DisplayName = data.GetString(nameof(clip.DisplayName), null);
                clip.FrameSpan = data.GetStruct<FrameSpan>(nameof(clip.FrameSpan));
                clip.MediaFrameOffset = data.GetLong(nameof(clip.MediaFrameOffset));
                clip.IsRenderingEnabled = data.GetBool(nameof(clip.IsRenderingEnabled), true);
                clip.AutomationData.ReadFromRBE(data.GetDictionary(nameof(clip.AutomationData)));
                foreach (RBEBase entry in data.GetList("Effects").List) {
                    if (!(entry is RBEDictionary dictionary))
                        throw new Exception($"Effect resource dictionary contained a non dictionary child: {entry.Type}");
                    string factoryId = dictionary.GetString(nameof(BaseEffect.FactoryId));
                    BaseEffect effect = EffectFactory.Instance.CreateModel(factoryId);
                    effect.ReadFromRBE(dictionary.GetDictionary("Data"));
                    clip.AddEffect(effect);
                }

                clip.ResourceHelper.ReadFromRootRBE(data);
            });

            // These 2 below purely exists to test the serialisation system
            Serialisation.Register<Clip>("1.1.0", (clip, data, ctx) => {
                AppLogger.WriteLine("Serialising Clip 1.1.0: " + ctx.CurrentVersion);
                ctx.SerialiseLastVersion(clip, data);
            }, (clip, data, ctx) => {
                AppLogger.WriteLine("Deserialising Clip 1.1.0: " + ctx.CurrentVersion);
                ctx.DeserialiseLastVersion(clip, data);
            });

            Serialisation.Register<Clip>("1.5.0", (clip, data, ctx) => {
                AppLogger.WriteLine("Serialising Clip 1.5.0: " + ctx.CurrentVersion);
                ctx.SerialiseLastVersion(clip, data);
            }, (clip, data, ctx) => {
                AppLogger.WriteLine("Deserialising Clip 1.5.0: " + ctx.CurrentVersion);
                ctx.DeserialiseLastVersion(clip, data);
            });
        }

        public KeyFrame GetDefaultKeyFrame(AutomationKey key) {
            return this.AutomationData[key].DefaultKeyFrame;
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
        /// Invoked when the project associated with our track's timeline changes
        /// </summary>
        /// <param name="e">The project change event args</param>
        protected virtual void OnProjectChanged(ProjectChangedEventArgs e) {
            this.ResourceHelper.SetManager(e.NewProject?.ResourceManager);
        }

        public long GetRelativeFrame(long playhead) => playhead - this.FrameBegin;

        public bool GetRelativeFrame(long playhead, out long frame) {
            frame = this.ConvertTimelineToRelativeFrame(playhead, out bool valid);
            return valid;
        }

        public void AddEffect(BaseEffect effect) => BaseEffect.AddEffectToClip(this, effect);
        public void InsertEffect(BaseEffect effect, int index) => BaseEffect.InsertEffectIntoClip(this, effect, index);

        /// <summary>
        /// Removes the an effect from this clip
        /// </summary>
        /// <param name="effect">The effect to remove</param>
        /// <returns></returns>
        /// <exception cref="Exception">The effect does not belong to this clip</exception>
        public bool RemoveEffect(BaseEffect effect) {
            if (!ReferenceEquals(effect.OwnerClip, this))
                throw new Exception("Effect does not belong to this clip");
            return BaseEffect.RemoveEffectFromOwner(effect);
        }

        /// <summary>
        /// Removes an effect at the given index
        /// </summary>
        /// <param name="index">The index of the effect</param>
        public void RemoveEffectAt(int index) => BaseEffect.RemoveEffectAt(this, index);

        /// <summary>
        /// Clears all of this clip's effects
        /// </summary>
        public void DestroyAndClearEffects() {
            for (int i = this.Effects.Count - 1; i >= 0; i--) {
                this.Effects[i].Destroy();
                BaseEffect.RemoveEffectAt(this, i);
            }
        }

        protected virtual void OnEffectAdded(BaseEffect effect, int index) {

        }

        protected virtual void OnEffectRemoved(BaseEffect effect) {

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
        public Clip Clone(ClipCloneFlags flags = ClipCloneFlags.All) {
            Clip clone = this.NewInstanceForClone();
            clone.DisplayName = this.DisplayName;
            clone.FrameSpan = this.FrameSpan;
            clone.MediaFrameOffset = this.MediaFrameOffset;
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
            if ((flags & ClipCloneFlags.Effects) != 0) {
                foreach (BaseEffect effect in this.internalEffectList) {
                    BaseEffect.AddEffectToClip(clone, effect.Clone());
                }
            }

            this.LoadUserDataIntoClone(clone, flags);
            clone.AutomationData.UpdateBackingStorage();
            foreach (BaseEffect effect in clone.Effects)
                effect.AutomationData.UpdateBackingStorage();

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
        /// Recursively destroys all effects and other destroyable resources with this clip, and finally destroys any clip data
        /// </summary>
        public void Destroy() {
            this.OnDestroyCore();
        }

        /// <summary>
        /// Disposes this clip's resources, if necessary. Shared resources (e.g. stored in <see cref="ResourceItem"/>
        /// instances) shouldn't be disposed as other clips may reference the same data
        /// <para>
        /// Exceptions should not be thrown from this method, and instead, added to the given <see cref="ErrorList"/>
        /// </para>
        /// </summary>
        protected virtual void OnDestroyCore() {
            this.DestroyAndClearEffects();
            this.ResourceHelper.Dispose();
        }

        #endregion

        #region Serialisation

        public static Clip ReadSerialisedWithId(RBEDictionary dictionary) {
            string id = dictionary.GetString(nameof(FactoryId));
            Clip clip = ClipFactory.Instance.CreateModel(id);
            Serialisation.Deserialise(clip, dictionary.GetDictionary("Data"), new SerialisationContext(IoC.Application.Version));
            return clip;
        }

        public static void WriteSerialisedWithId(RBEDictionary dictionary, Clip clip) {
            if (!(clip.FactoryId is string id))
                throw new Exception("Unknown clip type: " + clip.GetType());
            dictionary.SetString(nameof(FactoryId), id);
            Serialisation.Serialise(clip, dictionary.CreateDictionary("Data"), new SerialisationContext(IoC.Application.Version));
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

        public static void OnProjectChangedInternal(Clip clip, ProjectChangedEventArgs e) => clip.OnProjectChanged(e);
    }
}