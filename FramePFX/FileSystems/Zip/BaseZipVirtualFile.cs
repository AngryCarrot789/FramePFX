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
using System.Linq;

namespace FramePFX.FileSystems.Zip
{
    public abstract class BaseZipVirtualFile : VirtualFile
    {
        protected readonly string name;
        protected readonly string path;

        public override string Name => this.name;

        public override string Path => this.path;

        protected BaseZipVirtualFile(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

    public class ZipFileVirtualFile : BaseZipVirtualFile
    {
        public override bool IsDirectory => false;

        public override VirtualFile Parent { get; }

        public ZipFileVirtualFile(string name, string path) : base(name, path)
        {
        }

        public override IEnumerable<VirtualFile> GetChildren()
        {
            throw new InvalidOperationException("Cannot get children of a file");
        }
    }

    public class ZipDirectoryVirtualFile : BaseZipVirtualFile
    {
        private readonly List<BaseZipVirtualFile> files;

        public override bool IsDirectory => true;

        protected ZipDirectoryVirtualFile(string name, IEnumerable<BaseZipVirtualFile> files) : base(name, null)
        {
            this.files = files.ToList();
        }

        public override VirtualFile Parent { get; }

        public override IEnumerable<VirtualFile> GetChildren()
        {
            return this.files;
        }
    }
}