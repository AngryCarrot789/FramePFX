using System;
using FramePFX.FileBrowser.FileTree.Interfaces;
using FramePFX.FileBrowser.FileTree.Physical;

namespace FramePFX.FileBrowser.FileTree
{
    public static class EntrySorters
    {
        public static readonly Comparison<TreeEntry> CompareFileName = (a, b) =>
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

        public static readonly Comparison<TreeEntry> ComparePhysicalDirectoryAndFileName = (a, b) =>
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

        public static readonly Comparison<TreeEntry> CompareZippedDirectoryAndFileName = (a, b) =>
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