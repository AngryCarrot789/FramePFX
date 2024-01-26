using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;

namespace FramePFX.Editors.Timelines.Effects {
    public abstract class BaseEffect : IStrictFrameRange, IAutomatable {
        /// <summary>
        /// Gets the object that this effect is applied to. At the moment, this is either a <see cref="Clip"/> or <see cref="Track"/>
        /// </summary>
        public IHaveEffects Owner { get; private set; }

        /// <summary>
        /// Returns true when <see cref="Owner"/> is a clip, otherwise it is a track or null
        /// </summary>
        public bool IsClipEffect => this.Owner is Clip;

        /// <summary>
        /// Returns true when <see cref="Owner"/> is a track, otherwise it is a clip or null
        /// </summary>
        public bool IsTrackEffect => this.Owner is Track;

        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="Clip"/>
        /// </summary>
        public Clip OwnerClip => (Clip) this.Owner;

        /// <summary>
        /// Casts <see cref="Owner"/> to a <see cref="Track"/>
        /// </summary>
        public Track OwnerTrack => (Track) this.Owner;

        public Timeline Timeline => this.Owner?.Timeline;

        public Project Project => this.Timeline?.Project;

        public long RelativePlayHead => this.Owner?.RelativePlayHead ?? 0;

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

        public BaseEffect Clone() {
            if (!this.IsCloneable)
                throw new InvalidOperationException("This effect cannot be cloned");
            BaseEffect clone = EffectFactory.Instance.NewEffect(this.FactoryId);
            this.LoadDataIntoClone(clone);
            return clone;
        }

        protected virtual void LoadDataIntoClone(BaseEffect clone) {
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
        }

        protected virtual void OnAdded() {

        }

        protected virtual void OnRemoved() {

        }

        public long ConvertRelativeToTimelineFrame(long relative) {
            return this.Owner is Clip clip ? clip.ConvertRelativeToTimelineFrame(relative) : relative;
        }

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) {
            if (this.Owner is Clip clip) {
                return clip.ConvertTimelineToRelativeFrame(timeline, out inRange);
            }

            inRange = this.IsTrackEffect;
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
                throw new InvalidOperationException("Effect type is not accepted: " + effect.GetType());
            if (owner.Effects.Contains(effect))
                throw new InvalidOperationException("Cannot add an effect that was already added");
        }
    }
}