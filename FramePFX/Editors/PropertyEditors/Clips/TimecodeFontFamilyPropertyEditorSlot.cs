using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Clips {
    public class TimecodeFontFamilyPropertyEditorSlot : PropertyEditorSlot {
        public IEnumerable<TimecodeClip> Clips => this.Handlers.Cast<TimecodeClip>();

        public TimecodeClip SingleSelection => (TimecodeClip) this.Handlers[0];

        public string FontFamily { get; private set; }

        public override bool IsSelectable => true;

        public event PropertyEditorSlotEventHandler FontFamilyChanged;
        private bool isProcessingValueChange;

        public TimecodeFontFamilyPropertyEditorSlot() : base(typeof(TimecodeClip)) {

        }

        public void SetValue(string value) {
            this.isProcessingValueChange = true;

            this.FontFamily = value;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                TimecodeClip clip = (TimecodeClip) this.Handlers[i];
                clip.FontFamily = value;
            }

            this.FontFamilyChanged?.Invoke(this);
            this.isProcessingValueChange = false;
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.FontFamilyChanged += this.OnClipFontFamilyChanged;
            }

            this.RequeryOpacityFromHandlers();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.FontFamilyChanged -= this.OnClipFontFamilyChanged;
            }
        }

        public void RequeryOpacityFromHandlers() {
            this.FontFamily = GetEqualValue(this.Handlers, x => ((TimecodeClip) x).FontFamily, out string d) ? d : "<different values>";
            this.FontFamilyChanged?.Invoke(this);
        }

        private void OnClipFontFamilyChanged(Clip theClip) {
            if (this.isProcessingValueChange)
                return;

            TimecodeClip clip = (TimecodeClip) theClip;
            if (this.FontFamily != clip.FontFamily) {
                this.FontFamily = clip.FontFamily;
                this.FontFamilyChanged?.Invoke(this);
            }
        }
    }
}