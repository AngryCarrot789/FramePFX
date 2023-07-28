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

namespace FramePFX.Core.PropertyEditing.Editors.Editor {
    public class VideoClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        protected HistoryClipMediaTransformation transformationHistory;
        protected HistoryClipOpacity opacityHistory;

        private Vector2 mediaPosition;
        private Vector2 mediaScale;
        private Vector2 mediaScaleOrigin;
        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;
        private double opacity;

        public VideoClipViewModel Clip => (VideoClipViewModel) this.Handlers[0];

        public float MediaPositionX {
            get => this.MediaPosition.X;
            set => this.MediaPosition = new Vector2(value, this.MediaPosition.Y);
        }

        public float MediaPositionY {
            get => this.MediaPosition.Y;
            set => this.MediaPosition = new Vector2(this.MediaPosition.X, value);
        }

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition {
            get => this.mediaPosition;
            set {
                Vector2 oldVal = this.mediaPosition;
                this.mediaPosition = value;
                if (this.transformationHistory != null && this.HistoryManager != null && !this.IsChangingAny()) {
                    foreach (Transaction<Vector2> t in this.transformationHistory.MediaPosition)
                        t.Current = value;
                }

                if (this.Handlers.Count > 1 && this.isEditingMediaPosition) {
                    Vector2 change = value - oldVal;
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaPosition += change;
                    }
                }
                else {
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaPosition = value;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaPositionX));
                this.RaisePropertyChanged(nameof(this.MediaPositionY));
            }
        }

        public float MediaScaleX {
            get => this.MediaScale.X;
            set => this.MediaScale = new Vector2(value, this.MediaScale.Y);
        }

        public float MediaScaleY {
            get => this.MediaScale.Y;
            set => this.MediaScale = new Vector2(this.MediaScale.X, value);
        }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale {
            get => this.mediaScale;
            set {
                Vector2 oldVal = this.mediaScale;
                this.mediaScale = value;
                if (this.transformationHistory != null && this.HistoryManager != null && !this.IsChangingAny()) {
                    foreach (Transaction<Vector2> t in this.transformationHistory.MediaScale)
                        t.Current = value;
                }

                if (this.Handlers.Count > 1 && this.isEditingMediaPosition) {
                    Vector2 change = value - oldVal;
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaScale += change;
                    }
                }
                else {
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaScale = value;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleX));
                this.RaisePropertyChanged(nameof(this.MediaScaleY));
            }
        }

        public float MediaScaleOriginX {
            get => this.MediaScaleOrigin.X;
            set => this.MediaScaleOrigin = new Vector2(value, this.MediaScaleOrigin.Y);
        }

        public float MediaScaleOriginY {
            get => this.MediaScaleOrigin.Y;
            set => this.MediaScaleOrigin = new Vector2(this.MediaScaleOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin {
            get => this.mediaScaleOrigin;
            set {
                Vector2 oldVal = this.mediaScaleOrigin;
                this.mediaScaleOrigin = value;
                if (this.transformationHistory != null && this.HistoryManager != null && !this.IsChangingAny()) {
                    foreach (Transaction<Vector2> t in this.transformationHistory.MediaScaleOrigin)
                        t.Current = value;
                }

                if (this.Handlers.Count > 1 && this.isEditingMediaPosition) {
                    Vector2 change = value - oldVal;
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaScaleOrigin += change;
                    }
                }
                else {
                    foreach (object target in this.Handlers) {
                        ((VideoClipViewModel) target).MediaScaleOrigin = value;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
            }
        }

        #region WPF NumberDragger helpers

        public float BothPos {
            get => this.bothPos;
            set {
                this.MediaPosition += new Vector2(value - this.bothPos);
                this.RaisePropertyChanged();
                this.bothPos = 0;
            }
        }

        public float BothScale {
            get => this.bothScale;
            set {
                this.MediaScale += new Vector2(value - this.bothScale);
                this.RaisePropertyChanged();
                this.bothScale = 0;
            }
        }

        public float BothScaleOrigin {
            get => this.bothScaleOrigin;
            set {
                this.MediaScaleOrigin += new Vector2(value - this.bothScaleOrigin);
                this.RaisePropertyChanged();
                this.bothScaleOrigin = 0;
            }
        }

        #endregion

        public double Opacity {
            get => this.opacity;
            set {
                this.opacity = value;
                if (this.opacityHistory != null && this.HistoryManager != null && !this.IsChangingAny()) {
                    foreach (Transaction<double> t in this.opacityHistory.Opacity) {
                        t.Current = value;
                    }
                }

                foreach (object handler in this.Handlers) {
                    ((VideoClipViewModel) handler).Opacity = value;
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

        private bool isEditingMediaPosition;
        private bool isEditingMediaScale;
        private bool isEditingMediaScaleOrigin;
        private bool isEditingOpacity;

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.Clip.MediaPositionAutomationSequence;
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.Clip.MediaScaleAutomationSequence;
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.Clip.MediaScaleOriginAutomationSequence;
        public AutomationSequenceViewModel OpacityAutomationSequence => this.Clip.OpacityAutomationSequence;

        private readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler;
        private readonly RefreshAutomationValueEventHandler RefreshOpacityHandler;

        public VideoClipDataEditorViewModel() : base(typeof(VideoClipViewModel)) {
            this.RefreshMediaPositionHandler = this.RefreshMediaPosition;
            this.RefreshMediaScaleHandler = this.RefreshMediaScale;
            this.RefreshMediaScaleOriginHandler = this.RefreshMediaScaleOrigin;
            this.RefreshOpacityHandler = this.RefreshOpacity;

            this.ResetMediaPositionCommand =     new RelayCommand(() => this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand =        new RelayCommand(() => this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand =  new RelayCommand(() => this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue);
            this.ResetOpacityCommand =           new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);
            this.BeginEditMediaPositionCommand = new RelayCommand(() => {
                this.isEditingMediaPosition = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaPosition);
            this.BeginEditMediaScaleCommand =       new RelayCommand(() => {
                this.isEditingMediaScale = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaScale);
            this.BeginEditMediaScaleOriginCommand = new RelayCommand(() => {
                this.isEditingMediaScaleOrigin = true;
                this.BeginEditTransformation();
            }, () => !this.isEditingMediaScaleOrigin);
            this.EndEditMediaPositionCommand =      new RelayCommand(() => {
                this.isEditingMediaPosition = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaPosition);
            this.EndEditMediaScaleCommand =         new RelayCommand(() => {
                this.isEditingMediaScale = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaScale);
            this.EndEditMediaScaleOriginCommand =   new RelayCommand(() => {
                this.isEditingMediaScaleOrigin = false;
                this.EndEditTransformation();
            }, () => this.isEditingMediaScaleOrigin);

            this.BeginEditOpacityCommand = new RelayCommand(() => {
                this.isEditingOpacity = true;
                this.BeginEditOpacity();
            }, () => !this.isEditingOpacity);
            this.EndEditOpacityCommand =   new RelayCommand(() => {
                this.isEditingOpacity = false;
                this.EndEditOpacity();
            }, () => this.isEditingOpacity);
        }

        protected void BeginEditTransformation() {
            if (this.transformationHistory != null) {
                this.EndEditTransformation();
            }

            this.transformationHistory = new HistoryClipMediaTransformation(this.Clips);
        }

        protected void EndEditTransformation() {
            if (this.transformationHistory != null) {
                if (!this.transformationHistory.AreValuesUnchanged()) {
                    this.HistoryManager?.AddAction(this.transformationHistory, "Edit transformation");
                }

                this.transformationHistory = null;
            }
        }

        protected void BeginEditOpacity(bool ignoreIfAlreadyExists = false) {
            if (this.opacityHistory != null) {
                if (ignoreIfAlreadyExists)
                    return;
                this.EndEditOpacity();
            }

            this.opacityHistory = new HistoryClipOpacity(this.Clips);
        }

        protected void EndEditOpacity() {
            if (this.opacityHistory != null) {
                this.HistoryManager?.AddAction(this.opacityHistory, "Edit opacity");
                this.opacityHistory = null;
            }
        }

        private void RefreshMediaPosition(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaPosition, ((VideoClipViewModel) this.Handlers[0]).MediaPosition, nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        private void RefreshMediaScale(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaScale, ((VideoClipViewModel) this.Handlers[0]).MediaScale, nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        private void RefreshMediaScaleOrigin(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaScaleOrigin, ((VideoClipViewModel) this.Handlers[0]).MediaScaleOrigin, nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        private void RefreshOpacity(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.opacity, ((VideoClipViewModel) this.Handlers[0]).Opacity, nameof(this.Opacity));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.MediaPositionAutomationSequence.RefreshValue += this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue += this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue += this.RefreshMediaScaleOriginHandler;
                clip.OpacityAutomationSequence.RefreshValue += this.RefreshOpacityHandler;
            }

            this.mediaPosition = GetValueForObjects(this.Handlers, (x) => ((VideoClipViewModel) x).MediaPosition, out Vector2 a) ? a : default;
            this.mediaScale = GetValueForObjects(this.Handlers, (x) => ((VideoClipViewModel) x).MediaScale, out Vector2 b) ? b : default;
            this.mediaScaleOrigin = GetValueForObjects(this.Handlers, (x) => ((VideoClipViewModel) x).MediaScaleOrigin, out Vector2 c) ? c : default;
            this.opacity = GetValueForObjects(this.Handlers, (x) => ((VideoClipViewModel) x).Opacity, out double d) ? d : default;

            this.RaisePropertyChanged(nameof(this.MediaPositionAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginAutomationSequence));
            this.RaisePropertyChanged(nameof(this.OpacityAutomationSequence));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1) {
                VideoClipViewModel clip = (VideoClipViewModel) this.Handlers[0];
                clip.MediaPositionAutomationSequence.RefreshValue -= this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue -= this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue -= this.RefreshMediaScaleOriginHandler;
                clip.OpacityAutomationSequence.RefreshValue -= this.RefreshOpacityHandler;
            }

            this.transformationHistory = null;
            this.opacityHistory = null;
        }

        protected class HistoryClipMediaTransformation : BaseHistoryMultiHolderAction<VideoClipViewModel> {
            public readonly Transaction<Vector2>[] MediaPosition;
            public readonly Transaction<Vector2>[] MediaScale;
            public readonly Transaction<Vector2>[] MediaScaleOrigin;

            public HistoryClipMediaTransformation(IEnumerable<VideoClipViewModel> holders) : base(holders) {
                this.MediaPosition = Transactions.NewArray(this.Holders, x => x.MediaPosition);
                this.MediaScale = Transactions.NewArray(this.Holders, x => x.MediaScale);
                this.MediaScaleOrigin = Transactions.NewArray(this.Holders, x => x.MediaScaleOrigin);
            }

            protected override Task UndoAsyncCore(VideoClipViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Original;
                holder.MediaScale = this.MediaScale[i].Original;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(VideoClipViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Current;
                holder.MediaScale = this.MediaScale[i].Current;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Current;
                return Task.CompletedTask;
            }

            public bool AreValuesUnchanged() {
                return this.MediaPosition.All(x => x.AreUnchanged()) && this.MediaScale.All(x => x.AreUnchanged()) && this.MediaScaleOrigin.All(x => x.AreUnchanged());
            }
        }

        protected class HistoryClipOpacity : BaseHistoryMultiHolderAction<VideoClipViewModel> {
            public readonly Transaction<double>[] Opacity;

            public HistoryClipOpacity(IEnumerable<VideoClipViewModel> holders) : base(holders) {
                this.Opacity = Transactions.NewArray(this.Holders, x => x.Opacity);
            }

            protected override Task UndoAsyncCore(VideoClipViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(VideoClipViewModel holder, int i) {
                holder.Opacity = this.Opacity[i].Current;
                return Task.CompletedTask;
            }
        }
    }
}