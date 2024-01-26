namespace FramePFX.PropertyEditing {
    public delegate void PropertyEditorObjectEventHandler(BasePropertyEditorObject sender);

    /// <summary>
    /// A base class for all items in a property editor hierarchy, such as slots, groups, separators, etc.
    /// </summary>
    public abstract class BasePropertyEditorObject {
        /// <summary>
        /// Gets this property editor item's parent group. May be null for the root <see cref="BasePropertyEditorGroup"/>
        /// </summary>
        public BasePropertyEditorGroup Parent { get; private set; }

        protected BasePropertyEditorObject() {
        }

        public static void OnAddedToGroup(BasePropertyEditorObject propObj, BasePropertyEditorGroup parent) {
            propObj.Parent = parent;
        }

        public static void OnRemovedFromGroup(BasePropertyEditorObject propObj, BasePropertyEditorGroup parent) {
            propObj.Parent = null;
        }
    }
}