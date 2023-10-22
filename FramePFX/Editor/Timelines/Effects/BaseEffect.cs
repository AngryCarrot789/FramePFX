using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Effects {
    /// <summary>
    /// The base class for all types of effects (audio, video, etc.). This class supports automation
    /// </summary>
    public abstract class BaseEffect : IAutomatable, IStrictFrameRange {
        /// <summary>
        /// Whether or not this effect can be removed from a clip. This is also used to determine if an
        /// effect can be copy and pasted into another clip. When this is false, it cannot be copied nor removed
        /// </summary>
        public bool IsRemoveable { get; protected set; }

        /// <summary>
        /// This clip's factory ID, used for creating a new instance dynamically via reflection
        /// </summary>
        public string FactoryId => EffectFactory.Instance.GetTypeIdForModel(this.GetType());

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        /// <summary>
        /// The clip that this effect has been added to
        /// </summary>
        public Clip OwnerClip { get; private set; }

        public Project Project => this.OwnerClip.Track.Timeline.Project;

        protected BaseEffect() {
            this.IsRemoveable = true;
            this.AutomationData = new AutomationData(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessEffectList(IReadOnlyList<BaseEffect> effects, long frame, RenderContext render, bool isPreProcess) {
            // pre-process clip effects, such as translation, scale, etc.
            int i, count = effects.Count;
            if (count == 0) {
                return;
            }

            BaseEffect effect;
            if (isPreProcess) {
                for (i = 0; i < count; i++) {
                    if ((effect = effects[i]) is VideoEffect) {
                        ((VideoEffect) effect).PreProcessFrame(frame, render);
                    }
                }
            }
            else {
                for (i = count - 1; i >= 0; i--) {
                    if ((effect = effects[i]) is VideoEffect) {
                        ((VideoEffect) effect).PostProcessFrame(frame, render);
                    }
                }
            }
        }

        public static void AddEffectToClip(Clip clip, BaseEffect effect) {
            InsertEffectIntoClip(clip, effect, clip.Effects.Count);
        }

        public static void InsertEffectIntoClip(Clip clip, BaseEffect effect, int index) {
            if (clip == null)
                throw new NullReferenceException(nameof(clip));
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            if (index < 0 || index > clip.Effects.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and the number of effects ({clip.Effects.Count})");
            if (!clip.IsEffectTypeAllowed(effect))
                throw new Exception($"Effect type '{effect.GetType()}' is not applicable to the clip '{clip.GetType()}'");
            if (clip.Effects.Contains(effect))
                throw new Exception("Clip already contains the effect");
            if (effect.OwnerClip != null)
                throw new Exception("Effect exists in another clip");

            effect.OwnerClip = clip;
            effect.OnAddingToClip();
            Clip.InternalOnInsertingEffect(clip, index, effect);
            effect.OnAddedToClip();
            Clip.InternalOnEffectAdded(clip, index, effect);
        }

        public static bool RemoveEffectFromOwner(BaseEffect effect) {
            Clip owner = effect.OwnerClip;
            if (owner == null)
                return false;

            int index = owner.Effects.IndexOf(effect);
            if (index < 0) {
                AppLogger.WriteLine("Fatal error: Effect's owner was non-null but was not stored in its effects list");
                Debugger.Break();
                effect.OwnerClip = null;
                return false;
            }

            RemoveEffectAt(owner, index);
            return true;
        }

        public static void RemoveEffectAt(Clip clip, int index) {
            BaseEffect effect = clip.Effects[index];
            if (!ReferenceEquals(effect.OwnerClip, clip))
                throw new Exception("The effect was stored in the clip's effect list but the owner did not match the clip");
            effect.OnRemovingFromClip();
            Clip.InternalOnRemovingEffect(clip, index);
            effect.OnRemovedFromClip();
            Clip.InternalOnEffectRemoved(clip, effect);
            effect.OwnerClip = null;
        }

        public static void ClearEffects(Clip clip) {
            for (int i = clip.Effects.Count - 1; i >= 0; i--) {
                RemoveEffectAt(clip, i);
            }
        }

        /// <summary>
        /// Invoked when this effect is about to be added to <see cref="OwnerClip"/> (which is set prior to this call)
        /// </summary>
        protected virtual void OnAddingToClip() {
        }

        /// <summary>
        /// Invoked when this effect is added to the <see cref="OwnerClip"/>'s effect list
        /// </summary>
        protected virtual void OnAddedToClip() {
        }

        /// <summary>
        /// Invoked when this effect is about to be removed from the <see cref="OwnerClip"/>
        /// </summary>
        protected virtual void OnRemovingFromClip() {
        }

        /// <summary>
        /// Invoked when this effect has been removed from our previous owner's effect list.
        /// <see cref="OwnerClip"/> will be set to null after this call, meaning it is non-null when this is invoked
        /// </summary>
        protected virtual void OnRemovedFromClip() {
        }

        // TrackChanged and TrackTimelineChanged can be added or removed in
        // the added/removed methods or adding/removing methods; it doesn't matter

        // public virtual void OnClipTrackChanged(Track oldTrack, Track track) { }
        // public virtual void OnClipTrackTimelineChanged(Timeline oldTimeline, Timeline newTimeline) { }

        public virtual void WriteToRBE(RBEDictionary data) {
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
        }

        public long ConvertRelativeToTimelineFrame(long relative) {
            return this.OwnerClip?.ConvertRelativeToTimelineFrame(relative) ?? relative;
        }

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) {
            if (this.OwnerClip != null)
                return this.OwnerClip.ConvertTimelineToRelativeFrame(timeline, out inRange);
            inRange = false;
            return timeline;
        }

        public bool IsTimelineFrameInRange(long timeline) {
            return this.OwnerClip?.IsTimelineFrameInRange(timeline) ?? false;
        }

        public BaseEffect Clone() {
            RBEDictionary dictionary = new RBEDictionary();
            this.WriteToRBE(dictionary);
            BaseEffect effect = EffectFactory.Instance.CreateModel(this.FactoryId);
            effect.ReadFromRBE(dictionary);
            return effect;
        }
    }
}