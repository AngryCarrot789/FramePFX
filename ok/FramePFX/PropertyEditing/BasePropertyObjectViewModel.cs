using System;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// The base class for property groups and editors
    /// </summary>
    public class BasePropertyObjectViewModel : BaseViewModel {
        private bool isCurrentlyApplicable;

        /// <summary>
        /// Whether or not this item should be visible to the end user or not.
        /// Not taking this into account and showing it anyway may result a crashing
        /// </summary>
        public bool IsCurrentlyApplicable {
            get => this.isCurrentlyApplicable;
            set => this.RaisePropertyChanged(ref this.isCurrentlyApplicable, value);
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

        public BasePropertyObjectViewModel(Type applicableType) {
            this.ApplicableType = applicableType;
        }

        /// <summary>
        /// A helper function that determines if the given handler is applicable to this object (see <see cref="Type.IsInstanceOfType"/>)
        /// </summary>
        /// <param name="value">The handler</param>
        /// <returns>Handler is acceptable for this group</returns>
        public bool IsApplicable(object value) => this.ApplicableType.IsInstanceOfType(value);

        /// <summary>
        /// A helper function that determines if this object can accept a specific number of handler objects
        /// </summary>
        /// <param name="count">The number of handlers that are available</param>
        /// <returns>This property is applicable for the given number of handlers</returns>
        public bool IsHandlerCountAcceptable(int count) {
            switch (this.HandlerCountMode) {
                case HandlerCountMode.Any: return true;
                case HandlerCountMode.Single: return count == 1;
                case HandlerCountMode.Multi: return count > 1;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}