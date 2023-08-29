using System;
using FramePFX.Core.Editor.Timelines.Effects.ViewModels;

namespace FramePFX.Core.Editor.Timelines.Effects {
    public class EffectRegistry : ModelRegistry<BaseEffect, BaseEffectViewModel> {
        public static EffectRegistry Instance { get; } = new EffectRegistry();

        private EffectRegistry() {
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseEffect where TViewModel : BaseEffectViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public BaseEffect CreateModel(string id) {
            return (BaseEffect) Activator.CreateInstance(base.GetModelType(id));
        }

        public BaseEffectViewModel CreateViewModel(string id) {
            return (BaseEffectViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public BaseEffectViewModel CreateViewModelFromModel(BaseEffect model) {
            return (BaseEffectViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}