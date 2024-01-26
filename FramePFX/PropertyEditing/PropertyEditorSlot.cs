using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.PropertyEditing {
    public delegate void PropertyEditorSlotEventHandler(PropertyEditorSlot sender);

    /// <summary>
    /// The base class for a slot in a property editor. This is what stores the data used to
    /// modify one or more actual data properties in the UI. This is basically a single row in the editor
    /// </summary>
    public abstract class PropertyEditorSlot : BasePropertyEditorItem {
        private static readonly ReadOnlyCollection<object> EmptyList = new List<object>().AsReadOnly();

        private bool isSelected;

        public abstract bool IsSelectable { get; }

        public bool IsSelected {
            get => this.isSelected && this.IsSelectable;
            set {
                if (!this.IsSelectable)
                    throw new InvalidOperationException("Not selectable");
                if (this.isSelected == value)
                    return;
                this.isSelected = value;
                this.IsSelectedChanged?.Invoke(this);
            }
        }

        public IReadOnlyList<object> Handlers { get; private set; }

        /// <summary>
        /// Whether or not there are handlers currently using this property editor. Inverse of <see cref="IsEmpty"/>
        /// </summary>
        public bool HasHandlers => this.Handlers.Count > 0;

        /// <summary>
        /// Whether or not there are no handlers currently using this property editor. Inverse of <see cref="HasHandlers"/>
        /// </summary>
        public bool IsEmpty => this.Handlers.Count < 1;

        /// <summary>
        /// Whether or not this editor has more than 1 active handlers
        /// </summary>
        public bool IsMultiSelection => this.Handlers.Count > 1;

        /// <summary>
        /// Whether or not this editor has only 1 active handler
        /// </summary>
        public bool IsSingleSelection => this.Handlers.Count == 1;

        /// <summary>
        /// A mode which helps determine if this editor can be used based on the input handler list
        /// </summary>
        public virtual ApplicabilityMode ApplicabilityMode => ApplicabilityMode.All;

        public event PropertyEditorSlotEventHandler IsSelectedChanged;

        protected PropertyEditorSlot(Type applicableType) : base(applicableType) {
            this.Handlers = EmptyList;
        }

        /// <summary>
        /// Clears this editor's active handlers. This will not clear the underlying list, and instead, assigns it to an empty list
        /// <para>
        /// If there are no handlers currently loaded, then this function does nothing
        /// </para>
        /// </summary>
        public void ClearHandlers() {
            if (this.Handlers.Count < 1) {
                return;
            }

            this.OnClearingHandlers();
            this.Handlers = EmptyList;
            this.IsCurrentlyApplicable = false;
        }

        /// <summary>
        /// Called just before the handlers are cleared. When this is cleared, there is guaranteed to be 1 or more loaded handlers
        /// </summary>
        protected virtual void OnClearingHandlers() {

        }

        /// <summary>
        /// Clears the handler list and then sets up the new handlers for the given list. If the
        /// <see cref="BasePropertyObjectViewModel.HandlerCountMode"/> for this group is unacceptable,
        /// then nothing else happens. If all of the input objects are not applicable, then nothing happens. Otherwise,
        /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the handlers are loaded
        /// </summary>
        /// <param name="input">Input list of objects</param>
        public void SetHandlers(IReadOnlyList<object> targets) {
            this.ClearHandlers();
            if (!this.IsHandlerCountAcceptable(targets.Count)) {
                return;
            }

            if (!GetApplicable(this, targets, out IReadOnlyList<object> list)) {
                return;
            }

            this.IsCurrentlyApplicable = true;
            this.Handlers = list;
            this.OnHandlersLoaded();
        }

        /// <summary>
        /// Called just after all handlers are fulled loaded. When this is cleared, there is guaranteed to be 1 or more loaded handlers
        /// </summary>
        protected virtual void OnHandlersLoaded() {

        }

        /// <summary>
        /// Attempts to extract a value which is equal across all objects, using the given getter function
        /// </summary>
        /// <param name="objects">Input objects</param>
        /// <param name="getter">Getter function</param>
        /// <param name="equal">
        /// The value that is equal across all objects (will be set to <see cref="objects"/>[0]'s value)
        /// </param>
        /// <typeparam name="T">Type of object to get</typeparam>
        /// <returns>True if there is 1 object, or more than 1 and they have the same value, otherwise false</returns>
        public static bool GetEqualValue<T>(IReadOnlyList<object> objects, Func<object, T> getter, out T equal) {
            int count;
            if (objects == null || (count = objects.Count) < 1) {
                equal = default;
                return false;
            }

            equal = getter(objects[0]);
            if (count > 1) {
                EqualityComparer<T> comparator = EqualityComparer<T>.Default;
                for (int i = 1; i < count; i++) {
                    if (!comparator.Equals(getter(objects[i]), equal)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool GetApplicable(PropertyEditorSlot slot, IReadOnlyList<object> input, out IReadOnlyList<object> output) {
            switch (slot.ApplicabilityMode) {
                case ApplicabilityMode.All: {
                    // return sources.All(x => editor.IsApplicable(x));
                    for (int i = 0, c = input.Count; i < c; i++) {
                        if (!slot.IsApplicable(input[i])) {
                            output = null;
                            return false;
                        }
                    }

                    output = input;
                    return true;
                }
                case ApplicabilityMode.Any: {
                    for (int i = 0, c = input.Count; i < c; i++) {
                        if (slot.IsApplicable(input[i])) {
                            List<object> list = new List<object>();
                            do {
                                list.Add(input[i++]);
                            } while (i < c);

                            output = list;
                            return true;
                        }
                    }

                    output = null;
                    return false;
                }
                default: throw new Exception("Invalid " + nameof(ApplicabilityMode));
            }
        }
    }
}