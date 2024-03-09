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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using FramePFX.Utils;

namespace FramePFX.FileSystems
{
    /// <summary>
    /// A struct which safely stores a URL for a virtual file
    /// </summary>
    public readonly struct VirtualFileUrl : IEquatable<VirtualFileUrl>
    {
        public readonly string Url;
        private readonly int protocolIndex;

        /// <summary>
        /// Gets the protocol part of the URL
        /// </summary>
        public string Protocol => !this.IsValid ? null : this.Url.Substring(0, this.protocolIndex);

        /// <summary>
        /// Gets the path part of the URL
        /// </summary>
        public string Path => !this.IsValid ? null : this.Url.Substring(this.protocolIndex + VirtualFileManager.ProtocolSeparator.Length);

        /// <summary>
        /// Returns true when this struct was created through the factory method(s) or constructor
        /// </summary>
        public bool IsValid => this.Url != null;

        public VirtualFileUrl(string url)
        {
            StringUtils.ValidateNotWhiteSpaces(url, nameof(url));
            if ((this.protocolIndex = url.IndexOf(VirtualFileManager.ProtocolSeparator)) == 1)
                throw new ArgumentException("URL does not contain a protocol", nameof(url));
            this.Url = url.Replace('\\', '/');
        }

        public VirtualFileUrl(string protocol, string path)
        {
            StringUtils.ValidateNotWhiteSpaces(protocol, nameof(protocol));
            StringUtils.ValidateNotWhiteSpaces(path, nameof(protocol));
            this.Url = protocol + VirtualFileManager.ProtocolSeparator + path.Replace('\\', '/');
            this.protocolIndex = protocol.Length;
        }

        public void EnsureValid()
        {
            if (!this.IsValid)
                throw new InvalidOperationException("URL is not valid");
        }

        public bool Equals(VirtualFileUrl other)
        {
            return this.Url == other.Url;
        }

        public override bool Equals(object obj)
        {
            return obj is VirtualFileUrl other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Url != null ? this.Url.GetHashCode() : 0;
        }
    }
}