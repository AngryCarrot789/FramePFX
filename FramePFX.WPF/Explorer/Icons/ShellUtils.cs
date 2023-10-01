using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Explorer.Icons
{
    public static class ShellUtils
    {
        #region Structs

        private struct SHFILEINFO
        {
            public IntPtr hIcon; // Handle to the icon representing the file
            public int iIcon; // Index of the icon within the image list
            public uint dwAttributes; // Various attributes of the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName; // Path to the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName; // File type
        }

        #endregion

        #region DLLs

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObjetc);

        [DllImport("shell32")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

        #endregion

        [DllImport("shell32.dll", SetLastError = true)]
        static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, Int32 nFolder, ref IntPtr ppidl);

        [DllImport("user32")]
        public static extern int DestroyIcon(IntPtr hIcon);

        #region Flags

        private const uint FILE_ATTRIBUTE_READONLY = 0x1;
        private const uint FILE_ATTRIBUTE_HIDDEN = 0x2;
        private const uint FILE_ATTRIBUTE_SYSTEM = 0x4;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x20;
        private const uint FILE_ATTRIBUTE_DEVICE = 0x40;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_TEMPORARY = 0x100;
        private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
        private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        private const uint FILE_ATTRIBUTE_COMPRESSED = 0x800;
        private const uint FILE_ATTRIBUTE_OFFLINE = 0x1000;
        private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000;
        private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x4000;
        private const uint FILE_ATTRIBUTE_VIRTUAL = 0x10000;

        private const uint SHGFI_ICON = 0x100; // get icon
        private const uint SHGFI_DISPLAYNAME = 0x200; // get display name
        private const uint SHGFI_TYPENAME = 0x400; // get type name
        private const uint SHGFI_ATTRIBUTES = 0x800; // get attributes
        private const uint SHGFI_ICONLOCATION = 0x1000; // get icon location
        private const uint SHGFI_EXETYPE = 0x2000; // return exe type
        private const uint SHGFI_SYSICONINDEX = 0x4000; // get system icon index
        private const uint SHGFI_LINKOVERLAY = 0x8000; // put a link overlay on icon
        private const uint SHGFI_SELECTED = 0x10000; // show icon in selected state
        private const uint SHGFI_ATTR_SPECIFIED = 0x20000; // get only specified attributes
        private const uint SHGFI_LARGEICON = 0x0; // get large icon
        private const uint SHGFI_SMALLICON = 0x1; // get small icon
        private const uint SHGFI_OPENICON = 0x2; // get open icon
        private const uint SHGFI_SHELLICONSIZE = 0x4; // get shell size icon
        private const uint SHGFI_PIDL = 0x8; // pszPath is a pidl
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10; // use passed dwFileAttribute

        private const int SHIL_JUMBO = 0x4;
        private const int SHIL_EXTRALARGE = 0x2;

        #endregion

        public static BitmapSource IconToBitmapSource(Icon ico)
        {
            IntPtr hBitmap = ico.ToBitmap().GetHbitmap();
            if (hBitmap == IntPtr.Zero)
            {
                return null;
            }

            BitmapSource image = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return image;
        }

        public static Icon GetFileIcon(string path, ShellIconSize iconType, bool isDirectoryOrDrive)
        {
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            switch (iconType)
            {
                case ShellIconSize.Small:
                    flags |= SHGFI_SMALLICON;
                    break;
                case ShellIconSize.Large:
                    flags |= SHGFI_SHELLICONSIZE;
                    break;
            }


            uint attributes = FILE_ATTRIBUTE_NORMAL;
            if (isDirectoryOrDrive)
            {
                attributes |= FILE_ATTRIBUTE_DIRECTORY;
            }

            IntPtr success =
                SHGetFileInfo(
                    path,
                    attributes,
                    out SHFILEINFO shfi,
                    (uint) Marshal.SizeOf(typeof(SHFILEINFO)),
                    flags);

            return success == IntPtr.Zero ? null : Icon.FromHandle(shfi.hIcon);
        }

        public static BitmapSource GetFileIconAsBitmapSource(string path, ShellIconSize iconType = ShellIconSize.Normal, bool isDirectory = false)
        {
            using (Icon icon = GetFileIcon(path, iconType, isDirectory))
            {
                return icon != null ? IconToBitmapSource(icon) : null;
            }
        }

        public static string GetFileTypeDescription(string fileNameOrExtension)
        {
            if (SHGetFileInfo(fileNameOrExtension, FILE_ATTRIBUTE_NORMAL, out SHFILEINFO shfi, (uint) Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_USEFILEATTRIBUTES | SHGFI_TYPENAME) != IntPtr.Zero)
            {
                return shfi.szTypeName;
            }

            return null;
        }
    }
}