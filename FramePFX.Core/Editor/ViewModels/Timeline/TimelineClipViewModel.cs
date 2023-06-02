namespace FramePFX.Core.Editor.ViewModels.Timeline {
    public abstract class TimelineClipViewModel : BaseViewModel {
        private TimelineLayerViewModel layer;
        public TimelineLayerViewModel Layer {
            get => this.layer;
            set {
                TimelineLayerViewModel oldLayer = this.layer;
                this.OnLayerChanging(oldLayer, value);
                this.RaisePropertyChanged(ref this.layer, value);
                this.OnLayerChanging(oldLayer, value);
            }
        }

        protected TimelineClipViewModel() {

        }

        protected virtual void OnLayerChanging(TimelineLayerViewModel oldLayer, TimelineLayerViewModel newLayer) {

        }

        protected virtual void OnLayerChanged(TimelineLayerViewModel oldLayer, TimelineLayerViewModel newLayer) {

        }
    }
}