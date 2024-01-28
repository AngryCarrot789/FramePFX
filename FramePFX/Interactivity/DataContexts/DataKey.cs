using System;

namespace FramePFX.Interactivity.DataContexts {
    public class DataKey {
        public string ReadableName { get; }

        public Type DataType { get; }

        public DataKey(string readableName, Type type) {
            this.ReadableName = readableName;
            this.DataType = type;
        }

        public override string ToString() {
            return $"DataKey(\"{this.ReadableName}\")";
        }
    }

    public class DataKey<T> : DataKey {
        public DataKey(string readableName) : base(readableName, typeof(T)) {

        }
    }
}