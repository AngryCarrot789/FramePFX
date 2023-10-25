using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keyframe;

namespace FramePFX.Automation.Keys {
    /// <summary>
    /// A key for a property that can be automated. Only one instance should exist for a specific piece of data (e.g. media position, visibility, etc)
    /// </summary>
    public abstract class AutomationKey : IEquatable<AutomationKey>, IComparable<AutomationKey> {
        private static readonly Dictionary<string, Dictionary<string, AutomationKey>> RegistryMap = new Dictionary<string, Dictionary<string, AutomationKey>>();

        // the "::" splitter must not change, otherwise old projects won't load
        public const string FullIdSplitter = "::";
        private static volatile int RegistrationFlag;
        private static volatile int NextGlobalIndex;

        private readonly int hashCode;

        /// <summary>
        /// Gets this key's global index, relative to the entire appplication
        /// </summary>
        public int GlobalIndex { get; private set; }

        /// <summary>
        /// The "domain" aka "group" that this key is stored in. This is typically the readable name of the type that stores the key (e.g. VideoClip)
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// A unique ID for this key in its domain. The same ID can be used across multiple domains, but not the same domain
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Metadata about this automation key
        /// </summary>
        public KeyDescriptor Descriptor { get; }

        /// <summary>
        /// The <see cref="Domain"/> and <see cref="Id"/>, joined with <see cref="FullIdSplitter"/>
        /// </summary>
        public string FullId { get; }

        /// <summary>
        /// The data type of this key
        /// </summary>
        public abstract AutomationDataType DataType { get; }

        internal UpdateAutomationValueEventHandler __cachedUpdateHandler;

        protected AutomationKey(string domain, string id, KeyDescriptor descriptor) {
            this.Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            this.Domain = string.IsNullOrWhiteSpace(domain) ? throw new ArgumentException("domain cannot be null, empty or entirely whitespaces", nameof(domain)) : domain;
            this.Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("id cannot be null, empty or entirely whitespaces", nameof(id)) : id;
            this.FullId = this.Domain + FullIdSplitter + this.Id;
            unchecked {
                this.hashCode = (domain.GetHashCode() * 397) ^ id.GetHashCode();
            }
        }

