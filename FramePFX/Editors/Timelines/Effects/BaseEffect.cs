using System;
using FramePFX.Destroying;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;

namespace FramePFX.Editors.Timelines.Effects {
    public abstract class BaseEffect : IStrictFrameRange, IAutomatable, IDestroy {
        /// <summary>
        /// Gets the object that this effect is applied to. At the moment, this is either a <see cref="Clip"/> or <see cref="Track"/>
        /// </summary>
        public IHaveEffects Owner { get; private set; }

        /// <summary>
        /// Returns true when <see cref="Owner"/> is a clip, otherwise it is a track or null.
        /// This will return true for any type of clip remember!
        /// </summary>
        public bool IsClipEffect => this.Owner is Clip;

        /// <summary>
        /// Returns true when <see cref="Owner"/> is a <see cref="Track"/>, otherwise it is a clip or null.
        /// This will return true for any type of track remember!
        /// </summary>
        public bool IsTrackEffect => this.Owner is Track;

        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="Clip"/>
        /// </summary>
        public Clip OwnerClip => this.Owner as Clip;

        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="Track"/>
        /// </summary>
        public Track OwnerTrack => this.Owner as Track;

        public Timeline Timeline => this.Owner?.Timeline;

        public Project Project => this.Owner?.Project;

        public event TimelineChangedEventHandler TimelineChanged;

        public AutomationData AutomationData { get; }

        /// <summary>
        /// Returns true if this effect can be cloned. Default is true
        /// </summary>
        public virtual bool IsCloneable => true;

        /// <summary>
        /// This clip's factory ID, used for creating a new instance dynamically via reflection
        /// </summary>
        public string FactoryId => EffectFactory.Instance.GetId(this.GetType());

        protected BaseEffect() {
            this.AutomationData = new AutomationData(this);
        }

        public bool GetRelativePlayHead(out long playHead) {
            playHead = this.ConvertTimelineToRelativeFrame(this.Timeline?.PlayHeadPosition ?? 0, out bool isInRange);
            return isInRange;
        }

        public BaseEffect Clone() {
            if (!this.IsCloneable)
                throw new InvalidOperationException("This effect cannot be cloned");
            BaseEffect clone = EffectFactory.Instance.NewEffect(this.FactoryId);
            this.LoadDataIntoClone(clone);
            return clone;
        }

        public virtual bool IsObjectValidForOwner(IHaveEffects owner) => true;

        protected virtual void LoadDataIntoClone(BaseEffect clone) {
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
        }

        public static BaseEffect ReadSerialisedWithId(RBEDictionary dictionary) {
            string id = dictionary.GetString(nameof(FactoryId));
            RBEDictionary data = dictionary.GetDictionary("Data");
            BaseEffect effect = EffectFactory.Instance.NewEffect(id);
            effect.ReadFromRBE(data);
            return effect;
        }

        public static void WriteSerialisedWithIdList(IHaveEffects srcOwner, RBEList list) {
            foreach (BaseEffect effect in srcOwner.Effects) {
                if (!(effect.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + effect.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(FactoryId), id);
                effect.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public static void ReadSerialisedWithIdList(IHaveEffects dstOwner, RBEList list) {
            foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
                string factoryId = dictionary.GetString(nameof(FactoryId));
                BaseEffect effect = EffectFactory.Instance.NewEffect(factoryId);
                effect.ReadFromRBE(dictionary.GetDictionary("Data"));
                dstOwner.AddEffect(effect);
            }
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
        }

        protected virtual void OnAdded() {
            Timeline timeline = this.Owner.Timeline;
            if (timeline != null) {
                this.TimelineChanged?.Invoke(this, null, timeline);
            }
        }

        protected virtual void OnRemoved() {
            Timeline oldTimeline = this.Owner.Timeline;
            if (oldTimeline != null) {
                this.TimelineChanged?.Invoke(this, oldTimeline, null);
            }
        }

        public long ConvertRelativeToTimelineFrame(long relative) {
            return this.Owner is Clip clip ? clip.ConvertRelativeToTimelineFrame(relative) : relative;
        }

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) {
            if (this.Owner is Clip clip) {
                return clip.ConvertTimelineToRelativeFrame(timeline, out inRange);
            }

            inRange = this.Owner is Track;
            return timeline;
        }

        public bool IsTimelineFrameInRange(long timeline) {
            return this.Owner is Clip clip ? clip.IsTimelineFrameInRange(timeline) : this.Owner != null;
        }

        public bool IsRelativeFrameInRange(long relative) {
            return this.Owner is Clip clip ? clip.IsRelativeFrameInRange(relative) : this.Owner != null;
        }

        public bool IsAutomated(Parameter parameter) {
            return this.AutomationData.IsAutomated(parameter);
        }
        
        public static void OnAddedInternal(IHaveEffects owner, BaseEffect effect) {
            effect.Owner = owner;
            effect.OnAdded();
        }

        public static void OnRemovedInternal(BaseEffect effect) {
            try {
                effect.OnRemoved();
            }
            finally { // just in case it throws... not that it wouldn't crash the app anyway but still
                effect.Owner = null;
            }
        }

        internal static void ValidateInsertEffect(IHaveEffects owner, BaseEffect effect, int index) {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect), "Effect cannot be null");
            if (effect.Owner != null)
                throw new InvalidOperationException("Effect already exists in another object");
            if (index < 0 || index > owner.Effects.Count)
                throw new IndexOutOfRangeException($"Index is out of range: {index} < 0 || {index} > {owner.Effects.Count}");
            if (!owner.IsEffectTypeAccepted(effect.GetType()))
                throw new InvalidOperationException("Owner does not accept the effect type: " + effect.GetType());
            if (!effect.IsObjectValidForOwner(owner))
                throw new InvalidOperationException("Effect does not accept the owner type: " + owner.GetType());
            if (owner.Effects.Contains(effect))
                throw new InvalidOperationException("Cannot add an effect that was already added");
        }

        internal static void OnClipTimelineChanged(BaseEffect effect, Timeline oldTimeline, Timeline newTimeline) {
            effect.TimelineChanged?.Invoke(effect, oldTimeline, newTimeline);
        }

        public virtual void Destroy() {

        }
    }
}