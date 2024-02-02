using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FramePFX.Destroying;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Factories;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;

namespace FramePFX.Editors.Timelines.Clips {
    public delegate void ClipEventHandler(Clip clip);
    public delegate void ClipSpanChangedEventHandler(Clip clip, FrameSpan oldSpan, FrameSpan newSpan);
    public delegate void ClipMediaOffsetChangedEventHandler(Clip clip, long oldOffset, long newOffset);
    public delegate void ClipTrackChangedEventHandler(Clip clip, Track oldTrack, Track newTrack);
    public delegate void ClipActiveSequenceChangedEventHandler(Clip clip, AutomationSequence oldSequence, AutomationSequence newSequence);

    public abstract class Clip : IAutomatable, IStrictFrameRange, IResourceHolder, IHaveEffects, IDestroy {
        private readonly List<BaseEffect> internalEffectList;
        private FrameSpan span;
        private string displayName;
        private bool isSelected;
        private AutomationSequence activeSequence;
        private long mediaFrameOffset;

        public Track Track { get; private set; }

        public Timeline Timeline => this.Track?.Timeline;

        public Project Project => this.Timeline?.Project;

        public ReadOnlyCollection<BaseEffect> Effects { get; }

        public long RelativePlayHead {
            get {
                Timeline timeline = this.Timeline;
                return timeline != null ? (this.Timeline.PlayHeadPosition - this.span.Begin) : 0;
            }
        }

        public AutomationData AutomationData { get; }

