using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;
using FramePFX.History;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.PropertyEditors.Effects {
    public class MotionEffectDataEditorViewModel : BaseEffectDataEditorViewModel {
        private Vector2 mediaPosition;
        private Vector2 mediaScale;
        private Vector2 mediaScaleOrigin;
        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;

        public MotionEffectViewModel SingleSelection => (MotionEffectViewModel) this.Handlers[0];

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.SingleSelection.MediaPositionAutomationSequence;
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.SingleSelection.MediaScaleAutomationSequence;
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.SingleSelection.MediaScaleOriginAutomationSequence;

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
                bool useAddition = this.Handlers.Count > 1 && this.MediaPositionEditStateChangedCommand.IsEditing;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaPosition) this.MediaPositionEditStateChangedCommand.HistoryAction)?.MediaPosition;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaPosition + change) : value;
                    clip.MediaPosition = val;
                    if (array != null) {
                        array[i].Current = val;
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
                bool useAddition = this.Handlers.Count > 1 && this.MediaScaleEditStateChangedCommand.IsEditing;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaScale) this.MediaScaleEditStateChangedCommand.HistoryAction)?.MediaScale;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaScale + change) : value;
                    clip.MediaScale = val;
                    if (array != null) {
                        array[i].Current = val;
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
                bool useAddition = this.MediaScaleOriginEditStateChangedCommand.IsEditing && this.Handlers.Count > 1;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaScaleOrigin) this.MediaScaleOriginEditStateChangedCommand.HistoryAction)?.MediaScaleOrigin;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaScaleOrigin + change) : value;
                    clip.MediaScaleOrigin = val;
                    if (array != null) {
                        array[i].Current = val;
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

        public RelayCommand InsertMediaPositionKeyFrameCommand => this.SingleSelection?.InsertMediaPositionKeyFrameCommand;
        public RelayCommand InsertMediaScaleKeyFrameCommand => this.SingleSelection?.InsertMediaScaleKeyFrameCommand;
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand => this.SingleSelection?.InsertMediaScaleOriginKeyFrameCommand;

        public EditStateCommand MediaPositionEditStateChangedCommand { get; }
        public EditStateCommand MediaScaleEditStateChangedCommand { get; }
        public EditStateCommand MediaScaleOriginEditStateChangedCommand { get; }

        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }

        private readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler;

        public new IEnumerable<MotionEffectViewModel> Effects => this.Handlers.Cast<MotionEffectViewModel>();

        public RelayCommand<int> SetPresetScaleOriginCommand { get; }

        public MotionEffectDataEditorViewModel() : base(typeof(MotionEffectViewModel)) {
            this.RefreshMediaPositionHandler = this.RefreshMediaPosition;
            this.RefreshMediaScaleHandler = this.RefreshMediaScale;
            this.RefreshMediaScaleOriginHandler = this.RefreshMediaScaleOrigin;

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue);

            this.RefreshMediaPositionHandler = this.RefreshMediaPosition;
            this.RefreshMediaScaleHandler = this.RefreshMediaScale;
            this.RefreshMediaScaleOriginHandler = this.RefreshMediaScaleOrigin;
            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue);

            this.MediaPositionEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaPosition(this), "Modify media position");
            this.MediaScaleEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaScale(this), "Modify media scale");
            this.MediaScaleOriginEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaScaleOrigin(this), "Modify media scale origin");

            this.SetPresetScaleOriginCommand = new RelayCommand<int>((i) => {
                switch (i) {
                    case 0: this.MediaScaleOrigin = new Vector2(0.0f, 0.0f); break;
                    case 1: this.MediaScaleOrigin = new Vector2(0.5f, 0.0f); break;
                    case 2: this.MediaScaleOrigin = new Vector2(1.0f, 0.0f); break;
                    case 3: this.MediaScaleOrigin = new Vector2(0.0f, 0.5f); break;
                    case 4: this.MediaScaleOrigin = new Vector2(0.5f, 0.5f); break;
                    case 5: this.MediaScaleOrigin = new Vector2(1.0f, 0.5f); break;
                    case 6: this.MediaScaleOrigin = new Vector2(0.0f, 1.0f); break;
                    case 7: this.MediaScaleOrigin = new Vector2(0.5f, 1.0f); break;
                    case 8: this.MediaScaleOrigin = new Vector2(1.0f, 1.0f); break;
                }
            });
        }

        public void RequeryPositionFromHandlers() {
            this.mediaPosition = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaPosition, out Vector2 a) ? a : default;
            this.RaisePropertyChanged(nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        public void RequeryScaleFromHandlers() {
            this.mediaScale = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaScale, out Vector2 b) ? b : default;
            this.RaisePropertyChanged(nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        public void RequeryScaleOriginFromHandlers() {
            this.mediaScaleOrigin = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaScaleOrigin, out Vector2 c) ? c : default;
            this.RaisePropertyChanged(nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        private void RefreshMediaPosition(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaPosition, this.SingleSelection.MediaPosition, nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        private void RefreshMediaScale(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaScale, this.SingleSelection.MediaScale, nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        private void RefreshMediaScaleOrigin(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.mediaScaleOrigin, this.SingleSelection.MediaScaleOrigin, nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1) {
                MotionEffectViewModel clip = this.SingleSelection;
                clip.MediaPositionAutomationSequence.RefreshValue += this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue += this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue += this.RefreshMediaScaleOriginHandler;
            }

            this.RaisePropertyChanged(nameof(this.InsertMediaPositionKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleOriginKeyFrameCommand));

            this.RequeryPositionFromHandlers();
            this.RequeryScaleFromHandlers();
            this.RequeryScaleOriginFromHandlers();

            this.RaisePropertyChanged(nameof(this.MediaPositionAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginAutomationSequence));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1) {
                MotionEffectViewModel clip = this.SingleSelection;
                clip.MediaPositionAutomationSequence.RefreshValue -= this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue -= this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue -= this.RefreshMediaScaleOriginHandler;
            }

            this.MediaPositionEditStateChangedCommand.OnReset();
            this.MediaScaleEditStateChangedCommand.OnReset();
            this.MediaScaleOriginEditStateChangedCommand.OnReset();
        }

        protected class HistoryClipMediaTransformation : BaseHistoryMultiHolderAction<MotionEffectViewModel> {
            public readonly Transaction<Vector2>[] MediaPosition;
            public readonly Transaction<Vector2>[] MediaScale;
            public readonly Transaction<Vector2>[] MediaScaleOrigin;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaTransformation(IEnumerable<MotionEffectViewModel> holders, MotionEffectDataEditorViewModel editor) : base(holders) {
                this.MediaPosition = Transactions.NewArray(this.Holders, x => x.MediaPosition);
                this.MediaScale = Transactions.NewArray(this.Holders, x => x.MediaScale);
                this.MediaScaleOrigin = Transactions.NewArray(this.Holders, x => x.MediaScaleOrigin);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Original;
                holder.MediaScale = this.MediaScale[i].Original;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Original;
                this.editor.RequeryPositionFromHandlers();
                this.editor.RequeryScaleFromHandlers();
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Current;
                holder.MediaScale = this.MediaScale[i].Current;
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Current;
                this.editor.RequeryPositionFromHandlers();
                this.editor.RequeryScaleFromHandlers();
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            public bool AreValuesUnchanged() {
                return this.MediaPosition.All(x => x.IsUnchanged()) && this.MediaScale.All(x => x.IsUnchanged()) && this.MediaScaleOrigin.All(x => x.IsUnchanged());
            }
        }

        protected class HistoryClipMediaPosition : BaseHistoryMultiHolderAction<MotionEffectViewModel> {
            public readonly Transaction<Vector2>[] MediaPosition;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaPosition(MotionEffectDataEditorViewModel editor) : base(editor.Effects) {
                this.MediaPosition = Transactions.NewArray(this.Holders, x => x.MediaPosition);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Original;
                this.editor.RequeryPositionFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaPosition = this.MediaPosition[i].Current;
                this.editor.RequeryPositionFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaScale : BaseHistoryMultiHolderAction<MotionEffectViewModel> {
            public readonly Transaction<Vector2>[] MediaScale;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaScale(MotionEffectDataEditorViewModel editor) : base(editor.Effects) {
                this.MediaScale = Transactions.NewArray(this.Holders, x => x.MediaScale);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaScale = this.MediaScale[i].Original;
                this.editor.RequeryScaleFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaScale = this.MediaScale[i].Current;
                this.editor.RequeryScaleFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaScaleOrigin : BaseHistoryMultiHolderAction<MotionEffectViewModel> {
            public readonly Transaction<Vector2>[] MediaScaleOrigin;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaScaleOrigin(MotionEffectDataEditorViewModel editor) : base(editor.Effects) {
                this.MediaScaleOrigin = Transactions.NewArray(this.Holders, x => x.MediaScaleOrigin);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Original;
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i) {
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Current;
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }
        }
    }

    // Use different types because it's more convenient to create DataTemplates;
    // no need for a template selector to check the mode

    public class MotionEffectDataSingleEditorViewModel : MotionEffectDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        private bool isMediaPositionSelected;
        public bool IsMediaPositionSelected {
            get => this.isMediaPositionSelected;
            set {
                this.RaisePropertyChanged(ref this.isMediaPositionSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaPositionAutomationSequence.IsActive = value;
            }
        }

        private bool isMediaScaleSelected;
        public bool IsMediaScaleSelected {
            get => this.isMediaScaleSelected;
            set {
                this.RaisePropertyChanged(ref this.isMediaScaleSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaScaleAutomationSequence.IsActive = value;
            }
        }

        private bool isMediaScaleOriginSelected;
        public bool IsMediaScaleOriginSelected {
            get => this.isMediaScaleOriginSelected;
            set {
                this.RaisePropertyChanged(ref this.isMediaScaleOriginSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaScaleOriginAutomationSequence.IsActive = value;
            }
        }
    }

    public class MotionEffectDataMultiEditorViewModel : MotionEffectDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}