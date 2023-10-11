using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

namespace FramePFX.PropertyEditing.Editors {
    public abstract class AutomatablePropertyEditorViewModel<TValue> : BasePropertyEditorViewModel {
        private static readonly Dictionary<PropertyPath, PropertyData> GeneratedPropertyData;
        private readonly RefreshAutomationValueEventHandler RefreshValueForSingleHandler;
        private readonly PropertyPath propertyPath;
        private readonly PropertyData propertyData;
        private CastingList<IAutomatableViewModel> castedHandlerList;
        private TValue value;
        private bool isSelected;
        private bool isOverrideEnabled;

        /// <summary>
        /// Gets the first handler available
        /// </summary>
        public IAutomatableViewModel SingleHandler {
            get {
                if (this.IsSingleSelection)
                    return (IAutomatableViewModel) this.Handlers[0];
                return null;
            }
        }

        public IEnumerable<IAutomatableViewModel> MyHandlers => this.castedHandlerList ?? Enumerable.Empty<IAutomatableViewModel>();

        /// <summary>
        /// Gets the name of the property that this editor targets
        /// </summary>
        public string PropertyName => this.propertyPath.name;

        /// <summary>
        /// Gets a function that can be used to get the live value from a target (view model)
        /// </summary>
        public Func<IAutomatableViewModel, TValue> Getter => this.propertyData.getter;

        /// <summary>
        /// Gets a function that can be used to set the live value for a target (view model)
        /// </summary>
        public Action<IAutomatableViewModel, TValue> Setter => this.propertyData.setter;

        public TValue Value {
            get => this.value;
            set {
                TValue oldValue = this.value;
                this.RaisePropertyChanged(ref this.value, value);
                if (this.HasHandlers) {
                    bool isEditStateForced = false;
                    if (!this.EditStateChangedCommand.IsEditing) {
                        this.EditStateChangedCommand.OnBeginEdit();
                        isEditStateForced = true;
                    }

                    this.OnValueChanged(this.castedHandlerList, oldValue, value, !isEditStateForced && this.Handlers.Count > 1);
                    if (isEditStateForced) {
                        this.EditStateChangedCommand.OnFinishEdit();
                    }
                }
            }
        }

        public bool IsSelected {
            get => this.isSelected;
            set {
                this.RaisePropertyChanged(ref this.isSelected, value);
                if (this.IsEmpty)
                    return;

                if (value) {
                    foreach (IAutomatableViewModel item in this.MyHandlers) {
                        item.AutomationData[this.AutomationKey].IsActiveSequence = true;
                    }
                }
            }
        }

        public bool IsOverrideEnabled {
            get => this.isOverrideEnabled;
            set {
                this.RaisePropertyChanged(ref this.isOverrideEnabled, value);
                if (value && !this.IsEmpty) {
                    foreach (IAutomatableViewModel item in this.castedHandlerList) {
                        item.AutomationData[this.AutomationKey].IsOverrideEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the automation key that this editor uses
        /// </summary>
        public AutomationKey AutomationKey { get; }

        /// <summary>
        /// Gets the automation sequence that <see cref="AutomationKey"/> maps to, only for our single handler
        /// </summary>
        public AutomationSequenceViewModel SingleHandlerAutomationSequence {
            get {
                if (this.IsSingleSelection)
                    return this.SingleHandler.AutomationData[this.AutomationKey];
                return null;
            }
        }

        public RelayCommand ResetValueCommand { get; }

        public RelayCommand InsertKeyFrameCommand { get; }

        public EditStateCommand EditStateChangedCommand { get; }

        protected AutomatablePropertyEditorViewModel(Type targetType, string propertyName, AutomationKey automationKey) : base(targetType) {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null, empty or whitespaces");
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            this.propertyPath = new PropertyPath(targetType, propertyName);
            this.propertyData = GenerateData(this.propertyPath);
            this.AutomationKey = automationKey ?? throw new ArgumentNullException(nameof(automationKey));
            this.RefreshValueForSingleHandler = (sender, e) => this.RaisePropertyChanged(ref this.value, this.Getter(this.SingleHandler), nameof(this.Value));
            this.EditStateChangedCommand = new EditStateCommand(() => new HistoryValue(this), "Modify opacity");
            this.ResetValueCommand = new RelayCommand(this.ResetValue, () => this.HasHandlers);
            this.InsertKeyFrameCommand = new RelayCommand(() => {
                foreach (IAutomatableViewModel handler in this.MyHandlers) {
                    if (AutomationUtils.GetSuitableFrameForAutomatable(handler, this.AutomationKey, out long frame)) {
                        this.InsertKeyFrame(handler, frame);
                    }
                }
            }, () => this.IsMultiSelection);
        }

        static AutomatablePropertyEditorViewModel() {
            GeneratedPropertyData = new Dictionary<PropertyPath, PropertyData>();
        }

        /// <summary>
        /// Invoked when the value changes. This should use the <see cref="SetValueAndHistory"/> to update the handler's value and history state
        /// </summary>
        /// <param name="handlers">All of our handlers. Will always have at least 1 value</param>
        /// <param name="oldValue">The previous value</param>
        /// <param name="value">The new value</param>
        /// <param name="isIncrementOperation">
        /// True if there are multiple handlers and the edit state command was in the editing state
        /// BEFORE the value changed. This may be true for sliders, numeric draggers, etc., but
        /// will always be false for things like text boxes or things that typically don't involve mouse input
        /// </param>
        protected abstract void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, TValue oldValue, TValue value, bool isIncrementOperation);

        /// <summary>
        /// The implementation of the reset value function. This should use the <see cref="SetValueAndHistory"/> to update the handlers' value and history state
        /// </summary>
        /// <param name="handlers">All of our handlers. Will always have at least 1 value</param>
        protected abstract void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers);

        /// <summary>
        /// Invoked to invoke a key frame at the given frame
        /// </summary>
        protected abstract void InsertKeyFrame(IAutomatableViewModel handler, long frame);

        public void ResetValue() {
            if (!this.HasHandlers) {
                return;
            }

            bool isEditStateForced = false;
            if (!this.EditStateChangedCommand.IsEditing) {
                this.EditStateChangedCommand.OnBeginEdit();
                isEditStateForced = true;
            }

            this.OnResetValue(this.castedHandlerList);
            if (isEditStateForced) {
                this.EditStateChangedCommand.OnFinishEdit();
            }
        }

        protected virtual void SetValueAndHistory(int index, TValue value) {
            this.Setter((IAutomatableViewModel) this.Handlers[index], value);
            ((HistoryValue) this.EditStateChangedCommand.HistoryAction).Value[index].Current = value;
        }

        protected virtual void SetValuesAndHistory(TValue value) {
            Transaction<TValue>[] history = ((HistoryValue) this.EditStateChangedCommand.HistoryAction).Value;
            for (int i = 0, count = this.Handlers.Count; i < count; i++) {
                this.Setter((IAutomatableViewModel) this.Handlers[i], value);
                history[i].Current = value;
            }
        }

        public void RequeryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, this.propertyData.getter, out TValue d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Value));
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.castedHandlerList = new CastingList<IAutomatableViewModel>(this.Handlers);
            if (this.IsSingleSelection) {
                this.SingleHandlerAutomationSequence.RefreshValue += this.RefreshValueForSingleHandler;
            }