        /// <summary>
        /// Gets or sets this clip's frame span, that is, a beginning and duration property contain in a
        /// single struct that represents the location and duration of a clip within a track
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Begin or duration were negative</exception>
        public FrameSpan FrameSpan {
            get => this.span;
            set {
                FrameSpan oldSpan = this.span;
                if (oldSpan == value)
                    return;
                if (value.Begin < 0 || value.Duration < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Span contained negative values");
                this.span = value;
                Track.InternalOnClipSpanChanged(this, oldSpan);
                this.OnFrameSpanChanged(oldSpan, value);
            }
        }

        /// <summary>
        /// A frame offset (relative to the project FPS) that is how many frames ahead or behind this clip's media begins.
        /// This is changed when a clip's left grip is dragged. This is negative when the media starts before the clip
        /// starts, and is positive when the media begins after the clip starts.
        /// <para>
        /// When dragging the left grip, it is calculated as: <code>MediaFrameOffset += (oldSpan.Begin - newSpan.Begin)</code>
        /// </para>
        /// </summary>
        public long MediaFrameOffset {
            get => this.mediaFrameOffset;
            set {
                long oldValue = this.mediaFrameOffset;
                if (value == oldValue)
                    return;
                this.mediaFrameOffset = value;
                this.MediaFrameOffsetChanged?.Invoke(this, oldValue, value);
            }
        }

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        public bool IsSelected {
            get => this.isSelected;
            set {
                if (this.isSelected == value)
                    return;
                this.isSelected = value;
                Track.InternalOnIsClipSelectedChanged(this);
                this.IsSelectedChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Stores the sequence that this clip's automation sequence editor is using. This is only really used for the UI
        /// </summary>
        public AutomationSequence ActiveSequence {
            get => this.activeSequence;
            set {
                AutomationSequence oldSequence = this.activeSequence;
                if (oldSequence == value)
                    return;
                this.activeSequence = value;
                this.ActiveSequenceChanged?.Invoke(this, oldSequence, value);
            }
        }

        public ResourceHelper ResourceHelper { get; }

        public string FactoryId => ClipFactory.Instance.GetId(this.GetType());

        public event EffectOwnerEventHandler EffectAdded;
        public event EffectOwnerEventHandler EffectRemoved;
        public event EffectMovedEventHandler EffectMoved;
        public event ClipSpanChangedEventHandler FrameSpanChanged;
        public event ClipMediaOffsetChangedEventHandler MediaFrameOffsetChanged;
        public event ClipEventHandler DisplayNameChanged;
        public event ClipEventHandler IsSelectedChanged;

        public event ClipTrackChangedEventHandler TrackChanged;
        public event TimelineChangedEventHandler TimelineChanged;

        /// <summary>
        /// An event fired when this clip's automation sequence editor's sequence changes. The new sequence
        /// may not directly belong to the clip, but may belong to an effect added to the clip
        /// </summary>
        public event ClipActiveSequenceChangedEventHandler ActiveSequenceChanged;

        protected Clip() {
            this.internalEffectList = new List<BaseEffect>();
            this.Effects = this.internalEffectList.AsReadOnly();
            this.ResourceHelper = new ResourceHelper(this);
            this.AutomationData = new AutomationData(this);
        }

        protected virtual void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {
            this.FrameSpanChanged?.Invoke(this, oldSpan, newSpan);
        }

        public Clip Clone() => this.Clone(ClipCloneOptions.Default);

        public Clip Clone(ClipCloneOptions options) {
            string id = this.FactoryId;
            Clip clone = ClipFactory.Instance.NewClip(id);
            if (clone.GetType() != this.GetType())
                throw new Exception("Cloned object type does not match the item type");

            this.LoadDataIntoClone(clone, options);
            if (options.CloneEffects) {
                foreach (BaseEffect effect in this.Effects) {
                    clone.AddEffect(effect.Clone());
                }
            }

            if (options.CloneAutomationData) {
                this.AutomationData.LoadDataIntoClone(clone.AutomationData);
            }

            return clone;
        }

        protected virtual void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
            clone.span = this.span;
            clone.displayName = this.displayName;
            clone.mediaFrameOffset = this.mediaFrameOffset;
            clone.span = this.span;
        }

        public static void WriteSerialisedWithId(RBEDictionary dictionary, Clip clip) {
            if (!(clip.FactoryId is string id))
                throw new Exception("Unknown clip type: " + clip.GetType());
            dictionary.SetString(nameof(FactoryId), id);
            clip.WriteToRBE(dictionary.CreateDictionary("Data"));
        }

        public static Clip ReadSerialisedWithId(RBEDictionary dictionary) {
            string id = dictionary.GetString(nameof(FactoryId));
            Clip clip = ClipFactory.Instance.NewClip(id);
            clip.ReadFromRBE(dictionary.GetDictionary("Data"));
            return clip;
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetStruct(nameof(this.FrameSpan), this.FrameSpan);
            data.SetLong(nameof(this.MediaFrameOffset), this.MediaFrameOffset);
            // data.SetBool(nameof(this.IsRenderingEnabled), this.IsRenderingEnabled);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            BaseEffect.WriteSerialisedWithIdList(this, data.CreateList("Effects"));
            this.ResourceHelper.WriteToRootRBE(data);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            this.FrameSpan = data.GetStruct<FrameSpan>(nameof(this.FrameSpan));
            this.MediaFrameOffset = data.GetLong(nameof(this.MediaFrameOffset));
            // this.IsRenderingEnabled = data.GetBool(nameof(this.IsRenderingEnabled), true);
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            BaseEffect.ReadSerialisedWithIdList(this, data.GetList("Effects"));
            this.ResourceHelper.ReadFromRootRBE(data);
        }

        public void MoveToTrack(Track dstTrack) {
            if (ReferenceEquals(this.Track, dstTrack))
                return;
            this.MoveToTrack(dstTrack, dstTrack.Clips.Count);
        }

        public void MoveToTrack(Track dstTrack, int dstIndex) {
            if (this.Track == null) {
                dstTrack.InsertClip(dstIndex, this);
                return;
            }

            int index = this.Track.Clips.IndexOf(this);
            if (index == -1) {
                throw new Exception("Fatal error: clip did not exist in its owner track");
            }

            this.Track.MoveClipToTrack(index, dstTrack, dstIndex);
        }

        /// <summary>
        /// Invoked when this clip's track changes. The cause of this is either the clip being added to, removed
        /// from or moved between tracks. This method calls <see cref="OnProjectChanged"/> if possible. The
        /// old and new tracks will not match. This method must be called by overriders, as this method
        /// updates the resource helper, fires appropriate events, etc.
        /// </summary>
        /// <param name="oldTrack">The previous track</param>
        /// <param name="newTrack">The new track</param>
        protected virtual void OnTrackChanged(Track oldTrack, Track newTrack) {
            this.ResourceHelper.SetManager(newTrack?.Project?.ResourceManager);
            this.TrackChanged?.Invoke(this, oldTrack, newTrack);

            Timeline oldTimeline = oldTrack?.Timeline;
            Timeline newTimeline = newTrack?.Timeline;

            if (!ReferenceEquals(oldTimeline, newTimeline)) {
                this.TimelineChanged?.Invoke(this, oldTimeline, newTimeline);
            }

            Project oldProject = oldTimeline?.Project;
            Project newProject = newTimeline?.Project;
            if (oldProject != newProject) {
                this.OnProjectChanged(oldProject, newProject);
            }
        }

        /// <summary>
        /// Invoked when this clip's project changes. The cause of this can be many, such as a clip being added to or
        /// removed from a track, our track being added to a timeline, our track's timeline's project changing (only
        /// possible with composition timelines), and possibly other causes maybe. The old and new project will not match
        /// </summary>
        /// <param name="oldProject">The previous project</param>
        /// <param name="newProject">The new project</param>
        protected virtual void OnProjectChanged(Project oldProject, Project newProject) {

        }

        public bool IntersectsFrameAt(long playHead) {
            return this.span.Intersects(playHead);
        }

        /// <summary>
        /// Shrinks this clips and creates a clone in front of this clip, effectively "splitting" this clip into 2
        /// </summary>
        /// <param name="offset">The frame to split this clip at, relative to this clip</param>
        public void CutAt(long offset) {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");
            if (offset == 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be zero");
            if (offset >= this.span.Duration)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot exceed our span's range");
            long begin = this.span.Begin;
            FrameSpan spanLeft = FrameSpan.FromIndex(begin, begin + offset);
            FrameSpan spanRight = FrameSpan.FromIndex(spanLeft.EndIndex, this.span.EndIndex);

            Clip clone = this.Clone();
            this.Track.AddClip(clone);

            this.FrameSpan = spanLeft;
            clone.FrameSpan = spanRight;
        }

        public void Duplicate() {
            
        }

        public long ConvertRelativeToTimelineFrame(long relative) => this.span.Begin + relative;

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) {
            long frame = timeline - this.span.Begin;
            inRange = frame >= 0 && frame < this.span.Duration;
            return frame;
        }

        public bool IsTimelineFrameInRange(long timeline) {
            long frame = timeline - this.span.Begin;
            return frame >= 0 && frame < this.span.Duration;
        }

        public bool IsRelativeFrameInRange(long relative) {
            return relative >= 0 && relative < this.span.Duration;
        }

        public bool IsAutomated(Parameter parameter) {
            return this.AutomationData.IsAutomated(parameter);
        }

        public abstract bool IsEffectTypeAccepted(Type effectType);

        public void AddEffect(BaseEffect effect) {
            this.InsertEffect(this.internalEffectList.Count, effect);
        }

        public void InsertEffect(int index, BaseEffect effect) {
            BaseEffect.ValidateInsertEffect(this, effect, index);
            this.internalEffectList.Insert(index, effect);
            BaseEffect.OnAddedInternal(this, effect);
            this.OnEffectAdded(index, effect);
        }

        public bool RemoveEffect(BaseEffect effect) {
            if (effect.Owner != this)
                return false;

            int index = this.internalEffectList.IndexOf(effect);
            if (index == -1) {
                // what to do here?????
                Debug.WriteLine("EFFECT OWNER MATCHES THIS CLIP BUT IT IS NOT PLACED IN THE COLLECTION!!!");
                Debugger.Break();
                return false;
            }

            this.RemoveEffectAtInternal(index, effect);
            return true;
        }

        public void RemoveEffectAt(int index) {
            BaseEffect effect = this.internalEffectList[index];
            if (!ReferenceEquals(effect.Owner, this)) {
                Debug.WriteLine("EFFECT STORED IN CLIP HAS A MISMATCHING OWNER!!!");
                Debugger.Break();
            }

            this.RemoveEffectAtInternal(index, effect);
        }

        public void MoveEffect(int oldIndex, int newIndex) {
            if (newIndex < 0 || newIndex >= this.internalEffectList.Count)
                throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
            BaseEffect effect = this.internalEffectList[oldIndex];
            this.internalEffectList.RemoveAt(oldIndex);
            this.internalEffectList.Insert(newIndex, effect);
            this.EffectMoved?.Invoke(this, effect, oldIndex, newIndex);
        }

        private void RemoveEffectAtInternal(int index, BaseEffect effect) {
            this.internalEffectList.RemoveAt(index);
            BaseEffect.OnRemovedInternal(effect);
            this.OnEffectRemoved(index, effect);
        }

        private void OnEffectAdded(int index, BaseEffect effect) {
            this.EffectAdded?.Invoke(this, effect, index);
        }

        private void OnEffectRemoved(int index, BaseEffect effect) {
            this.EffectRemoved?.Invoke(this, effect, index);
        }

        public virtual void Destroy() {
            for (int i = this.Effects.Count - 1; i >= 0; i--) {
                BaseEffect effect = this.Effects[i];
                effect.Destroy();
                this.RemoveEffectAt(i);
            }
        }

        internal static void OnAddedToTrack(Clip clip, Track track) {
            Track oldTrack = clip.Track;
            if (ReferenceEquals(oldTrack, track)) {
                throw new Exception("Clip added to the same track?");
            }

            clip.Track = track;
            clip.OnTrackChanged(oldTrack, track);
        }

        internal static void OnRemovedFromTrack(Clip clip) {
            Track oldTrack = clip.Track;
            if (ReferenceEquals(oldTrack, null)) {
                throw new Exception("Clip removed from no track???");
            }

            clip.Track = null;
            clip.OnTrackChanged(oldTrack, null);
        }

        internal static void OnMovedToTrack(Clip clip, Track oldTrack, Track newTrack) {
            clip.Track = newTrack;
            clip.OnTrackChanged(oldTrack, newTrack);
        }

        internal static void OnTrackTimelineChanged(Clip clip, Timeline oldTimeline, Timeline newTimeline) {
            clip.TimelineChanged?.Invoke(clip, oldTimeline, newTimeline);
            foreach (BaseEffect effect in clip.Effects) {
                BaseEffect.OnClipTimelineChanged(effect, oldTimeline, newTimeline);
            }
        }
    }
}