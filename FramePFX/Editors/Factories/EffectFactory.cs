using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.Editors.Factories {
    public class EffectFactory : ReflectiveObjectFactory<BaseEffect> {
        public static EffectFactory Instance { get; } = new EffectFactory();

        private EffectFactory() {

        }

        public BaseEffect NewEffect(string id) {
            return base.NewInstance(id);
        }
    }
}