namespace FramePFX.WPF.PropertyEditing
{
    /// <summary>
    /// An interface for property editor controls that are selectable
    /// </summary>
    public interface ISelectablePropertyControl : IPropertyEditorControl
    {
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets whether this control is actually selectable
        /// </summary>
        bool IsSelectable { get; }

        /// <summary>
        /// Sets this control as the selected item
        /// </summary>
        /// <param name="isPrimarySelection">
        /// True to clear all other selections and make this the primary
        /// selection, otherwise false to add this to the selected collection
        /// </param>
        /// <returns></returns>
        bool SetSelected(bool selected, bool isPrimarySelection);
    }
}