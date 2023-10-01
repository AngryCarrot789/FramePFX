namespace FramePFX.PropertyEditing
{
    /// <summary>
    /// An interface for property editors and groups, but not separators
    /// </summary>
    public interface IPropertyEditorItem : IPropertyEditorObject
    {
        /// <summary>
        /// Gets the property editor registry that owns this editor item
        /// </summary>
        PropertyEditorRegistry PropertyEditor { get; }
    }
}