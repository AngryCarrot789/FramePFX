using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Editors.PropertyEditors.Clips {
    public delegate void VideoClipPropertyEditorEventHandler(VideoClipOpacityPropertyEditorSlot sender);
    
    public class VideoClipOpacityPropertyEditorSlot : PropertyEditorSlot {
        private double opacity;

        public IEnumerable<VideoClip> Clips => this.Handlers.Cast<VideoClip>();

        public VideoClip SingleSelection => (VideoClip) this.Handlers[0];

        public double Opacity {
            get => this.opacity;
            set {
                double oldVal = this.opacity;
                this.opacity = value;
                bool useAddition = this.Handlers.Count > 1;
                double change = value - oldVal;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    VideoClip clip = (VideoClip) this.Handlers[i];
                    double newClipValue = Maths.Clamp(useAddition ? (clip.Opacity + change) : value, 0.0, 1.0);

                    // not like WPF has boxed and unboxed the NumberDragger value many
                    // times, this should be fine performance wise
                    AutomatedControlUtils.SetDefaultKeyFrameOrAddNew(clip, VideoClip.OpacityParameter, newClipValue);
                }

                this.OpacityChanged?.Invoke(this);
            }
        }

        public override bool IsSelectable => true;

        public event VideoClipPropertyEditorEventHandler OpacityChanged;

        public VideoClipOpacityPropertyEditorSlot() : base(typeof(VideoClip)) {

        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.AutomationData[VideoClip.OpacityParameter].ParameterChanged += this.OnParameterChanged;
            }

            this.RequeryOpacityFromHandlers();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.AutomationData[VideoClip.OpacityParameter].ParameterChanged -= this.OnParameterChanged;
            }
        }

        public void RequeryOpacityFromHandlers() {
            this.Opacity = GetEqualValue(this.Handlers, (x) => ((VideoClip) x).Opacity, out double d) ? d : default;
            this.OpacityChanged?.Invoke(this);
        }

        // Event handler only added for single selection
        private void OnParameterChanged(AutomationSequence sequence) {
            this.Opacity = this.SingleSelection.Opacity;
            this.OpacityChanged?.Invoke(this);
        }
    }
}