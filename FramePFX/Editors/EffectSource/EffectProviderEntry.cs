using System;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.Editors.EffectSource {
    public class EffectProviderEntry {
        public Type EffectType { get; }

        public string EffectId { get; }

        public string DisplayName { get; }

        public Action<BaseEffect> PostProcessor { get; }

        public EffectProviderEntry(string effectId, string displayName, Action<BaseEffect> postProcessor) {
            this.EffectType = EffectFactory.Instance.GetType(effectId);
            this.EffectId = effectId;
            this.DisplayName = displayName;
            this.PostProcessor = postProcessor;
        }

        public BaseEffect CreateEffect() {
            BaseEffect effect = EffectFactory.Instance.NewEffect(this.EffectId);
            this.PostProcessor?.Invoke(effect);
            return effect;
        }
    }
}