        public static AutomationKeyFloat RegisterFloat(string domain, string id, KeyDescriptorFloat descriptor) {
            AutomationKeyFloat key = new AutomationKeyFloat(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyFloat RegisterFloat(string domain, string id, float defaultValue, float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, int precision = -1, float step = float.NaN) {
            AutomationKeyFloat key = new AutomationKeyFloat(domain, id, new KeyDescriptorFloat(defaultValue, minimum, maximum, precision, step));
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyDouble RegisterDouble(string domain, string id, KeyDescriptorDouble descriptor) {
            AutomationKeyDouble key = new AutomationKeyDouble(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyDouble RegisterDouble(string domain, string id, double defaultValue, double minimum = double.NegativeInfinity, double maximum = double.PositiveInfinity, int precision = -1, double step = double.NaN) {
            AutomationKeyDouble key = new AutomationKeyDouble(domain, id, new KeyDescriptorDouble(defaultValue, minimum, maximum, precision, step));
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyLong RegisterLong(string domain, string id, KeyDescriptorLong descriptor) {
            AutomationKeyLong key = new AutomationKeyLong(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyLong RegisterLong(string domain, string id, long defaultValue, long minimum = long.MinValue, long maximum = long.MaxValue, long step = 1) {
            AutomationKeyLong key = new AutomationKeyLong(domain, id, new KeyDescriptorLong(defaultValue, minimum, maximum, step));
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyBoolean RegisterBool(string domain, string id, KeyDescriptorBoolean descriptor) {
            AutomationKeyBoolean key = new AutomationKeyBoolean(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyBoolean RegisterBool(string domain, string id, bool defaultValue = false) {
            AutomationKeyBoolean key = new AutomationKeyBoolean(domain, id, new KeyDescriptorBoolean(defaultValue));
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyVector2 RegisterVec2(string domain, string id, KeyDescriptorVector2 descriptor) {
            AutomationKeyVector2 key = new AutomationKeyVector2(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyVector2 RegisterVec2(string domain, string id, Vector2 defaultValue, Vector2 minimum, Vector2 maximum, int precision = -1) {
            AutomationKeyVector2 key = new AutomationKeyVector2(domain, id, new KeyDescriptorVector2(defaultValue, minimum, maximum, precision));
            RegisterInternal(key);
            return key;
        }

        private static void RegisterInternal(AutomationKey key) {
            while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
                Thread.SpinWait(32);

            try {
                if (!RegistryMap.TryGetValue(key.Domain, out Dictionary<string, AutomationKey> map)) {
                    RegistryMap[key.Domain] = map = new Dictionary<string, AutomationKey>();
                }
                else if (map.TryGetValue(key.Id, out AutomationKey existingKey)) {
                    throw new Exception($"Key already exists with the ID '{key.Id}': {existingKey}");
                }

                map[key.Id] = key;
                key.GlobalIndex = NextGlobalIndex;
                Interlocked.Increment(ref NextGlobalIndex);
            }
            finally {
                RegistrationFlag = 0;
            }
        }

        public static AutomationKey GetKey(string domain, string id) {
            return TryGetKey(domain, id, out AutomationKey key) ? key : null;
        }

        public static bool TryGetKey(string domain, string id, out AutomationKey key) {
            while (Interlocked.CompareExchange(ref RegistrationFlag, 2, 0) != 0)
                Thread.Sleep(1);

            bool x;
            Dictionary<string, AutomationKey> map;
            try {
                x = RegistryMap.TryGetValue(domain, out map);
            }
            finally {
                RegistrationFlag = 0;
            }

            if (x && map.TryGetValue(id, out key))
                return true;
            key = null;
            return false;
        }

        /// <summary>
        /// A helper function for creating a key frame instance that works with this automation key
        /// </summary>
        /// <returns></returns>
        public abstract KeyFrame CreateKeyFrame();

        public override string ToString() {
            return $"{this.GetType().Name}({this.FullId})";
        }

        public bool Equals(AutomationKey other) {
            // cheaper to rule out the hash code than always doing type and string comparison
            return !ReferenceEquals(other, null) && this.hashCode == other.hashCode && this.GetType() == other.GetType() && this.Domain == other.Domain && this.Id == other.Id;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is AutomationKey && this.Equals((AutomationKey) obj);
        }

        /// <summary>
        /// Checks if two automation keys are equal
        /// </summary>
        /// <param name="a">Left value</param>
        /// <param name="b">Right value</param>
        /// <returns>A and B are equal</returns>
        public static bool operator ==(AutomationKey a, AutomationKey b) {
            // a == b || a != null && b != null && a.equals(b)
            return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b);
        }

        public static bool operator !=(AutomationKey a, AutomationKey b) {
            // a != b && (a == null || b == null || !a.equals(b))
            return !ReferenceEquals(a, b) && (ReferenceEquals(a, null) || ReferenceEquals(b, null) || !a.Equals(b));
        }

        public sealed override int GetHashCode() => this.hashCode;

        public int CompareTo(AutomationKey other) {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            return this.GlobalIndex.CompareTo(other.GlobalIndex);
        }
    }

    public class AutomationKeyFloat : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Float;

        public new KeyDescriptorFloat Descriptor => (KeyDescriptorFloat) base.Descriptor;

        internal AutomationKeyFloat(string domain, string id, KeyDescriptorFloat descriptor) : base(domain, id, descriptor) {
        }

        public override KeyFrame CreateKeyFrame() => this.CreateKeyFrameCore();
        public KeyFrameFloat CreateKeyFrameCore() => new KeyFrameFloat {Value = this.Descriptor.DefaultValue};
    }

    public class AutomationKeyDouble : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Double;

        public new KeyDescriptorDouble Descriptor => (KeyDescriptorDouble) base.Descriptor;

        internal AutomationKeyDouble(string domain, string id, KeyDescriptorDouble descriptor) : base(domain, id, descriptor) {
        }

        public override KeyFrame CreateKeyFrame() => this.CreateKeyFrameCore();
        public KeyFrameDouble CreateKeyFrameCore() => new KeyFrameDouble {Value = this.Descriptor.DefaultValue};
    }

    public class AutomationKeyLong : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Long;

        public new KeyDescriptorLong Descriptor => (KeyDescriptorLong) base.Descriptor;

        internal AutomationKeyLong(string domain, string id, KeyDescriptorLong descriptor) : base(domain, id, descriptor) {
        }

        public override KeyFrame CreateKeyFrame() => this.CreateKeyFrameCore();
        public KeyFrameLong CreateKeyFrameCore() => new KeyFrameLong {Value = this.Descriptor.DefaultValue};
    }

    public class AutomationKeyBoolean : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public new KeyDescriptorBoolean Descriptor => (KeyDescriptorBoolean) base.Descriptor;

        internal AutomationKeyBoolean(string domain, string id, KeyDescriptorBoolean descriptor) : base(domain, id, descriptor) {
        }

        public override KeyFrame CreateKeyFrame() => this.CreateKeyFrameCore();
        public KeyFrameBoolean CreateKeyFrameCore() => new KeyFrameBoolean {Value = this.Descriptor.DefaultValue};
    }

    public class AutomationKeyVector2 : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Vector2;

        public new KeyDescriptorVector2 Descriptor => (KeyDescriptorVector2) base.Descriptor;

        internal AutomationKeyVector2(string domain, string id, KeyDescriptorVector2 descriptor) : base(domain, id, descriptor) {
        }

        public override KeyFrame CreateKeyFrame() => this.CreateKeyFrameCore();
        public KeyFrameVector2 CreateKeyFrameCore() => new KeyFrameVector2 {Value = this.Descriptor.DefaultValue};
    }
}