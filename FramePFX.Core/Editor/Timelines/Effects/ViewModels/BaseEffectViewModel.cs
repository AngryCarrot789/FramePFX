namespace FramePFX.Core.Editor.Timelines.Effects.ViewModels {
    public class BaseEffectViewModel : BaseViewModel {
        public BaseEffect Effect { get; }

        public BaseEffectViewModel(BaseEffect effect) {
            this.Effect = effect;
        }
    }
}