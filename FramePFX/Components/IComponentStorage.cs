using System;

namespace FramePFX.Components
{
    public interface IComponentStorage
    {
        /// <summary>
        /// Tries to get a component of the given type
        /// </summary>
        /// <param name="type">The type of component</param>
        /// <param name="component">The component found, or null, if the component does not exist</param>
        /// <returns>True if the component exists, otherwise false</returns>
        bool TryGetComponent(Type type, out object component);
    }
}