using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Core.Automation.Keyframe;

namespace FramePFX.Core.Automation.Keys {
    /// <summary>
    /// A key for a property that can be automated
    /// </summary>
    public abstract class AutomationKey {
        private static readonly Dictionary<string, Dictionary<string, AutomationKey>> RegistryMap = new Dictionary<string, Dictionary<string, AutomationKey>>();

        public string Domain { get; }

        public string Id { get; }

        public KeyDescriptor Descriptor { get; }

        public string FullId => this.Domain + "->" + this.Id;

        public abstract AutomationDataType DataType { get; }

        protected AutomationKey(string domain, string id, KeyDescriptor descriptor) {
            this.Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            this.Domain = string.IsNullOrWhiteSpace(domain) ? throw new ArgumentException("domain cannot be null, empty or entirely whitespaces", nameof(domain)) : domain;
            this.Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("id cannot be null, empty or entirely whitespaces", nameof(id)) : id;
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

        public static AutomationKeyBoolean RegisterBoolean(string domain, string id, KeyDescriptorBoolean descriptor) {
            AutomationKeyBoolean key = new AutomationKeyBoolean(domain, id, descriptor);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyBoolean RegisterBoolean(string domain, string id, bool defaultValue = false) {
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
            if (!RegistryMap.TryGetValue(key.Domain, out Dictionary<string, AutomationKey> map))
                RegistryMap[key.Domain] = map = new Dictionary<string, AutomationKey>();
            if (map.TryGetValue(key.Id, out AutomationKey existingKey))
                throw new Exception($"Key already exists: {existingKey}");
            map[key.Id] = key;
        }

        public static AutomationKey GetKey(string domain, string id) {
            if (RegistryMap.TryGetValue(domain, out Dictionary<string, AutomationKey> map)) {
                if (map.TryGetValue(id, out AutomationKey key)) {
                    return key;
                }
            }

            return null;
        }

        public static bool TryGetKey(string domain, string id, out AutomationKey key) {
            return (key = GetKey(domain, id)) != null;
        }

        /// <summary>
        /// A helper function for creating a key frame instance that works with this automation key
        /// </summary>
        /// <returns></returns>
        public abstract KeyFrame CreateKeyFrame();

        public override string ToString() {
            return $"{this.GetType()}({this.Domain} -> {this.Id})";
        }
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