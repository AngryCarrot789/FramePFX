//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;

namespace FramePFX.Interactivity.DataContexts {
    public abstract class DataKey {
        private static readonly Dictionary<string, DataKey> Registry;

        /// <summary>
        /// A unique identifier for this data key
        /// </summary>
        public string Id { get; }

        protected DataKey(string id) {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        static DataKey() {
            Registry = new Dictionary<string, DataKey>();
        }

        public static DataKey GetKeyById(string id) {
            return Registry.TryGetValue(id, out DataKey key) ? key : null;
        }

        protected static void RegisterInternal(string id, DataKey key) {
            if (ReferenceEquals(key, null))
                throw new ArgumentNullException(nameof(key));
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (Registry.ContainsKey(id))
                throw new InvalidOperationException("ID already in use: " + id);
            Registry[id] = key;
        }

        public static bool operator ==(DataKey a, DataKey b) {
            return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b);
        }

        public static bool operator !=(DataKey a, DataKey b) {
            return !ReferenceEquals(a, b) && (ReferenceEquals(a, null) || ReferenceEquals(b, null) || !a.Equals(b));
        }

        protected bool Equals(DataKey other) {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is DataKey key && this.Equals(key);
        }

        public override int GetHashCode() => this.Id.GetHashCode();

        public override string ToString() {
            return $"DataKey(\"{this.Id}\")";
        }
    }

    public class DataKey<T> : DataKey {
        private DataKey(string id) : base(id) {

        }

        public static DataKey<T> Create(string id) {
            DataKey<T> key = new DataKey<T>(id);
            RegisterInternal(id, key);
            return key;
        }

        public bool TryGetContext(IDataContext context, out T value) {
            if (context.TryGetContext(this.Id, out object obj)) {
                value = (T) obj;
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        public T GetContext(IDataContext context, T def = default) {
            return context.TryGetContext(this.Id, out object obj) ? (T) obj : def;
        }
    }
}