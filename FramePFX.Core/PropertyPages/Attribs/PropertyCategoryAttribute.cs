using System;
using System.ComponentModel;

namespace FramePFX.Core.PropertyPages.Attribs {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyCategoryAttribute : Attribute {
        public string Name { get; }

        public PropertyCategoryAttribute(string name) {
            this.Name = name;
        }
    }
}