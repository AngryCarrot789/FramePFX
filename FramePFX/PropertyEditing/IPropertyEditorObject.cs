namespace FramePFX.PropertyEditing
{
    /// <summary>
    /// An interface for an object that can be placed in a property editor hierarchy. This includes groups, editors and separators
    /// </summary>
    public interface IPropertyEditorObject
    {
        /// <summary>
        /// Gets the group that this object is placed in
        /// </summary>
        BasePropertyGroupViewModel Parent { get; }
    }
}