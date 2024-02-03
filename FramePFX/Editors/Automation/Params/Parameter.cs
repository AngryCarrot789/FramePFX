using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FramePFX.Editors.Automation.Keyframes;

namespace FramePFX.Editors.Automation.Params {
    public delegate T AutoGetter<out T>(object a);
    public delegate void AutoSetter<in T>(object a, T v);

    /// <summary>
    /// A class that stores information about a registered parameter for a specific type of automatable object
    /// </summary>
    public abstract class Parameter : IEquatable<Parameter>, IComparable<Parameter> {
        public static readonly ParameterKey Empty = default;

        private static readonly Dictionary<ParameterKey, Parameter> RegistryMap;
        private static readonly Dictionary<Type, List<Parameter>> TypeToParametersMap;

        // Just in case parameters are not registered on the main thread for some reason,
        // this is used to provide protection against two parameters having the same GlobalIndex
        private static volatile int RegistrationFlag;
        private static int NextGlobalIndex = 1;

        public const string FullIdSplitter = "::";

        /// <summary>
        /// This parameter's unique key that identifies it. This is used for serialisation instead
        /// of <see cref="GlobalIndex"/>, because the global index may not remain constant as more
        /// and more parameters are registered during the development of this video editor
        /// </summary>
        public ParameterKey Key { get; }

        /// <summary>
        /// Gets the class type that owns this parameter. This is usually the calling
        /// class that registered the parameter itself
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// The automation data type of this parameter. This determines what type
        /// of key frames can be associated with this parameter
        /// </summary>
        public AutomationDataType DataType { get; }

        /// <summary>
        /// Gets this parameter's descriptor, which contains information about the behaviour of the parameter's
        /// value such as minimum and maximum value range, default value, rounding, decimal precision, etc.
        /// </summary>
        public ParameterDescriptor Descriptor { get; }

        /// <summary>
        /// Gets the globally registered index of this parameter. This is the only property used for equality
        /// comparison between parameters. The global index should not be serialised because it may not be the
        /// same as more parameters are registered, even if <see cref="Key"/> remains the same
        /// </summary>
        public int GlobalIndex { get; private set; }

        /// <summary>
        /// Gets this parameter's special flags, which add extra functionality
        /// </summary>
        public ParameterFlags Flags { get; }

        /// <summary>
        /// An event fired when this parameter changes in any <see cref="AutomationSequence"/> throughout the entire application
        /// </summary>
        public event ParameterChangedEventHandler ParameterValueChanged;

