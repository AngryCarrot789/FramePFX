using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Clips {
    public class ClipDisplayNamePropertyEditorSlot : PropertyEditorSlot {
        public IEnumerable<VideoClip> Clips => this.Handlers.Cast<VideoClip>();

        public VideoClip SingleSelection => (VideoClip) this.Handlers[0];

        public string DisplayName { get; private set; }

        public override bool IsSelectable => true;

        public event PropertyEditorSlotEventHandler DisplayNameChanged;
        private bool isProcessingValueChange;

        public ClipDisplayNamePropertyEditorSlot() : base(typeof(Clip)) {

        }

        public void SetValue(string value) {
            this.isProcessingValueChange = true;

            this.DisplayName = value;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                Clip clip = (Clip) this.Handlers[i];
                clip.DisplayName = value;
            }

            this.DisplayNameChanged?.Invoke(this);
            this.isProcessingValueChange = false;
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.DisplayNameChanged += this.OnClipDisplayNameChanged;
            }

            this.RequeryOpacityFromHandlers();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.DisplayNameChanged -= this.OnClipDisplayNameChanged;
            }
        }

        public void RequeryOpacityFromHandlers() {
            this.DisplayName = GetEqualValue(this.Handlers, x => ((Clip) x).DisplayName, out string d) ? d : "<different values>";
            this.DisplayNameChanged?.Invoke(this);
        }

        private void OnClipDisplayNameChanged(Clip clip) {
            if (this.isProcessingValueChange)
                return;
            if (this.DisplayName != clip.DisplayName) {
                this.DisplayName = clip.DisplayName;
                this.DisplayNameChanged?.Invoke(this);
            }
        }
    }
}