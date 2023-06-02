﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop; // for WPF support

// for WPF support

namespace FrameControlEx.Views.FilePicking {
    public class FolderPicker {
        public virtual string ResultPath { get; protected set; }
        public virtual string ResultName { get; protected set; }
        public virtual string InputPath { get; set; }
        public virtual bool ForceFileSystem { get; set; }
        public virtual string Title { get; set; }
        public virtual string OkButtonLabel { get; set; }
        public virtual string FileNameLabel { get; set; }

        protected virtual int SetOptions(int options) {
            if (this.ForceFileSystem) {
                options |= (int) FOS.FOS_FORCEFILESYSTEM;
            }

            return options;
        }

        public static Window GetCurrentActiveWindow() {
            IntPtr window = GetActiveWindow();
            if (window != IntPtr.Zero) {
                foreach (Window win in Application.Current.Windows) {
                    if (new WindowInteropHelper(win).Handle == window) {
                        return win;
                    }
                }
            }

            return Application.Current.MainWindow;
        }

        // for WPF support
        public bool? ShowDialog(Window owner = null, bool throwOnError = false) {
            IntPtr window = IntPtr.Zero;
            if (owner == null) {
                if ((window = GetActiveWindow()) == IntPtr.Zero) {
                    owner = Application.Current.MainWindow;
                }
            }

            if (window == IntPtr.Zero && owner != null) {
                window = new WindowInteropHelper(owner).Handle;
            }

            return this.ShowDialog(window, throwOnError);
        }

        // for all .NET
        public virtual bool? ShowDialog(IntPtr owner, bool throwOnError = false) {
            IFileOpenDialog dialog = (IFileOpenDialog) new FileOpenDialog();
            if (!string.IsNullOrEmpty(this.InputPath)) {
                if (CheckHr(SHCreateItemFromParsingName(this.InputPath, null, typeof(IShellItem).GUID, out IShellItem item), throwOnError) != 0) {
                    return null;
                }

                dialog.SetFolder(item);
            }

            FOS options = FOS.FOS_PICKFOLDERS;
            options = (FOS) this.SetOptions((int) options);
            dialog.SetOptions(options);

            if (this.Title != null) {
                dialog.SetTitle(this.Title);
            }

            if (this.OkButtonLabel != null) {
                dialog.SetOkButtonLabel(this.OkButtonLabel);
            }

            if (this.FileNameLabel != null) {
                dialog.SetFileName(this.FileNameLabel);
            }

            if (owner == IntPtr.Zero) {
                owner = Process.GetCurrentProcess().MainWindowHandle;
                if (owner == IntPtr.Zero) {
                    owner = GetDesktopWindow();
                }
            }

            int hr = dialog.Show(owner);
            if (hr == ERROR_CANCELLED) {
                return null;
            }

            if (CheckHr(hr, throwOnError) != 0) {
                return null;
            }

            if (CheckHr(dialog.GetResult(out IShellItem result), throwOnError) != 0) {
                return null;
            }

            if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out string path), throwOnError) != 0) {
                return null;
            }

            this.ResultPath = path;
            if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out path), false) == 0) {
                this.ResultName = path;
            }

            return true;
        }

        private static int CheckHr(int hr, bool throwOnError) {
            if (hr != 0) {
                if (throwOnError) {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            return hr;
        }

        [DllImport("shell32")]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        [DllImport("user32")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        private const int ERROR_CANCELLED = unchecked((int) 0x800704C7);

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
        private class FileOpenDialog {
        }

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog {
            [PreserveSig] int Show(IntPtr parent); // IModalWindow
            [PreserveSig] int SetFileTypes();  // not fully defined
            [PreserveSig] int SetFileTypeIndex(int iFileType);
            [PreserveSig] int GetFileTypeIndex(out int piFileType);
            [PreserveSig] int Advise(); // not fully defined
            [PreserveSig] int Unadvise();
            [PreserveSig] int SetOptions(FOS fos);
            [PreserveSig] int GetOptions(out FOS pfos);
            [PreserveSig] int SetDefaultFolder(IShellItem psi);
            [PreserveSig] int SetFolder(IShellItem psi);
            [PreserveSig] int GetFolder(out IShellItem ppsi);
            [PreserveSig] int GetCurrentSelection(out IShellItem ppsi);
            [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            [PreserveSig] int GetResult(out IShellItem ppsi);
            [PreserveSig] int AddPlace(IShellItem psi, int alignment);
            [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            [PreserveSig] int Close(int hr);
            [PreserveSig] int SetClientGuid();  // not fully defined
            [PreserveSig] int ClearClientData();
            [PreserveSig] int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
            [PreserveSig] int GetResults([MarshalAs(UnmanagedType.IUnknown)] out object ppenum);
            [PreserveSig] int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem {
            [PreserveSig] int BindToHandler(); // not fully defined
            [PreserveSig] int GetParent(); // not fully defined
            [PreserveSig] int GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            [PreserveSig] int GetAttributes();  // not fully defined
            [PreserveSig] int Compare();  // not fully defined
        }

        private enum SIGDN : uint {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }

        [Flags]
        private enum FOS {
            FOS_OVERWRITEPROMPT = 0x2,
            FOS_STRICTFILETYPES = 0x4,
            FOS_NOCHANGEDIR = 0x8,
            FOS_PICKFOLDERS = 0x20,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_NOVALIDATE = 0x100,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_CREATEPROMPT = 0x2000,
            FOS_SHAREAWARE = 0x4000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_FORCEPREVIEWPANEON = 0x40000000,
            FOS_SUPPORTSTREAMABLEITEMS = unchecked((int) 0x80000000)
        }
    }
}