using System;
using System.Collections.Generic;

namespace FramePFX.Core.PropertyEditing
{
    /// <summary>
    /// A base class for a register, used for setting up a property editor hierarchy based on a collection of data sources
    /// </summary>
    public class PropertyEditorRegistry
    {
        /// <summary>
        /// The root group container for this registry. This group by itself is invalid
        /// and should never be used apart from storing child objects
        /// </summary>
        public PropertyGroupViewModel Root { get; }

        public PropertyEditorRegistry()
        {
            this.Root = new PropertyGroupViewModel(null, "<root>");
        }

        /// <summary>
        /// Convenience function for creating a sub-group in our root group container
        /// </summary>
        protected PropertyGroupViewModel CreateRootGroup(Type type, string name, bool isExpandedByDefault = true)
        {
            return this.Root.CreateSubGroup(type, name, isExpandedByDefault);
        }

        /// <summary>
        /// Sets up this registry for the given collection of data sources
        /// </summary>
        /// <param name="dataSources">A input list of data sources</param>
        public void SetupObjects(IReadOnlyList<object> dataSources)
        {
            this.Root.ClearHandlersRecursive();
            if (dataSources.Count > 0)
            {
                this.Root.SetupHierarchyState(dataSources);
            }
        }
    }
}