using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FramePFX.Destroying;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Tracks {
    public delegate void TrackEventHandler(Track track);
    public delegate void TrackSelectedEventHandler(Track track, bool isPrimarySelection);
    public delegate void TrackClipIndexEventHandler(Track track, Clip clip, int index);
    public delegate void ClipMovedEventHandler(Clip clip, Track oldTrack, int oldIndex, Track newTrack, int newIndex);

    public abstract class Track : IAutomatable, IHaveEffects, IDestroy {
        public const double MinimumHeight = 20;
        public const double DefaultHeight = 56;
        public const double MaximumHeight = 250;

        public string FactoryId => TrackFactory.Instance.GetId(this.GetType());

        public Timeline Timeline { get; private set; }

        public long RelativePlayHead => this.Timeline?.PlayHeadPosition ?? 0;

        public Project Project => this.Timeline?.Project;

        public ReadOnlyCollection<Clip> Clips { get; }

        public ReadOnlyCollection<BaseEffect> Effects { get; }

        public double Height {
            get => this.height;
            set {
                value = Maths.Clamp(value, MinimumHeight, MaximumHeight);
                if (this.height == value)
                    return;
                this.height = value;
                this.HeightChanged?.Invoke(this);
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

        public SKColor Colour {
            get => this.colour;
            set {
                if (this.colour == value)
                    return;
                this.colour = value;
                this.ColourChanged?.Invoke(this);
            }
        }

        public bool IsSelected => this.isSelected;

        public long LargestFrameInUse => this.cache.LargestActiveFrame;

        public IEnumerable<Clip> SelectedClips => this.selectedClips;

        public int SelectedClipCount => this.selectedClips.Count;

        /// <summary>
        /// Gets the index of this track within our owner timeline
        /// </summary>
        public int IndexInTimeline => this.indexInTimeline;

        public AutomationData AutomationData { get; }

        public event TrackClipIndexEventHandler ClipAdded;
        public event TrackClipIndexEventHandler ClipRemoved;
        public event ClipMovedEventHandler ClipMovedTracks;
        public event TrackEventHandler HeightChanged;
        public event TrackEventHandler DisplayNameChanged;
        public event TrackEventHandler ColourChanged;
        public event TrackSelectedEventHandler IsSelectedChanged;
        public event EffectOwnerEventHandler EffectAdded;
        public event EffectOwnerEventHandler EffectRemoved;
        public event EffectMovedEventHandler EffectMoved;

        public event TimelineChangedEventHandler TimelineChanged;

        private readonly List<Clip> clips;
        private readonly ClipRangeCache cache;
        private readonly List<Clip> selectedClips;
        private readonly List<BaseEffect> internalEffectList;
        private double height = DefaultHeight;
        private string displayName = "Track";
        private SKColor colour;
        private bool isSelected;
        private int indexInTimeline; // updated by timeline

        protected Track() {
            this.indexInTimeline = -1;
            this.clips = new List<Clip>();
            this.Clips = new ReadOnlyCollection<Clip>(this.clips);
            this.cache = new ClipRangeCache();
            this.cache.FrameDataChanged += this.OnRangeCachedFrameDataChanged;
            this.internalEffectList = new List<BaseEffect>();
            this.Effects = this.internalEffectList.AsReadOnly();
            this.colour = RenderUtils.RandomColour();
            this.selectedClips = new List<Clip>();
            this.AutomationData = new AutomationData(this);
        }

        /// <summary>
        /// Sets this track's selected state
        /// </summary>
        /// <param name="value">The new selection state</param>
        /// <param name="isPrimary">Represents the UI focused state, as if the user clicked on the track to focus it</param>
        public void SetIsSelected(bool value, bool isPrimary = false) {
            bool isSelectionChange = this.isSelected != value;
            if (isSelectionChange || isPrimary) {
                if (isSelectionChange) {
                    this.isSelected = value;
                    Timeline.InternalOnTrackSelectedChanged(this);
                }

                this.IsSelectedChanged?.Invoke(this, isPrimary);
            }
        }

        public bool IsAutomated(Parameter parameter) {
            return this.AutomationData.IsAutomated(parameter);
        }

        private void OnRangeCachedFrameDataChanged(ClipRangeCache handler) {
            this.Timeline?.UpdateLargestFrame();
        }

        public Track Clone() => this.Clone(TrackCloneOptions.Default);

        public Track Clone(TrackCloneOptions options) {
            string id = this.FactoryId;
            Track track = TrackFactory.Instance.NewTrack(id);
            this.LoadDataIntoClone(track, options);
            return track;
        }

        protected virtual void LoadDataIntoClone(Track clone, TrackCloneOptions options) {
            clone.height = Maths.Clamp(this.height, MinimumHeight, MaximumHeight);
            clone.displayName = this.displayName;
            clone.colour = this.colour;
            if (options.CloneClips) {
                for (int i = 0; i < this.clips.Count; i++) {
                    clone.InsertClip(i, this.clips[i].Clone(options.ClipCloneOptions));
                }
            }
        }

        public void AddClip(Clip clip) => this.InsertClip(this.clips.Count, clip);

        public void InsertClip(int index, Clip clip) {
            if (!this.IsClipTypeAccepted(clip.GetType()))
                throw new InvalidOperationException("This track (" + this.GetType().Name + ") does not accept the clip type " + clip.GetType().Name);
            if (this.clips.Contains(clip))
                throw new InvalidOperationException("This track already contains the clip");
            this.InternalInsertClipAt(index, clip);
            Clip.OnAddedToTrack(clip, this);
            this.ClipAdded?.Invoke(this, clip, index);
            this.InvalidateRender();
        }

        public bool RemoveClip(Clip clip) {
            int index = this.clips.IndexOf(clip);
            if (index == -1)
                return false;
            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            this.InternalRemoveClipAt(index, clip);
            Clip.OnRemovedFromTrack(clip);
            this.ClipRemoved?.Invoke(this, clip, index);
            this.InvalidateRender();
        }

        public void MoveClipToTrack(int srcIndex, Track dstTrack, int dstIndex) {
            if (dstTrack == null)
                throw new ArgumentOutOfRangeException(nameof(dstTrack));
            if (dstIndex < 0 || dstIndex > dstTrack.clips.Count)
                throw new ArgumentOutOfRangeException(nameof(dstIndex), "dstIndex is out of range");
            if (dstTrack.Timeline != this.Timeline)
                throw new ArgumentException("Clips cannot be moved across timelines");
            Clip clip = this.clips[srcIndex];
            if (!dstTrack.IsClipTypeAccepted(clip.GetType()))
                throw new InvalidOperationException("The destination track (" + dstTrack.GetType().Name + ") does not accept the clip type " + clip.GetType().Name);
            this.InternalRemoveClipAt(srcIndex, clip);
            dstTrack.InternalInsertClipAt(dstIndex, clip);
            Clip.OnMovedToTrack(clip, this, dstTrack);
            this.ClipMovedTracks?.Invoke(clip, this, srcIndex, dstTrack, dstIndex);
            dstTrack.ClipMovedTracks?.Invoke(clip, this, srcIndex, dstTrack, dstIndex);
            this.InvalidateRender();
            dstTrack.InvalidateRender();
        }

        private void InternalInsertClipAt(int index, Clip clip) {
            this.clips.Insert(index, clip);
            this.cache.OnClipAdded(clip);
            if (clip.IsSelected)
                this.selectedClips.Add(clip);
        }

        private void InternalRemoveClipAt(int index, Clip clip) {
            this.clips.RemoveAt(index);
            this.cache.OnClipRemoved(clip);
            if (clip.IsSelected)
                this.selectedClips.Remove(clip);
            Timeline.OnClipRemovedFromTrack(this, clip);
        }

        public abstract bool IsClipTypeAccepted(Type type);

        public bool IsRegionEmpty(FrameSpan span) => this.cache.IsRegionEmpty(span);

        public Clip GetClipAtFrame(long frame) => this.cache.GetPrimaryClipAt(frame);

        public void ClearClipSelection(Clip except = null) {
            // Use back to front removal since OnIsClipSelectedChanged can process
            // that more efficiently, and in general, back to front is more efficient
            List<Clip> list = this.selectedClips;
            for (int i = list.Count - 1; i >= 0; i--) {
                Clip clip = list[i];
                if (clip != except)
                    clip.IsSelected = false;
            }

            Timeline.InternalOnTrackSelectionCleared(this);
        }

        public void SelectAll() {
            foreach (Clip clip in this.Clips) {
                clip.IsSelected = true;
            }
        }

        public void InvalidateRender() {
            this.Timeline?.InvalidateRender();
        }

        public virtual void Destroy() {
            for (int i = this.clips.Count - 1; i >= 0; i--) {
                Clip clip = this.clips[i];
                clip.Destroy();
                this.RemoveClipAt(i);
            }
        }

        public override string ToString() {
            return $"{this.GetType().Name} ({this.clips.Count.ToString()} clips between {this.cache.SmallestActiveFrame.ToString()} and {this.cache.LargestActiveFrame.ToString()})";
        }

        /// <summary>
        /// Adds all clips within the given frame span to the given list
        /// </summary>
        /// <param name="list">The destination list</param>
        /// <param name="span">The span range</param>
        public void CollectClipsInSpan(List<Clip> list, FrameSpan span) {
            this.cache.GetClipsInRange(list, span);
        }

        public List<Clip> GetClipsInSpan(FrameSpan span) {
            List<Clip> list = new List<Clip>();
            this.CollectClipsInSpan(list, span);
            return list;
        }

        public IEnumerable<Clip> GetClipsAtFrame(long frame) {
            List<Clip> list = new List<Clip>();
            this.cache.GetClipsAtFrame(list, frame);
            return list;
        }

        public abstract bool IsEffectTypeAccepted(Type effectType);

        public void AddEffect(BaseEffect effect) {
            this.InsertEffect(this.internalEffectList.Count, effect);
        }

        public void InsertEffect(int index, BaseEffect effect) {
            BaseEffect.ValidateInsertEffect(this, effect, index);
            BaseEffect.OnAddedInternal(this, effect);
            this.OnEffectAdded(index, effect);
        }

        public bool RemoveEffect(BaseEffect effect) {
            if (effect.Owner != this)
                return false;

            int index = this.internalEffectList.IndexOf(effect);
            if (index == -1) { // what to do here?????
                Debug.WriteLine("EFFECT OWNER MATCHES THIS CLIP BUT IT IS NOT PLACED IN THE COLLECTION");
                Debugger.Break();
                return false;
            }

            this.RemoveEffectAtInternal(index, effect);
            return true;
        }

        public void RemoveEffectAt(int index) {
            BaseEffect effect = this.Effects[index];
            if (!ReferenceEquals(effect.Owner, this)) {
                Debug.WriteLine("EFFECT STORED IN CLIP HAS A MISMATCHING OWNER");
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

        public FrameSpan GetSpanUntilClipOrLimitedDuration(long frame, long defaultDuration = 300, long maxDurationLimit = 100000000) {
            if (this.TryGetSpanUntilClip(frame, out FrameSpan span, defaultDuration, maxDurationLimit))
                return span;
            return new FrameSpan(frame, defaultDuration);
        }

        /// <summary>
        /// Tries to calculate a frame span that can fill in the space, starting at the frame parameter and extending
        /// either the unlimitedDuration parameter or the amount of space between frame and the nearest clip.
        /// When a clip intersects frame, this method returns false. Use <see cref="GetSpanUntilClipOrLimitedDuration"/> to return a span with defaultDuration instead
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="span">The output span</param>
        /// <param name="defaultDuration">The default duration for the span when there are no clips in the way</param>
        /// <param name="maxDurationLimit">An upper limit for how long the output span can be</param>
        /// <returns></returns>
        public bool TryGetSpanUntilClip(long frame, out FrameSpan span, long defaultDuration = 300, long maxDurationLimit = 100000000U) {
            long minimum = long.MaxValue;
            if (this.clips.Count > 0) {
                foreach (Clip clip in this.clips) {
                    long begin = clip.FrameSpan.Begin;
                    if (begin > frame) {
                        if (clip.IntersectsFrameAt(frame)) {
                            span = default;
                            return false;
                        }
                        else {
                            minimum = Math.Min(begin, minimum);
                            if (minimum <= frame) {
                                break;
                            }
                        }
                    }
                }
            }

            if (minimum > frame && minimum != long.MaxValue) {
                span = new FrameSpan(frame, Math.Min(minimum - frame, maxDurationLimit));
            }
            else {
                span = new FrameSpan(frame, defaultDuration);
            }

            return true;
        }

        #region Internal Access Helpers -- Used internally only

        internal static void InternalOnAddedToTimeline(Track track, Timeline timeline) {
            track.Timeline = timeline;
        }

        internal static void InternalOnRemovedFromTimeline1(Track track, Timeline timeline) {
            track.Timeline = null;
        }

        internal static void InternalOnClipSpanChanged(Clip clip, FrameSpan oldSpan) {
            clip.Track?.cache.OnSpanChanged(clip, oldSpan);
        }

        internal static void InternalOnTrackTimelineChanged(Track track, Timeline oldTimeline, Timeline newTimeline) {
            track.TimelineChanged?.Invoke(track, oldTimeline, newTimeline);
            foreach (Clip clip in track.clips) {
                Clip.OnTrackTimelineChanged(clip, oldTimeline, newTimeline);
            }
        }

        internal static void InternalOnIsClipSelectedChanged(Clip clip) {
            // If the track is null, it means the clip was either removed previously
            // and therefore its selection has already been processed, or has never been
            // added and therefore we don't care about the new selected state
            if (clip.Track == null)
                return;

            List<Clip> list = clip.Track.selectedClips;
            if (clip.IsSelected) {
                list.Add(clip);
            }
            else if (list.Count > 0) {
                if (list[0] == clip) { // check front to back removal
                    list.RemoveAt(0);
                }
                else { // assume back to front removal
                    int index = list.LastIndexOf(clip);
                    if (index == -1) {
                        throw new Exception("Clip was never selected");
                    }

                    list.RemoveAt(index);
                }
            }

            Timeline.OnIsClipSelectedChanged(clip);
        }

        internal static void InternalUpdateTrackIndex(Track track, int newIndex) {
            track.indexInTimeline = newIndex;
        }

        #endregion

    }
}