using System.Collections.Generic;
using System.Linq;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.History;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.History.Tasks;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Clips {
    public class TextClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        private readonly HistoryBuffer<HistoryFontFamilty> historyFontFamily;
        private readonly HistoryBuffer<HistoryText> historyText;

        public TextVideoClipViewModel SingleSelection => (TextVideoClipViewModel) this.Handlers[0];
        public IEnumerable<TextVideoClipViewModel> Clips => this.Handlers.Cast<TextVideoClipViewModel>();

        private string fontFamily;

        public string FontFamily {
            get => this.fontFamily;
            set {
                if (!this.historyFontFamily.TryGetAction(out HistoryFontFamilty action))
                    this.historyFontFamily.PushAction(this.HistoryManager, action = new HistoryFontFamilty(this), "Change font family");
                this.fontFamily = value;
                action.SetCurrentValue(value);
                this.RaisePropertyChanged();
            }
        }

        private string text;

        public string Text {
            get => this.text;
            set {
                if (!this.historyText.TryGetAction(out HistoryText action))
                    this.historyText.PushAction(this.HistoryManager, action = new HistoryText(this), "Change text");
                this.text = value;
                action.SetCurrentValue(value);
                this.RaisePropertyChanged();
            }
        }

        public static string DifferentValueText => Services.Translator.GetString("S.PropertyEditor.NamedObject.DifferingDisplayNames");

        public TextClipDataEditorViewModel() : base(typeof(TextVideoClipViewModel)) {
            this.historyFontFamily = new HistoryBuffer<HistoryFontFamilty>();
            this.historyText = new HistoryBuffer<HistoryText>();
        }

        static TextClipDataEditorViewModel() {
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.RequeryFontFamiltyFromHandlers();
            this.RequeryTextFromHandlers();
        }

        public void RequeryFontFamiltyFromHandlers() {
            this.fontFamily = GetEqualValue(this.Handlers, x => ((TextVideoClipViewModel) x).FontFamily, out string d) ? d : DifferentValueText;
            this.RaisePropertyChanged(nameof(this.FontFamily));
        }

        public void RequeryTextFromHandlers() {
            this.text = GetEqualValue(this.Handlers, x => ((TextVideoClipViewModel) x).Text, out string d) ? d : DifferentValueText;
            this.RaisePropertyChanged(nameof(this.Text));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }
        }

        private void RefreshOpacityHandler(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.fontFamily, this.SingleSelection.FontFamily, nameof(this.FontFamily));
        }

        protected class HistoryFontFamilty : HistoryBasicSingleProperty<TextVideoClipViewModel, string> {
            public HistoryFontFamilty(TextClipDataEditorViewModel editor) : base(editor.Clips, x => x.FontFamily, (x, v) => x.FontFamily = v, editor.RequeryFontFamiltyFromHandlers) {
            }
        }

        protected class HistoryText : HistoryBasicSingleProperty<TextVideoClipViewModel, string> {
            public HistoryText(TextClipDataEditorViewModel editor) : base(editor.Clips, x => x.Text, (x, v) => x.Text = v, editor.RequeryTextFromHandlers) {
            }
        }
    }

    // Use different types because it's more convenient to create DataTemplates;
    // no need for a template selector to check the mode

    public class TextClipDataSingleEditorViewModel : TextClipDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;
    }

    public class TextClipDataMultiEditorViewModel : TextClipDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}