using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.History;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Clips
{
    public class VideoClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel
    {
        private readonly RefreshAutomationValueEventHandler RefreshOpacityHandler;

        private double opacity;

        public double Opacity
        {
            get => this.opacity;
            set
            {
                double oldVal = this.opacity;
                this.opacity = value;
                bool useAddition = this.Handlers.Count > 1 && this.OpacityEditStateChangedCommand.IsEditing;
                double change = value - oldVal;
                Transaction<double>[] array = ((HistoryClipOpacity) this.OpacityEditStateChangedCommand.HistoryAction)?.Opacity;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[i];
                    double val = useAddition ? (clip.Opacity + change) : value;
                    clip.Opacity = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public RelayCommand ResetOpacityCommand { get; }

        public EditStateCommand OpacityEditStateChangedCommand { get; }

        public AutomationSequenceViewModel OpacityAutomationSequence => this.SingleSelection.OpacityAutomationSequence;

        public VideoClipViewModel SingleSelection => (VideoClipViewModel) this.Handlers[0];
        public IEnumerable<VideoClipViewModel> Clips => this.Handlers.Cast<VideoClipViewModel>();

        public VideoClipDataEditorViewModel() : base(typeof(VideoClipViewModel))
        {
            this.RefreshOpacityHandler = this.RefreshOpacity;
            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);
            this.OpacityEditStateChangedCommand = new EditStateCommand(() => new HistoryClipOpacity(this), "Modify opacity");
        }

        private void RefreshOpacity(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.opacity, this.SingleSelection.Opacity, nameof(this.Opacity));
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1)
            {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.RequeryOpacityFromHandlers();
            this.RaisePropertyChanged(nameof(this.OpacityAutomationSequence));
        }

        public void RequeryOpacityFromHandlers()
        {
            this.opacity = GetEqualValue(this.Handlers, (x) => ((VideoClipViewModel) x).Opacity, out double d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Opacity));
        }

        protected override void OnClearHandlers()
        {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1)
            {
                this.SingleSelection.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }

            this.OpacityEditStateChangedCommand.OnReset();
        }

        protected class HistoryClipOpacity : BaseHistoryMultiHolderAction<VideoClipViewModel>
        {
            public readonly Transaction<double>[] Opacity;
            public readonly VideoClipDataEditorViewModel editor;

            public HistoryClipOpacity(VideoClipDataEditorViewModel editor) : base(editor.Clips)
            {
                this.Opacity = Transactions.NewArray(this.Holders, x => x.Opacity);
                this.editor = editor;
            }

            protected override Task UndoAsync(VideoClipViewModel holder, int i)
            {
                holder.Opacity = this.Opacity[i].Original;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(VideoClipViewModel holder, int i)
            {
                holder.Opacity = this.Opacity[i].Current;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }
        }
    }

    public class VideoClipDataSingleEditorViewModel : VideoClipDataEditorViewModel
    {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public RelayCommand InsertOpacityKeyFrameCommand => this.SingleSelection?.InsertOpacityKeyFrameCommand;

        public long MediaFrameOffset => this.SingleSelection.MediaFrameOffset;

        private bool isOpacitySelected;

        public bool IsOpacitySelected
        {
            get => this.isOpacitySelected;
            set
            {
                this.RaisePropertyChanged(ref this.isOpacitySelected, value);
                if (!this.IsEmpty)
                    this.OpacityAutomationSequence.IsActiveSequence = value;
            }
        }

        public VideoClipDataSingleEditorViewModel()
        {
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            this.SingleSelection.PropertyChanged += this.OnClipPropertyChanged;

            // not really sure if this is necessary...
            this.RaisePropertyChanged(nameof(this.InsertOpacityKeyFrameCommand));
        }

        protected override void OnClearHandlers()
        {
            base.OnClearHandlers();
            this.IsOpacitySelected = false;
            this.SingleSelection.PropertyChanged -= this.OnClipPropertyChanged;
        }

        private void OnClipPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoClipViewModel.MediaFrameOffset))
            {
                this.RaisePropertyChanged(nameof(this.MediaFrameOffset));
            }
        }
    }

    public class VideoClipDataMultipleEditorViewModel : VideoClipDataEditorViewModel
    {
        public override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}