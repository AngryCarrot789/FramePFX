using System;
using System.Collections.Generic;

namespace FramePFX.Core {
    public class SimpleIoC {
        private readonly Dictionary<Type, object> services;

        public SimpleIoC() {
            this.services = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the service instance of the given generic type
        /// </summary>
        /// <typeparam name="T">The service type (typically the base type)</typeparam>
        /// <returns>The instance of the service</returns>
        /// <exception cref="ServiceNotFoundException">Thrown if there isn't a ViewModel of that type</exception>
        /// <exception cref="InvalidCastException">Thrown if the target service type doesn't match the actual service type</exception>
        public T Provide<T>() {
            if (this.services.TryGetValue(typeof(T), out object service)) {
                if (service is T t) {
                    return t;
                }

                throw new InvalidCastException($"The target service type '{typeof(T)}' is incompatible with actual service type '{(service == null ? "NULL" : service.GetType().Name)}'");
            }

            #if DEBUG
            return default;
            #else
            throw new Exception($"No service registered with type: {typeof(T)}");
            #endif
        }

        /// <summary>
        /// Gets the service instance of the given generic type
        /// </summary>
        /// <returns>The instance of the service</returns>
        /// <exception cref="ServiceNotFoundException">Thrown if there isn't a ViewModel of that type</exception>
        /// <exception cref="InvalidCastException">Thrown if the target service type doesn't match the actual service type</exception>
        public object Provide(Type type) {
            if (this.services.TryGetValue(type, out object service)) {
                if (type.IsInstanceOfType(service)) {
                    return service;
                }

                throw new InvalidCastException($"The target service type '{type}' is incompatible with actual service type '{(service == null ? "NULL" : service.GetType().Name)}'");
            }

            #if DEBUG
            return default;
            #else
            throw new Exception($"No service registered with type: {type}");
            #endif
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
            this.services[type] = service;
        }

        /// <summary>
        /// Returns whether this IoC manager contains a given service
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns></returns>
        public bool IsRegistered<T>() {
            return this.services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Returns whether this IoC manager contains a given service
        /// </summary>
        /// <returns></returns>
        public bool IsRegistered(Type type) {
            return this.services.ContainsKey(type);
        }
    }
}
