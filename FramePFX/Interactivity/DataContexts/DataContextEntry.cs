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

namespace FramePFX.Interactivity.DataContexts {
    public class DataContextEntry {
        /// <summary>
        /// Gets the data key
        /// </summary>
        public DataKey Key { get; }

        /// <summary>
        /// Gets the data value
        /// </summary>
        public object Value { get; }

        public DataContextEntry(DataKey key, object value) {
            this.Key = key;
            this.Value = value;
        }

        public static DataContextEntry Of<T>(DataKey<T> key, T value) => new DataContextEntry(key, value);
    }
}