            this.RequeryValueFromHandlers();
            this.RaisePropertyChanged(nameof(this.MyHandlers));
            this.RaisePropertyChanged(nameof(this.SingleHandler));
            this.RaisePropertyChanged(nameof(this.SingleHandlerAutomationSequence));
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            this.castedHandlerList = null;
            if (this.IsSingleSelection) {
                this.SingleHandlerAutomationSequence.RefreshValue -= this.RefreshValueForSingleHandler;
            }

            this.EditStateChangedCommand.OnReset();
            this.RaisePropertyChanged(nameof(this.MyHandlers));
            this.RaisePropertyChanged(nameof(this.SingleHandler));
            this.RaisePropertyChanged(nameof(this.SingleHandlerAutomationSequence));
        }

        #region Expression caching

        private static PropertyData GenerateData(PropertyPath path) {
            if (!GeneratedPropertyData.TryGetValue(path, out PropertyData data))
                GeneratedPropertyData[path] = data = PropertyData.Generate(path);
            return data;
        }

        private class PropertyPathEqualityComparer : IEqualityComparer<PropertyPath> {
            public bool Equals(PropertyPath a, PropertyPath b) {
                return a.type == b.type && a.name == b.name;
            }

            public int GetHashCode(PropertyPath obj) {
                return unchecked((obj.type.GetHashCode() * 397) ^ obj.name.GetHashCode());
            }
        }

        private readonly struct PropertyPath {
            public readonly Type type;
            public readonly string name;

            public PropertyPath(Type type, string name) {
                this.type = type;
                this.name = name;
            }
        }

        private struct PropertyData {
            public Func<object, TValue> getter;
            public Action<object, TValue> setter;

            public static PropertyData Generate(PropertyPath path) {
                PropertyData data = new PropertyData();
                data.getter = CreateGetter(path.type, path.name);
                data.setter = CreateSetter(path.type, path.name);
                return data;
            }

            private static Func<object, TValue> CreateGetter(Type type, string propertyName) {
                ParameterExpression target = Expression.Parameter(typeof(object), "target");
                Expression body = Expression.Property(Expression.Convert(target, type), propertyName);
                Expression<Func<object, TValue>> lambda = Expression.Lambda<Func<object, TValue>>(body, target);
                return lambda.Compile();
            }

            private static Action<object, TValue> CreateSetter(Type type, string propertyName) {
                ParameterExpression target = Expression.Parameter(typeof(object), "target");
                ParameterExpression value = Expression.Parameter(typeof(TValue), "value");
                Expression body = Expression.Assign(Expression.Property(Expression.Convert(target,type), propertyName), value);
                Expression<Action<object, TValue>> lambda = Expression.Lambda<Action<object, TValue>>(body, target, value);
                return lambda.Compile();
            }
        }

        #endregion

        protected class HistoryValue : BaseHistoryMultiHolderAction<IAutomatableViewModel> {
            public readonly Transaction<TValue>[] Value;
            public readonly AutomatablePropertyEditorViewModel<TValue> editor;

            public HistoryValue(AutomatablePropertyEditorViewModel<TValue> editor) : base(editor.MyHandlers) {
                Transaction<TValue>[] array = new Transaction<TValue>[this.Holders.Count];
                for (int i = 0; i < array.Length; i++) {
                    array[i] = Transactions.ForBoth(editor.Getter(this.Holders[i]));
                }

                this.Value = array;
                this.editor = editor;
            }

            protected override Task UndoAsync(IAutomatableViewModel holder, int i) {
                this.editor.Setter(holder, this.Value[i].Original);
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(IAutomatableViewModel holder, int i) {
                this.editor.Setter(holder, this.Value[i].Current);
                return Task.CompletedTask;
            }

            protected override Task OnUndoCompleteAsync(ErrorList errors) {
                this.editor.RequeryValueFromHandlers();
                return base.OnUndoCompleteAsync(errors);
            }

            protected override Task OnRedoCompleteAsync(ErrorList errors) {
                this.editor.RequeryValueFromHandlers();
                return base.OnRedoCompleteAsync(errors);
            }
        }
    }
}