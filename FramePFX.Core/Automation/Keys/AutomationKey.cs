using System;
using System.Reflection;

namespace FramePFX.Core.Automation {
    /// <summary>
    /// A property that can be automated/animated
    /// </summary>
    public class AutomationKey {
        public Type OwnerType { get; }
        public string Property { get; }

        public event AutomationEvent OnPropertyChanged;

        protected AutomationKey(Type ownerType, string property) {
            this.OwnerType = ownerType;
            this.Property = property;
        }

        // public static AutomationProperty Register<T>(string property) {
        //     if (string.IsNullOrEmpty(property))
        //         throw new ArgumentNullException(nameof(property));
        //     Type type = typeof(T);
        //     var propertyInfo = type.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        //     return new AutomationProperty(type, property, null);
        // }

        public override string ToString() {
            return $"{this.GetType()}({this.OwnerType} -> {this.Property})";
        }
    }
}