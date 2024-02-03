using System;
using System.Collections.Generic;
using System.Threading;

namespace FramePFX.Editors.DataTransfer {
    public delegate void DataParameterValueChangedEventHandler(DataParameter parameter, ITransferableData owner);

    /// <summary>
    /// A data parameter is similar to a <see cref="FramePFX.Editors.Automation.Params.Parameter"/>, except the
    /// value cannot be automated. The purpose of this is to simplify the data transfer between objects and things
    /// like slots, as parameters do
    /// </summary>
    public abstract class DataParameter : IEquatable<DataParameter>, IComparable<DataParameter> {
        private static readonly Dictionary<string, DataParameter> RegistryMap;
        private static readonly Dictionary<Type, List<DataParameter>> TypeToParametersMap;

        // Just in case parameters are not registered on the main thread for some reason,
        // this is used to provide protection against two parameters having the same GlobalIndex
        private static volatile int RegistrationFlag;
        private static int NextGlobalIndex = 1;

        /// <summary>
        /// Gets the class type that owns this parameter. This is usually always the class that
        /// this data parameter is defined in (as a static readonly field)
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// Gets this data parameter's unique key that identifies it relative to our <see cref="OwnerType"/>
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the globally registered index of this data parameter. This is the only property used for equality
        /// comparison between parameters for speed purposes. The global index should not be serialised because it
        /// may not be the same as more parameters are registered, even if <see cref="Key"/> remains the same
        /// </summary>
        public int GlobalIndex { get; private set; }

        /// <summary>
        /// Gets this data parameter's special flags, which add extra functionality
        /// </summary>
        public DataParameterFlags Flags { get; }

        public event DataParameterValueChangedEventHandler ValueChanged;

        protected DataParameter(Type ownerType, string key, DataParameterFlags flags) {
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null, empty or consist of only whitespaces");
            if (flags < DataParameterFlags.None || flags > DataParameterFlags.AffectsRender)
                throw new ArgumentOutOfRangeException(nameof(flags), flags, "Flags value was invalid");

            this.OwnerType = ownerType;
            this.Key = key;
            this.Flags = flags;
        }

        #region Registering parameters

        /// <summary>
        /// Registers the given parameter
        /// </summary>
        /// <param name="parameter">The parameter to register</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The parameter was already registered</exception>
        /// <exception cref="Exception">The parameter's key is already in use</exception>
        public static T Register<T>(T parameter) where T : DataParameter {
            RegisterCore(parameter);
            return parameter;
        }

        private static void RegisterCore(DataParameter parameter) {
            if (parameter.GlobalIndex != 0) {
                throw new InvalidOperationException("Data parameter was already registered with a global index of " + parameter.GlobalIndex);
            }

            string path = parameter.Key;
            while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
                Thread.SpinWait(32);

            try {
                if (RegistryMap.TryGetValue(path, out DataParameter existingParameter)) {
                    throw new Exception($"Key already exists with the ID '{path}': {existingParameter}");
                }

                RegistryMap[path] = parameter;
                if (!TypeToParametersMap.TryGetValue(parameter.OwnerType, out List<DataParameter> list))
                    TypeToParametersMap[parameter.OwnerType] = list = new List<DataParameter>();
                list.Add(parameter);
                parameter.GlobalIndex = NextGlobalIndex++;
            }
            finally {
                RegistrationFlag = 0;
            }
        }

        #endregion

        static DataParameter() {
            RegistryMap = new Dictionary<string, DataParameter>();
            TypeToParametersMap = new Dictionary<Type, List<DataParameter>>();
        }

        /// <summary>
        /// Gets the object value from the given owner, boxing if required
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <returns>The object, possibly boxed, value</returns>
        public abstract object GetObjectValue(ITransferableData owner);

        /// <summary>
        /// Sets the value from the given object, unboxing if required.Throws a cast exception if impossible
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <param name="value">The new value</param>
        public abstract void SetObjectValue(ITransferableData owner, object value);

        /// <summary>
        /// Begins a value change transactions. This MUST be called, otherwise there may be application-wide
        /// data corruption due to value change events not being fired
        /// </summary>
        /// <param name="owner">The owner whose value is going to be changed</param>
        protected virtual void OnBeginValueChange(ITransferableData owner) {
            TransferableData.InternalBeginValueChange(this, owner);
        }

        /// <summary>
        /// Ends a value change transactions. This MUST be called for the same reasons as mentioned
        /// in <see cref="OnBeginValueChange"/>, except now we need to finalize the states
        /// </summary>
        /// <param name="owner">The owner whose value has now changed</param>
        protected virtual void OnEndValueChange(ITransferableData owner) {
            TransferableData.InternalEndValueChange(this, owner);
        }

        public static DataParameter GetParameterByKey(string key) {
            if (!TryGetParameterByKey(key, out DataParameter parameter))
                throw new Exception("No such parameter with the key: " + key.ToString());
            return parameter;
        }

        public static DataParameter GetParameterByKey(string key, DataParameter def) {
            return TryGetParameterByKey(key, out DataParameter parameter) ? parameter : def;
        }

        public static bool TryGetParameterByKey(string key, out DataParameter parameter) {
            if (key == null) {
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

        public bool Equals(DataParameter other) {
            return !ReferenceEquals(other, null) && this.GlobalIndex == other.GlobalIndex;
        }

        public override bool Equals(object obj) {
            return obj is DataParameter parameter && this.GlobalIndex == parameter.GlobalIndex;
        }

        // GlobalIndex is only set once in RegisterInternal, therefore this code is fine
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => this.GlobalIndex;

        public int CompareTo(DataParameter other) {
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
        public static List<DataParameter> GetApplicableParameters(Type targetType, bool inHierarchy = true) {
            List<DataParameter> parameters = new List<DataParameter>();
            if (TypeToParametersMap.TryGetValue(targetType, out List<DataParameter> list)) {
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

        internal static void InternalOnParameterValueChanged(DataParameter parameter, ITransferableData owner) {
            parameter.ValueChanged?.Invoke(parameter, owner);
        }
    }
}