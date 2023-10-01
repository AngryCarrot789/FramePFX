namespace FramePFX.PropertyEditing
{
    /// <summary>
    /// A mode for the state of a <see cref="DynamicPropertyGroupViewModel"/>
    /// </summary>
    public enum DynamicMode
    {
        /// <summary>
        /// This group is currently empty (the default state and also set after <see cref="DynamicPropertyGroupViewModel.ClearHierarchyState"/>)
        /// </summary>
        Inactive,

        /// <summary>
        /// Each handler has their own unique group; none of them are shared
        /// </summary>
        SingleHandlerPerSubGroup,

        /// <summary>
        /// One or more handlers share a sub group; this is the opposite of <see cref="SingleHandlerPerSubGroup"/>
        /// </summary>
        MultipleHandlersPerSubGroup,

        /// <summary>
        /// This is the state set when calling <see cref="DynamicPropertyGroupViewModel.SetupHierarchyStateExtended"/> and
        /// there are more than 1 input lists
        /// </summary>
        Extended
    }
}