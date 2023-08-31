using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines.Clips;
using FramePFX.History;

namespace FramePFX.PropertyEditing.Editor.Editor {
    public class VideoClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        protected HistoryClipOpacity opacityHistory;
        private bool isEditingOpacity;
        private double opacity;

        public VideoClipViewModel Clip => (VideoClipViewModel) this.Handlers[0];

        public double Opacity {
            get => this.opacity;
            set {
                double oldVal = this.opacity;
                this.opacity = value;
                bool useAddition = this.Handlers.Count > 1 && this.isEditingOpacity;
                double change = value - oldVal;
                Transaction<double>[] array = this.opacityHistory?.Opacity;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[i];
                    double val = useAddition ? (clip.Opacity + change) : value;
                    clip.Opacity = val;
                    if (array != null) {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public RelayCommand ResetOpacityCommand { get; }
        public RelayCommand BeginEditOpacityCommand { get; }
        public RelayCommand EndEditOpacityCommand { get; }

        public IEnumerable<VideoClipViewModel> Clips => this.Handlers.Cast<VideoClipViewModel>();

        public AutomationSequenceViewModel OpacityAutomationSequence => this.Clip.OpacityAutomationSequence;

        private readonly RefreshAutomationValueEventHandler RefreshOpacityHandler;

        public VideoClipDataEditorViewModel() : base(typeof(VideoClipViewModel)) {
            this.RefreshOpacityHandler = this.RefreshOpacity;

            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);
            this.BeginEditOpacityCommand = new RelayCommand(() => {
                this.isEditingOpacity = true;
                this.BeginEditOpacity();
            }, () => !this.isEditingOpacity);
            this.EndEditOpacityCommand = new RelayCommand(() => {
                this.isEditingOpacity = false;
                this.EndEditOpacity();
            }, () => this.isEditingOpacity);
        }

        protected void BeginEditOpacity(bool ignoreIfAlreadyExists = false) {
            if (this.opacityHistory != null) {
                if (ignoreIfAlreadyExists)
                    return;
                this.EndEditOpacity();
            }

            this.opacityHistory = new HistoryClipOpacity(this.Clips, this);
        }

        protected void EndEditOpacity() {
            if (this.opacityHistory != null) {
                this.HistoryManager?.AddAction(this.opacityHistory, "Edit opacity");
                this.opacityHistory = null;
            }
        }

        // these are only handled for single selection

        private void RefreshOpacity(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.opacity, ((VideoClipViewModel) this.Handlers[0]).Opacity, nameof(this.Opacity));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.RequeryOpacityFromHandlers();
            this.RaisePropertyChanged(nameof(this.OpacityAutomationSequence));
        }

        public void RequeryOpacityFromHandlers() {
            this.opacity = GetEqualValue(this.Handlers, (x) => ((VideoClipViewModel) x).Opacity, out double d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Opacity));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1) {
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }

            this.opacityHistory = null;
        }

        protected class HistoryClipOpacity : BaseHistoryMultiHolderAction<VideoClipViewModel> {
            public readonly Transaction<double>[] Opacity;
            public readonly VideoClipDataEditorViewModel editor;

            public HistoryClipOpacity(IEnumerable<VideoClipViewModel> holders, VideoClipDataEditorViewModel editor) : base(holders) {
                this.Opacity = Transactions.NewArray(this.Holders, x => x.Opacity);
                this.editor = editor;
            }

            protected override Task UndoAsyncCore(VideoClipViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Original;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(VideoClipViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Current;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }
        }
    }
}