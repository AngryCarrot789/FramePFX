namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An interface for property editors and groups, but not separators
    /// </summary>
    public interface IPropertyEditorItem : IPropertyEditorObject {
        /// <summary>
        /// Gets or sets if this item is selected. This modifies our property editor's selected items automatically
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets the property editor registry that owns this editor item
        /// </summary>
        PropertyEditorRegistry PropertyEditor { get; }
    }
}