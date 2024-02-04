using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.Editors.Factories {
    public class EffectFactory : ReflectiveObjectFactory<BaseEffect> {
        public static EffectFactory Instance { get; } = new EffectFactory();

        private EffectFactory() {
            this.RegisterType("vfx_motion", typeof(MotionEffect));
            this.RegisterType("vfx_pixelate", typeof(CPUPixelateEffect));
        }

        public BaseEffect NewEffect(string id) {
            return base.NewInstance(id);
        }
    }
}