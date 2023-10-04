using System;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;

namespace FramePFX.Editor.Registries
{
    public class EffectFactory : ModelFactory<BaseEffect, BaseEffectViewModel>
    {
        public static EffectFactory Instance { get; } = new EffectFactory();

        private EffectFactory()
        {
            this.Register<MotionEffect, MotionEffectViewModel>("motion");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseEffect where TViewModel : BaseEffectViewModel
        {
            base.Register<TModel, TViewModel>(id);
        }

        public new BaseEffect CreateModel(string id)
        {
            return (BaseEffect) Activator.CreateInstance(base.GetModelType(id));
        }

        public new BaseEffectViewModel CreateViewModelFromModel(BaseEffect model)
        {
            return (BaseEffectViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}