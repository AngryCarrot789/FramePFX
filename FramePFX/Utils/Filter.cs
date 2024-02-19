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
using System.Linq;
using System.Text;

namespace FramePFX.Utils {
    public sealed class Filter {
        private readonly StringBuilder sb;
        private bool hasFirst;

        public Filter() {
            this.sb = new StringBuilder(32);
        }

        public Filter(string filter) {
            this.sb = new StringBuilder(filter ?? "");
            this.hasFirst = this.sb.Length > 0;
        }

        public static Filter Of() {
            return new Filter();
        }

        private StringBuilder Prepare() {
            if (this.hasFirst) {
                return this.sb.Append('|');
            }
            else {
                this.hasFirst = true;
                return this.sb;
            }
        }

        public Filter AddAllFiles() {
            this.Prepare().Append("All Files|*.*");
            return this;
        }

        public Filter AddFilter(string readableName, params string[] extensions) {
            if (string.IsNullOrWhiteSpace(readableName))
                throw new ArgumentException("Readable name cannot be null, empty, or consist of only whitespaces", nameof(readableName));
            if (extensions.Any(string.IsNullOrEmpty))
                throw new ArgumentException("One of the extension was null, empty, or consisted of only whitespaces", nameof(extensions));
            if (extensions.Any(x => x[0] == '.'))
                throw new ArgumentException("Extension should not contain a . char at the start", nameof(extensions));

            this.Prepare().Append(readableName).Append('|').Append(string.Join(";", extensions.Select(x => "*." + x)));
            return this;
        }

        public Filter AddFilter(string readableName, string extension) {
            if (string.IsNullOrWhiteSpace(readableName))
                throw new ArgumentException("Readable name cannot be null, empty, or consist of only whitespaces", nameof(readableName));
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("Extension was null, empty, or consisted of only whitespaces", nameof(extension));
            if (extension[0] == '.')
                throw new ArgumentException("Extension should not contain a . char at the start: " + extension, nameof(extension));

            this.Prepare().Append(readableName).Append('|').Append("*.").Append(extension.StartsWith(".") ? extension.Substring(1) : extension);
            return this;
        }

        public override string ToString() {
            return this.sb.ToString();
        }

        public override int GetHashCode() {
            return this.sb.ToString().GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is Filter filter && this.sb.Equals(filter.sb);
        }
    }
}