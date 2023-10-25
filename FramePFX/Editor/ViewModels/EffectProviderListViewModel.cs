using System;
using System.Collections.Generic;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;

namespace FramePFX.Editor.ViewModels {
    public class EffectProviderListViewModel : BaseViewModel {
        public IReadOnlyList<EffectProviderViewModel> Effects { get; }

        public EffectProviderListViewModel() {
            List<EffectProviderViewModel> list = new List<EffectProviderViewModel> {
                new EffectProviderViewModel("Motion", typeof(MotionEffect)),
            };

            this.Effects = list;
        }
    }

    public class EffectProviderViewModel : BaseViewModel {
        public string Name { get; }
        public string EffectFactoryId { get; }
        public Type EffectType { get; }

        public EffectProviderViewModel(string name, Type effectType) {
            this.EffectFactoryId = EffectFactory.Instance.GetTypeIdForModel(effectType);
            if (string.IsNullOrEmpty(this.EffectFactoryId))
                throw new InvalidOperationException("Unknown effect type: " + effectType);
            this.EffectType = effectType;
            this.Name = name;
        }
    }
}