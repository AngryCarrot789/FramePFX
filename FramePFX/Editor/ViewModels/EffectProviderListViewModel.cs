using System;
using System.Collections.Generic;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;

namespace FramePFX.Editor.ViewModels
{
    public class EffectProviderListViewModel : BaseViewModel
    {
        public IReadOnlyList<EffectProviderViewModel> Effects { get; }

        public EffectProviderListViewModel()
        {
            List<EffectProviderViewModel> list = new List<EffectProviderViewModel>
            {
                new EffectProviderViewModel("Motion", EffectFactory.Instance.GetTypeIdForViewModel(typeof(MotionEffectViewModel))),
                new EffectProviderViewModel("Twirl", EffectFactory.Instance.GetTypeIdForViewModel(typeof(TwirlEffectViewModel)))
            };

            this.Effects = list;
        }
    }

    public class EffectProviderViewModel : BaseViewModel
    {
        public string Name { get; }
        public string EffectFactoryId { get; }

        public EffectProviderViewModel(string name, string effectFactoryId)
        {
            this.EffectFactoryId = effectFactoryId ?? throw new ArgumentNullException(nameof(effectFactoryId));
            this.Name = name;
        }
    }
}