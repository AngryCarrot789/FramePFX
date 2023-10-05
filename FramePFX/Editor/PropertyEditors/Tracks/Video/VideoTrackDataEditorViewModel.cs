using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.History;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Tracks.Video {
    public class VideoTrackDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        private readonly RefreshAutomationValueEventHandler RefreshOpacityHandler;

        private double opacity;

        public double Opacity {
            get => this.opacity;
            set {
                double oldVal = this.opacity;
                this.opacity = value;
                bool useAddition = this.Handlers.Count > 1 && this.OpacityEditStateChangedCommand.IsEditing;
                double change = value - oldVal;
                Transaction<double>[] array = ((HistoryClipOpacity) this.OpacityEditStateChangedCommand.HistoryAction)?.Opacity;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    VideoTrackViewModel clip = (VideoTrackViewModel) this.Handlers[i];
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

        public EditStateCommand OpacityEditStateChangedCommand { get; }

        public AutomationSequenceViewModel OpacityAutomationSequence => this.SingleSelection.OpacityAutomationSequence;

        public VideoTrackViewModel SingleSelection => (VideoTrackViewModel) this.Handlers[0];
        public IEnumerable<VideoTrackViewModel> Clips => this.Handlers.Cast<VideoTrackViewModel>();

        public VideoTrackDataEditorViewModel() : base(typeof(VideoTrackViewModel)) {
            this.RefreshOpacityHandler = this.RefreshOpacity;
            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);
            this.OpacityEditStateChangedCommand = new EditStateCommand(() => new HistoryClipOpacity(this), "Modify opacity");
        }

        private void RefreshOpacity(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.opacity, this.SingleSelection.Opacity, nameof(this.Opacity));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.RequeryOpacityFromHandlers();
            this.RaisePropertyChanged(nameof(this.OpacityAutomationSequence));
        }

        public void RequeryOpacityFromHandlers() {
            this.opacity = GetEqualValue(this.Handlers, (x) => ((VideoTrackViewModel) x).Opacity, out double d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Opacity));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1) {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }

            this.OpacityEditStateChangedCommand.OnReset();
        }

        protected class HistoryClipOpacity : BaseHistoryMultiHolderAction<VideoTrackViewModel> {
            public readonly Transaction<double>[] Opacity;
            public readonly VideoTrackDataEditorViewModel editor;

            public HistoryClipOpacity(VideoTrackDataEditorViewModel editor) : base(editor.Clips) {
                this.Opacity = Transactions.NewArray(this.Holders, x => x.Opacity);
                this.editor = editor;
            }

            protected override Task UndoAsync(VideoTrackViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Original;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(VideoTrackViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Current;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }
        }
    }

    public class VideoTrackDataSingleEditorViewModel : VideoTrackDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public RelayCommand InsertOpacityKeyFrameCommand => this.SingleSelection?.InsertOpacityKeyFrameCommand;

        private bool isOpacitySelected;

        public bool IsOpacitySelected {
            get => this.isOpacitySelected;
            set {
                this.RaisePropertyChanged(ref this.isOpacitySelected, value);
                if (!this.IsEmpty && value)
                    this.OpacityAutomationSequence.IsActiveSequence = true;
            }
        }

        public VideoTrackDataSingleEditorViewModel() {
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();

            // not really sure if this is necessary...
            this.RaisePropertyChanged(nameof(this.InsertOpacityKeyFrameCommand));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            this.IsOpacitySelected = false;
        }
    }

    public class VideoTrackDataMultipleEditorViewModel : VideoTrackDataEditorViewModel {
        public override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}