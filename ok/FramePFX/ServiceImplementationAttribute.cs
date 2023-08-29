using System;

namespace FramePFX {
    /// <summary>
    /// An attribute applied to the implementation of a service
    /// <para>
    /// I only use this when i'm too lazy to add the type to the App.cs class
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ServiceImplementationAttribute : Attribute {
        /// <summary>
        /// The target service type (typically an interface or view model type)
        /// </summary>
        public Type Type { get; set; }

        public ServiceImplementationAttribute(Type type) {
            this.Type = type;
        }
    }
}