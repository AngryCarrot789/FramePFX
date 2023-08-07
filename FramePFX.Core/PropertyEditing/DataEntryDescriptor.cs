using System;

namespace FramePFX.Core.PropertyEditing
{
    /// <summary>
    /// A class which describes a single property
    /// </summary>
    public class DataEntryDescriptor
    {
        /// <summary>
        /// The property name. This is what will be passed to <see cref="IPropertyEditReceiver.OnExternalPropertyModified"/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the property, e.g. <see cref="double"/>
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// <para>
        /// Whether or not this property can be modified when there are multiple selections
        /// </para>
        /// <para>
        /// By default, this is false. When a single-selection is present and any value is modified, the old
        /// value is typically replaced with the new value
        /// </para>
        /// <para>
        /// However, when there are multiple selections, things like numeric properties may be incremented and decremented. Other
        /// things like strings and booleans are just replaced as if there was a single selection
        /// </para>
        /// </summary>
        public bool CanSupportMultiSelect { get; }

        public DataEntryDescriptor(string name, Type type, bool canSupportMultiSelect = false)
        {
            this.Name = name;
            this.Type = type;
            this.CanSupportMultiSelect = canSupportMultiSelect;
        }
    }
}