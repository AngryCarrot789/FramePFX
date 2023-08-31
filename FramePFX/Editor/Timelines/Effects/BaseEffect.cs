using FramePFX.Automation;
using FramePFX.RBC;

namespace FramePFX.Editor.Timelines.Effects {
    /// <summary>
    /// The base class for all types of effects (audio, video, etc.)
    /// </summary>
    public abstract class BaseEffect : IAutomatable {
        public bool IsRemoveable { get; protected set; }

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

        public virtual void WriteToRBE(RBEDictionary data) {
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            this.AutomationData.UpdateBackingStorage();
        }
    }
}