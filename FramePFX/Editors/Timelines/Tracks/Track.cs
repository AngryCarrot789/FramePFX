//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Serialisation;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.RBC;
using FramePFX.Utils;
using FramePFX.Utils.Destroying;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Tracks
{
    public delegate void TrackEventHandler(Track track);

    public delegate void TrackSelectedEventHandler(Track track, bool isPrimarySelection);

    public delegate void TrackClipIndexEventHandler(Track track, Clip clip, int index);

    public delegate void ClipMovedEventHandler(Clip clip, Track oldTrack, int oldIndex, Track newTrack, int newIndex);

    public abstract class Track : IDisplayName, IAutomatable, IHaveEffects, IDestroy
    {
        public static readonly SerialisationRegistry SerialisationRegistry;

        public const double MinimumHeight = 20;
        public const double DefaultHeight = 56;
        public const double MaximumHeight = 250;
        public const double HeightCmpTol = 0.0000000000001D;

        public string FactoryId => TrackFactory.Instance.GetId(this.GetType());

        public Timeline Timeline { get; private set; }

        public Project Project { get; private set; }

        public ReadOnlyCollection<Clip> Clips { get; }

        public IReadOnlyList<BaseEffect> Effects => this.internalEffectList;

        public double Height
        {
            get => this.height;
            set
            {
                value = Maths.Clamp(value, MinimumHeight, MaximumHeight);
                if (Maths.Equals(this.height, value, HeightCmpTol))
                    return;
                this.height = value;
                this.HeightChanged?.Invoke(this);
            }
        }

        public string DisplayName
        {
            get => this.displayName;
            set
            {
                string oldName = this.displayName;
                if (oldName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this, oldName, value);
            }
        }

        public SKColor Colour
        {
            get => this.colour;
            set
            {
                if (this.colour == value)
                    return;
                this.colour = value;
                this.ColourChanged?.Invoke(this);
            }
        }

        public bool IsSelected => this.isSelected;

        public long LargestFrameInUse => this.cache.LargestActiveFrame;

        public IEnumerable<Clip> SelectedClips => this.selectedClips;

        public int SelectedClipsCount => this.selectedClips.Count;

        /// <summary>
        /// Gets the index of this track within our owner timeline
        /// </summary>
        public int IndexInTimeline => this.indexInTimeline;

        public AutomationData AutomationData { get; }

        public event TrackClipIndexEventHandler ClipAdded;
        public event TrackClipIndexEventHandler ClipRemoved;
        public event ClipMovedEventHandler ClipMovedTracks;
        public event TrackEventHandler HeightChanged;
        public event DisplayNameChangedEventHandler DisplayNameChanged;
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

        protected Track()
        {
            this.indexInTimeline = -1;
            this.clips = new List<Clip>();
            this.Clips = new ReadOnlyCollection<Clip>(this.clips);
            this.cache = new ClipRangeCache();
            this.cache.FrameDataChanged += this.OnRangeCachedFrameDataChanged;
            this.internalEffectList = new List<BaseEffect>();
            this.colour = RenderUtils.RandomColour();
            this.selectedClips = new List<Clip>();
            this.AutomationData = new AutomationData(this);
        }

        static Track()
        {
            SerialisationRegistry = new SerialisationRegistry();
            SerialisationRegistry.Register<Track>(0, (track, data, ctx) =>
            {
                // should maybe guard against NaN/Infinity?
                track.Height = Maths.Clamp(data.GetDouble(nameof(track.Height), DefaultHeight), MinimumHeight, MaximumHeight);
                track.DisplayName = data.GetString(nameof(track.DisplayName), null);
                if (data.TryGetUInt(nameof(track.Colour), out uint colourU32))
                    track.Colour = new SKColor(colourU32);
                track.AutomationData.ReadFromRBE(data.GetDictionary(nameof(track.AutomationData)));
                BaseEffect.ReadSerialisedWithIdList(track, data.GetList("Effects"));
                foreach (RBEDictionary dictionary in data.GetList(nameof(track.Clips)).Cast<RBEDictionary>())
                {
                    track.AddClip(Clip.ReadSerialisedWithId(dictionary));
                }
            }, (track, data, ctx) =>
            {
                data.SetDouble(nameof(track.Height), track.Height);
                data.SetString(nameof(track.DisplayName), track.DisplayName);
                data.SetUInt(nameof(track.Colour), (uint) track.Colour);
                track.AutomationData.WriteToRBE(data.CreateDictionary(nameof(track.AutomationData)));
                RBEList list = data.CreateList(nameof(track.Clips));
                BaseEffect.WriteSerialisedWithIdList(track, data.CreateList("Effects"));
                foreach (Clip clip in track.clips)
                {
                    Clip.WriteSerialisedWithId(list.AddDictionary(), clip);
                }
            });
        }

        public bool GetRelativePlayHead(out long playHead)
        {
            playHead = this.Timeline?.PlayHeadPosition ?? 0L;
            return true;
        }

        /// <summary>
        /// Sets this track's selected state
        /// </summary>
        /// <param name="value">The new selection state</param>
        /// <param name="isPrimary">Represents the UI focused state, as if the user clicked on the track to focus it</param>
        public void SetIsSelected(bool value, bool isPrimary = false)
        {
            bool isSelectionChange = this.isSelected != value;
            if (isSelectionChange || isPrimary)
            {
                if (isSelectionChange)
                {
                    this.isSelected = value;
                    Timeline.InternalOnTrackSelectedChanged(this);
                }

                this.IsSelectedChanged?.Invoke(this, isPrimary);
            }
        }

        public bool IsAutomated(Parameter parameter)
        {
            return this.AutomationData.IsAutomated(parameter);
        }

        private void OnRangeCachedFrameDataChanged(ClipRangeCache handler)
        {
            this.Timeline?.UpdateLargestFrame();
        }

        protected virtual void OnProjectChanged(Project oldProject, Project newProject)
        {
        }

        public Track Clone() => this.Clone(TrackCloneOptions.Default);

        public Track Clone(TrackCloneOptions options)
        {
            string id = this.FactoryId;
            Track track = TrackFactory.Instance.NewTrack(id);
            this.LoadDataIntoClone(track, options);
            return track;
        }

        protected virtual void LoadDataIntoClone(Track clone, TrackCloneOptions options)
        {
            clone.height = Maths.Clamp(this.height, MinimumHeight, MaximumHeight);
            clone.displayName = this.displayName;
            clone.colour = this.colour;

            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
            foreach (BaseEffect effect in this.Effects)
            {
                if (effect.IsCloneable)
                    clone.AddEffect(effect.Clone());
            }

            if (options.ClipCloneOptions is ClipCloneOptions clipCloneOptions)
            {
                for (int i = 0; i < this.clips.Count; i++)
                {
                    clone.InsertClip(i, this.clips[i].Clone(clipCloneOptions));
                }
            }
        }

        public void AddClip(Clip clip) => this.InsertClip(this.clips.Count, clip);

        public void InsertClip(int index, Clip clip)
        {
            if (!this.IsClipTypeAccepted(clip.GetType()))
                throw new InvalidOperationException("This track (" + this.GetType().Name + ") does not accept the clip type " + clip.GetType().Name);
            if (this.clips.Contains(clip))
                throw new InvalidOperationException("This track already contains the clip");
            this.InternalInsertClipAt(index, clip);
            Clip.InternalOnClipAddedToTrack(clip, this);
            this.ClipAdded?.Invoke(this, clip, index);
            this.InvalidateRender();
        }

        public bool RemoveClip(Clip clip)
        {
            int index = this.clips.IndexOf(clip);
            if (index == -1)
                return false;
            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index)
        {
            Clip clip = this.clips[index];
            this.InternalRemoveClipAt(index, clip);
            Clip.InternalOnClipRemovedFromTrack(clip);
            this.ClipRemoved?.Invoke(this, clip, index);
            this.InvalidateRender();
        }

        public void MoveClipToTrack(int srcIndex, Track dstTrack, int dstIndex)
        {
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
            Clip.InternalOnClipMovedToTrack(clip, this, dstTrack);
            this.ClipMovedTracks?.Invoke(clip, this, srcIndex, dstTrack, dstIndex);
            dstTrack.ClipMovedTracks?.Invoke(clip, this, srcIndex, dstTrack, dstIndex);
            this.InvalidateRender();
            dstTrack.InvalidateRender();
        }

        private void InternalInsertClipAt(int index, Clip clip)
        {
            this.clips.Insert(index, clip);
            this.cache.OnClipAdded(clip);
            if (clip.IsSelected)
                this.selectedClips.Add(clip);
        }

        private void InternalRemoveClipAt(int index, Clip clip)
        {
            this.clips.RemoveAt(index);
            this.cache.OnClipRemoved(clip);
            if (clip.IsSelected)
                this.selectedClips.Remove(clip);
        }

        public abstract bool IsClipTypeAccepted(Type type);

        public bool IsRegionEmpty(FrameSpan span) => this.cache.IsRegionEmpty(span);

        public Clip GetClipAtFrame(long frame) => this.cache.GetPrimaryClipAt(frame);

        public void ClearClipSelection(Clip except = null)
        {
            // Use back to front removal since OnIsClipSelectedChanged can process
            // that more efficiently, and in general, back to front is more efficient
            List<Clip> list = this.selectedClips;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Clip clip = list[i];
                if (clip != except)
                    clip.IsSelected = false;
            }
        }

        public void SelectAll()
        {
            foreach (Clip clip in this.Clips)
            {
                clip.IsSelected = true;
            }
        }

        public void InvalidateRender()
        {
            this.Timeline?.RenderManager.InvalidateRender();
        }

        public virtual void Destroy()
        {
            this.ClearTrack();
        }

        /// <summary>
        /// Destroys and removes all clips. All clips are removed back to front so this method calls <see cref="ClipRemoved"/> for every clip
        /// </summary>
        public void ClearTrack()
        {
            for (int i = this.clips.Count - 1; i >= 0; i--)
            {
                Clip clip = this.clips[i];
                clip.Destroy();
                this.RemoveClipAt(i);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} ({this.clips.Count.ToString()} clips between {this.cache.SmallestActiveFrame.ToString()} and {this.cache.LargestActiveFrame.ToString()})";
        }

        /// <summary>
        /// Adds all clips within the given frame span to the given list
        /// </summary>
        /// <param name="list">The destination list</param>
        /// <param name="span">The span range</param>
        public void CollectClipsInSpan(List<Clip> list, FrameSpan span)
        {
            this.cache.GetClipsInRange(list, span);
        }

        public List<Clip> GetClipsInSpan(FrameSpan span)
        {
            List<Clip> list = new List<Clip>();
            this.CollectClipsInSpan(list, span);
            return list;
        }

        public IEnumerable<Clip> GetClipsAtFrame(long frame)
        {
            List<Clip> list = new List<Clip>();
            this.cache.GetClipsAtFrame(list, frame);
            return list;
        }

        public abstract bool IsEffectTypeAccepted(Type effectType);

        public void AddEffect(BaseEffect effect)
        {
            this.InsertEffect(this.internalEffectList.Count, effect);
        }

        public void InsertEffect(int index, BaseEffect effect)
        {
            BaseEffect.ValidateInsertEffect(this, effect, index);
            this.internalEffectList.Insert(index, effect);
            BaseEffect.OnAddedInternal(this, effect);
            this.OnEffectAdded(index, effect);
        }

        public bool RemoveEffect(BaseEffect effect)
        {
            if (effect.Owner != this)
                return false;

            int index = this.internalEffectList.IndexOf(effect);
            if (index == -1)
            {
                // what to do here?????
                Debug.WriteLine("EFFECT OWNER MATCHES THIS CLIP BUT IT IS NOT PLACED IN THE COLLECTION");
                Debugger.Break();
                return false;
            }

            this.RemoveEffectAtInternal(index, effect);
            return true;
        }

        public void RemoveEffectAt(int index)
        {
            BaseEffect effect = this.Effects[index];
            if (!ReferenceEquals(effect.Owner, this))
            {
                Debug.WriteLine("EFFECT STORED IN CLIP HAS A MISMATCHING OWNER");
                Debugger.Break();
            }

            this.RemoveEffectAtInternal(index, effect);
        }

        public void MoveEffect(int oldIndex, int newIndex)
        {
            if (newIndex < 0 || newIndex >= this.internalEffectList.Count)
                throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
            BaseEffect effect = this.internalEffectList[oldIndex];
            this.internalEffectList.RemoveAt(oldIndex);
            this.internalEffectList.Insert(newIndex, effect);
            this.EffectMoved?.Invoke(this, effect, oldIndex, newIndex);
        }

        private void RemoveEffectAtInternal(int index, BaseEffect effect)
        {
            this.internalEffectList.RemoveAt(index);
            BaseEffect.OnRemovedInternal(effect);
            this.OnEffectRemoved(index, effect);
        }

        private void OnEffectAdded(int index, BaseEffect effect)
        {
            this.EffectAdded?.Invoke(this, effect, index);
        }

        private void OnEffectRemoved(int index, BaseEffect effect)
        {
            this.EffectRemoved?.Invoke(this, effect, index);
        }

        public FrameSpan GetSpanUntilClipOrLimitedDuration(long frame, long defaultDuration = 300, long maxDurationLimit = 100000000)
        {
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
        public bool TryGetSpanUntilClip(long frame, out FrameSpan span, long defaultDuration = 300, long maxDurationLimit = 100000000U)
        {
            long minimum = long.MaxValue;
            if (this.clips.Count > 0)
            {
                foreach (Clip clip in this.clips)
                {
                    long begin = clip.FrameSpan.Begin;
                    if (begin >= frame)
                    {
                        if (clip.IntersectsFrameAt(frame))
                        {
                            span = default;
                            return false;
                        }
                        else
                        {
                            minimum = Math.Min(begin, minimum);
                            if (minimum <= frame)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (minimum > frame && minimum != long.MaxValue)
            {
                span = new FrameSpan(frame, Math.Min(minimum - frame, maxDurationLimit));
            }
            else
            {
                span = new FrameSpan(frame, defaultDuration);
            }

            return true;
        }

        #region Internal Access Helpers -- Used internally only

        internal static void InternalOnAddedToTimeline(Track track, Timeline timeline)
        {
            InternalOnTrackTimelineChanged(track, null, timeline);
        }

        internal static void InternalOnRemovedFromTimeline1(Track track, Timeline timeline)
        {
            InternalOnTrackTimelineChanged(track, timeline, null);
        }

        internal static void InternalOnTrackTimelineChanged(Track track, Timeline oldTimeline, Timeline newTimeline)
        {
            track.Timeline = newTimeline;
            track.Project = newTimeline?.Project;
            track.TimelineChanged?.Invoke(track, oldTimeline, newTimeline);
            Project oldProject = oldTimeline?.Project;
            Project newProject = newTimeline?.Project;
            if (!ReferenceEquals(oldProject, newProject))
            {
                track.Project = newProject;
                track.OnProjectChanged(oldProject, newProject);
            }

            foreach (Clip clip in track.clips)
            {
                Clip.InternalOnTrackTimelineChanged(clip, oldTimeline, newTimeline);
            }
        }

        internal static void InternalOnClipSpanChanged(Clip clip, FrameSpan oldSpan)
        {
            clip.Track?.cache.OnSpanChanged(clip, oldSpan);
        }

        internal static void InternalOnIsClipSelectedChanged(Clip clip)
        {
            // If the track is null, it means the clip was either removed previously
            // and therefore its selection has already been processed, or has never been
            // added and therefore we don't care about the new selected state
            if (clip.Track == null)
                return;

            List<Clip> list = clip.Track.selectedClips;
            if (clip.IsSelected)
            {
                list.Add(clip);
            }
            else if (list.Count > 0)
            {
                if (list[0] == clip)
                {
                    // check front to back removal
                    list.RemoveAt(0);
                }
                else
                {
                    // assume back to front removal
                    int index = list.LastIndexOf(clip);
                    if (index == -1)
                    {
                        throw new Exception("Clip was never selected");
                    }

                    list.RemoveAt(index);
                }
            }
        }

        internal static void InternalSetPrecomputedTrackIndex(Track track, int newIndex)
        {
            track.indexInTimeline = newIndex;
        }

        internal static void InternalOnTimelineProjectChanged(Track track, Project oldProject, Project newProject)
        {
            if (ReferenceEquals(track.Project, newProject))
            {
                throw new InvalidOperationException("Fatal error: clip's project equals the new project???");
            }

            track.Project = newProject;
            track.OnProjectChanged(oldProject, newProject);
            foreach (Clip clip in track.clips)
            {
                Clip.InternalOnTimelineProjectChanged(clip, oldProject, newProject);
            }
        }

        // Only used for faster code
        internal static List<BaseEffect> InternalGetEffectListUnsafe(Track track) => track.internalEffectList;

        #endregion
    }
}