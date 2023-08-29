using FramePFX.Automation;

namespace FramePFX.Editor.Timelines.Effects {
    /// <summary>
    /// The base class for all types of effects (audio, video, etc.)
    /// </summary>
    public abstract class BaseEffect : IAutomatable {
        public bool CanRemove { get; protected set; }

        /// <summary>
        /// This clip's factory ID, used for creating a new instance dynamically via reflection
        /// </summary>
        public string FactoryId => EffectRegistry.Instance.GetTypeIdForModel(this.GetType());

        public AutomationData AutomationData { get; }

        public AutomationEngine AutomationEngine => this.OwnerClip?.AutomationEngine;

        public bool IsAutomationChangeInProgress { get; set; }

        /// <summary>
        /// The clip that this effect has been added to
        /// </summary>
        public Clip OwnerClip { get; set; }

        protected BaseEffect() {
            this.AutomationData = new AutomationData(this);
        }
    }
}