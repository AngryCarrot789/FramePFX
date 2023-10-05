namespace FramePFX.WPF.PropertyEditing {
    /// <summary>
    /// An interface shared for <see cref="PropertyEditorItem"/> and <see cref="PropertyEditorItemsControl"/>
    /// </summary>
    public interface IPropertyEditorControl {
        /// <summary>
        /// Gets the root property editor that this item is stored in
        /// </summary>
        PropertyEditor PropertyEditor { get; }
    }
}