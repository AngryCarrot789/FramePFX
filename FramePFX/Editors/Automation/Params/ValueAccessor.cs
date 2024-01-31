using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FramePFX.Editors.Automation.Params {
    /// <summary>
    /// A class used by parameters to get and set the effective value of a specific parameter for an object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueAccessor<T> {
        /// <summary>
        /// Returns true when the boxed getter and setters are preferred,
        /// e.g. this instance is reflection-based which always used boxed values
        /// </summary>
        public bool IsObjectPreferred { get; protected set; }

        /// <summary>
        /// Gets the generic value
        /// </summary>
        public abstract T GetValue(IAutomatable instance);

        /// <summary>
        /// Gets the boxed value
        /// </summary>
        public abstract object GetObjectValue(IAutomatable instance);

        /// <summary>
        /// Sets the generic value
        /// </summary>
        public abstract void SetValue(IAutomatable instance, T value);

        /// <summary>
        /// Sets the object value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        public abstract void SetObjectValue(IAutomatable instance, object value);
    }

    public static class ValueAccessors {
        private static ParameterExpression InstanceParameter;

        /// <summary>
        /// Creates a reflection-based value accessor. This is recommended for
        /// low memory usage and also low general usage of the accessor itself
        /// </summary>
        /// <param name="owner">The class that contains the property or field</param>
        /// <param name="propertyOrField">The name of the property or field</param>
        /// <typeparam name="T">The value type</typeparam>
        /// <returns>A value accessor</returns>
        /// <exception cref="Exception">No property or field found with the specified name</exception>
        public static ValueAccessor<T> Reflective<T>(Type owner, string propertyOrField) {
            MemberInfo info = GetPropertyOrField(owner, propertyOrField);
            if (info is PropertyInfo)
                return new ReflectivePropertyValueAccessor<T>((PropertyInfo) info);
            return new ReflectiveFieldValueAccessor<T>((FieldInfo) info);
        }

        /// <summary>
        /// Creates a new expression-based value accessor, which is faster than reflection but has more memory overhead
        /// </summary>
        /// <param name="owner">The class that contains the property or field</param>
        /// <param name="propertyOrField">The name of the property or field</param>
        /// <typeparam name="T">The value type</typeparam>
        /// <returns>A value accessor</returns>
        public static ValueAccessor<T> LinqExpression<T>(Type owner, string propertyOrField) {
            MemberInfo targetMember = GetPropertyOrField(owner, propertyOrField);
            Type lowestOwnerType = targetMember.DeclaringType;
            if (lowestOwnerType == null)
                throw new Exception($"The target member named '{propertyOrField}' does not have a declaring type somehow");

            ParameterExpression paramInstance = InstanceParameter ?? (InstanceParameter = Expression.Parameter(typeof(IAutomatable), "instance"));
            UnaryExpression castToOwner = Expression.Convert(paramInstance, lowestOwnerType);
            MemberExpression dataMember = Expression.MakeMemberAccess(castToOwner, targetMember);

            AutoGetter<T> getter = Expression.Lambda<AutoGetter<T>>(dataMember, paramInstance).Compile();

            ParameterExpression paramValue = Expression.Parameter(typeof(T), "value");
            BinaryExpression assignValue = Expression.Assign(dataMember, paramValue);
            AutoSetter<T> setter = Expression.Lambda<AutoSetter<T>>(assignValue, paramInstance, paramValue).Compile();

            return new DelegateValueAccessor<T>(getter, setter);
        }

        /// <summary>
        /// Creates a value accessor using the given getter and setter
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ValueAccessor<T> Lambda<T>(AutoGetter<T> getter, AutoSetter<T> setter) {
            return new DelegateValueAccessor<T>(getter, setter);
        }

        private class ReflectiveFieldValueAccessor<T> : ValueAccessor<T> {
            private readonly FieldInfo info;

            public ReflectiveFieldValueAccessor(FieldInfo info) {
                this.IsObjectPreferred = true;
                this.info = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override T GetValue(IAutomatable instance) {
                return (T) this.info.GetValue(instance);
            }

            public override object GetObjectValue(IAutomatable instance) {
                return this.info.GetValue(instance);
            }

            public override void SetValue(IAutomatable instance, T value) {
                this.info.SetValue(instance, value);
            }

            public override void SetObjectValue(IAutomatable instance, object value) {
                this.info.SetValue(instance, value);
            }
        }

        private class ReflectivePropertyValueAccessor<T> : ValueAccessor<T> {
            private readonly PropertyInfo info;

            public ReflectivePropertyValueAccessor(PropertyInfo info) {
                this.IsObjectPreferred = true;
                this.info = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override T GetValue(IAutomatable instance) {
                return (T) this.info.GetValue(instance);
            }

            public override object GetObjectValue(IAutomatable instance) {
                return this.info.GetValue(instance);
            }

            public override void SetValue(IAutomatable instance, T value) {
                this.info.SetValue(instance, value);
            }

            public override void SetObjectValue(IAutomatable instance, object value) {
                this.info.SetValue(instance, value);
            }
        }

        private class DelegateValueAccessor<T> : ValueAccessor<T> {
            private readonly AutoGetter<T> get;
            private readonly AutoSetter<T> set;

            public DelegateValueAccessor(AutoGetter<T> get, AutoSetter<T> set) {
                this.get = get ?? throw new ArgumentNullException(nameof(get));
                this.set = set ?? throw new ArgumentNullException(nameof(set));
            }

            public override T GetValue(IAutomatable instance) {
                return this.get(instance);
            }

            public override object GetObjectValue(IAutomatable instance) {
                return this.get(instance);
            }

            public override void SetValue(IAutomatable instance, T value) {
                this.set(instance, value);
            }

            public override void SetObjectValue(IAutomatable instance, object value) {
                this.set(instance, (T) value);
            }
        }

        private static MemberInfo GetPropertyOrField(Type type, string name) {
            // Can't get public or private in a single method call, according to the Expression class
            PropertyInfo p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (!ReferenceEquals(p, null))
                return p;
            FieldInfo f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (!ReferenceEquals(f, null))
                return f;
            if (!ReferenceEquals(p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy), null))
                return p;
            if (!ReferenceEquals(f = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy), null))
                return f;
            throw new Exception($"No such field or property with the name '{name}' in the type hierarchy for '{type.Name}'");
        }
    }
}