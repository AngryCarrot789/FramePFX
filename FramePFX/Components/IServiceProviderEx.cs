using System;

namespace FramePFX.Components {
    public interface IServiceProviderEx : IServiceProvider {
        /// <summary>
        /// Returns whether this object contains a service of the given type
        /// </summary>
        /// <param name="serviceType">The type of service</param>
        /// <returns>True if the service exists, otherwise false</returns>
        bool HasService(Type serviceType);

        /// <summary>
        /// Tries to get a service of the given type
        /// </summary>
        /// <param name="serviceType">The type of service</param>
        /// <param name="servicent">The service found, or null, if the service does not exist</param>
        /// <returns>True if the component exists, otherwise false</returns>
        bool TryGetService(Type serviceType, out object service);
    }
}