        protected Parameter(Type ownerType, ParameterKey key, ParameterDescriptor descriptor, ParameterFlags flags) {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));
            this.Key = key;
            this.OwnerType = ownerType;
            this.Descriptor = descriptor;
            this.DataType = descriptor.DataType;
            this.Flags = flags;
        }

        static Parameter() {
            RegistryMap = new Dictionary<ParameterKey, Parameter>();
            TypeToParametersMap = new Dictionary<Type, List<Parameter>>();
        }

        /// <summary>
        /// Adds the given event handler to all of the given parameters
        /// </summary>
        /// <param name="handler">The handler to add</param>
        /// <param name="parameters">The parameters to add an event handler for</param>
        public static void AddMultipleHandlers(ParameterChangedEventHandler handler, params Parameter[] parameters) {
            foreach (Parameter parameter in parameters) {
                parameter.ParameterValueChanged += handler;
            }
        }

        /// <summary>
        /// Calculates and sets effective value of the sequence's data owner. Calling this method directly will
        /// not result in any events being fired, therefore, this method shouldn't really be called directly.
        /// Instead, use <see cref="AutomationSequence.UpdateValue(long)"/> which fires the appropriate sequence of events
        /// </summary>
        /// <param name="sequence">The sequence used to reference the parameter and automation data owner</param>
        /// <param name="frame">The frame which should be used to calculate the new effective value</param>
        public abstract void SetValue(AutomationSequence sequence, long frame);

        public abstract object GetObjectValue(IAutomatable automatable);

        public abstract object GetObjectValue(long frame, AutomationSequence sequence);

        public KeyFrame CreateKeyFrame(long frame = 0L) => KeyFrame.CreateDefault(this, frame);

        #region Registering parameters

        public static ParameterFloat RegisterFloat(Type ownerType, string domain, string name, ValueAccessor<float> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterFloat(ownerType, domain, name, new ParameterDescriptorFloat(), accessor, flags);
        public static ParameterFloat RegisterFloat(Type ownerType, string domain, string name, float defaultValue, ValueAccessor<float> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterFloat(ownerType, domain, name, new ParameterDescriptorFloat(defaultValue), accessor, flags);
        public static ParameterFloat RegisterFloat(Type ownerType, string domain, string name, float defaultValue, float minValue, float maxValue, ValueAccessor<float> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterFloat(ownerType, domain, name, new ParameterDescriptorFloat(defaultValue, minValue, maxValue), accessor, flags);
        public static ParameterFloat RegisterFloat(Type ownerType, string domain, string name, ParameterDescriptorFloat desc, ValueAccessor<float> accessor, ParameterFlags flags = ParameterFlags.None) {
            return (ParameterFloat) Register(new ParameterFloat(ownerType, new ParameterKey(domain, name), desc, accessor, flags));
        }

        public static ParameterDouble RegisterDouble(Type ownerType, string domain, string name, ValueAccessor<double> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterDouble(ownerType, domain, name, new ParameterDescriptorDouble(), accessor, flags);
        public static ParameterDouble RegisterDouble(Type ownerType, string domain, string name, double defaultValue, ValueAccessor<double> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterDouble(ownerType, domain, name, new ParameterDescriptorDouble(defaultValue), accessor, flags);
        public static ParameterDouble RegisterDouble(Type ownerType, string domain, string name, double defaultValue, double minValue, double maxValue, ValueAccessor<double> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterDouble(ownerType, domain, name, new ParameterDescriptorDouble(defaultValue, minValue, maxValue), accessor, flags);
        public static ParameterDouble RegisterDouble(Type ownerType, string domain, string name, ParameterDescriptorDouble desc, ValueAccessor<double> accessor, ParameterFlags flags = ParameterFlags.None) {
            return (ParameterDouble) Register(new ParameterDouble(ownerType, new ParameterKey(domain, name), desc, accessor, flags));
        }

        public static ParameterLong RegisterLong(Type ownerType, string domain, string name, ValueAccessor<long> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterLong(ownerType, domain, name, new ParameterDescriptorLong(), accessor, flags);
        public static ParameterLong RegisterLong(Type ownerType, string domain, string name, long defaultValue, ValueAccessor<long> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterLong(ownerType, domain, name, new ParameterDescriptorLong(defaultValue), accessor, flags);
        public static ParameterLong RegisterLong(Type ownerType, string domain, string name, long defaultValue, long minValue, long maxValue, ValueAccessor<long> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterLong(ownerType, domain, name, new ParameterDescriptorLong(defaultValue, minValue, maxValue), accessor, flags);
        public static ParameterLong RegisterLong(Type ownerType, string domain, string name, ParameterDescriptorLong desc, ValueAccessor<long> accessor, ParameterFlags flags = ParameterFlags.None) {
            return (ParameterLong) Register(new ParameterLong(ownerType, new ParameterKey(domain, name), desc, accessor, flags));
        }

        public static ParameterBoolean RegisterBoolean(Type ownerType, string domain, string name, ValueAccessor<bool> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterBoolean(ownerType, domain, name, new ParameterDescriptorBoolean(), accessor, flags);
        public static ParameterBoolean RegisterBoolean(Type ownerType, string domain, string name, bool defaultValue, ValueAccessor<bool> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterBoolean(ownerType, domain, name, new ParameterDescriptorBoolean(defaultValue), accessor, flags);
        public static ParameterBoolean RegisterBoolean(Type ownerType, string domain, string name, ParameterDescriptorBoolean desc, ValueAccessor<bool> accessor, ParameterFlags flags = ParameterFlags.None) {
            return (ParameterBoolean) Register(new ParameterBoolean(ownerType, new ParameterKey(domain, name), desc, accessor, flags));
        }

        public static ParameterVector2 RegisterVector2(Type ownerType, string domain, string name, ValueAccessor<Vector2> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterVector2(ownerType, domain, name, new ParameterDescriptorVector2(), accessor, flags);
        public static ParameterVector2 RegisterVector2(Type ownerType, string domain, string name, Vector2 defaultValue, ValueAccessor<Vector2> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterVector2(ownerType, domain, name, new ParameterDescriptorVector2(defaultValue), accessor, flags);
        public static ParameterVector2 RegisterVector2(Type ownerType, string domain, string name, Vector2 defaultValue, Vector2 minValue, Vector2 maxValue, ValueAccessor<Vector2> accessor, ParameterFlags flags = ParameterFlags.None) => RegisterVector2(ownerType, domain, name, new ParameterDescriptorVector2(defaultValue, minValue, maxValue), accessor, flags);
        public static ParameterVector2 RegisterVector2(Type ownerType, string domain, string name, ParameterDescriptorVector2 desc, ValueAccessor<Vector2> accessor, ParameterFlags flags = ParameterFlags.None) {
            return (ParameterVector2) Register(new ParameterVector2(ownerType, new ParameterKey(domain, name), desc, accessor, flags));
        }

        /// <summary>
        /// Registers the given parameter
        /// </summary>
        /// <param name="parameter">The parameter to register</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The parameter was already registered</exception>
        /// <exception cref="Exception">The parameter's key is already in use</exception>
        public static Parameter Register(Parameter parameter) {
            if (parameter.GlobalIndex != 0) {
                throw new InvalidOperationException("Parameter was already registered with a global index of " + parameter.GlobalIndex);
            }

            ParameterKey path = parameter.Key;
            while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
                Thread.SpinWait(32);

            try {
                if (RegistryMap.TryGetValue(path, out Parameter existingParameter)) {
                    throw new Exception($"Key already exists with the ID '{path}': {existingParameter}");
                }

                RegistryMap[path] = parameter;
                if (!TypeToParametersMap.TryGetValue(parameter.OwnerType, out List<Parameter> list))
                    TypeToParametersMap[parameter.OwnerType] = list = new List<Parameter>();
                list.Add(parameter);
                parameter.GlobalIndex = NextGlobalIndex++;
            }
            finally {
                RegistrationFlag = 0;
            }

            return parameter;
        }

        #endregion

        public static Parameter GetParameterByKey(ParameterKey key) {
            if (!TryGetParameterByKey(key, out Parameter parameter))
                throw new Exception("No such parameter with the key: " + key.ToString());
            return parameter;
        }

        public static Parameter GetParameterByKey(ParameterKey key, Parameter def) {
            return TryGetParameterByKey(key, out Parameter parameter) ? parameter : def;
        }

        public static bool TryGetParameterByKey(ParameterKey key, out Parameter parameter) {
            if (key.IsEmpty) {
                parameter = null;
                return false;
            }

            while (Interlocked.CompareExchange(ref RegistrationFlag, 2, 0) != 0)
                Thread.Sleep(1);

            try {
                return RegistryMap.TryGetValue(key, out parameter);
            }
            finally {
                RegistrationFlag = 0;
            }
        }

        public bool Equals(Parameter other) {
            return !ReferenceEquals(other, null) && this.GlobalIndex == other.GlobalIndex;
        }

        public override bool Equals(object obj) {
            return obj is Parameter parameter && this.GlobalIndex == parameter.GlobalIndex;
        }

        // GlobalIndex is only set once in RegisterInternal, therefore this code is fine
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => this.GlobalIndex;

        public int CompareTo(Parameter other) {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            return this.GlobalIndex.CompareTo(other.GlobalIndex);
        }

        /// <summary>
        /// Returns an enumerable of all parameters that are applicable to the given type.
        /// </summary>
        /// <param name="targetType">The type to get the applicable parameters of</param>
        /// <param name="inHierarchy">
        /// When true, it will also accumulate the parameters of every base type. When false,
        /// it just gets the parameters for the exact given type (parameters whose owner types match)</param>
        /// <returns>An enumerable of parameters</returns>
        public static List<Parameter> GetApplicableParameters(Type targetType, bool inHierarchy = true) {
            List<Parameter> parameters = new List<Parameter>();
            if (TypeToParametersMap.TryGetValue(targetType, out List<Parameter> list)) {
                parameters.AddRange(list);
            }

            if (inHierarchy) {
                for (Type bType = targetType.BaseType; bType != null; bType = bType.BaseType) {
                    if (TypeToParametersMap.TryGetValue(bType, out list)) {
                        parameters.AddRange(list);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Invokes the <see cref="ParameterValueChanged"/> event for the given sequence. This is only fired
        /// when the underlying effective value actually changes for the sequence's owner
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="sequence"></param>
        /// <exception cref="ArgumentException"></exception>
        internal static void InternalOnParameterValueChanged(Parameter parameter, AutomationSequence sequence) {
            if (sequence.Parameter.GlobalIndex != parameter.GlobalIndex)
                throw new ArgumentException("Sequence's parameter does not match the current instance");
            parameter.ParameterValueChanged?.Invoke(sequence);
        }
    }

    public sealed class ParameterFloat : Parameter {
        private readonly ValueAccessor<float> accessor;

        /// <summary>
        /// Gets the <see cref="ParameterDescriptorFloat"/> for this parameter. This just casts the base <see cref="Parameter.Descriptor"/> property
        /// </summary>
        public new ParameterDescriptorFloat Descriptor => (ParameterDescriptorFloat) base.Descriptor;

        public ParameterFloat(Type ownerType, ParameterKey key, ParameterDescriptorFloat descriptor, ValueAccessor<float> accessor, ParameterFlags flags = ParameterFlags.None) : base(ownerType, key, descriptor, flags) {
            this.accessor = accessor;
        }

        public override void SetValue(AutomationSequence sequence, long frame) {
            this.accessor.SetValue(sequence.AutomationData.Owner, sequence.GetFloatValue(frame));
        }

        public float GetEffectiveValue(IAutomatable automatable) => this.accessor.GetValue(automatable);

        public override object GetObjectValue(IAutomatable automatable) => this.accessor.GetObjectValue(automatable);

        public override object GetObjectValue(long frame, AutomationSequence sequence) => sequence.GetFloatValue(frame);
    }

    public sealed class ParameterDouble : Parameter {
        private readonly ValueAccessor<double> accessor;

        /// <summary>
        /// Gets the <see cref="ParameterDescriptorDouble"/> for this parameter. This just casts the base <see cref="Parameter.Descriptor"/> property
        /// </summary>
        public new ParameterDescriptorDouble Descriptor => (ParameterDescriptorDouble) base.Descriptor;

        public ParameterDouble(Type ownerType, ParameterKey key, ParameterDescriptorDouble descriptor, ValueAccessor<double> accessor, ParameterFlags flags = ParameterFlags.None) : base(ownerType, key, descriptor, flags) {
            this.accessor = accessor;
        }

        public override void SetValue(AutomationSequence sequence, long frame) {
            this.accessor.SetValue(sequence.AutomationData.Owner, sequence.GetDoubleValue(frame));
        }

        public double GetEffectiveValue(IAutomatable automatable) => this.accessor.GetValue(automatable);

        public override object GetObjectValue(IAutomatable automatable) => this.accessor.GetObjectValue(automatable);

        public override object GetObjectValue(long frame, AutomationSequence sequence) => sequence.GetDoubleValue(frame);
    }

    public sealed class ParameterLong : Parameter {
        private readonly ValueAccessor<long> accessor;

        /// <summary>
        /// Gets the <see cref="ParameterDescriptorLong"/> for this parameter. This just casts the base <see cref="Parameter.Descriptor"/> property
        /// </summary>
        public new ParameterDescriptorLong Descriptor => (ParameterDescriptorLong) base.Descriptor;

        public ParameterLong(Type ownerType, ParameterKey key, ParameterDescriptorLong descriptor, ValueAccessor<long> accessor, ParameterFlags flags = ParameterFlags.None) : base(ownerType, key, descriptor, flags) {
            this.accessor = accessor;
        }

        public override void SetValue(AutomationSequence sequence, long frame) {
            this.accessor.SetValue(sequence.AutomationData.Owner, sequence.GetLongValue(frame));
        }

        public long GetEffectiveValue(IAutomatable automatable) => this.accessor.GetValue(automatable);

        public override object GetObjectValue(IAutomatable automatable) => this.accessor.GetObjectValue(automatable);

        public override object GetObjectValue(long frame, AutomationSequence sequence) => sequence.GetLongValue(frame);
    }

    public sealed class ParameterBoolean : Parameter {
        private readonly ValueAccessor<bool> accessor;

        /// <summary>
        /// Gets the <see cref="ParameterDescriptorBoolean"/> for this parameter. This just casts the base <see cref="Parameter.Descriptor"/> property
        /// </summary>
        public new ParameterDescriptorBoolean Descriptor => (ParameterDescriptorBoolean) base.Descriptor;

        public ParameterBoolean(Type ownerType, ParameterKey key, ParameterDescriptorBoolean descriptor, ValueAccessor<bool> accessor, ParameterFlags flags = ParameterFlags.None) : base(ownerType, key, descriptor, flags) {
            this.accessor = accessor;
        }

        public override void SetValue(AutomationSequence sequence, long frame) {
            this.accessor.SetValue(sequence.AutomationData.Owner, sequence.GetBooleanValue(frame));
        }

        public bool GetEffectiveValue(IAutomatable automatable) => this.accessor.GetValue(automatable);

        public override object GetObjectValue(IAutomatable automatable) => this.accessor.GetObjectValue(automatable);

        public override object GetObjectValue(long frame, AutomationSequence sequence) => sequence.GetBooleanValue(frame);
    }

    public sealed class ParameterVector2 : Parameter {
        private readonly ValueAccessor<Vector2> accessor;

        /// <summary>
        /// Gets the <see cref="ParameterDescriptorVector2"/> for this parameter. This just casts the base <see cref="Parameter.Descriptor"/> property
        /// </summary>
        public new ParameterDescriptorVector2 Descriptor => (ParameterDescriptorVector2) base.Descriptor;

        public ParameterVector2(Type ownerType, ParameterKey key, ParameterDescriptorVector2 descriptor, ValueAccessor<Vector2> accessor, ParameterFlags flags = ParameterFlags.None) : base(ownerType, key, descriptor, flags) {
            this.accessor = accessor;
        }

        public override void SetValue(AutomationSequence sequence, long frame) {
            this.accessor.SetValue(sequence.AutomationData.Owner, sequence.GetVector2Value(frame));
        }

        public Vector2 GetEffectiveValue(IAutomatable automatable) => this.accessor.GetValue(automatable);

        public override object GetObjectValue(IAutomatable automatable) => this.accessor.GetObjectValue(automatable);

        public override object GetObjectValue(long frame, AutomationSequence sequence) => sequence.GetVector2Value(frame);
    }
}