using System;

namespace FramePFX.Components {
    public static class ServiceProviderExtension {
        /// <summary>
        /// Tries to get a component of type <see cref="T"/>
        /// </summary>
        /// <param name="storage">The component storage</param>
        /// <param name="component">The component found, or null, if the component does not exist</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>True if the component exists, otherwise false</returns>
        public static bool TryGetService<T>(this IServiceProviderEx storage, out T component) {
            if (storage.TryGetService(typeof(T), out object value)) {
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
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>The component (non-null)</returns>
        /// <exception cref="Exception">No such component exists</exception>
        public static T GetService<T>(this IServiceProviderEx storage) {
            if (storage.TryGetService(typeof(T), out object value))
                return (T) value;
            throw new Exception($"No such component of type '{typeof(T)}'");
        }

        public static T GetServiceOrDefault<T>(this IServiceProviderEx storage) {
            return storage.TryGetService(out T service) ? service : default;
        }
    }
}