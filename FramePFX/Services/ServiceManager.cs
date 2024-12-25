// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FramePFX.Utils;

namespace FramePFX.Services;

public sealed class ServiceManager
{
    private readonly Dictionary<Type, ServiceEntry> services;

    /// <summary>
    /// Gets the owner of this service container
    /// </summary>
    public object Owner { get; }

    public ServiceManager(object owner)
    {
        this.services = new Dictionary<Type, ServiceEntry>();
        this.Owner = owner;
    }

    /// <summary>
    /// Non-generic version of <see cref="HasService{T}"/>
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="createdOnly">
    /// True to check only if a service instance exists. False to also
    /// check if the service is lazily creatable but not yet created
    /// </param>
    /// <returns>See summary</returns>
    public bool HasService(Type serviceType, bool createdOnly = false)
    {
        Validate.NotNull(serviceType);
        return createdOnly ? this.TryGetService(serviceType, out _, false) : this.services.ContainsKey(serviceType);
    }

    /// <summary>
    /// Non-generic version of <see cref="GetService{T}"/>
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <returns></returns>
    public object GetService(Type serviceType)
    {
        Validate.NotNull(serviceType);
        if (!this.TryGetService(serviceType, out object? service))
            throw new Exception($"No service registered with type: {serviceType}");

        return service;
    }

    /// <summary>
    /// Tries to get a service of the given type
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="service"></param>
    /// <param name="canCreate"></param>
    /// <returns></returns>
    public bool TryGetService(Type serviceType, [NotNullWhen(true)] out object? service, bool canCreate = true)
    {
        Validate.NotNull(serviceType);
        if (!this.services.TryGetValue(serviceType, out ServiceEntry entry))
        {
            service = null;
            return false;
        }

        if (entry.isLazyEntry)
        {
            service = ((Func<object, object>) entry.value)(this.Owner);
            Debug.Assert(serviceType.IsInstanceOfType(service), "New service instance is incompatible with target type");
            this.services[serviceType] = new ServiceEntry(false, service);
        }
        else
        {
            service = entry.value;
        }

        return true;
    }

    /// <summary>
    /// Checks if a service of the given type is fully initialised
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>See summary</returns>
    public bool HasService<T>() where T : class => this.HasService(typeof(T));

    /// <summary>
    /// Gets a constant service or lazily initialises a service and returns it.
    /// Throws <see cref="InvalidOperationException"/> if no such service exists
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service</returns>
    public T GetService<T>() where T : class => (T) this.GetService(typeof(T));

    /// <summary>
    /// Tries to get a service of the given type. Returns false if the service does not exists and create is false
    /// </summary>
    /// <param name="service">The found or newly created service</param>
    /// <param name="canCreate">True to create the service if it is lazy initialisable</param>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>True if the service was found or created, false if not</returns>
    public bool TryGetService<T>([NotNullWhen(true)] out T? service, bool canCreate = true) where T : class
    {
        if (this.TryGetService(typeof(T), out object? serviceObject, canCreate))
        {
            service = (T) serviceObject;
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Registers a constant service
    /// </summary>
    /// <param name="service">The service</param>
    /// <typeparam name="T">The service type</typeparam>
    public void RegisterConstant<T>(T service) where T : class
    {
        Validate.NotNull(service);

        this.services[typeof(T)] = new ServiceEntry(false, service);
    }

    /// <summary>
    /// Registers a constant service
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="service">The service</param>
    /// <exception cref="ArgumentNullException">Service or service type are null</exception>
    /// <exception cref="ArgumentException">Service is not assignable to the service type</exception>
    public void RegisterConstant(Type serviceType, object service)
    {
        Validate.NotNull(serviceType);
        Validate.NotNull(service);
        if (!serviceType.IsInstanceOfType(service))
            throw new ArgumentException($"The target service type '{serviceType}' is incompatible with actual service type '{(service.GetType().Name)}'");

        this.services[serviceType] = new ServiceEntry(false, service);
    }

    /// <summary>
    /// Registers a lazily initialised service using a factory method
    /// </summary>
    /// <param name="factory"></param>
    /// <typeparam name="T"></typeparam>
    public void RegisterLazy<T>(Func<object, T> factory) where T : class
    {
        Validate.NotNull(factory);
        this.services[typeof(T)] = new ServiceEntry(true, new Func<object, object>(o => factory(o)!));
    }

    private readonly struct ServiceEntry
    {
        public readonly bool isLazyEntry;
        public readonly object value;

        public ServiceEntry(bool isLazyEntry, object value)
        {
            this.isLazyEntry = isLazyEntry;
            this.value = value;
        }
    }
}