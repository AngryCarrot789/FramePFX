using System;

namespace FramePFX.Core {
    /// <summary>
    /// An attribute applied to the implementation of a service
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ServiceAttribute : Attribute {
        /// <summary>
        /// The target service type (typically an interface or view model type)
        /// </summary>
        public Type Type { get; set; }

        public ServiceAttribute(Type type = null) {
            this.Type = type;
        }
    }
}