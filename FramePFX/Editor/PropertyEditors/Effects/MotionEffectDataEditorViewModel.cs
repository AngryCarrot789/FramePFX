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

namespace FramePFX.Editor.PropertyEditors.Effects
{
    public class MotionEffectDataEditorViewModel : BaseEffectDataEditorViewModel
    {
        private Vector2 mediaPosition;
        private Vector2 mediaScale;
        private Vector2 mediaScaleOrigin;
        private double mediaRotation;
        private Vector2 mediaRotationOrigin;
        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;
        private float bothRotationOrigin;

        public MotionEffectViewModel SingleSelection => (MotionEffectViewModel) this.Handlers[0];

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.SingleSelection.MediaPositionAutomationSequence;
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.SingleSelection.MediaScaleAutomationSequence;
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.SingleSelection.MediaScaleOriginAutomationSequence;
        public AutomationSequenceViewModel MediaRotationAutomationSequence => this.SingleSelection.MediaRotationAutomationSequence;
        public AutomationSequenceViewModel MediaRotationOriginAutomationSequence => this.SingleSelection.MediaRotationOriginAutomationSequence;

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
                bool useAddition = this.Handlers.Count > 1 && this.MediaPositionEditStateChangedCommand.IsEditing;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaPosition) this.MediaPositionEditStateChangedCommand.HistoryAction)?.MediaPosition;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
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
                bool useAddition = this.Handlers.Count > 1 && this.MediaScaleEditStateChangedCommand.IsEditing;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaScale) this.MediaScaleEditStateChangedCommand.HistoryAction)?.MediaScale;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
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
                bool useAddition = this.MediaScaleOriginEditStateChangedCommand.IsEditing && this.Handlers.Count > 1;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaScaleOrigin) this.MediaScaleOriginEditStateChangedCommand.HistoryAction)?.MediaScaleOrigin;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
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

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public double MediaRotation
        {
            get => this.mediaRotation;
            set
            {
                double oldVal = this.mediaRotation;
                this.mediaRotation = value;
                bool useAddition = this.Handlers.Count > 1 && this.MediaRotationEditStateChangedCommand.IsEditing;
                double change = value - oldVal;
                Transaction<double>[] array = ((HistoryClipMediaRotation) this.MediaRotationEditStateChangedCommand.HistoryAction)?.MediaRotation;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
                    double val = useAddition ? (clip.MediaRotation + change) : value;
                    clip.MediaRotation = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public float MediaRotationOriginX
        {
            get => this.MediaRotationOrigin.X;
            set => this.MediaRotationOrigin = new Vector2(value, this.MediaRotationOrigin.Y);
        }

        public float MediaRotationOriginY
        {
            get => this.MediaRotationOrigin.Y;
            set => this.MediaRotationOrigin = new Vector2(this.MediaRotationOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaRotationOrigin
        {
            get => this.mediaRotationOrigin;
            set
            {
                Vector2 oldVal = this.mediaRotationOrigin;
                this.mediaRotationOrigin = value;
                bool useAddition = this.MediaRotationOriginEditStateChangedCommand.IsEditing && this.Handlers.Count > 1;
                Vector2 change = value - oldVal;
                Transaction<Vector2>[] array = ((HistoryClipMediaRotationOrigin) this.MediaRotationOriginEditStateChangedCommand.HistoryAction)?.MediaRotationOrigin;
                for (int i = 0, c = this.Handlers.Count; i < c; i++)
                {
                    MotionEffectViewModel clip = (MotionEffectViewModel) this.Handlers[i];
                    Vector2 val = useAddition ? (clip.MediaRotationOrigin + change) : value;
                    clip.MediaRotationOrigin = val;
                    if (array != null)
                    {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaRotationOriginX));
                this.RaisePropertyChanged(nameof(this.MediaRotationOriginY));
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

        public float BothRotationOrigin
        {
            get => this.bothRotationOrigin;
            set
            {
                this.MediaRotationOrigin += new Vector2(value - this.bothRotationOrigin);
                this.RaisePropertyChanged();
                this.bothRotationOrigin = 0;
            }
        }

        #endregion

        public RelayCommand InsertMediaPositionKeyFrameCommand => this.SingleSelection?.InsertMediaPositionKeyFrameCommand;
        public RelayCommand InsertMediaScaleKeyFrameCommand => this.SingleSelection?.InsertMediaScaleKeyFrameCommand;
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand => this.SingleSelection?.InsertMediaScaleOriginKeyFrameCommand;
        public RelayCommand InsertMediaRotationKeyFrameCommand => this.SingleSelection?.InsertMediaRotationKeyFrameCommand;
        public RelayCommand InsertMediaRotationOriginKeyFrameCommand => this.SingleSelection?.InsertMediaRotationOriginKeyFrameCommand;

        public EditStateCommand MediaPositionEditStateChangedCommand { get; }
        public EditStateCommand MediaScaleEditStateChangedCommand { get; }
        public EditStateCommand MediaScaleOriginEditStateChangedCommand { get; }
        public EditStateCommand MediaRotationEditStateChangedCommand { get; }
        public EditStateCommand MediaRotationOriginEditStateChangedCommand { get; }

        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }
        public RelayCommand ResetMediaRotationCommand { get; }
        public RelayCommand ResetMediaRotationOriginCommand { get; }

        private readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaRotationHandler;
        private readonly RefreshAutomationValueEventHandler RefreshMediaRotationOriginHandler;

        public new IEnumerable<MotionEffectViewModel> Effects => this.Handlers.Cast<MotionEffectViewModel>();

        public RelayCommand<int> SetPresetScaleOriginCommand { get; }
        public RelayCommand<int> SetPresetRotationOriginCommand { get; }

        public MotionEffectDataEditorViewModel() : base(typeof(MotionEffectViewModel))
        {
            this.RefreshMediaPositionHandler = this.RefreshMediaPosition;
            this.RefreshMediaScaleHandler = this.RefreshMediaScale;
            this.RefreshMediaScaleOriginHandler = this.RefreshMediaScaleOrigin;
            this.RefreshMediaRotationHandler = this.RefreshMediaRotation;
            this.RefreshMediaRotationOriginHandler = this.RefreshMediaRotationOrigin;

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue);
            this.ResetMediaRotationCommand = new RelayCommand(() => this.MediaRotation = MotionEffect.MediaRotationKey.Descriptor.DefaultValue);
            this.ResetMediaRotationOriginCommand = new RelayCommand(() => this.MediaRotationOrigin = MotionEffect.MediaRotationOriginKey.Descriptor.DefaultValue);

            this.MediaPositionEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaPosition(this), "Modify media position");
            this.MediaScaleEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaScale(this), "Modify media scale");
            this.MediaScaleOriginEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaScaleOrigin(this), "Modify media scale origin");
            this.MediaRotationEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaRotation(this), "Modify media rotation");
            this.MediaRotationOriginEditStateChangedCommand = new EditStateCommand(() => new HistoryClipMediaRotationOrigin(this), "Modify media rotation origin");

            this.SetPresetScaleOriginCommand = new RelayCommand<int>((i) =>
            {
                if (GetOriginVectorForCommand(i, out Vector2 vec))
                {
                    this.MediaScaleOrigin = vec;
                }
            });

            this.SetPresetRotationOriginCommand = new RelayCommand<int>((i) =>
            {
                if (GetOriginVectorForCommand(i, out Vector2 vec))
                {
                    this.MediaRotationOrigin = vec;
                }
            });
        }

        private static bool GetOriginVectorForCommand(int index, out Vector2 vec)
        {
            switch (index)
            {
                case 0:
                    vec = new Vector2(0.0f, 0.0f);
                    break;
                case 1:
                    vec = new Vector2(0.5f, 0.0f);
                    break;
                case 2:
                    vec = new Vector2(1.0f, 0.0f);
                    break;
                case 3:
                    vec = new Vector2(0.0f, 0.5f);
                    break;
                case 4:
                    vec = new Vector2(0.5f, 0.5f);
                    break;
                case 5:
                    vec = new Vector2(1.0f, 0.5f);
                    break;
                case 6:
                    vec = new Vector2(0.0f, 1.0f);
                    break;
                case 7:
                    vec = new Vector2(0.5f, 1.0f);
                    break;
                case 8:
                    vec = new Vector2(1.0f, 1.0f);
                    break;
                default:
                    vec = default;
                    return false;
            }

            return true;
        }

        public void RequeryPositionFromHandlers()
        {
            this.mediaPosition = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaPosition, out Vector2 a) ? a : default;
            this.RaisePropertyChanged(nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        public void RequeryScaleFromHandlers()
        {
            this.mediaScale = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaScale, out Vector2 b) ? b : default;
            this.RaisePropertyChanged(nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        public void RequeryScaleOriginFromHandlers()
        {
            this.mediaScaleOrigin = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaScaleOrigin, out Vector2 c) ? c : default;
            this.RaisePropertyChanged(nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        public void RequeryRotationFromHandlers()
        {
            this.mediaRotation = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaRotation, out double b) ? b : default;
            this.RaisePropertyChanged(nameof(this.MediaRotation));
        }

        public void RequeryRotationOriginFromHandlers()
        {
            this.mediaRotationOrigin = GetEqualValue(this.Handlers, (x) => ((MotionEffectViewModel) x).MediaRotationOrigin, out Vector2 c) ? c : default;
            this.RaisePropertyChanged(nameof(this.MediaRotationOrigin));
            this.RaisePropertyChanged(nameof(this.MediaRotationOriginX));
            this.RaisePropertyChanged(nameof(this.MediaRotationOriginY));
        }

        private void RefreshMediaPosition(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaPosition, this.SingleSelection.MediaPosition, nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
        }

        private void RefreshMediaScale(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaScale, this.SingleSelection.MediaScale, nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
        }

        private void RefreshMediaScaleOrigin(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaScaleOrigin, this.SingleSelection.MediaScaleOrigin, nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
        }

        private void RefreshMediaRotation(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaRotation, this.SingleSelection.MediaRotation, nameof(this.MediaRotation));
        }

        private void RefreshMediaRotationOrigin(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e)
        {
            this.RaisePropertyChanged(ref this.mediaRotationOrigin, this.SingleSelection.MediaRotationOrigin, nameof(this.MediaRotationOrigin));
            this.RaisePropertyChanged(nameof(this.MediaRotationOriginX));
            this.RaisePropertyChanged(nameof(this.MediaRotationOriginY));
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            if (this.Handlers.Count == 1)
            {
                MotionEffectViewModel clip = this.SingleSelection;
                clip.MediaPositionAutomationSequence.RefreshValue += this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue += this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue += this.RefreshMediaScaleOriginHandler;
                clip.MediaRotationAutomationSequence.RefreshValue += this.RefreshMediaRotationHandler;
                clip.MediaRotationOriginAutomationSequence.RefreshValue += this.RefreshMediaRotationOriginHandler;
            }

            this.RaisePropertyChanged(nameof(this.InsertMediaPositionKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleOriginKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaRotationKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaRotationOriginKeyFrameCommand));

            this.RequeryPositionFromHandlers();
            this.RequeryScaleFromHandlers();
            this.RequeryScaleOriginFromHandlers();
            this.RequeryRotationFromHandlers();
            this.RequeryRotationOriginFromHandlers();

            this.RaisePropertyChanged(nameof(this.MediaPositionAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaRotationAutomationSequence));
            this.RaisePropertyChanged(nameof(this.MediaRotationOriginAutomationSequence));
        }

        protected override void OnClearHandlers()
        {
            base.OnClearHandlers();
            if (this.Handlers.Count == 1)
            {
                MotionEffectViewModel clip = this.SingleSelection;
                clip.MediaPositionAutomationSequence.RefreshValue -= this.RefreshMediaPositionHandler;
                clip.MediaScaleAutomationSequence.RefreshValue -= this.RefreshMediaScaleHandler;
                clip.MediaScaleOriginAutomationSequence.RefreshValue -= this.RefreshMediaScaleOriginHandler;
                clip.MediaRotationAutomationSequence.RefreshValue -= this.RefreshMediaRotationHandler;
                clip.MediaRotationOriginAutomationSequence.RefreshValue -= this.RefreshMediaRotationOriginHandler;
            }

            this.MediaPositionEditStateChangedCommand.OnReset();
            this.MediaScaleEditStateChangedCommand.OnReset();
            this.MediaScaleOriginEditStateChangedCommand.OnReset();
            this.MediaRotationEditStateChangedCommand.OnReset();
            this.MediaRotationOriginEditStateChangedCommand.OnReset();
        }

        protected class HistoryClipMediaPosition : BaseHistoryMultiHolderAction<MotionEffectViewModel>
        {
            public readonly Transaction<Vector2>[] MediaPosition;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaPosition(MotionEffectDataEditorViewModel editor) : base(editor.Effects)
            {
                this.MediaPosition = Transactions.NewArray(this.Holders, x => x.MediaPosition);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaPosition = this.MediaPosition[i].Original;
                this.editor.RequeryPositionFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaPosition = this.MediaPosition[i].Current;
                this.editor.RequeryPositionFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaScale : BaseHistoryMultiHolderAction<MotionEffectViewModel>
        {
            public readonly Transaction<Vector2>[] MediaScale;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaScale(MotionEffectDataEditorViewModel editor) : base(editor.Effects)
            {
                this.MediaScale = Transactions.NewArray(this.Holders, x => x.MediaScale);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaScale = this.MediaScale[i].Original;
                this.editor.RequeryScaleFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaScale = this.MediaScale[i].Current;
                this.editor.RequeryScaleFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaScaleOrigin : BaseHistoryMultiHolderAction<MotionEffectViewModel>
        {
            public readonly Transaction<Vector2>[] MediaScaleOrigin;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaScaleOrigin(MotionEffectDataEditorViewModel editor) : base(editor.Effects)
            {
                this.MediaScaleOrigin = Transactions.NewArray(this.Holders, x => x.MediaScaleOrigin);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Original;
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaScaleOrigin = this.MediaScaleOrigin[i].Current;
                this.editor.RequeryScaleOriginFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaRotation : BaseHistoryMultiHolderAction<MotionEffectViewModel>
        {
            public readonly Transaction<double>[] MediaRotation;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaRotation(MotionEffectDataEditorViewModel editor) : base(editor.Effects)
            {
                this.MediaRotation = Transactions.NewArray(this.Holders, x => x.MediaRotation);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaRotation = this.MediaRotation[i].Original;
                this.editor.RequeryRotationFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaRotation = this.MediaRotation[i].Current;
                this.editor.RequeryRotationFromHandlers();
                return Task.CompletedTask;
            }
        }

        protected class HistoryClipMediaRotationOrigin : BaseHistoryMultiHolderAction<MotionEffectViewModel>
        {
            public readonly Transaction<Vector2>[] MediaRotationOrigin;
            public readonly MotionEffectDataEditorViewModel editor;

            public HistoryClipMediaRotationOrigin(MotionEffectDataEditorViewModel editor) : base(editor.Effects)
            {
                this.MediaRotationOrigin = Transactions.NewArray(this.Holders, x => x.MediaRotationOrigin);
                this.editor = editor;
            }

            protected override Task UndoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaRotationOrigin = this.MediaRotationOrigin[i].Original;
                this.editor.RequeryRotationOriginFromHandlers();
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(MotionEffectViewModel holder, int i)
            {
                holder.MediaRotationOrigin = this.MediaRotationOrigin[i].Current;
                this.editor.RequeryRotationOriginFromHandlers();
                return Task.CompletedTask;
            }
        }
    }

    // Use different types because it's more convenient to create DataTemplates;
    // no need for a template selector to check the mode

    public class MotionEffectDataSingleEditorViewModel : MotionEffectDataEditorViewModel
    {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        private bool isMediaPositionSelected;

        public bool IsMediaPositionSelected
        {
            get => this.isMediaPositionSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isMediaPositionSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaPositionAutomationSequence.IsActiveSequence = value;
            }
        }

        private bool isMediaScaleSelected;

        public bool IsMediaScaleSelected
        {
            get => this.isMediaScaleSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isMediaScaleSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaScaleAutomationSequence.IsActiveSequence = value;
            }
        }

        private bool isMediaScaleOriginSelected;

        public bool IsMediaScaleOriginSelected
        {
            get => this.isMediaScaleOriginSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isMediaScaleOriginSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaScaleOriginAutomationSequence.IsActiveSequence = value;
            }
        }

        private bool isMediaRotationSelected;

        public bool IsMediaRotationSelected
        {
            get => this.isMediaRotationSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isMediaRotationSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaRotationAutomationSequence.IsActiveSequence = value;
            }
        }

        private bool isMediaRotationOriginSelected;

        public bool IsMediaRotationOriginSelected
        {
            get => this.isMediaRotationOriginSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isMediaRotationOriginSelected, value);
                if (this.IsEmpty)
                    return;
                this.MediaRotationOriginAutomationSequence.IsActiveSequence = value;
            }
        }
    }

    public class MotionEffectDataMultiEditorViewModel : MotionEffectDataEditorViewModel
    {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}