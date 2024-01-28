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

namespace FramePFX.Editors.Timelines.Clips {
    public delegate void ClipSpanChangedEventHandler(Clip clip, FrameSpan oldSpan, FrameSpan newSpan);
    public delegate void ClipEventHandler(Clip clip);
    public delegate void ClipTrackChangedEventHandler(Clip clip, Track oldTrack, Track newTrack);
    public delegate void ClipActiveSequenceChangedEventHandler(Clip clip, AutomationSequence oldSequence, AutomationSequence newSequence);

    public abstract class Clip : IAutomatable, IStrictFrameRange, IResourceHolder, IHaveEffects, IDestroy {
        private readonly List<BaseEffect> internalEffectList;
        private FrameSpan span;
        private string displayName;
        private bool isSelected;
        private AutomationSequence activeSequence;

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

        public FrameSpan FrameSpan {
            get => this.span;
            set {
                FrameSpan oldSpan = this.span;
                if (oldSpan == value)
                    return;
                this.span = value;
                Track.InternalOnClipSpanChanged(this, oldSpan);
                this.OnFrameSpanChanged(oldSpan, value);
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
        }

        public void MoveToTrack(Track dstTrack) {
            this.MoveToTrack(dstTrack, dstTrack.Clips.Count);
        }

        public void MoveToTrack(Track dstTrack, int dstIndex) {
            if (this.Track == null) {
                dstTrack.InsertClip(dstIndex, this);
                return;
            }

            int index = this.Track.Clips.IndexOf(this);
            if (index == -1) {
                throw new Exception("Clip did not exist in its owner track");
            }

            this.Track.MoveClipToTrack(index, dstTrack, dstIndex);
        }

        protected virtual void OnTrackChanged(Track oldTrack, Track newTrack) {
            this.ResourceHelper.SetManager(newTrack?.Project?.ResourceManager);
            this.TrackChanged?.Invoke(this, oldTrack, newTrack);

            Timeline oldTimeline = oldTrack?.Timeline;
            Timeline newTimeline = newTrack?.Timeline;

            if (!ReferenceEquals(oldTimeline, newTimeline)) {
                this.TimelineChanged?.Invoke(this, oldTimeline, newTimeline);
            }
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