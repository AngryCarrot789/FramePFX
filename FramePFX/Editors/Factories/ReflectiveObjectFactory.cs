using System;

namespace FramePFX.Editors.Factories {
    /// <summary>
    /// An object factory that allows reflective creation of objects that use a default constructor
    /// </summary>
    public class ReflectiveObjectFactory<T> : ObjectFactory where T : class {
        public ReflectiveObjectFactory() {

        }

        protected override bool IsTypeValid(Type type) {
            return typeof(T).IsAssignableFrom(type);
        }

        protected T NewInstance(string id) {
            Type type = this.GetType(id);
            return (T) Activator.CreateInstance(type);
        }
    }
}