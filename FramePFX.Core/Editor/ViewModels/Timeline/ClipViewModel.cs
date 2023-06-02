using System;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    public abstract class TimelineClipViewModel : BaseViewModel {
        private TimelineLayerViewModel layer;
        public TimelineLayerViewModel Layer {
            get => this.layer;
            set {
                this.Model.Layer = value?.Model;
                TimelineLayerViewModel oldLayer = this.layer;
                this.OnLayerChanging(oldLayer, value);
                this.RaisePropertyChanged(ref this.layer, value);
                this.OnLayerChanging(oldLayer, value);
            }
        }

        public ClipModel Model { get; }

        protected TimelineClipViewModel(ClipModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        protected virtual void OnLayerChanging(TimelineLayerViewModel oldLayer, TimelineLayerViewModel newLayer) {

        }

        protected virtual void OnLayerChanged(TimelineLayerViewModel oldLayer, TimelineLayerViewModel newLayer) {

        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing clip")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {

        }
    }
}