using System;
using System.Collections.Generic;

namespace FramePFX {
    public class ServiceManager : IServiceManager {
        private readonly Dictionary<Type, object> services;

        public ServiceManager() {
            this.services = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Returns whether this IoC manager contains a given service
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns></returns>
        public bool HasService<T>() {
            return this.services.ContainsKey(typeof(T));
        }

        public bool HasService(Type serviceType) {
            return this.services.ContainsKey(serviceType);
        }

        /// <summary>
        /// Gets the service instance of the given generic type
        /// </summary>
        /// <typeparam name="T">The service type (typically the base type)</typeparam>
        /// <returns>The instance of the service</returns>
        /// <exception cref="ServiceNotFoundException">Thrown if there isn't a ViewModel of that type</exception>
        /// <exception cref="InvalidCastException">Thrown if the target service type doesn't match the actual service type</exception>
        public T GetService<T>() {
            if (this.services.TryGetValue(typeof(T), out object service)) {
                if (service is T t) {
                    return t;
                }

                throw new InvalidCastException($"The target service type '{typeof(T)}' is incompatible with actual service type '{(service == null ? "NULL" : service.GetType().Name)}'");
            }

            throw new Exception($"No service registered with type: {typeof(T)}");
        }

        /// <summary>
        /// Gets the service instance of the given generic type
        /// </summary>
        /// <returns>The instance of the service</returns>
        /// <exception cref="ServiceNotFoundException">Thrown if there isn't a ViewModel of that type</exception>
        /// <exception cref="InvalidCastException">Thrown if the target service type doesn't match the actual service type</exception>
        public object GetService(Type type) {
            if (this.services.TryGetValue(type, out object service))
                return service;
            throw new Exception($"No service registered with type: {type}");
        }

        public bool TryGetService(Type serviceType, out object service) {
            return this.services.TryGetValue(serviceType, out service);
        }

        public bool TryGetService<T>(out T service) {
            if (this.services.TryGetValue(typeof(T), out object objService)) {
                service = (T) objService;
                return true;
            }
            else {
                service = default;
                return false;
            }
        }

        /// <summary>
        /// Registers (or replaces) the given service of the given generic type
        /// </summary>
        /// <typeparam name="T">The service type (typically an interface, for an API service)</typeparam>
        /// <param name="service"></param>
        public void Register<T>(T service) {
            this.services[typeof(T)] = service;
        }

        /// <summary>
        /// Registers (or replaces) the given service of the given generic type
        /// </summary>
        /// <param name="service"></param>
        public void Register(Type type, object service) {
            if (!type.IsInstanceOfType(service))
                throw new InvalidCastException($"The target service type '{type}' is incompatible with actual service type '{(service == null ? "NULL" : service.GetType().Name)}'");
            this.services[type] = service;
        }
    }
}