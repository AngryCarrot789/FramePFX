namespace FramePFX.Utils {
    /// <summary>
    /// A property that can inherit a value from a parent instance
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    public class InheritedProperty<T> {
        private readonly InheritedProperty<T> parent;
        private T internalValue;

        /// <summary>
        /// Gets the current value (if <see cref="HasLocalValue"/> is true) or the parent's value, or sets the current value and marks <see cref="HasLocalValue"/> as true
        /// </summary>
        public T Value {
            get => this.HasLocalValue ? this.internalValue : (this.parent != null ? this.parent.Value : default);
            set {
                this.HasLocalValue = true;
                this.internalValue = value;
            }
        }

        /// <summary>
        /// Whether or not this current instance has a value set or not
        /// </summary>
        public bool HasLocalValue { get; private set; }

        public InheritedProperty() {
        }

        public InheritedProperty(T value) {
            this.Value = value;
        }

        public InheritedProperty(InheritedProperty<T> parent) {
            this.parent = parent;
        }

        public InheritedProperty(InheritedProperty<T> parent, T value) {
            this.parent = parent;
            this.Value = value;
        }

        public bool GetValue(out T value) {
            if (this.HasLocalValue) {
                value = this.internalValue;
                return true;
            }

            if (this.parent != null && this.parent.GetValue(out value)) {
                return true;
            }

            value = default;
            return false;
        }

        public void Clear() {
            this.HasLocalValue = false;
            this.internalValue = default;
        }
    }
}