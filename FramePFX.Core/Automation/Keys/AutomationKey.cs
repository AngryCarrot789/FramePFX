using System;
using System.Collections.Generic;
using FramePFX.Core.Automation.Keyframe;

namespace FramePFX.Core.Automation.Keys {
    /// <summary>
    /// A property that can be automated/animated
    /// </summary>
    public abstract class AutomationKey {
        private static readonly Dictionary<string, Dictionary<string, AutomationKey>> RegistryMap = new Dictionary<string, Dictionary<string, AutomationKey>>();

        public string Domain { get; }

        public string Id { get; }

        public string FullId => this.Domain + "@" + this.Id;

        public abstract AutomationDataType DataType { get; }

        public static AutomationKeyDouble RegisterDouble(string domain, string id) {
            AutomationKeyDouble key = new AutomationKeyDouble(domain, id);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyLong RegisterLong(string domain, string id) {
            AutomationKeyLong key = new AutomationKeyLong(domain, id);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyBoolean RegisterBoolean(string domain, string id) {
            AutomationKeyBoolean key = new AutomationKeyBoolean(domain, id);
            RegisterInternal(key);
            return key;
        }

        public static AutomationKeyVector2 RegisterVector2(string domain, string id) {
            AutomationKeyVector2 key = new AutomationKeyVector2(domain, id);
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

        protected AutomationKey(string domain, string id) {
            this.Domain = domain;
            this.Id = id;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.Domain} -> {this.Id})";
        }
    }

    public class AutomationKeyDouble : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Double;

        internal AutomationKeyDouble(string domain, string id) : base(domain, id) {

        }
    }

    public class AutomationKeyLong : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Long;

        internal AutomationKeyLong(string domain, string id) : base(domain, id) {

        }
    }

    public class AutomationKeyBoolean : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Boolean;

        internal AutomationKeyBoolean(string domain, string id) : base(domain, id) {

        }
    }

    public class AutomationKeyVector2 : AutomationKey {
        public override AutomationDataType DataType => AutomationDataType.Vector2;

        internal AutomationKeyVector2(string domain, string id) : base(domain, id) {

        }
    }
}