// This is used to modify PropOrFieldKey at compile time to account for there
// possible being more than one property or field with the same name (and possibly
// different property or field types).
// If we're being realistic, this shouldn't be needed... unless for some reason
// the binaries are modified externally to inject a duplicate property or field?
// In that case, well who cares it should probably fail at that point but we might
// as well handle it :D

#define SHOULD_BE_REALLY_ANAL

using System.Linq.Expressions;
using System.Reflection;
using Expression = System.Linq.Expressions.Expression;

namespace FramePFX.Utils.Accessing;

public static class ValueAccessors
{
    // Is using a map of maps even a good idea? It might be slower...
    private static readonly Dictionary<Type, Dictionary<PropOrFieldKey, object>> CachedLinqAccessors;
    private static ParameterExpression? InstanceParameter;

    static ValueAccessors()
    {
        CachedLinqAccessors = new Dictionary<Type, Dictionary<PropOrFieldKey, object>>();
    }

    /// <summary>
    /// Creates a reflection-based value accessor. This is recommended for
    /// low memory usage and also low general usage of the accessor itself
    /// </summary>
    /// <param name="owner">The class that contains the property or field</param>
    /// <param name="propertyOrField">The name of the property or field</param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <returns>A value accessor</returns>
    /// <exception cref="Exception">No property or field found with the specified name</exception>
    public static ValueAccessor<TValue> Reflective<TValue>(Type owner, string propertyOrField)
    {
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
    /// <param name="canUseCached">
    /// When true, tries to get a cached accessor, otherwise creates one and caches it.
    /// When false, a new accessor is always created and is not cached
    /// </param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <returns>A value accessor</returns>
    public static ValueAccessor<TValue> LinqExpression<TValue>(Type owner, string propertyOrField, bool canUseCached = false)
    {
        MemberInfo targetMember = GetPropertyOrField(owner, propertyOrField);
        Type? memberOwnerType = targetMember.DeclaringType;
        if (memberOwnerType == null)
            throw new Exception($"The target member named '{propertyOrField}' does not have a declaring type somehow");

        if (canUseCached)
        {
            return GetOrCreateCachedLinqAccessor<TValue>(memberOwnerType, propertyOrField, targetMember);
        }
        else
        {
            return CreateLinqAccessor<TValue>(memberOwnerType, targetMember);
        }
    }

    /// <summary>
    /// Creates an accessor that uses reflection for the first few get or set operations, and then switches to a Linq expression
    /// </summary>
    /// <param name="owner">The class that contains the property or field</param>
    /// <param name="propertyOrField">The name of the property or field</param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ValueAccessor<TValue> FastStartupAccessor<TValue>(Type owner, string propertyOrField)
    {
        // Using Time.GetSystemTicks(), this constructor takes 1ms~ compared to 7ms~ for LinqExpression() (both first calls before JIT).
        // JIT constructor is roughly 2x to 4x as fast compared to JIT LinqExpression(), but the JIT fast startup
        // was like 0.2 ms so it's not exactly saving much time
        // The numbers probably change based on the owner type but in general this class does help with startup time, slightly
        return new FastStartupValueAccessor<TValue>(owner, propertyOrField);
    }

    private static ValueAccessor<TValue> GetOrCreateCachedLinqAccessor<TValue>(Type memberOwnerType, string propertyOrField, MemberInfo targetMember)
    {
        PropOrFieldKey key = new PropOrFieldKey(propertyOrField, targetMember);
        Dictionary<PropOrFieldKey, object>? memberToAccessor;
        lock (CachedLinqAccessors)
        {
            if (!CachedLinqAccessors.TryGetValue(memberOwnerType, out memberToAccessor))
                CachedLinqAccessors[memberOwnerType] = memberToAccessor = new Dictionary<PropOrFieldKey, object>();
        }

        lock (memberToAccessor)
        {
            bool result = memberToAccessor.TryGetValue(key, out object? rawAccessor);
            ValueAccessor<TValue> accessor;
            if (result)
            {
                // Assuming the CLR won't allow duplicate properties or fields with the same name,
                // or if that's not the case then assuming order is at least maintained during runtime and
                // the property/field types cannot be changed, then this should cast successfully
                accessor = (ValueAccessor<TValue>) rawAccessor!;
            }
            else
            {
                memberToAccessor[key] = accessor = CreateLinqAccessor<TValue>(memberOwnerType, targetMember);
            }

            return accessor;
        }
    }

    private static ValueAccessor<TValue> CreateLinqAccessor<TValue>(Type memberOwnerType, MemberInfo memberInfo)
    {
        ParameterExpression paramInstance = InstanceParameter ??= Expression.Parameter(typeof(object), "instance");
        UnaryExpression castToOwner = Expression.Convert(paramInstance, memberOwnerType);
        MemberExpression dataMember = Expression.MakeMemberAccess(castToOwner, memberInfo);

        AccessGetter<TValue> getter = Expression.Lambda<AccessGetter<TValue>>(dataMember, paramInstance).Compile();

        ParameterExpression paramValue = Expression.Parameter(typeof(TValue), "value");
        BinaryExpression assignValue = Expression.Assign(dataMember, paramValue);
        AccessSetter<TValue> setter = Expression.Lambda<AccessSetter<TValue>>(assignValue, paramInstance, paramValue).Compile();

        return new GetSetValueAccessor<TValue>(getter, setter!);
    }

    /// <summary>
    /// Creates a value accessor using the given getter and setter
    /// </summary>
    /// <param name="getter">The value getter</param>
    /// <param name="setter">The value setter</param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <returns>A value accessor</returns>
    public static ValueAccessor<TValue> GetSet<TValue>(AccessGetter<TValue> getter, AccessSetter<TValue> setter)
    {
        return new GetSetValueAccessor<TValue>(getter, setter!);
    }

    // /// <summary>
    // /// Creates a value accessor that uses a dictionary to map an owner to a <see cref="TValue"/>
    // /// </summary>
    // /// <typeparam name="TValue">The value type</typeparam>
    // /// <returns>The storage value accessor</returns>
    // public static ValueAccessor<TValue> MappedStorage<TValue>() => new MappedStorageValueAccessor<TValue>();

    private class ReflectiveFieldValueAccessor<TValue> : ValueAccessor<TValue>
    {
        private readonly FieldInfo info;

        public ReflectiveFieldValueAccessor(FieldInfo info)
        {
            this.IsObjectPreferred = true;
            this.info = info ?? throw new ArgumentNullException(nameof(info));
        }

        public override TValue? GetValue(object owner)
        {
            return (TValue?) this.info.GetValue(owner);
        }

        public override object? GetObjectValue(object owner)
        {
            return this.info.GetValue(owner);
        }

        public override void SetValue(object owner, TValue? value)
        {
            this.info.SetValue(owner, value);
        }

        public override void SetObjectValue(object owner, object? value)
        {
            this.info.SetValue(owner, value);
        }
    }

    private class ReflectivePropertyValueAccessor<TValue> : ValueAccessor<TValue>
    {
        private readonly PropertyInfo info;

        public ReflectivePropertyValueAccessor(PropertyInfo info)
        {
            this.IsObjectPreferred = true;
            this.info = info ?? throw new ArgumentNullException(nameof(info));
        }

        public override TValue? GetValue(object owner)
        {
            return (TValue?) this.info.GetValue(owner);
        }

        public override object? GetObjectValue(object owner)
        {
            return this.info.GetValue(owner);
        }

        public override void SetValue(object owner, TValue? value)
        {
            this.info.SetValue(owner, value);
        }

        public override void SetObjectValue(object owner, object? value)
        {
            this.info.SetValue(owner, value);
        }
    }

    private class GetSetValueAccessor<TValue> : ValueAccessor<TValue>
    {
        private readonly AccessGetter<TValue?> get;
        private readonly AccessSetter<TValue?> set;

        public GetSetValueAccessor(AccessGetter<TValue?> get, AccessSetter<TValue?> set)
        {
            this.get = get ?? throw new ArgumentNullException(nameof(get));
            this.set = set ?? throw new ArgumentNullException(nameof(set));
        }

        public override TValue? GetValue(object owner)
        {
            return this.get(owner);
        }

        public override object? GetObjectValue(object owner)
        {
            return this.get(owner);
        }

        public override void SetValue(object owner, TValue? value)
        {
            this.set(owner, value);
        }

        public override void SetObjectValue(object owner, object? value)
        {
            this.set(owner, (TValue?) value);
        }
    }

    private class FastStartupValueAccessor<TValue> : ValueAccessor<TValue>
    {
        private readonly object locker = new object();
        private int isExpression;
        private int count;
        private ValueAccessor<TValue> accessor;

        private readonly Type ownerType;
        private readonly string propertyOrFieldName;

        public FastStartupValueAccessor(Type ownerType, string propertyOrFieldName)
        {
            this.accessor = Reflective<TValue>(ownerType, propertyOrFieldName);
            this.ownerType = ownerType;
            this.propertyOrFieldName = propertyOrFieldName;
        }

        private void CheckUpgrade()
        {
            if (this.isExpression == 0)
            {
                lock (this.locker)
                {
                    if (this.isExpression == 0 && ++this.count > 10)
                    {
                        Interlocked.Exchange(ref this.isExpression, 1);
                        this.accessor = LinqExpression<TValue>(this.ownerType, this.propertyOrFieldName);
                    }
                }
            }
        }

        public override TValue? GetValue(object owner)
        {
            this.CheckUpgrade();
            return this.accessor.GetValue(owner);
        }

        public override object? GetObjectValue(object owner)
        {
            this.CheckUpgrade();
            return this.accessor.GetObjectValue(owner);
        }

        public override void SetValue(object owner, TValue? value)
        {
            this.CheckUpgrade();
            this.accessor.SetValue(owner, value);
        }

        public override void SetObjectValue(object owner, object? value)
        {
            this.CheckUpgrade();
            this.accessor.SetObjectValue(owner, value);
        }
    }

    private static MemberInfo GetPropertyOrField(Type type, string name)
    {
        // Can't get public or private in a single method call, according to the Expression class
        PropertyInfo? p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        if (!ReferenceEquals(p, null))
            return p;
        FieldInfo? f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        if (!ReferenceEquals(f, null))
            return f;
        if (!ReferenceEquals(p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy), null))
            return p;
        if (!ReferenceEquals(f = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy), null))
            return f;
        throw new Exception($"No such field or property with the name '{name}' in the type hierarchy for '{type.Name}'");
    }

