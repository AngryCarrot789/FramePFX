using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FramePFX.Editors.Automation.Params {
    /// <summary>
    /// A class used by parameters to get and set the effective value of a specific parameter for an object
    /// </summary>
    /// <typeparam name="TValue">The type of value this accessor accesses</typeparam>
    public abstract class ValueAccessor<TValue> {
        /// <summary>
        /// Returns true when the boxed getter and setters are preferred,
        /// e.g. this instance is reflection-based which always used boxed values
        /// </summary>
        public bool IsObjectPreferred { get; protected set; }

        /// <summary>
        /// Gets the generic value
        /// </summary>
        public abstract TValue GetValue(object owner);

        /// <summary>
        /// Gets the boxed value
        /// </summary>
        public abstract object GetObjectValue(object owner);

        /// <summary>
        /// Sets the generic value
        /// </summary>
        public abstract void SetValue(object owner, TValue value);

        /// <summary>
        /// Sets the object value
        /// </summary>
        public abstract void SetObjectValue(object owner, object value);
    }

    public static class ValueAccessors {
        private static ParameterExpression InstanceParameter;

        /// <summary>
        /// Creates a reflection-based value accessor. This is recommended for
        /// low memory usage and also low general usage of the accessor itself
        /// </summary>
        /// <param name="owner">The class that contains the property or field</param>
        /// <param name="propertyOrField">The name of the property or field</param>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns>A value accessor</returns>
        /// <exception cref="Exception">No property or field found with the specified name</exception>
        public static ValueAccessor<TValue> Reflective<TValue>(Type owner, string propertyOrField) {
            MemberInfo info = GetPropertyOrField(owner, propertyOrField);
            if (info is PropertyInfo)
                return new ReflectivePropertyValueAccessor<TValue>((PropertyInfo) info);
            return new ReflectiveFieldValueAccessor<TValue>((FieldInfo) info);
        }

        /// <summary>
        /// Creates a new expression-based value accessor, which is faster than reflection but has more memory overhead
        /// </summary>
        /// <param name="owner">The class that contains the property or field</param>
        /// <param name="propertyOrField">The name of the property or field</param>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns>A value accessor</returns>
        public static ValueAccessor<TValue> LinqExpression<TValue>(Type owner, string propertyOrField) {
            MemberInfo targetMember = GetPropertyOrField(owner, propertyOrField);
            Type memberOwnerType = targetMember.DeclaringType;
            if (memberOwnerType == null)
                throw new Exception($"The target member named '{propertyOrField}' does not have a declaring type somehow");

            ParameterExpression paramInstance = InstanceParameter ?? (InstanceParameter = Expression.Parameter(typeof(object), "instance"));
            UnaryExpression castToOwner = Expression.Convert(paramInstance, memberOwnerType);
            MemberExpression dataMember = Expression.MakeMemberAccess(castToOwner, targetMember);

            AutoGetter<TValue> getter = Expression.Lambda<AutoGetter<TValue>>(dataMember, paramInstance).Compile();

            ParameterExpression paramValue = Expression.Parameter(typeof(TValue), "value");
            BinaryExpression assignValue = Expression.Assign(dataMember, paramValue);
            AutoSetter<TValue> setter = Expression.Lambda<AutoSetter<TValue>>(assignValue, paramInstance, paramValue).Compile();

            return new DelegateValueAccessor<TValue>(getter, setter);
        }

        /// <summary>
        /// Creates a value accessor using the given getter and setter
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static ValueAccessor<TValue> Lambda<TValue>(AutoGetter<TValue> getter, AutoSetter<TValue> setter) {
            return new DelegateValueAccessor<TValue>(getter, setter);
        }

        private class ReflectiveFieldValueAccessor<TValue> : ValueAccessor<TValue> {
            private readonly FieldInfo info;

            public ReflectiveFieldValueAccessor(FieldInfo info) {
                this.IsObjectPreferred = true;
                this.info = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override TValue GetValue(object owner) {
                return (TValue) this.info.GetValue(owner);
            }

            public override object GetObjectValue(object owner) {
                return this.info.GetValue(owner);
            }

            public override void SetValue(object owner, TValue value) {
                this.info.SetValue(owner, value);
            }

            public override void SetObjectValue(object owner, object value) {
                this.info.SetValue(owner, value);
            }
        }

        private class ReflectivePropertyValueAccessor<TValue> : ValueAccessor<TValue> {
            private readonly PropertyInfo info;

            public ReflectivePropertyValueAccessor(PropertyInfo info) {
                this.IsObjectPreferred = true;
                this.info = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override TValue GetValue(object owner) {
                return (TValue) this.info.GetValue(owner);
            }

            public override object GetObjectValue(object owner) {
                return this.info.GetValue(owner);
            }

            public override void SetValue(object owner, TValue value) {
                this.info.SetValue(owner, value);
            }

            public override void SetObjectValue(object owner, object value) {
                this.info.SetValue(owner, value);
            }
        }

        private class DelegateValueAccessor<TValue> : ValueAccessor<TValue> {
            private readonly AutoGetter<TValue> get;
            private readonly AutoSetter<TValue> set;

            public DelegateValueAccessor(AutoGetter<TValue> get, AutoSetter<TValue> set) {
                this.get = get ?? throw new ArgumentNullException(nameof(get));
                this.set = set ?? throw new ArgumentNullException(nameof(set));
            }

            public override TValue GetValue(object owner) {
                return this.get(owner);
            }

            public override object GetObjectValue(object owner) {
                return this.get(owner);
            }

            public override void SetValue(object owner, TValue value) {
                this.set(owner, value);
            }

            public override void SetObjectValue(object owner, object value) {
                this.set(owner, (TValue) value);
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