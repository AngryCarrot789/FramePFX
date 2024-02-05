using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FramePFX.Natives {
    /// <summary>
    /// A class which contains all of the native methods available through the PFX native composition engine
    /// </summary>
    internal class PFXNative {
        private const string DLL_NAME = "FramePFX.NativeEngine.dll";
#if DEBUG
        private const string DLL_PATH = "..\\..\\..\\..\\x64\\Debug\\" + DLL_NAME;
#else
        private const string DLL_PATH = "..\\..\\..\\..\\x64\\Release\\" + DLL_NAME;
#endif

        #region System Helpers

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        private delegate int PFXCEFUNC_InitEngine();

        private static PFXCEFUNC_InitEngine InitEngine;

        private static IntPtr LibraryAddress;

        public static void InitialiseLibrary() {
            LibraryAddress = LoadLibrary(DLL_PATH);
            if (LibraryAddress == IntPtr.Zero) {
                throw new Exception("Failed to load library", new Win32Exception());
            }

            InitEngine = GetFunction<PFXCEFUNC_InitEngine>("PFXCE_InitEngine");
            if (InitEngine() != 1) {
                throw new Exception("Engine initialisation failed");
            }
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