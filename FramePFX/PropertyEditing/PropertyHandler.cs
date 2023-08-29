namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A class for storing data about a specific handler
    /// </summary>
    public class PropertyHandler {
        /// <summary>
        /// The handler that can be modified by the property
        /// </summary>
        public object Target { get; }

        public PropertyHandler(object target) {
            this.Target = target;
        }
    }
}