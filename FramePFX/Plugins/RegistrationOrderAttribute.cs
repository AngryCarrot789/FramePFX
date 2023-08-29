using System;

namespace FramePFX.Plugins {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegistrationOrderAttribute : Attribute {
        /// <summary>
        /// The order. 0 is the default, meaning unordered
        /// </summary>
        public int Order { get; set; }

        public RegistrationOrderAttribute(int order = 0) {
            this.Order = order;
        }
    }
}