using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace FramePFX.Natives {
    /// <summary>
    /// A class which contains all of the native methods available through the PFX native composition engine
    /// </summary>
    internal class PFXNative {
        #region System Helpers

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        // Prefix: PFXCEFUNC_

        private delegate int PFXCEFUNC_InitEngine();
        public unsafe delegate int PFXCEFUNC_PixelateVfx(uint* pImg, int srcWidth, int srcHeight, int left, int top, int right, int bottom, int blockSize);

        private static PFXCEFUNC_InitEngine InitEngine;
        public static PFXCEFUNC_PixelateVfx PFXCE_PixelateVfx;

        private static IntPtr LibraryAddress;

        public static void InitialiseLibrary() {
            const string DLL_NAME = "FramePFX.NativeEngine.dll";
            string dllPath = Path.Combine(Path.GetFullPath("."), DLL_NAME);
            if (!File.Exists(dllPath)) {
                #if DEBUG
                dllPath = "..\\..\\..\\..\\x64\\Debug\\" + DLL_NAME;
                #else
                dllPath = "..\\..\\..\\..\\x64\\Release\\" + DLL_NAME;
                #endif
            }

            if (!File.Exists(dllPath)) {
                throw new Exception("Library DLL could not be found. Make sure you built the C++ project first");
            }

            LibraryAddress = LoadLibrary(dllPath);
            if (LibraryAddress == IntPtr.Zero) {
                throw new Exception("Failed to load library", new Win32Exception());
            }

            InitEngine = GetFunction<PFXCEFUNC_InitEngine>("PFXCE_InitEngine");
            if (InitEngine() != 1) {
                throw new Exception("Engine initialisation failed");
            }

            PFXCE_PixelateVfx = GetFunction<PFXCEFUNC_PixelateVfx>("PFXCE_PixelateVfx");
        }

        public static void ShutdownLibrary() {
            if (LibraryAddress != IntPtr.Zero) {
                try {
                    FreeLibrary(LibraryAddress);
                }
                finally {
                    LibraryAddress = IntPtr.Zero;
                }
            }
        }

        private static T GetFunction<T>(string functionName) where T : Delegate {
            IntPtr pFuncAddress = GetProcAddress(LibraryAddress, functionName);
            if (pFuncAddress == IntPtr.Zero)
                throw new Exception("Could not find function address for name: " + functionName);
            Delegate theDelegate = Marshal.GetDelegateForFunctionPointer(pFuncAddress, typeof(T));
            if (theDelegate == null)
                throw new Exception("Could not create delegate for function pointer");
            return (T)theDelegate;
        }
    }
}