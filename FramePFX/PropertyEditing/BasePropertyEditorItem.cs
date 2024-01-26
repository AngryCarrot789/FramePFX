using System;

namespace FramePFX.PropertyEditing {
    public delegate void BasePropertyEditorItemEventHandler(BasePropertyEditorItem sender);

    /// <summary>
    /// A base class for natural items in a property editor, such as a slot or group
    /// </summary>
    public abstract class BasePropertyEditorItem : BasePropertyEditorObject {
        private bool isCurrentlyApplicable;

        /// <summary>
        /// Gets or sets if this item is applicable, meaning it is visible and interactable
        /// </summary>
        public bool IsCurrentlyApplicable {
            get => this.isCurrentlyApplicable;
            protected set {
                if (this.isCurrentlyApplicable == value)
                    return;
                this.isCurrentlyApplicable = value;
                this.IsCurrentlyApplicableChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// The lowest applicable type. This will be null for the root group container. A valid group will contain a non-null applicable type
        /// </summary>
        public Type ApplicableType { get; }

        /// <summary>
        /// The handler count mode for this object, which determines if this object is applicable for
        /// a specific number of handlers
        /// </summary>
        public virtual HandlerCountMode HandlerCountMode => HandlerCountMode.Any;

        public event BasePropertyEditorItemEventHandler IsCurrentlyApplicableChanged;

        protected BasePropertyEditorItem(Type applicableType) {
            this.ApplicableType = applicableType ?? throw new ArgumentNullException(nameof(applicableType));
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