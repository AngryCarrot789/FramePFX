using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media.Imaging;

namespace FramePFX.WPF.Explorer.Icons {
    // Main credits:
    // https://www.c-sharpcorner.com/forums/getting-folders-icon-imageunable-to-take-large-icons
    public static class ShellEx {
        // Constants that we need in the function call

        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_SMALLICON = 0x1;
        private const int SHGFI_LARGEICON = 0x0;
        private const int SHIL_JUMBO = 0x4;
        private const int SHIL_EXTRALARGE = 0x2;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_SYSICONINDEX = 0x4000;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint SHGFI_LINKOVERLAY = 0x000008000;

        // This structure will contain information about the file

        public struct SHFILEINFO {
            public IntPtr hIcon; // Handle to the icon representing the file
            public int iIcon; // Index of the icon within the image list
            public uint dwAttributes; // Various attributes of the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szDisplayName; // Path to the file

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName; // File type

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static bool TryCloseHandle(IntPtr hObject) {
            try {
                return CloseHandle(hObject);
            }
            catch { // throws with 0 as a win32 error when a debugger is attached... for some reason :/
                int error = Marshal.GetLastWin32Error();
                if (error != 0) {
                    throw new Win32Exception(error);
                }
                else {
                    return true;
                }
            }
        }

        private struct IMAGELISTDRAWPARAMS {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap; // x offest from the upperleft of bitmap
            public int yBitmap; // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGEINFO {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator Point(POINT p) {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p) {
                return new POINT(p.X, p.Y);
            }
        }

        #region Private ImageList COM Interop (XP)

        [ComImport()]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //helpstring("Image List"),
        interface IImageList {
            [PreserveSig]
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

            [PreserveSig]
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);

            [PreserveSig]
            int SetOverlayImage(int iImage, int iOverlay);

            [PreserveSig]
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

            [PreserveSig]
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(int i);

            [PreserveSig]
            int GetIcon(int i, int flags, ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

            [PreserveSig]
            int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int Clone(ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(int i, ref RECT prc);

            [PreserveSig]
            int GetIconSize(ref int cx, ref int cy);

            [PreserveSig]
            int SetIconSize(int cx, int cy);

            [PreserveSig]
            int GetImageCount(ref int pi);

            [PreserveSig]
            int SetImageCount(int uNewCount);

            [PreserveSig]
            int SetBkColor(int clrBk, ref int pclr);

            [PreserveSig]
            int GetBkColor(ref int pclr);

            [PreserveSig]
            int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(IntPtr hwndLock, int x, int y);

            [PreserveSig]
            int DragLeave(IntPtr hwndLock);

            [PreserveSig]
            int DragMove(int x, int y);

            [PreserveSig]
            int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

            [PreserveSig]
            int DragShowNolock(int fShow);

            [PreserveSig]
            int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(int i, ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(int iOverlay, ref int piIndex);
        }

        #endregion

        ///
        /// SHGetImageList is not exported correctly in XP.  See KB316931
        /// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
        /// Apparently (and hopefully) ordinal 727 isn't going to change.
        ///
        [DllImport("shell32.dll", EntryPoint = "#727", SetLastError = true)]
        private extern static int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

        // The signature of SHGetFileInfo (located in Shell32.dll)
        [DllImport("Shell32.dll", SetLastError = true)]
        public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, uint uFlags);

        [DllImport("Shell32.dll", SetLastError = true)]
        public static extern int SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, uint uFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, Int32 nFolder, ref IntPtr ppidl);

        [DllImport("user32", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        private static BitmapSource getBitmapSourceForIcon(Icon ic) {
            BitmapSource ic2 = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ic.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            ic2.Freeze();
            return ic2;
        }

        public static BitmapSource GetBitmapSourceForSystemIcon(bool small, CSIDL csidl) {

            IntPtr pidlTrash = IntPtr.Zero;
            int hr = SHGetSpecialFolderLocation(IntPtr.Zero, (int) csidl, ref pidlTrash);
            if (hr != 0) {
                // most likely invalid CSIDL; unsupported icon maybe?
                // i put [broken; invalid] infront of the enum entries that
                // cause an exception to be thrown
                throw new Win32Exception(hr);
            }

            SHFILEINFO shinfo = new SHFILEINFO();

            // Get a handle to the large icon
            uint flags;
            uint SHGFI_PIDL = 0x000000008;
            if (small) {
                flags = SHGFI_PIDL | SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;
            }
            else {
                flags = SHGFI_PIDL | SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES;
            }

            int res = SHGetFileInfo(pidlTrash, 0, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (res == 0) {
                throw new Win32Exception(res);
            }

            Icon myIcon = Icon.FromHandle(shinfo.hIcon);
            Marshal.FreeCoTaskMem(pidlTrash);
            BitmapSource bs = getBitmapSourceForIcon(myIcon);
            myIcon.Dispose();
            bs.Freeze(); // important for no memory leaks
            DestroyIcon(shinfo.hIcon);
            TryCloseHandle(shinfo.hIcon);
            return bs;

        }

        public static BitmapSource getBitmapSourceIconForPath(string path, bool small, bool checkDisk, bool addOverlay) {
            SHFILEINFO shinfo = new SHFILEINFO();

            uint flags;
            if (small) {
                flags = SHGFI_ICON | SHGFI_SMALLICON;
            }
            else {
                flags = SHGFI_ICON | SHGFI_LARGEICON;
            }

            if (!checkDisk)
                flags |= SHGFI_USEFILEATTRIBUTES;
            if (addOverlay)
                flags |= SHGFI_LINKOVERLAY;

            int res = SHGetFileInfo(path, 0, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (res == 0) {
                throw new FileNotFoundException();
            }

            Icon myIcon = Icon.FromHandle(shinfo.hIcon);

            BitmapSource bs = getBitmapSourceForIcon(myIcon);
            myIcon.Dispose();
            bs.Freeze();
            DestroyIcon(shinfo.hIcon);
            TryCloseHandle(shinfo.hIcon);
            return bs;

        }

        public static BitmapSource GetBitmapSourceForPath(string path, bool jumbo, bool checkDisk) {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_SYSICONINDEX;
            if (!checkDisk) // This does not seem to work. If I try it, a folder icon is always returned.
                flags |= SHGFI_USEFILEATTRIBUTES;

            int res = SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (res == 0) {
                throw new FileNotFoundException("File not found", path);
            }

            int iconIndex = shinfo.iIcon;

            // Get the System IImageList object from the Shell:
            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            int size = jumbo ? SHIL_JUMBO : SHIL_EXTRALARGE;
            int hres = SHGetImageList(size, ref iidImageList, out IImageList imageList);
            if (hres != 0) {
                throw new Win32Exception();
            }

            IntPtr hIcon = IntPtr.Zero;
            const int ILD_TRANSPARENT = 1;
            hres = imageList.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            if (hres != 0) {
                throw new Win32Exception();
            }

            Icon myIcon = Icon.FromHandle(hIcon);
            BitmapSource bs = getBitmapSourceForIcon(myIcon);
            myIcon.Dispose();
            bs.Freeze(); // very important to avoid memory leak
            DestroyIcon(hIcon);
            TryCloseHandle(hIcon);
            return bs;
        }
    }
}