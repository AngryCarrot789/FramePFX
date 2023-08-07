using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;
using FramePFX.Core.History;

namespace FramePFX.Core.PropertyEditing.Editors.Editor
{
    public class VideoClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel
    {
        protected HistoryClipMediaTransformation transformationHistory;
        protected HistoryClipOpacity opacityHistory;

        private bool isEditingMediaPosition;
        private bool isEditingMediaScale;
        private bool isEditingMediaScaleOrigin;
        private bool isEditingOpacity;

        private Vector2 mediaPosition;
        private Vector2 mediaScale;
        private Vector2 mediaScaleOrigin;
        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;
        private double opacity;

        public VideoClipViewModel Clip => (VideoClipViewModel) this.Handlers[0];

        public float MediaPositionX
        {
            get => this.MediaPosition.X;
            set => this.MediaPosition = new Vector2(value, this.MediaPosition.Y);
        }

        public float MediaPositionY
        {
            get => this.MediaPosition.Y;
            set => this.MediaPosition = new Vector2(this.MediaPosition.X, value);
        }

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition
        {
            get => this.mediaPosition;
            set
            {
                Vector2 oldVal = this.mediaPosition;
                this.mediaPosition = value;
                bool useAddition = this.Handlers.Count > 1 && this.isEditingMediaPosition;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = this.transformationHistory?.MediaPosition;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaPosition + change) : value;
                    clip.MediaPosition = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaPositionX));
                this.RaisePropertyChanged(nameof(this.MediaPositionY));
            }
        }

        public float MediaScaleX
        {
            get => this.MediaScale.X;
            set => this.MediaScale = new Vector2(value, this.MediaScale.Y);
        }

        public float MediaScaleY
        {
            get => this.MediaScale.Y;
            set => this.MediaScale = new Vector2(this.MediaScale.X, value);
        }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale
        {
            get => this.mediaScale;
            set
            {
                Vector2 oldVal = this.mediaScale;
                this.mediaScale = value;
                bool useAddition = this.Handlers.Count > 1 && this.isEditingMediaScale;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = this.transformationHistory?.MediaScale;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaScale + change) : value;
                    clip.MediaScale = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleX));
                this.RaisePropertyChanged(nameof(this.MediaScaleY));
            }
        }

        public float MediaScaleOriginX
        {
            get => this.MediaScaleOrigin.X;
            set => this.MediaScaleOrigin = new Vector2(value, this.MediaScaleOrigin.Y);
        }

        public float MediaScaleOriginY
        {
            get => this.MediaScaleOrigin.Y;
            set => this.MediaScaleOrigin = new Vector2(this.MediaScaleOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin
        {
            get => this.mediaScaleOrigin;
            set
            {
                Vector2 oldVal = this.mediaScaleOrigin;
                this.mediaScaleOrigin = value;
                bool useAddition = this.Handlers.Count > 1 && this.isEditingMediaScaleOrigin;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = this.transformationHistory?.MediaScaleOrigin;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaScaleOrigin + change) : value;
                    clip.MediaScaleOrigin = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
            }
        }

        #region WPF NumberDragger helpers

        public float BothPos
        {
            get => this.bothPos;
            set
            {
                this.MediaPosition += new Vector2(value - this.bothPos);
                this.RaisePropertyChanged();
                this.bothPos = 0;
            }
        }

        public float BothScale
        {
            get => this.bothScale;
            set
            {
                this.MediaScale += new Vector2(value - this.bothScale);
                this.RaisePropertyChanged();
                this.bothScale = 0;
            }
        }

        public float BothScaleOrigin
        {
            get => this.bothScaleOrigin;
            set
            {
                this.MediaScaleOrigin += new Vector2(value - this.bothScaleOrigin);
                this.RaisePropertyChanged();
                this.bothScaleOrigin = 0;
            }
        }

        #endregion

        public double Opacity
        {
            get => this.opacity;
            set
            {
                double oldVal = this.opacity;
                this.opacity = value;
                bool useAddition = this.Handlers.Count > 1 && this.isEditingOpacity;
                double change = value - oldVal;
                Transaction<double>[] array = this.opacityHistory?.Opacity;
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

        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }
        public RelayCommand ResetOpacityCommand { get; }

        public RelayCommand BeginEditMediaPositionCommand { get; }
        public RelayCommand EndEditMediaPositionCommand { get; }

        public RelayCommand BeginEditMediaScaleCommand { get; }
        public RelayCommand EndEditMediaScaleCommand { get; }

        public RelayCommand BeginEditMediaScaleOriginCommand { get; }
        public RelayCommand EndEditMediaScaleOriginCommand { get; }

        public RelayCommand BeginEditOpacityCommand { get; }
        public RelayCommand EndEditOpacityCommand { get; }

        public IEnumerable<VideoClipViewModel> Clips => this.Handlers.Cast<VideoClipViewModel>();

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.Clip.MediaPositionAutomationSequence;
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.Clip.MediaScaleAutomationSequence;
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.Clip.MediaScaleOriginAutomationSequence;
        public AutomationSequenceViewModel OpacityAutomationSequence => this.Clip.OpacityAutomationSequence;

        private readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler;
        private readonly RefreshAutomationValueEventHandler RefreshOpacityHandler;

        public VideoClipDataEditorViewModel() : base(typeof(VideoClipViewModel))
        {
            this.RefreshMediaPositionHandler = this.RefreshMediaPosition;
            this.RefreshMediaScaleHandler = this.RefreshMediaScale;
            this.RefreshMediaScaleOriginHandler = this.RefreshMediaScaleOrigin;
            this.RefreshOpacityHandler = this.RefreshOpacity;

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue);
            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);

            this.BeginEditMediaPositionCommand = new RelayCommand(() =>
            {
                this.isEditingMediaPosition = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaPosition);
            this.EndEditMediaPositionCommand = new RelayCommand(() =>
            {
                this.isEditingMediaPosition = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaPosition);

            this.BeginEditMediaScaleCommand = new RelayCommand(() =>
            {
                this.isEditingMediaScale = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaScale);
            this.EndEditMediaScaleCommand = new RelayCommand(() =>
            {
                this.isEditingMediaScale = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaScale);

            this.BeginEditMediaScaleOriginCommand = new RelayCommand(() =>
            {
                this.isEditingMediaScaleOrigin = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaScaleOrigin);
            this.EndEditMediaScaleOriginCommand = new RelayCommand(() =>
            {
                this.isEditingMediaScaleOrigin = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaScaleOrigin);

            this.BeginEditOpacityCommand = new RelayCommand(() =>
            {
                this.isEditingOpacity = true;
                this.BeginEditOpacity();
            }, () => !this.isEditingOpacity);
            this.EndEditOpacityCommand = new RelayCommand(() =>
            {
                this.isEditingOpacity = false;
                this.EndEditOpacity();
            }, () => this.isEditingOpacity);
        }

        protected void BeginEditTransformation()
        {
            if (this.transformationHistory != null)
            {
                this.EndEditTransformation();
            }

            this.transformationHistory = new HistoryClipMediaTransformation(this.Clips, this);
        }

        protected void EndEditTransformation()
        {
            if (this.transformationHistory != null)
            {
                if (!this.transformationHistory.AreValuesUnchanged())
                {
                    this.HistoryManager?.AddAction(this.transformationHistory, "Edit transformation");
                }

                this.transformationHistory = null;
            }
        }

        protected void BeginEditOpacity(bool ignoreIfAlreadyExists = false)
        {
            if (this.opacityHistory != null)
            {
                if (ignoreIfAlreadyExists)
                    return;
                this.EndEditOpacity();
            }

            this.opacityHistory = new HistoryClipOpacity(this.Clips, this);
        }

        protected void EndEditOpacity()
        {
            if (this.opacityHistory != null)
            {
                this.HistoryManager?.AddAction(this.opacityHistory, "Edit opacity");
                this.opacityHistory = null;
            }
        }

        // these are only handled for single selection

        private void RefreshMediaPosition(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaPosition, ((VideoClipViewModel) this.Handlers[0]).MediaPosition, nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        private void RefreshMediaScale(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaScale, ((VideoClipViewModel) this.Handlers[0]).MediaScale, nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        private void RefreshMediaScaleOrigin(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaScaleOrigin, ((VideoClipViewModel) this.Handlers[0]).MediaScaleOrigin, nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        private void RefreshOpacity(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.opacity, ((VideoClipViewModel) this.Handlers[0]).Opacity, nameof(this.Opacity));
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1)
            {
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.MediaPositionAutomationSequence.RefreshValue += this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue += this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue += this.RefreshMediaScaleOriginHandler;
                clip.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.RequeryPositionFromHandlers();
            this.RequeryScaleFromHandlers();
            this.RequeryScaleOriginFromHandlers();
            this.RequeryOpacityFromHandlers();

            this.RaisePropertyChanged(nameof(this.MediaPositionAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginAutomationSequence));
            this.RaisePropertyChanged(nameof(this.OpacityAutomationSequence));
        }

        public void RequeryPositionFromHandlers()
        {
            this.mediaPosition = GetEqualValue(this.Handlers, (x) => ((VideoClipViewModel) x).MediaPosition, out Vector2 a) ? a : default;
            this.RaisePropertyChanged(nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        public void RequeryScaleFromHandlers()
        {
            this.mediaScale = GetEqualValue(this.Handlers, (x) => ((VideoClipViewModel) x).MediaScale, out Vector2 b) ? b : default;
            this.RaisePropertyChanged(nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        public void RequeryScaleOriginFromHandlers()
        {
            this.mediaScaleOrigin = GetEqualValue(this.Handlers, (x) => ((VideoClipViewModel) x).MediaScaleOrigin, out Vector2 c) ? c : default;
            this.RaisePropertyChanged(nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
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
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.MediaPositionAutomationSequence.RefreshValue -= this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue -= this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue -= this.RefreshMediaScaleOriginHandler;
                clip.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }

            this.transformationHistory = null;
            this.opacityHistory = null;
        }

        protected class HistoryClipMediaTransformation : BaseHistoryMultiHolderAction<VideoClipViewModel>
        {
            public readonly Transaction<Vector2>[] MediaPosition;
            public readonly Transaction<Vector2>[] MediaScale;
            public readonly Transaction<Vector2>[] MediaScaleOrigin;
            public readonly VideoClipDataEditorViewModel editor;

            public HistoryClipMediaTransformation(IEnumerable<VideoClipViewModel> holders, VideoClipDataEditorViewModel editor) : base(holders)
            {
                this.MediaPosition = Transactions.NewArray(this.Holders, x => x.MediaPosition);
                this.MediaScale = Transactions.NewArray(this.Holders, x => x.MediaScale);
                this.MediaScaleOrigin = Transactions.NewArray(this.Holders, x => x.MediaScaleOrigin);
                this.editor = editor;
            }

            protected override Task UndoAsyncCore(VideoClipViewModel holder, int i)
            {
                holder.MediaPosition = this.MediaPosition[i].Original;
                holder.MediaScale = this.MediaScale[i].Original;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Original;
                this.editor.RequeryPositionFromHandlers();
                this.editor.RequeryScaleFromHandlers();
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(VideoClipViewModel holder, int i)
            {
                holder.MediaPosition = this.MediaPosition[i].Current;
                holder.MediaScale = this.MediaScale[i].Current;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Current;
                this.editor.RequeryPositionFromHandlers();
                this.editor.RequeryScaleFromHandlers();
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            public bool AreValuesUnchanged()
            {
                return this.MediaPosition.All(x => x.IsUnchanged()) && this.MediaScale.All(x => x.IsUnchanged()) && this.MediaScaleOrigin.All(x => x.IsUnchanged());
            }
        }

        protected class HistoryClipOpacity : BaseHistoryMultiHolderAction<VideoClipViewModel>
        {
            public readonly Transaction<double>[] Opacity;
            public readonly VideoClipDataEditorViewModel editor;

            public HistoryClipOpacity(IEnumerable<VideoClipViewModel> holders, VideoClipDataEditorViewModel editor) : base(holders)
            {
                this.Opacity = Transactions.NewArray(this.Holders, x => x.Opacity);
                this.editor = editor;
            }

            protected override Task UndoAsyncCore(VideoClipViewModel holder, int i)
            {
                holder.Opacity = this.Opacity[i].Original;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(VideoClipViewModel holder, int i)
            {
                holder.Opacity = this.Opacity[i].Current;
                this.editor.RequeryOpacityFromHandlers();
                return Task.CompletedTask;
            }
        }
    }
}