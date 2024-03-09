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
using System.Collections.Generic;
using FramePFX.FileSystems.Physical;
using FramePFX.FileSystems.Zip;

namespace FramePFX.FileSystems
{
    /// <summary>
    /// A class which manages all types of virtual file systems
    /// </summary>
    public class VirtualFileManager
    {
        private readonly Dictionary<string, WeakReference<ZipVirtualFileSystem>> weakZipVfsMap;

        public const string ProtocolSeparator = "://";
        public const char PathPartSeparator = '/';

        public VirtualFileManager()
        {
            this.weakZipVfsMap = new Dictionary<string, WeakReference<ZipVirtualFileSystem>>();
        }

        /// <summary>
        /// Gets a virtual file for a full URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public VirtualFileSystem GetFileForUrl(VirtualFileUrl url)
        {
            url.EnsureValid();
            return null;
        }

        public static VirtualFileUrl GetUrl(VirtualFile file)
        {
            string protocol = file.FileSystem.Protocol;
            string path = file.Name;


            return new VirtualFileUrl(protocol, path);
        }
    }
}