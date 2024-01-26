using System;

namespace FramePFX.Editors.Automation.Params {
    /// <summary>
    /// A key for an automatable parameter, used to identify a parameter globally
    /// </summary>
    public readonly struct ParameterKey : IEquatable<ParameterKey> {
        /// <summary>
        /// Gets this parameter key's domain
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Gets the name of the parameter, which is unique relative to the domain
        /// </summary>
        public string Name { get; }

        public ParameterKey(string domain, string name) {
            this.Domain = domain ?? throw new ArgumentNullException(nameof(domain));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static bool TryParse(string input, out ParameterKey key) {
            int index = input.IndexOf(Parameter.FullIdSplitter);
            if (index == -1) {
                key = default;
                return false;
            }
            else {
                key = new ParameterKey(input.Substring(0, index), input.Substring(index + Parameter.FullIdSplitter.Length));
                return true;
            }
        }

        public static ParameterKey Parse(string input) {
            if (TryParse(input, out ParameterKey key))
                return key;
            throw new FormatException("Invalid parameter key string: " + input);
        }

        public override string ToString() {
            return this.Domain + Parameter.FullIdSplitter + this.Name;
        }

        public override bool Equals(object obj) {
            return obj is ParameterKey key && this.Equals(key);
        }

        public override int GetHashCode() {
            return unchecked((this.Domain.GetHashCode() * 397) ^ this.Name.GetHashCode());
        }

        public bool Equals(ParameterKey key) {
            return this.Domain == key.Domain && this.Name == key.Name;
        }
    }
}