using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.PropertyEditors;
using FramePFX.History;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Editors
{
    public abstract class AutomatablePropertyEditorViewModel<TValue> : BasePropertyEditorViewModel
    {
        private TValue value;

        public TValue Value
        {
            get => this.value;
            set
            {
                TValue oldValue = this.value;
                this.RaisePropertyChanged(ref this.value, value);
                if (this.HasHandlers)
                {
                    bool isEditStateForced = false;
                    if (!this.EditStateChangedCommand.IsEditing)
                    {
                        this.EditStateChangedCommand.OnBeginEdit();
                        isEditStateForced = true;
                    }

                    this.OnValueChanged(this.castedHandlerList, oldValue, value);
                    if (isEditStateForced)
                    {
                        this.EditStateChangedCommand.OnFinishEdit();
                    }
                }
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.RaisePropertyChanged(ref this.isSelected, value);
                if (this.IsEmpty)
                    return;

                if (value)
                {
                    foreach (IAutomatableViewModel item in this.MyHandlers)
                    {
                        item.AutomationData[this.AutomationKey].IsActiveSequence = true;
                    }
                }
            }
        }

        private bool isOverrideEnabled;

        public bool IsOverrideEnabled
        {
            get => this.isOverrideEnabled;
            set
            {
                this.RaisePropertyChanged(ref this.isOverrideEnabled, value);
                if (this.IsEmpty)
                    return;

                if (value)
                {
                    foreach (IAutomatableViewModel item in this.MyHandlers)
                    {
                        item.AutomationData[this.AutomationKey].IsOverrideEnabled = value;
                    }
                }
            }
        }

        public RelayCommand ResetValueCommand { get; }

        public RelayCommand InsertKeyFrameCommand { get; }

        public EditStateCommand EditStateChangedCommand { get; }

        public IAutomatableViewModel SingleHandler => (IAutomatableViewModel) this.Handlers[0];

        public IEnumerable<IAutomatableViewModel> MyHandlers => this.Handlers.Cast<IAutomatableViewModel>();

        public AutomationKey AutomationKey { get; }

        public Func<IAutomatableViewModel, TValue> Getter { get; }

        public Action<IAutomatableViewModel, TValue> Setter { get; }

        public AutomationSequenceViewModel SingleHandlerAutomationSequence => this.SingleHandler.AutomationData[this.AutomationKey];

        private readonly Func<object, TValue> nonGenericOwnerGetter;
        private readonly Action<object, TValue> nonGenericOwnerSetter;
        private readonly RefreshAutomationValueEventHandler RefreshValueForSingleHandler;
        private CastingList<IAutomatableViewModel> castedHandlerList;

        protected AutomatablePropertyEditorViewModel(Type applicableType, AutomationKey automationKey, Func<IAutomatableViewModel, TValue> getter, Action<IAutomatableViewModel, TValue> setter) : base(applicableType)
        {
            this.Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            this.Setter = setter ?? throw new ArgumentNullException(nameof(setter));
            this.nonGenericOwnerGetter = x => this.Getter((IAutomatableViewModel) x);
            this.nonGenericOwnerSetter = (x, y) => this.Setter((IAutomatableViewModel) x, y);
            this.AutomationKey = automationKey ?? throw new ArgumentNullException(nameof(automationKey));
            this.RefreshValueForSingleHandler = (sender, e) => this.RaisePropertyChanged(ref this.value, this.Getter(this.SingleHandler), nameof(this.Value));
            this.EditStateChangedCommand = new EditStateCommand(() => new HistoryValue(this), "Modify opacity");
            this.ResetValueCommand = new RelayCommand(this.ResetValue, () => this.HasHandlers);
            this.InsertKeyFrameCommand = new RelayCommand(() =>
            {
                foreach (IAutomatableViewModel handler in this.MyHandlers)
                {
                    if (AutomationUtils.GetSuitableFrameForAutomatable(handler, this.AutomationKey, out long frame))
                    {
                        this.InsertKeyFrame(handler, frame);
                    }
                }
            }, () => this.HasHandlers);
        }

        /// <summary>
        /// Invoked when the value changes. This should use the <see cref="SetValueAndHistory"/> to update the handler's value and history state
        /// </summary>
        /// <param name="handlers">All of our handlers. Will always have at least 1 value</param>
        /// <param name="oldValue">The previous value</param>
        /// <param name="value">The new value</param>
        protected abstract void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, TValue oldValue, TValue value);

        /// <summary>
        /// The implementation of the reset value function. This should use the <see cref="SetValueAndHistory"/> to update the handlers' value and history state
        /// </summary>
        /// <param name="handlers">All of our handlers. Will always have at least 1 value</param>
        protected abstract void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers);

        /// <summary>
        /// Invoked to invoke a key frame at the given frame
        /// </summary>
        protected abstract void InsertKeyFrame(IAutomatableViewModel handler, long frame);

        public void ResetValue()
        {
            if (!this.HasHandlers)
            {
                return;
            }

            bool isEditStateForced = false;
            if (!this.EditStateChangedCommand.IsEditing)
            {
                this.EditStateChangedCommand.OnBeginEdit();
                isEditStateForced = true;
            }

            this.OnResetValue(this.castedHandlerList);
            if (isEditStateForced)
            {
                this.EditStateChangedCommand.OnFinishEdit();
            }
        }

        protected virtual void SetValueAndHistory(int index, TValue value)
        {
            this.Setter((IAutomatableViewModel) this.Handlers[index], value);
            ((HistoryValue) this.EditStateChangedCommand.HistoryAction).Value[index].Current = value;
        }

        protected virtual void SetValuesAndHistory(TValue value)
        {
            Transaction<TValue>[] history = ((HistoryValue) this.EditStateChangedCommand.HistoryAction).Value;
            for (int i = 0, count = this.Handlers.Count; i < count; i++)
            {
                this.Setter((IAutomatableViewModel) this.Handlers[i], value);
                history[i].Current = value;
            }
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            this.castedHandlerList = new CastingList<IAutomatableViewModel>(this.Handlers);

            if (this.Handlers.Count == 1)
            {
                this.SingleHandlerAutomationSequence.RefreshValue += this.RefreshValueForSingleHandler;
            }

            this.RequeryValueFromHandlers();
            this.RaisePropertyChanged(nameof(this.SingleHandlerAutomationSequence));
        }

        public void RequeryValueFromHandlers()
        {
            this.value = GetEqualValue(this.Handlers, this.nonGenericOwnerGetter, out TValue d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Value));
        }

        protected override void OnClearHandlers()
        {
            base.OnClearHandlers();
            this.castedHandlerList = null;
            if (this.Handlers.Count == 1)
            {
                this.SingleHandlerAutomationSequence.RefreshValue -= this.RefreshValueForSingleHandler;
            }

            this.EditStateChangedCommand.OnReset();
        }

        protected class HistoryValue : BaseHistoryMultiHolderAction<IAutomatableViewModel>
        {
            public readonly Transaction<TValue>[] Value;
            public readonly AutomatablePropertyEditorViewModel<TValue> editor;

            public HistoryValue(AutomatablePropertyEditorViewModel<TValue> editor) : base(editor.MyHandlers)
            {
                Transaction<TValue>[] array = new Transaction<TValue>[this.Holders.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Transactions.ForBoth(editor.Getter(this.Holders[i]));
                }

                this.Value = array;
                this.editor = editor;
            }

            protected override Task UndoAsync(IAutomatableViewModel holder, int i)
            {
                this.editor.Setter(holder, this.Value[i].Original);
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(IAutomatableViewModel holder, int i)
            {
                this.editor.Setter(holder, this.Value[i].Current);
                return Task.CompletedTask;
            }

            protected override Task OnUndoCompleteAsync(ErrorList errors)
            {
                this.editor.RequeryValueFromHandlers();
                return base.OnUndoCompleteAsync(errors);
            }

            protected override Task OnRedoCompleteAsync(ErrorList errors)
            {
                this.editor.RequeryValueFromHandlers();
                return base.OnRedoCompleteAsync(errors);
            }
        }
    }
}