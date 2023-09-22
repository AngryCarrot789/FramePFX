using System;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// The base class for property groups and editors
    /// </summary>
    public class BasePropertyObjectViewModel : BaseViewModel, IPropertyEditorItem {
        private PropertyEditorRegistry propertyEditor;
        private bool isCurrentlyApplicable;

        /// <summary>
        /// Whether or not this item should be visible to the end user or not.
        /// Not taking this into account and showing it anyway may result in crashing
        /// </summary>
        public bool IsCurrentlyApplicable {
            get => this.isCurrentlyApplicable;
            set {
                if (this.isCurrentlyApplicable == value)
                    return;
                this.RaisePropertyChanged(ref this.isCurrentlyApplicable, value);
            }
        }

        public PropertyEditorRegistry PropertyEditor {
            get => this.propertyEditor;
            private set => this.RaisePropertyChanged(ref this.propertyEditor, value);
        }

        /// <summary>
        /// How deep the current property is within its parent hierarchy
        /// </summary>
        public int HierarchyDepth { get; private set; } = -1;

        /// <summary>
        /// The lowest applicable type. This will be null for the root group container. A valid group will contain a non-null applicable type
        /// </summary>
        public Type ApplicableType { get; }

        /// <summary>
        /// The handler count mode for this object, which determines if this object is applicable for
        /// a specific number of handlers
        /// </summary>
        public virtual HandlerCountMode HandlerCountMode => HandlerCountMode.Any;

        public BasePropertyGroupViewModel Parent { get; internal set; }

        public BasePropertyObjectViewModel(Type applicableType) {
            this.ApplicableType = applicableType;
        }

        public void SetPropertyEditor(PropertyEditorRegistry registry) {
            this.PropertyEditor = registry;
            if (this is BasePropertyGroupViewModel) {
                foreach (IPropertyEditorObject obj in ((BasePropertyGroupViewModel) this).PropertyObjects) {
                    if (obj is BasePropertyObjectViewModel) {
                        ((BasePropertyObjectViewModel) obj).PropertyEditor = registry;
                    }
                }
            }
        }

        /// <summary>
        /// A helper function that determines if the given handler is applicable to this object (see <see cref="Type.IsInstanceOfType"/>)
        /// </summary>
        /// <param name="value">The handler</param>
        /// <returns>Handler is acceptable for this group</returns>
        public bool IsApplicable(object value) => this.ApplicableType.IsInstanceOfType(value);

        /// <summary>
        /// A helper function that determines if this object can accept a specific number of handler objects.
        /// <para>
        /// This always returns false for values 0 and below
        /// </para>
        /// </summary>
        /// <param name="count">The number of handlers that are available</param>
        /// <returns>This property is applicable for the given number of handlers</returns>
        public bool IsHandlerCountAcceptable(int count) {
            return IsHandlerCountAcceptable(this.HandlerCountMode, count);
        }

        public virtual void RecalculateHierarchyDepth() {
            BasePropertyGroupViewModel parent = this.Parent;
            this.HierarchyDepth = parent == null ? -1 : (parent.HierarchyDepth + 1);
        }

        protected int GetHierarchyDepth() {
            int count = -1;
            for (BasePropertyGroupViewModel parent = this.Parent; parent != null; parent = parent.Parent)
                count++;
            return count;
        }

        public static bool IsHandlerCountAcceptable(HandlerCountMode mode, int count) {
            switch (mode) {
                case HandlerCountMode.Any: return count > 0;
                case HandlerCountMode.Single: return count == 1;
                case HandlerCountMode.Multi: return count > 1;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}