#if SHOULD_BE_REALLY_ANAL
    private readonly struct PropOrFieldKey : IEquatable<PropOrFieldKey>
    {
        public readonly string Name;
        public readonly Type DataType;
        private readonly int HashCode;

        private PropOrFieldKey(string name, Type dataType)
        {
            this.Name = name;
            this.DataType = dataType;
            this.HashCode = unchecked((this.Name.GetHashCode() * 397) ^ this.DataType.GetHashCode());
        }

        public PropOrFieldKey(string name, MemberInfo targetMember) : this(name, targetMember is PropertyInfo ? ((PropertyInfo) targetMember).PropertyType : ((FieldInfo) targetMember).FieldType) { }

        public bool Equals(PropOrFieldKey other) => this.Name == other.Name && this.DataType == other.DataType;

        public override bool Equals(object? obj) => obj is PropOrFieldKey other && this.Equals(other);

        public override int GetHashCode() => this.HashCode;
    }
#else
        private readonly struct PropOrFieldKey : IEquatable<PropOrFieldKey> {
            public readonly string Name;
            private readonly int HashCode;

            private PropOrFieldKey(string name) {
                this.Name = name;
                this.HashCode = this.Name.GetHashCode();
            }

            public PropOrFieldKey(string name, MemberInfo targetMember) : this(name) { }

            public bool Equals(PropOrFieldKey other) => this.Name == other.Name;

            public override bool Equals(object obj) => obj is PropOrFieldKey other && this.Equals(other);

            public override int GetHashCode() => this.HashCode;
        }
#endif
}