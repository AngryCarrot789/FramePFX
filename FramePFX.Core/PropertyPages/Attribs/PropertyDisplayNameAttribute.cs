using System;

namespace FramePFX.Core.PropertyPages.Attribs {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyDisplayNameAttribute : Attribute {
        public string Name { get; }

        public PropertyDisplayNameAttribute(string name) {
            this.Name = name;
        }
    }
}