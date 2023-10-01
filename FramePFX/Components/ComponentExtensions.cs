using System;

namespace FramePFX.Components
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Tries to get a component of type <see cref="T"/>
        /// </summary>
        /// <param name="storage">The component storage</param>
        /// <param name="component">The component found, or null, if the component does not exist</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>True if the component exists, otherwise false</returns>
        public static bool TryGetComponent<T>(this IComponentStorage storage, out T component)
        {
            if (storage.TryGetComponent(typeof(T), out object value))
            {
                component = (T) value;
                return true;
            }

            component = default;
            return false;
        }

        /// <summary>
        /// Gets a component of the given type
        /// </summary>
        /// <param name="storage">The component storage</param>
        /// <param name="type">The type of component</param>
        /// <returns>The component (non-null)</returns>
        /// <exception cref="Exception">No such component exists</exception>
        public static object GetComponent(this IComponentStorage storage, Type type)
        {
            if (storage.TryGetComponent(type, out object value))
                return value;
            throw new Exception($"No such component of type '{type}'");
        }

        /// <summary>
        /// Gets a component of the given type
        /// </summary>
        /// <param name="storage">The component storage</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>The component (non-null)</returns>
        /// <exception cref="Exception">No such component exists</exception>
        public static T GetComponent<T>(this IComponentStorage storage)
        {
            if (storage.TryGetComponent(typeof(T), out object value))
                return (T) value;
            throw new Exception($"No such component of type '{typeof(T)}'");
        }
    }
}