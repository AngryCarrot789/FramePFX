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
using FramePFX.FileBrowser.FileTree.Interfaces;
using FramePFX.FileBrowser.FileTree.Physical;

namespace FramePFX.FileBrowser.FileTree
{
    public static class EntrySorters
    {
        public static readonly Comparison<VFSFileEntry> CompareFileName = (a, b) =>
        {
            if (a is IFileName nA)
            {
                if (b is IFileName nB)
                {
                    return string.Compare(nA.FileName, nB.FileName);
                }
                else
                {
                    return -1;
                }
            }
            else if (b is IFileName)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        };

        public static readonly Comparison<VFSFileEntry> ComparePhysicalDirectoryAndFileName = (a, b) =>
        {
            if (a is PhysicalVirtualFolder)
            {
                return b is PhysicalVirtualFolder ? CompareFileName(a, b) : -1;
            }
            else
            {
                return b is PhysicalVirtualFolder ? 1 : CompareFileName(a, b);
            }
        };

        public static readonly Comparison<VFSFileEntry> CompareZippedDirectoryAndFileName = (a, b) =>
        {
            if (a.IsDirectory)
            {
                if (b.IsDirectory)
                {
                    return CompareFileName(a, b);
                }
                else
                {
                    return -1; // A comes before B
                }
            }
            else if (b.IsDirectory)
            {
                return 1; // A comes after B
            }
            else
            {
                return CompareFileName(a, b);
            }
        };
    }
}